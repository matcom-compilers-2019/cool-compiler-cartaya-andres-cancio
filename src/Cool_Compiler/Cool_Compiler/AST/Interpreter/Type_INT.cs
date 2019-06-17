using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Iterpreter
{

    public class Method_AritmeticOP : IMethod
    {
        public static Dictionary<string, Func<int, int, int>> Operators = new Dictionary<string, Func<int, int, int>>();

        public Method_AritmeticOP(string op, Base_Object_Type type) : 
            base(op, type, new Attribute[] { new Attribute("a", type, 0), new Attribute("b", type, 1) })
        {

        }
        public override Base_Object_Value Call(Base_Object_Value[] ObjectParam)
        {
            int a = (int) ObjectParam[0].Value;
            int b = (int) ObjectParam[1].Value;
            return new Base_Object_Value(Operators[Name](a, b), Type_INT.Singleton());
        }
    }

    public class Type_INT : Base_Object_Type
    {
        static Type_INT singleton;
        public static Type_INT Singleton()
        {
            if (singleton == null)
                singleton = new Type_INT();
            return singleton;
        }
        public Type_INT() :
            base("int", Type_OBJECT.Singleton(), new Attribute[] { }, null  )
        {
            if (!Method_AritmeticOP.Operators.ContainsKey("+"))
            {
                Method_AritmeticOP.Operators.Add("+", (x, y) => x + y);
                Method_AritmeticOP.Operators.Add("-", (x, y) => x - y);
                Method_AritmeticOP.Operators.Add("*", (x, y) => x * y);
                Method_AritmeticOP.Operators.Add("/", (x, y) => x / y);
                Method_AritmeticOP.Operators.Add("<", (x, y) => (x < y) ? 1 : 0);
                Method_AritmeticOP.Operators.Add("<=", (x, y) => (x <= y) ? 1 : 0);
                Method_AritmeticOP.Operators.Add("=", (x, y) => (x == y) ? 1 : 0);
                Method_AritmeticOP.Operators.Add("~", (x, y) => ~y);
                Method_AritmeticOP.Operators.Add("not", (x, y) => (y != 0) ? 0 : 1);
            }
            List<IMethod> m = new List<IMethod>();
            foreach (var item in Method_AritmeticOP.Operators.Keys)
                 m.Add(new Method_AritmeticOP(item, this));       

            Methods = m.ToArray();
        }

        public override bool IsTrue(Base_Object_Value obj)
        {
            return (int)obj.Value != 0;
        }

        public override object Default()
        {
            return 0;
        }
    }
}
