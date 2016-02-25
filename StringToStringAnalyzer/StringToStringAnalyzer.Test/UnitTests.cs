using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using StringToStringAnalyzer;

namespace StringToStringAnalyzer.Test
{
   [TestClass]
   public class UnitTest : CodeFixVerifier
   {

      //No diagnostics expected to show up
      [TestMethod]
      public void TestIdentifiesNoIssuesWithEmptyCode()
      {
         var test = @"";

         VerifyCSharpDiagnostic(test);
      }

      //Diagnostic and CodeFix both triggered and checked for constant string .ToString
      [TestMethod]
      public void TestIdentifiesIssueOnConstantString()
      {
         var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           const string sample = ""sample"";
           public Something()
           {
              Debug.WriteLine(sample.ToString());
           }
        }
    }";
         var expected = new DiagnosticResult
         {
            Id = "StringToStringAnalyzer",
            Message = String.Format("ToString() called on string member '{0}'", "sample"),
            Severity = DiagnosticSeverity.Warning,
            Locations =
                 new[] {
                            new DiagnosticResultLocation("Test0.cs", 16, 31)
                     }
         };

         VerifyCSharpDiagnostic(test, expected);

         var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           const string sample = ""sample"";
           public Something()
           {
              Debug.WriteLine(sample);
           }
        }
    }";
         VerifyCSharpFix(test, fixtest);
      }

      //Diagnostic not triggered for ToString() with formatting
      [TestMethod]
      public void TestIdentifiesNoIssuesOnToStringWithFormatting()
      {
         var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           const string sample = ""sample"";
           public Something()
           {
              Console.WriteLine(sample.ToString(""N""));
           }
        }
    }";

         VerifyCSharpDiagnostic(test);

      }

      //Diagnostic and CodeFix both triggered and checked for string.Trim().ToString()
      [TestMethod]
      public void TestIdentifiesIssuesWithStringMethod()
      {
         var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           const string sample = ""sample"";
           public Something()
           {
              System.Diagnostics.Debug.WriteLine(sample.Trim().ToString());
           }
        }
    }";
         var expected = new DiagnosticResult
         {
            Id = "StringToStringAnalyzer",
            Message = String.Format("ToString() called on string member '{0}'", "sample.Trim()"),
            Severity = DiagnosticSeverity.Warning,
            Locations =
                 new[] {
                            new DiagnosticResultLocation("Test0.cs", 16, 50)
                     }
         };

         VerifyCSharpDiagnostic(test, expected);

         var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           const string sample = ""sample"";
           public Something()
           {
              System.Diagnostics.Debug.WriteLine(sample.Trim());
           }
        }
    }";
         VerifyCSharpFix(test, fixtest);
      }

      //Diagnostic and CodeFix both triggered and checked for string[3].ToString()
      [TestMethod]
      public void TestIdentifiesIssuesWithStringArray()
      {
         var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           public Something()
           {
              string[] sample = { ""sample"" };
              System.Diagnostics.Debug.WriteLine(sample[0].ToString());
           }
        }
    }";
         var expected = new DiagnosticResult
         {
            Id = "StringToStringAnalyzer",
            Message = String.Format("ToString() called on string member '{0}'", "sample[0]"),
            Severity = DiagnosticSeverity.Warning,
            Locations =
                 new[] {
                            new DiagnosticResultLocation("Test0.cs", 16, 50)
                     }
         };

         VerifyCSharpDiagnostic(test, expected);

         var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class Something
        {   
           public Something()
           {
              string[] sample = { ""sample"" };
              System.Diagnostics.Debug.WriteLine(sample[0]);
           }
        }
    }";
         VerifyCSharpFix(test, fixtest);
      }

      protected override CodeFixProvider GetCSharpCodeFixProvider()
      {
         return new StringToStringAnalyzerCodeFixProvider();
      }

      protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
      {
         return new StringToStringAnalyzerAnalyzer();
      }
   }
}