using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptEnumValue
    {
        public string Name;
        public int? Value;

        public TypeScriptEnumValue(string Name)
        {
            this.Name = Name;
        }

        public override string ToString()
        {
            return Value.HasValue ? string.Format("{0} = {1}", Name, Value) : Name;
        }
    }
}
