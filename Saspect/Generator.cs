using System.Text;
using Microsoft.CodeAnalysis;

namespace Saspect;

[Generator]
public class Generator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var log = new StringBuilder();

		var valueProvider = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: (syntaxNode, cancellationToken) => GeneratorInput.TriggerFor(syntaxNode, cancellationToken, log),
				transform: (syntaxContext, cancellationToken) => GeneratorInput.ValueProviderFor(syntaxContext, cancellationToken, log))
			.Where(m => m is not null)!;

		var valueProviderWithCompilation = context.CompilationProvider.Combine(valueProvider.Collect());

		context.RegisterSourceOutput(valueProviderWithCompilation,
			(outputContext, input) =>
			{
				if (outputContext.CancellationToken.IsCancellationRequested)
					return;
				GeneratorOutput.Generate(input, outputContext, log);
				GeneratorLog.Output(log, outputContext);
			});
	}
}
