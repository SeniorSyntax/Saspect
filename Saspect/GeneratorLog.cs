using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Saspect;

internal class GeneratorLog
{
	internal static void Output(StringBuilder log, SourceProductionContext outputContext)
	{
// 		var code = $@"
// using System;
// namespace Saspect {{
// 	{GeneratorOutput.GeneratedCodeAttribute()}
// 	[System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
// 	internal static class GeneratorOutput
// 	{{
// 		internal static void AMethod() {{}}
// 	}}
// }}
//
// /*
// {log}
// */
// ";
//
// 		outputContext.AddSource(
// 			"Saspect.GeneratorOutput.g.cs",
// 			SourceText.From(code, Encoding.UTF8)
// 		);
	}
}
