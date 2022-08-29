using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saspect;

internal static class SyntaxExtensions
{
	internal static bool ClassWithMethodAttribute(ClassDeclarationSyntax clasz, StringBuilder log)
	{
		log.AppendLine($@"classWithMethodAttribute {clasz.Identifier}");

		var classMethodWithAttribute =
			from m in clasz.Members.OfType<MethodDeclarationSyntax>()
			from al in m.AttributeLists
			where al.Attributes.Count > 0
			select m;

		var result = classMethodWithAttribute.Any();

		log.AppendLine($@"/classWithMethodAttribute {clasz.Identifier} {result}");

		return result;
	}
	
	internal static bool ClassIsAbstract(ClassDeclarationSyntax clasz, StringBuilder log)
	{
		log.AppendLine($@"classIsAbstract {clasz.Identifier}");
		var result = clasz.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword));
		log.AppendLine($@"/classIsAbstract {clasz.Identifier} {result}");
		return result;
	}
	
	internal static bool ClassWithAspectAttribute(ClassDeclarationSyntax clasz, INamedTypeSymbol classSymbol, StringBuilder log)
	{
		log.AppendLine($@"classWithAspectAttribute {clasz.Identifier}");

		var result = classSymbol
			.GetMembers().OfType<IMethodSymbol>()
			.SelectMany(x => x.GetAttributes())
			.Any(x => x.AttributeClass?.BaseType?.ToString() == typeof(AspectAttribute).FullName);

		log.AppendLine($@"/classWithAspectAttribute {clasz.Identifier} {result}");

		return result;
	}

	internal static CompilationUnitSyntax GetFile(ClassDeclarationSyntax clasz, StringBuilder log)
	{
		log.AppendLine($"getFile {clasz.Identifier}");
		return clasz.Ancestors().OfType<CompilationUnitSyntax>().Single();
	}
	
	internal static string GetNamespace(ClassDeclarationSyntax clasz, StringBuilder log)
	{
		log.AppendLine($"getNamespace {clasz.Identifier}");
		
		var fileNamespace = clasz.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>();
		if (fileNamespace.Any())
			return fileNamespace.Single().Name.ToString();
		var parentNamespace = clasz.Ancestors().OfType<NamespaceDeclarationSyntax>();
		if (parentNamespace.Any())
			return parentNamespace.Single().Name.ToString();
		
		throw new Exception("Cant find namespace");
	}

}
