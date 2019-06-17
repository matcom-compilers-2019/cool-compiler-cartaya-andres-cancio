using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Iterpreter
{

    public class Method_TypeName : IMethod
    {
        public override Base_Object_Value Call(Base_Object_Value[] ObjectParam)
        {
            return new Base_Object_Value(ObjectParam[0].Type.Name, Type_STRING.Singleton());
        }

        public Method_TypeName() : base("type_name", Type_STRING.Singleton(), new Attribute[] { new Attribute("self", Type_OBJECT.Singleton(), 0) })
        { }

    }

    public class Type_OBJECT : Base_Object_Type
    {

        static Type_OBJECT singleton;
        public static Type_OBJECT Singleton()
        {
            if (singleton == null) singleton = new Type_OBJECT();
            return singleton;
        }

        public override object Default()
        {
            return null;
        }

        public override bool IsTrue(Base_Object_Value obj)
        {
            return obj.Value != null;
        }

        public Type_OBJECT() : base("object", null, new Attribute[] { }, new IMethod[] { })
        {

        }
    }
}
