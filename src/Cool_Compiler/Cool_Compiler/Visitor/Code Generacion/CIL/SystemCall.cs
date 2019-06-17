using Cool_Compiler.AST.CIL;
using Cool_Compiler.Visitor.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Visitor.Code_Generacion.CIL
{
    public static class SystemCallGenerator
    {
        public static CIL_FunctionDef GeneratePrint()
        {
            List<string> par = new List<string> { "self", "x" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_IOPrint("x"),
                new CIL_Return("self", "IO")

            });
            return new CIL_FunctionDef("IO.out_string", new CIL_Params(par), new CIL_Params(new List<string>()), lis);
        }

        public static CIL_FunctionDef GeneratePrintInt()
        {

            List<string> par = new List<string> { "self", "x" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_IOPrintInt("x"),
                new CIL_Return("self", "IO")

            });
            return new CIL_FunctionDef("IO.out_int", new CIL_Params(par), new CIL_Params(new List<string>()), lis);
        }

        public static CIL_FunctionDef GenerateReadInt()
        {

            List<string> par = new List<string> { "self" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_IOReadInt("x"),
                new CIL_Return("x", "var")

            });
            return new CIL_FunctionDef("IO.read_int", new CIL_Params(par), new CIL_Params(new List<string>() { "x" }), lis);
        }

        internal static CIL_FunctionDef Entry()
        {
            List<string> par = new List<string> { };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {

                new CIL_Allocate("Main","self", false),
                new CIL_FunctionCall("main", new CIL_Params(new List<string>{ "self" }),
                                    "Main", "self", false, "Main")
                    

            });
            return new CIL_FunctionDef("main", new CIL_Params(par), new CIL_Params(new List<string> { "self" }), lis);

        }

        public static CIL_FunctionDef Abort()
        {

            List<string> par = new List<string> { "self"};
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_Abort(),    
                new CIL_Return("0", "Int")
            });
            return new CIL_FunctionDef("Object.abort", new CIL_Params(par), new CIL_Params(new List<string>()), lis);
        }

        public static CIL_FunctionDef TypeName()
        {

            List<string> par = new List<string> { "self" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_TypeOf("self", "t"),
                new CIL_TypeName("t", "res", true),
                new CIL_Return("res", "var")

            });
            return new CIL_FunctionDef("Object.type_name", new CIL_Params(par), new CIL_Params(new List<string>() { "res" , "t" }), lis);
        }

        public static CIL_FunctionDef Copy()
        {
            List<string> par = new List<string> { "self" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_Copy("self", "res"),
                new CIL_Return("res", "var")

            });
            return new CIL_FunctionDef("Object.copy", new CIL_Params(par), new CIL_Params(new List<string>() { "res" }), lis);
        }

        public static CIL_FunctionDef StrConcat()
        {
            List<string> par = new List<string> { "self", "other" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_StrConcat("self", "other", "self"),
                new CIL_Return("self", "var")
            });
            return new CIL_FunctionDef("String.concat", new CIL_Params(par), new CIL_Params(new List<string>() { "res" }), lis);
        }
        public static CIL_FunctionDef StrLenght()
        {
            List<string> par = new List<string> { "self" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_LengthStr("self", "x")
            });
            return new CIL_FunctionDef("String.lenght", new CIL_Params(par), new CIL_Params(new List<string>() { "x" }), lis);
        }

        public static CIL_FunctionDef StrSubstr()
        {
            List<string> par = new List<string> { "self", "i", "j" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_StrStr("self", "i", "j", "x")
            });
            return new CIL_FunctionDef("String.lenght", new CIL_Params(par), new CIL_Params(new List<string>() { "x" }), lis);
        }

        public static CIL_FunctionDef Descend()
        {
            List<string> par = new List<string> { "s", "p" };
            var lis = new CIL_StamentList(new List<CIL_AST_Node>()
            {
                new CIL_TypeOf("Object", "obj"),
                new CIL_Asig("result", "0", "Int"),
                new CIL_Label("descend.iterate"),
                new CIL_ExpBin("s", "p", "=", "exp0"),
                new CIL_If_Goto("exp0", "descend.end"),
                new CIL_ExpBin("p", "obj", "=", "exp0"),
                new CIL_If_Goto("exp0", "descend.wrong"),
                new CIL_FatherType("p", "p"),
                new CIL_ExpBin("result", "1", "+", "result"),
                new CIL_Goto("descend.iterate"),
                new CIL_Label("descend.wrong"),
                new CIL_Asig("result", "-1", "Int"),
                new CIL_Label("descend.end"),
                new CIL_Return("result", "var")
            });
            return new CIL_FunctionDef("descend", new CIL_Params(par), new CIL_Params(new List<string>() { "result", "exp0", "obj" }), lis);
        }
    }
}
