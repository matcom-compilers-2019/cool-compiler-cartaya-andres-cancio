using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Tree;
using System.IO;
using Cool_Compiler.Transpiler;
using Cool_Compiler.AST;
using Cool_Compiler.Visitor.Runtime;

namespace UTP
{
    [TestClass]
    public class AST_Test
    {
        [TestMethod]
        public void AST_Build_test()
        {
            string text = @"1+1; (2+4)*3 + 1; true + false -1;";
            AST_Root r = BuildAST_Cool.BUILD(text);
            Assert.IsNotNull(r);
            Assert.AreEqual(r.Children[0].Children.Count, 3);
        }


        [TestMethod]
        public void AST_Asignation_test()
        {
            string text = @"b <- 11; a <- b + 2; b <- 1; a + b + 1;";
            AST_Root r = BuildAST_Cool.BUILD(text);
            AST_Execute_Interpreter v = new AST_Execute_Interpreter();
            Assert.IsNotNull(r);
            Assert.AreEqual(v.Visit(r), 15);
        }

        [TestMethod]
        public void AST_If_Z_test()
        {
            string text = @"a <- 1; if a = 1 then { a <- 2; } fi; a * 2;";
            AST_Root r = BuildAST_Cool.BUILD(text);
            AST_Execute_Interpreter v = new AST_Execute_Interpreter();
            Assert.IsNotNull(r);
            Assert.AreEqual(v.Visit(r), 4);
        }

        [TestMethod]
        public void AST_If_Else_test()
        {
            string text = @"a <- 0; if a = 1 then { a <- 2; } else {a <- 3} fi; a * 2;";
            AST_Root r = BuildAST_Cool.BUILD(text);
            AST_Execute_Interpreter v = new AST_Execute_Interpreter();
            Assert.IsNotNull(r);
            Assert.AreEqual(v.Visit(r), 6);
        }

        

        [TestMethod]
        public void AST_STRCte()
        {
            string text = "id <- \"string separate space\"; a <- \"other\nstr\n\";";
            AST_Root r = BuildAST_Cool.BUILD(text);
            var v = (AST_StringCte)r.Children[0].Children[0].Children[0];
            var v2 = (AST_StringCte)r.Children[0].Children[1].Children[0];

            Assert.AreEqual("string separate space", v.ToString());
            Assert.AreEqual("other\nstr\n", v2.ToString());

        }

        [TestMethod]
        public void AST_While_Fib_test()
        {
            string text = @"
                                a <- 0; 
                                b <- 1;
                                n <- 2; 
                                while n < 9 loop { 
                                    c <- a + b;
                                    a <- b;
                                    b <- c;
                                    n <- n + 1;
                                    } pool;
                                b;
                                ";
            AST_Root r = BuildAST_Cool.BUILD(text);
            AST_Execute_Interpreter v = new AST_Execute_Interpreter();
            Assert.IsNotNull(r);
            Assert.AreEqual(v.Visit(r), 21);
        }


    }
}
