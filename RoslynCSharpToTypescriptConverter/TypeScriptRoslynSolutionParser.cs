using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptRoslynSolutionParser
    {
        private Solution solution;
        private List<TypeScriptClass> _classes;
        public IEnumerable<TypeScriptClass> Classes { get { return _classes; } }

        public TypeScriptRoslynSolutionParser(string solutionPath)
        {
            var workspace = MSBuildWorkspace.Create();

            //Get the Visual Studio Solution
            solution = workspace.OpenSolutionAsync(solutionPath).Result;
        }


        public void ParseSolution()
        {
            if (_classes != null) return;

            _classes = new List<TypeScriptClass>();

            //Get the Projects in the Solution
            foreach (var project in solution.Projects)
            {
                //Get each class in the project
                foreach (var CSharpClass in project.Documents)
                {
                    _classes.Add(ParseDocument(CSharpClass));
                }
            }
        }

        private TypeScriptClass ParseDocument(Document doc)
        {
            //If Roslyn can parse the class, then parse it.
            if (doc.SupportsSyntaxTree)
            {
                //Begin the Typescript class definition
                var tClass = new TypeScriptClass(doc.Name.Remove(doc.Name.Length - 3));

                var semanticModel = doc.GetSemanticModelAsync().Result;
                //Get the parse tree
                var t = doc.GetSyntaxTreeAsync().Result;
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

                            var prop = new TypeScriptProperty();
                            prop.Name = propMember.Identifier.Text;
                            prop.Type = GetTypescriptType(semanticModel, propMember.Type);

                            tClass.Properties.Add(prop);
                        }
                        else if (member is FieldDeclarationSyntax)
                        {
                            //We have to cast in order to get the properties that we need to make the typescript equivalent
                            var fieldMember = (FieldDeclarationSyntax)member;

                            //In case multiple variables are declared on one line ex:(int a, b, c;) we want to make multiple typescript variables
                            foreach (var variable in fieldMember.Declaration.Variables)
                            {
                                var prop = new TypeScriptProperty();
                                prop.Name = variable.Identifier.Text;
                                prop.Type = GetTypescriptType(semanticModel, fieldMember.Declaration.Type);

                                tClass.Properties.Add(prop);
                            }

                        }
                        else if (member is EnumDeclarationSyntax)
                        {
                            //The enum declaration in C# is exactly how it is in typescript, so there is no need of formatting other than tabbing.
                            var enumMember = (EnumDeclarationSyntax)member;
                            var tEnum = new TypeScriptEnum(enumMember.Identifier.Text);

                            foreach (var enumVal in enumMember.Members)
                            {
                                var tEnumElement = new TypeScriptEnumValue(enumVal.Identifier.Text);
                                if (enumVal.EqualsValue != null)
                                {
                                    tEnumElement.Value = Convert.ToInt32(((LiteralExpressionSyntax)enumVal.EqualsValue.Value).Token.Value);
                                }
                                tEnum.Values.Add(tEnumElement);
                            }

                            tClass.Enums.Add(tEnum);
                        }
                    }
                }

                return tClass;
            }

            return null;
        }

        /// <summary>
        /// Gets the typescript identifier in string representation.
        /// </summary>
        /// <param name="typeSymbol">The type symbol in C#.</param>
        ///  This must be a separate function from GetTypeScriptType for recursive purposes.
        ///  (Enumerables, where there are multiple types in one variable declaration, must be recursed);
        private TypeScriptBaseType GetTypescriptIdentifier(ITypeSymbol typeSymbol)
        {
            TypeScriptBaseType type;

            //For enumerables, this will get the IEnumerable, without the <T>
            var typeName = typeSymbol.OriginalDefinition.Name;

            //If the type name is a number in C#, return the Typescript number equivalent
            if (CSharpConstants.NUMBER_IDENTIFIERS.Contains(typeSymbol.Name.TrimEnd('?')))
            {
                type = new TypeScriptBaseType(TypeScriptConstants.NUMBER_IDENTIFIER);
            }
            //If the type name is a string in C#, return the Typescript string equivalent
            else if (CSharpConstants.STRING_IDENTIFIERS.Contains(typeSymbol.Name))
            {
                type = new TypeScriptBaseType(TypeScriptConstants.STRING_IDENTIFIER);
            }
            //If the type name is a boolean in C#, return the Typescript boolean equivalent
            else if (CSharpConstants.BOOLEAN_IDENTIFIERS.Contains(typeSymbol.Name))
            {
                type = new TypeScriptBaseType(TypeScriptConstants.BOOLEAN_IDENTIFIER);
            }
            //If the type is an array in C#, return the Typescript array equivalent
            else if (typeSymbol.TypeKind == TypeKind.Array)
            {
                //We must recurse here, in case the element type is another array (two dimensional or more arrays)
                type = new TypeScriptEnumerableType
                {
                    InnerType = GetTypescriptIdentifier(((IArrayTypeSymbol)typeSymbol).ElementType),
                };
            }
            //If the type is a named symbol (we really only care about enumerables in here), return the Typescript Equivalent
            else if (typeSymbol is INamedTypeSymbol)
            {
                var namedType = (INamedTypeSymbol)typeSymbol;
                //If the type is an Enumerable in C#, return the Typescript array equivalent.
                if (CSharpConstants.ENUMERABLE_IDENTIFIERS.Contains(typeName))
                {
                    //We must recurse here, otherwise we would be duplicating logic on how to get the typescript equivalent of the type argument.
                    //There is only one type argument in an enumerable, which is why First() is used
                    type = new TypeScriptEnumerableType
                    {
                        InnerType = GetTypescriptIdentifier(namedType.TypeArguments.First())
                    };
                }
                else
                {
                    if (CSharpConstants.NULLABLE_IDENTIFIER.Equals(typeName))
                    {
                        type = new TypeScriptBaseType(TypeScriptConstants.ANY_IDENTIFIER);
                    }
                    else
                    {
                        //If it's not an enumerable, just return the default name
                        type = new TypeScriptBaseType(typeSymbol.OriginalDefinition.Name);
                    }
                }
            }
            else
            {
                //If it's not a typescript primitive type, just return the name of the type 
                type = new TypeScriptBaseType(typeSymbol.OriginalDefinition.Name);
            }

            return type;
        }

        /// <summary>
        /// Gets the string typescript equivalent representation of the C# type.
        /// </summary>
        /// <param name="model">The semantic model.</param>
        /// <param name="type">The type in C#.</param>
        public TypeScriptBaseType GetTypescriptType(SemanticModel model, TypeSyntax type)
        {
            var typeInfo = model.GetTypeInfo(type);
            return GetTypescriptIdentifier(typeInfo.Type);
        }
    }
}
