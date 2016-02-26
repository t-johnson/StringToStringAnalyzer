# StringToStringAnalyzer
Visual Studio 2015 Analyzer to find and fix ToString() calls on strings.

#### Finds and fixes instances like this:

   Debug.WriteLine("abc".ToString()); 

gets fixed to:

   Debug.WriteLine("abc");

where "abc" could be a literal, or a variable, or returned from a method
