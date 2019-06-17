using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST;
using Cool_Compiler.Iterpreter;

namespace Cool_Compiler.Visitor.Runtime
{
    public class SMemory
    {
        Dictionary<string, Base_Object_Value> memo;

        public Base_Object_Value GetValue(string id)
        {
            if (memo.ContainsKey(id)) return memo[id];
            return Type_OBJECT.Singleton().Instanciate();
        }

        public Base_Object_Value SetValue(string id, Base_Object_Value b)
        {
            memo[id] = b;
            return b;
        }
        public SMemory()
        {
            memo = new Dictionary<string, Base_Object_Value>();
        }
    }
}
