using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCSharpToTypeScriptConverter
{
    public class TypeScriptClass
    {
        public string Name;
        public ICollection<TypeScriptProperty> Properties;
        public ICollection<TypeScriptEnum> Enums;

        public TypeScriptClass(string name)
        {
            this.Name = name;
            this.Properties = new List<TypeScriptProperty>();
            this.Enums = new List<TypeScriptEnum>();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine(string.Format("class {0} {{", Name));
            foreach (var prop in Properties)
            {
                builder.AppendLine(string.Format("\t{0}", prop.ToString()));
            }
            foreach (var tEnum in Enums)
            {
                builder.AppendLine(string.Format("\t{0}", tEnum.ToString()));
            }

            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
