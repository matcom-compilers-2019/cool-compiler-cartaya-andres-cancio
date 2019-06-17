using Cool_Compiler.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.Visitor.Semantic;

namespace Runtime
{
    public static class Travels
    {
        public static void DFS(AST_Node a, int level = 0)
        {
            if (a == null) return;
            for (int i = 0; i < level; i++) Console.Write("-");
            Console.Write(":");

            Console.WriteLine("< {0} {1}", a.GetType().ToString(), a.ToString());

            foreach (var item in a.Children)
            {
                DFS(item, level + 1);
            }

            for (int i = 0; i < level; i++) Console.Write("-");
            Console.Write(":");

            Console.WriteLine("> {0} {1}", a.GetType().ToString(), a.ToString());

        }

    }
}
