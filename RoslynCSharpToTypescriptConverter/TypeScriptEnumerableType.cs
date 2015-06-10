using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptEnumerableType : TypeScriptBaseType
    {
        public TypeScriptBaseType InnerType;
        
        public override string ToString()
        {
            return string.Format("Array<{0}>", InnerType.ToString());
        }
    }
}
