using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptProperty
    {
        public string Name;
        public TypeScriptBaseType Type;

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Type.ToString());
        }
    }
}
