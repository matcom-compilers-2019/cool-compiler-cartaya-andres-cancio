using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Iterpreter
{
    public class Type_STRING : Base_Object_Type
    {
        static Type_STRING singleton;
        public static Type_STRING Singleton()
        {
            if (singleton == null) singleton = new Type_STRING();
            return singleton;
        }

        public Type_STRING() : base("string", Type_OBJECT.Singleton(), new Attribute[] { }, new IMethod[] { })
        {

        }

        public override object Default()
        {
            return "";
        }

        public override bool IsTrue(Base_Object_Value obj)
        {
            return (string)obj.Value != "";
        }
    }
}
