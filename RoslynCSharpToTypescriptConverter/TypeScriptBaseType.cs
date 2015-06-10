using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptBaseType
    {
        public string Name;

        public TypeScriptBaseType() { }

        public TypeScriptBaseType(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
