using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptEnum
    {
        public string Name;
        public ICollection<TypeScriptEnumValue> Values;

        public TypeScriptEnum(string Name)
        {
            this.Name = Name;
            Values = new List<TypeScriptEnumValue>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("enum {0}{{", Name);
            builder.Append(string.Join(", ", Values));
            builder.Append("}};");

            return builder.ToString();
        }
    }
}
