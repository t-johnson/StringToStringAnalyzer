using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StringToStringAnalyzer
{
   [DiagnosticAnalyzer(LanguageNames.CSharp)]
   public class StringToStringAnalyzerAnalyzer : DiagnosticAnalyzer
   {
      public const string DiagnosticId = "StringToStringAnalyzer";

      // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
      // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
      private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
      private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
      private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
      private const string Category = "Naming";

      private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

      public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

      public override void Initialize(AnalysisContext context)
      {
         context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
      }

      private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
      {
         if (context.Node.IsKind(SyntaxKind.InvocationExpression))
         {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
            MemberAccessExpressionSyntax memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

            //only member access expressions
            if (memberAccess == null)
            {
               return;
            }

            var parent = memberAccess.ChildNodes().First();

            //this is the to string function
            if (memberAccess.Name.Identifier.ValueText != "ToString")
            {
               return;
            }

            var type = context.SemanticModel.GetTypeInfo(parent);
            if (type.Type.SpecialType != SpecialType.System_String)
            {
               return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), parent.ToString()));
         }
      }
   }
}
