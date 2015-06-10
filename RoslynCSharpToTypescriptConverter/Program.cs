using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.MSBuild;

namespace RoslynCSharpToTypeScriptConverter
{
    class Program
    {
        public static int a, b, c;
        public int[][] s;
        public IReadOnlyCollection<string[]> Why { get; set; }
        private string[] OtherStuffs { get; set; }

        static void Main(string[] args)
        {
            if (args.Length < 1) { throw new ArgumentException("Please enter a solution path"); }

            var solutionParser = new TypeScriptRoslynSolutionParser(args[0]); 
            solutionParser.ParseSolution();

            StringBuilder builder = new StringBuilder();

            foreach (var tClass in solutionParser.Classes)
            {
                builder.AppendLine(tClass.ToString());
            }

            Console.WriteLine(builder.ToString());

            //We have this here for debugging purposes
            Console.ReadLine();
        }
    }
}