using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Saspect;

internal class GeneratorOutput
{
	internal static string GeneratedCodeAttribute()
	{
		var assembly = typeof(GeneratorOutput).Assembly;
		return @$"[System.CodeDom.Compiler.GeneratedCode(""{assembly.GetName().Name}"", ""{assembly.GetName().Version}"")]";
	}

	internal static void Generate((Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) source, SourceProductionContext context, StringBuilder log)
	{
		var compilation = source.Left;
		var classes = source.Right; //.Distinct();

		generate(classes, compilation, context, log);
	}

	private static void generate(ImmutableArray<ClassDeclarationSyntax> classes, Compilation compilation, SourceProductionContext context, StringBuilder log)
	{
		classes.ForEach(clasz =>
		{
			if (SyntaxExtensions.ClassIsAbstract(clasz, log))
				return;

			var model = compilation.GetSemanticModel(clasz.SyntaxTree);
			var classSymbol = model.GetDeclaredSymbol(clasz);

			if (!SyntaxExtensions.ClassWithAspectAttribute(clasz, classSymbol, log))
				return;

			var className = clasz.Identifier.ToString();
			log.AppendLine("class: " + className);

			try
			{
				var code = new StringBuilder();

				var file = SyntaxExtensions.GetFile(clasz, log);
				file.Usings.ForEach(u => { code.Append(u.GetText()); });

				var namespaz = SyntaxExtensions.GetNamespace(clasz, log);
				log.AppendLine("namespace: " + namespaz);

				code.Append($@"
namespace {namespaz}
{{
");
				var generatedFullName = generateClass(namespaz, clasz, compilation, code);
				code.Append($@"
}}
");

				log.AppendLine($"writing: {generatedFullName}");

				context.AddSource(
					$"{generatedFullName}.Aspected.g.cs",
					SourceText.From(code.ToString(), Encoding.UTF8)
				);
			}
			catch (Exception e)
			{
				log.AppendLine("Boom " + e);
			}
		});
	}

	private static string generateClass(
		string namespaz,
		ClassDeclarationSyntax clasz,
		Compilation compilation,
		StringBuilder code)
	{
		var className = clasz.Identifier.ToString();
		var aspectedClassName = $"{className}Aspected";

		// nested class
		if (clasz.Parent is ClassDeclarationSyntax pc)
		{
			className = $@"{pc.Identifier}.{className}";
			aspectedClassName = $@"{pc.Identifier}_{aspectedClassName}";
		}

		var methods = clasz.Members.OfType<MethodDeclarationSyntax>();

		code.Append(@$"
	{GeneratedCodeAttribute()}
	[System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	[System.Diagnostics.DebuggerStepThrough]
	[Saspect.GeneratedAspectProxy]
	public class {aspectedClassName} : {className}
	{{
		private Saspect.AspectInterceptor _interceptor;
");

		generateCtor(code, clasz, aspectedClassName);

		code.Append($@"
		{{
			_interceptor = interceptor;
		}}
");

		methods.ForEach(method =>
		{
			// possible to get from class symbol, and if so, faster?
			var model = compilation.GetSemanticModel(method.SyntaxTree);
			var methodSymbol = model.GetDeclaredSymbol(method);

			generateMethod(code, method, methodSymbol);
		});

		code.Append($@"
	}}
");

		return $"{namespaz}.{aspectedClassName}";
	}

	private static void generateCtor(StringBuilder code, ClassDeclarationSyntax clasz, string className)
	{
		code.Append($@"
		public {className}");

		var ctor = clasz.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();

		if (ctor == null)
		{
			code.Append($"(Saspect.AspectInterceptor interceptor)");
			return;
		}

		var parameters = string.Join(", ",
			ctor.ParameterList.Parameters
				.Select(x => x.GetText().ToString().Trim())
				.ToArray()
		);

		var arguments = string.Join(", ",
			ctor.ParameterList.Parameters
				.Select(x => x.Identifier.ToString())
				.ToArray()
		);

		code.Append($"(Saspect.AspectInterceptor interceptor, {parameters}) : base({arguments})");
	}

	private static void generateMethod(StringBuilder code, MethodDeclarationSyntax method, IMethodSymbol methodSymbol)
	{
		var methodName = method.Identifier.ToString();
		var genericTypes = method.TypeParameterList?.GetText();
		var parameters = method.ParameterList.GetText();
		var returnType = method.ReturnType;

		if (!methodWithAspectAttribute(methodSymbol))
			return;

		if (returnType.ToString() == "void")
			generateVoidMethod(code, method, returnType, methodName, genericTypes, parameters);
		else
			generateFuncMethod(code, method, returnType, methodName, genericTypes, parameters);
	}

	private static bool methodWithAspectAttribute(IMethodSymbol methodSymbol)
	{
		var hasAspectAttribute = methodSymbol
			.GetAttributes()
			.Any(x => x.AttributeClass?.BaseType?.ToString() == typeof(AspectAttribute).FullName);

		return hasAspectAttribute;
	}

	private static void generateVoidMethod(StringBuilder code, MethodDeclarationSyntax method, TypeSyntax returnType, string methodName, SourceText genericTypes, SourceText parameters)
	{
		var parameterTypes = getParameterTypeArray(method);
		var arguments = getArgumentsObjectArray(method);

		var isProtected = method.Modifiers.Any(x => x.IsKind(SyntaxKind.ProtectedKeyword));
		var accessModifier = isProtected ? "protected" : "public";
		var bindingFlags = isProtected ? "System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance" : "System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance";

		code.Append($@"
		{accessModifier} override {returnType} {methodName}{genericTypes}{parameters}	    {{
			System.Action<Saspect.InvocationInfo> proceed = x => {{ 
				base.{methodName}({arguments});
			}};
			_interceptor.Intercept(GetType(), nameof({methodName}), {parameterTypes}, new object[] {{{arguments}}}, proceed, {bindingFlags});
		}}
");
	}

	private static void generateFuncMethod(StringBuilder code, MethodDeclarationSyntax method, TypeSyntax returnType, string methodName, SourceText genericTypes, SourceText parameters)
	{
		var parameterTypes = getParameterTypeArray(method);
		var arguments = getArgumentsObjectArray(method);

		var isProtected = method.Modifiers.Any(x => x.IsKind(SyntaxKind.ProtectedKeyword));
		var accessModifier = isProtected ? "protected" : "public";
		var bindingFlags = isProtected ? "System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance" : "System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance";

		code.Append($@"
		{accessModifier} override {returnType} {methodName}{genericTypes}{parameters}		{{
			{returnType} returnValue = default({returnType});
			System.Action<Saspect.InvocationInfo> proceed = x => {{ 
				returnValue = base.{methodName}({arguments});
				x.ReturnValue = returnValue;
			}};
			_interceptor.Intercept(GetType(), nameof({methodName}), {parameterTypes}, new object[] {{{arguments}}}, proceed, {bindingFlags});
			return returnValue;
		}}
");
	}

	private static string getArgumentsObjectArray(MethodDeclarationSyntax method)
	{
		return string.Join(", ",
			method.ParameterList.Parameters
				.Select(x => x.Identifier.ToString())
				.ToArray()
		);
	}

	private static string getParameterTypeArray(MethodDeclarationSyntax method)
	{
		var parameterTypes = "new System.Type[] {" +
		                     string.Join(", ",
			                     method.ParameterList.Parameters
				                     .Select(x => $"typeof({x.Type.ToString()})")
				                     .ToArray()
		                     ) + "}";
		return parameterTypes;
	}
}
