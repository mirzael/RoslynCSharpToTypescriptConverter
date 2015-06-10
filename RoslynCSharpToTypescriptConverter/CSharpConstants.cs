using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCSharpToTypeScriptConverter
{
    public class CSharpConstants
    {
        public static string[] NUMBER_IDENTIFIERS = { "int", "Int16", "Int32", "Int64", "uint",
                                                              "long", "ulong", "short", "double", "ushort",
                                                              "decimal", "float", "byte", "sbyte", "Double",
                                                              "Decimal"};
        public static string[] STRING_IDENTIFIERS = { "string", "String" };
        public static string[] BOOLEAN_IDENTIFIERS = { "bool", "Boolean" };
        public static string[] ENUMERABLE_IDENTIFIERS = { "IEnumerable", "List", "ICollection", "IList",
                                                            "Queue", "Stack", "SortedList", "ReadOnlyCollection",
                                                            "IReadOnlyCollection","ObservableCollection" };
        public static string NULLABLE_IDENTIFIER = "Nullable";
    }
}
