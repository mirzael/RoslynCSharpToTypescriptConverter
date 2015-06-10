using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        public const string TYPESCRIPT_NUMBER_IDENTIFIER = "number";
        public const string TYPESCRIPT_STRING_IDENTIFIER = "string";
        public const string TYPESCRIPT_BOOLEAN_IDENTIFIER = "boolean";
        public const string TYPESCRIPT_ARRAY_IDENTIFIER = "Array";
        public static string[] CSHARP_NUMBER_IDENTIFIERS = { "int", "Int16", "Int32", "Int64", "uint",
                                                              "long", "ulong", "short", "double", "ushort",
                                                              "decimal", "float", "byte", "sbyte", "Double",
                                                              "Decimal"};
        public static string[] CSHARP_STRING_IDENTIFIERS = { "string", "String" };
        public static string[] CSHARP_BOOLEAN_IDENTIFIERS = { "bool", "Boolean" };
        public static string[] CSHARP_ENUMERABLE_IDENTIFIERS = { "IEnumerable", "List", "ICollection", "IList",
                                                            "Queue", "Stack", "SortedList", "ReadOnlyCollection",
                                                            "IReadOnlyCollection","ObservableCollection" };
        public static int a, b, c;
        public int[][] s;
        public Jawn MyJawn { get; set; }
        public int NumberThingie { get; set; }
        public long StufftoTest { get; set; }
        public IReadOnlyCollection<string[]> Why { get; set; }
        public string[] OtherStuffs { get; set; }

        static void Main(string[] args)
        {
            var workspace = MSBuildWorkspace.Create();

            //Get the Visual Studio Solution
            var solution = workspace.OpenSolutionAsync(args[0]).Result;

            //Get the Projects in the Solution
            foreach (var project in solution.Projects)
            {
                //Get each class in the project
                foreach (var CSharpClass in project.Documents)
                {
                    //If Roslyn can parse the class, then parse it.
                    if (CSharpClass.SupportsSyntaxTree)
                    {
                        //Begin the Typescript class definition
                        Console.WriteLine(string.Format("class {0} {{", CSharpClass.Name.Remove(CSharpClass.Name.Length - 3)));

                        var semanticModel = CSharpClass.GetSemanticModelAsync().Result;
                        //Get the parse tree
                        var t = CSharpClass.GetSyntaxTreeAsync().Result;
                        var root = t.GetRoot();

                        //Get all of the nodes in the tree
                        //We only care about the nodes that are part of Declaring a C# class
                        foreach (var node in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                        {
                            //Get each member of the class
                            foreach (var member in node.Members)
                            {
                                //We only want to make TypeScript equivalent of Properties, Fields, and Enums
                                if (member is PropertyDeclarationSyntax)
                                {
                                    //We have to cast in order to get the properties that we need to make the typescript equivalent
                                    var propMember = (PropertyDeclarationSyntax)member;
                                    Console.WriteLine(string.Format("\t{0}: {1};", propMember.Identifier, GetTypescriptType(semanticModel, propMember.Type)));
                                }
                                else if (member is FieldDeclarationSyntax)
                                {
                                    //We have to cast in order to get the properties that we need to make the typescript equivalent
                                    var fieldMember = (FieldDeclarationSyntax)member;

                                    //In case multiple variables are declared on one line ex:(int a, b, c;) we want to make multiple typescript variables
                                    foreach (var variable in fieldMember.Declaration.Variables)
                                    {
                                        Console.WriteLine(string.Format("\t{0}: {1};", variable.Identifier, GetTypescriptType(semanticModel, fieldMember.Declaration.Type)));
                                    }

                                }
                                else if (member is EnumDeclarationSyntax)
                                {
                                    //The enum declaration in C# is exactly how it is in typescript, so there is no need of formatting other than tabbing.
                                    Console.WriteLine(string.Format("\t{0}", member));
                                }
                            }
                        }

                        //End the Typescript class definition
                        Console.WriteLine("}");
                    }
                }
            }
            //We have this here for debugging purposes
            Console.ReadLine();
        }

        /// <summary>
        /// Gets the typescript identifier in string representation.
        /// </summary>
        /// <param name="typeSymbol">The type symbol in C#.</param>
        ///  This must be a separate function from GetTypeScriptType for recursive purposes.
            ///  (Enumerables, where there are multiple types in one variable declaration, must be recursed);
        private static string GetTypescriptIdentifier(ITypeSymbol typeSymbol)
        {
            //For enumerables, this will get the IEnumerable, without the <T>
            var typeName = typeSymbol.OriginalDefinition.Name;

            string retVal;

            //If the type name is a number in C#, return the Typescript number equivalent
            if (CSHARP_NUMBER_IDENTIFIERS.Contains(typeName))
            {
                retVal = TYPESCRIPT_NUMBER_IDENTIFIER;
            }
            //If the type name is a string in C#, return the Typescript string equivalent
            else if (CSHARP_STRING_IDENTIFIERS.Contains(typeName))
            {
                retVal = TYPESCRIPT_STRING_IDENTIFIER;
            }
            //If the type name is a boolean in C#, return the Typescript boolean equivalent
            else if (CSHARP_BOOLEAN_IDENTIFIERS.Contains(typeName))
            {
                retVal = TYPESCRIPT_BOOLEAN_IDENTIFIER;
            }
            //If the type is an array in C#, return the Typescript array equivalent
            else if (typeSymbol.TypeKind == TypeKind.Array)
            {
                //We must recurse here, in case the element type is another array (two dimensional or more arrays)
                retVal = string.Format("{0}<{1}>", TYPESCRIPT_ARRAY_IDENTIFIER, GetTypescriptIdentifier(((IArrayTypeSymbol)typeSymbol).ElementType));
            }
            //If the type is a named symbol (we really only care about enumerables in here), return the Typescript Equivalent
            else if (typeSymbol is INamedTypeSymbol)
            {
                var namedType = (INamedTypeSymbol)typeSymbol;
                //If the type is an Enumerable in C#, return the Typescript array equivalent.
                if (CSHARP_ENUMERABLE_IDENTIFIERS.Contains(typeName))
                {
                    //We must recurse here, otherwise we would be duplicating logic on how to get the typescript equivalent of the type argument.
                    //There is only one type argument in an enumerable, which is why First() is used
                    retVal = string.Format("{0}<{1}>", TYPESCRIPT_ARRAY_IDENTIFIER, GetTypescriptIdentifier(namedType.TypeArguments.First()));
                }
                else
                {
                    //If it's not an enumerable, just return the default name
                    retVal = typeSymbol.OriginalDefinition.Name;
                }
            }
            else
            {
                //If it's not a typescript primitive type, just return the name of the type 
                retVal = typeSymbol.OriginalDefinition.Name;
            }

            return retVal;
        }

        /// <summary>
        /// Gets the string typescript equivalent representation of the C# type.
        /// </summary>
        /// <param name="model">The semantic model.</param>
        /// <param name="type">The type in C#.</param>
        public static string GetTypescriptType(SemanticModel model, TypeSyntax type)
        {
            var typeInfo = model.GetTypeInfo(type);
            return GetTypescriptIdentifier(typeInfo.Type);
        }
    }
}
