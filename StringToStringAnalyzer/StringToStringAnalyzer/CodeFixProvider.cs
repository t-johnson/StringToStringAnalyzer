using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;

namespace StringToStringAnalyzer
{
   [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringToStringAnalyzerCodeFixProvider)), Shared]
   public class StringToStringAnalyzerCodeFixProvider : CodeFixProvider
   {
      private const string title = "Remove ToString() calls on Strings";

      public sealed override ImmutableArray<string> FixableDiagnosticIds
      {
         get { return ImmutableArray.Create(StringToStringAnalyzerAnalyzer.DiagnosticId); }
      }

      public sealed override FixAllProvider GetFixAllProvider()
      {
         // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
         return WellKnownFixAllProviders.BatchFixer;
      }

      public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
      {
         var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

         var diagnostic = context.Diagnostics.First();
         var diagnosticSpan = diagnostic.Location.SourceSpan;

         // Find the InvocationExpressionSyntax identified by the diagnostic.
         var invocation = root.FindToken(diagnosticSpan.Start).Parent.Ancestors().OfType<InvocationExpressionSyntax>().First(t => 
            t.IsKind(SyntaxKind.InvocationExpression) &&
            ((MemberAccessExpressionSyntax)t.Expression).Name.Identifier.ValueText == "ToString");

         // Register a code action that will invoke the fix.
         context.RegisterCodeFix(
             CodeAction.Create(
                 title: title,
                 createChangedDocument: c => RemoveMethodCallAsnyc(context.Document, invocation, c),
                 equivalenceKey: title),
             diagnostic);
      }

      private async Task<Document> RemoveMethodCallAsnyc(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
      {
         var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
         var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
         var memberSymbol = memberAccessExpr.Expression;

         //clone its trivia
         memberSymbol = memberSymbol.WithTriviaFrom(invocationExpr);
         var root = await document.GetSyntaxRootAsync();
         var newRoot = root.ReplaceNode(invocationExpr, memberSymbol);

         var newDocument = document.WithSyntaxRoot(newRoot);
         return newDocument;
      }
   }
}