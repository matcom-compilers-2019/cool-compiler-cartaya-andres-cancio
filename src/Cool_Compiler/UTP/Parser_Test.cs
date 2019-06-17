using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Tree;
using System.IO;
using Cool_Compiler;
using Cool_Compiler.Transpiler;


namespace UTP
{

    class CoolVisitor : CoolBaseVisitor<int>
    {
        public override int VisitInt([NotNull] CoolParser.IntContext context)
        {
            return int.Parse(context.INT().GetText());
        }

        public override int VisitBool([NotNull] CoolParser.BoolContext context)
        {
            if (context.GetText() == "true") return 1;
            return 0;
        }

        public override int VisitSumsub([NotNull] CoolParser.SumsubContext context)
        {
            int s = Visit(context.expr(0));
            int t = Visit(context.expr(1));
            if (context.op.Type == CoolLexer.ADD)
                return s + t;
            return s - t;
        }

        public override int VisitCompar([NotNull] CoolParser.ComparContext context)
        {
            int s = Visit(context.expr(0));
            int t = Visit(context.expr(1));
            if (context.op.Type == CoolLexer.EQUAL_CM) return (s == t)?1:0;
            if (context.op.Type == CoolLexer.LESS) return (s < t)?1:0;
            return (s <= t) ? 1 : 0;

        }

        public override int VisitLog_neg([NotNull] CoolParser.Log_negContext context)
        {
            int s = Visit(context.expr());
            return (s == 0) ? 1 : 0;
        }

        public override int VisitLog_arit([NotNull] CoolParser.Log_aritContext context)
        {
            int s = Visit(context.expr());
            if (context.op.Type == CoolLexer.SUB) return -s;
            return ~s;
        }

        public override int VisitMultdiv([NotNull] CoolParser.MultdivContext context)
        {
            int s = Visit(context.expr(0));
            int t = Visit(context.expr(1));
            if (context.op.Type == CoolLexer.MUL)
                return s * t;
            if (context.op.Type == CoolLexer.POW)
                return (int)Math.Pow(s, t);
            return s / t;
        }

        public override int VisitBracket([NotNull] CoolParser.BracketContext context)
        {
            return Visit(context.expr());
        }

        public override int VisitStament([NotNull] CoolParser.StamentContext context)
        {
            return Visit(context.expr());
        }

        public override int VisitProg([NotNull] CoolParser.ProgContext context)
        {
            Visit(context.stamentlist());            
            return 0;
        }
    }

    [TestClass]
    public class Parser_Test
    {
        public static CoolParser CalcParser(string text)
        {
            AntlrInputStream input = new AntlrInputStream(text);
            CoolLexer lexer = new CoolLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            CoolParser parser = new CoolParser(tokens);
            return parser;
        }

        [TestMethod]
        public void Parser_BuildCalc()
        {
            string text = "4 + 5; 6 + 7; 8 + 9;";
            var parser = CalcParser(text);
            Assert.IsNotNull(parser);
        }


        [TestMethod]
        public void Parser_Calc()
        {
            string text = @"
                            4+5*2;
                            (6*3)/7;
                            8+9;
                            9/3;
                            2*2*2*2 + 5 + 2;
                            true + false;
                            true < 2;
                            2*2*2*2 = (2+2)*(2+2);
                            true <= false;
                            not (not true + false*true + 0*(1+5+8)/9 + 1/9);
                            (0-1);
                            (0-1)*(0-1);
                            ~(0-1) ;          
                            ";
            var parser = CalcParser(text);
            CoolParser.ProgramContext tree = parser.program();
            CoolVisitor visitor = new CoolVisitor();
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(0)), 14);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(1)), 2);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(2)), 17);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(3)), 3);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(4)), 2 * 2 * 2 * 2 + 5 + 2);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(5)), 1);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(6)), 1);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(7)), 1);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(8)), 0);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(9)), 1);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(10)), -1);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(11)), 1);
            Assert.AreEqual(visitor.Visit((tree.children[0]).GetChild(12)), 0);
        }

        [TestMethod]
        public void Parser_Class_Attr()
        {
            string text = @"
                            class A 
                            {
                                x : Int;
                                y : Bool;
                                z : Int <- 1 << 20;                               
                            };
                            ";
            var parser = CalcParser(text);
            CoolParser.ProgramContext tree = parser.program();
            var v = tree.children[0];
            Assert.IsNotNull(tree);
        }

        


    }
}
