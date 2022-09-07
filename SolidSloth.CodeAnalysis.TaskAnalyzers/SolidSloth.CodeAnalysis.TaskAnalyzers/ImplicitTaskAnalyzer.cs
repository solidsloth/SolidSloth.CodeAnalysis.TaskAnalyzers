using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace SolidSloth.CodeAnalysis.TaskAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ImplicitTaskAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SSCA03E8";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Assignment";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterOperationAction(AnalyzeVariableDeclaration, OperationKind.VariableDeclaration);
        }

        private static void AnalyzeVariableDeclaration(OperationAnalysisContext ctx)
        {
            var identifier = ctx.Operation.Syntax.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            if (identifier?.IsVar != true) return;

            var identifierSymbol = ctx.Operation.SemanticModel.GetSymbolInfo(identifier).Symbol as INamedTypeSymbol;

            if (identifierSymbol is null) return;

            var baseTypeName = identifierSymbol.BaseType.ToString();

            if (baseTypeName != "System.Threading.Tasks.Task") return;

            var loc = Location.Create(ctx.Operation.SemanticModel.SyntaxTree, identifier.Span);
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, loc, "This is a task beign assigned to var"));
        }
    }
}
