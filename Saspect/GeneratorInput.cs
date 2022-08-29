using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saspect;

internal static class GeneratorInput
{
	internal static bool TriggerFor(SyntaxNode node, CancellationToken cancellationToken, StringBuilder log)
	{
		if (node is not ClassDeclarationSyntax clasz)
			return false;

		var possiblyAspectedClass = SyntaxExtensions.ClassWithMethodAttribute(clasz, log);

		return possiblyAspectedClass
		       || clasz.Members.OfType<ClassDeclarationSyntax>()
			       .Any(x => SyntaxExtensions.ClassWithMethodAttribute(clasz, log));
	}

	public static ClassDeclarationSyntax ValueProviderFor(GeneratorSyntaxContext context, CancellationToken cancellationToken, StringBuilder log)
	{
		var @class = context.Node as ClassDeclarationSyntax;
		return @class;
	}

}
