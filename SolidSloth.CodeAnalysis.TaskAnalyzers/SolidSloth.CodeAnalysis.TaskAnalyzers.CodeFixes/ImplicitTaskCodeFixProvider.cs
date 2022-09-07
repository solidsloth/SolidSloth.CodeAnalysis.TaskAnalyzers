using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Document = Microsoft.CodeAnalysis.Document;

namespace SolidSloth.CodeAnalysis.TaskAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplicitTaskCodeFixProvider)), Shared]
    public class ImplicitTaskCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ImplicitTaskAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var variableDeclaration = root.FindNode(context.Span).Parent;

            var title = "Await the task";
            
            var addAwaitAction = CodeAction.Create(title, ct => AddAwait(context.Document, variableDeclaration, ct), equivalenceKey: title);
            context.RegisterCodeFix(addAwaitAction, diagnostic);
        }

        private async Task<Document> AddAwait(Document document, SyntaxNode variableDeclaration, CancellationToken cancellationToken)
        {
            var equalsClause = variableDeclaration.DescendantNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();
            if (equalsClause is null) return document;

            var rightHand = equalsClause.DescendantNodes().FirstOrDefault();
            if (rightHand is null) return document;

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            editor.ReplaceNode(rightHand, editor.Generator.AwaitExpression(rightHand));

            return editor.GetChangedDocument();
        }
    }
}
