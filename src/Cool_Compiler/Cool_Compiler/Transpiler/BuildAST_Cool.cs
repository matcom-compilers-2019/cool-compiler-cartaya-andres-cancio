using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Tree;
using Cool_Compiler.AST;

namespace Cool_Compiler.Transpiler
{
    public class BuildAST_Cool
    {
        public static AST_Root BUILD(string text)
        {
            AntlrInputStream input = new AntlrInputStream(text);
            CoolLexer lexer = new CoolLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            CoolParser parser = new CoolParser(tokens);

            AntlrInputStream input2 = new AntlrInputStream(text);
            CoolLexer lexer2 = new CoolLexer(input2);
            CommonTokenStream tokens2 = new CommonTokenStream(lexer2);
            CoolParser parser2 = new CoolParser(tokens2);
            BuildAST_Cool_Errors v2 = new BuildAST_Cool_Errors();
            v2.Visit(parser2.program());
            if (!v2.Ok) return null;
            
            BuildAST_Cool_Visitor v = new BuildAST_Cool_Visitor();

            var s = (AST_Root)v.Visit(parser.program());
            return s;
        }
    }

    public class BuildAST_Cool_Errors : CoolBaseVisitor<bool>
    {
        public bool Ok = true;


        public override bool VisitErrorNode(IErrorNode node)
        {
            Ok = false;
            return false;
        }
    }

    public class BuildAST_Cool_Visitor : CoolBaseVisitor<AST_Node>
    {
        public override AST_Node VisitBool([NotNull] CoolParser.BoolContext context)
        {
            return new AST_Cte(context, new AST_Token(context.GetText(), (context.cte.Type == CoolLexer.TRUE)?"true":"false"));
        }
        

        public override AST_Node VisitGetid([NotNull] CoolParser.GetidContext context)
        {
            return new AST_Id(context.ID(), context.ID().GetText());
        }

        public override AST_Node VisitInt([NotNull] CoolParser.IntContext context)
        {
            return new AST_Cte(context.INT(), new AST_Token(context.INT().GetText(), "Int"));
        }

        public override AST_Node VisitBracket([NotNull] CoolParser.BracketContext context)
        {
            return Visit(context.expr());
        }

        public override AST_Node VisitCompar([NotNull] CoolParser.ComparContext context)
        {
            AST_Expresion l = (AST_Expresion)Visit(context.expr(0));
            AST_Expresion r = (AST_Expresion)Visit(context.expr(1));
            return new AST_BinaryOp(context, l, r, context.op);
        }

        public override AST_Node VisitMultdiv([NotNull] CoolParser.MultdivContext context)
        {
            AST_Expresion l = (AST_Expresion)Visit(context.expr(0));
            AST_Expresion r = (AST_Expresion)Visit(context.expr(1));
            return new AST_BinaryOp(context, l, r, context.op);
        }
        public override AST_Node VisitLog_arit([NotNull] CoolParser.Log_aritContext context)
        {
            AST_Expresion l = (AST_Expresion)Visit(context.expr());
            return new AST_UnaryOp(context, l, context.op);
        }
        public override AST_Node VisitLog_neg([NotNull] CoolParser.Log_negContext context)
        {
            AST_Expresion l = (AST_Expresion)Visit(context.expr());
            return new AST_UnaryOp(context, l, context.NOT().Symbol);
        }

        public override AST_Node VisitSumsub([NotNull] CoolParser.SumsubContext context)
        {
            AST_Expresion l = (AST_Expresion)Visit(context.expr(0));
            AST_Expresion r = (AST_Expresion)Visit(context.expr(1));
            return new AST_BinaryOp(context, l, r, context.op);
        }

        public override AST_Node VisitStamentlist([NotNull] CoolParser.StamentlistContext context)
        {
            List<AST_Expresion> l = new List<AST_Expresion>();
            foreach (var item in context.stament())
            {
                l.Add((AST_Expresion)Visit(item));
            }
            return new AST_StamentList(context, l);
        }

        public override AST_Node VisitProg([NotNull] CoolParser.ProgContext context)
        {
            List<AST_Node> l = new List<AST_Node>();
            foreach (var item in context.children)
            {
                l.Add(Visit(item));
            }
            return new AST_Root(context, l);
        }

        public override AST_Node VisitStament([NotNull] CoolParser.StamentContext context)
        {
            return Visit(context.expr());
        }

        public override AST_Node VisitAsignacion([NotNull] CoolParser.AsignacionContext context)
        {
            AST_Expresion exp = (AST_Expresion)Visit(context.expr());
            string id = context.ID().GetText();
            return new AST_Asignacion(context, id, exp);
        }

        public override AST_Node VisitCorpus([NotNull] CoolParser.CorpusContext context)
        {
            return Visit(context.stamentlist());
        }

        public override AST_Node VisitIf([NotNull] CoolParser.IfContext context)
        {
            AST_Expresion cond = (AST_Expresion)Visit(context.expr(0));
            AST_Expresion corpus = (AST_Expresion)Visit(context.expr(1));
            return new AST_If(context, cond, corpus, null);
        }

        public override AST_Node VisitIfelse([NotNull] CoolParser.IfelseContext context)
        {
            AST_Expresion cond = (AST_Expresion)Visit(context.expr(0));
            AST_Expresion corpus1 = (AST_Expresion)Visit(context.expr(1));
            AST_Expresion corpus2 = (AST_Expresion)Visit(context.expr(2));
            return new AST_If(context, cond, corpus1, corpus2);
        }

        public override AST_Node VisitWhile([NotNull] CoolParser.WhileContext context)
        {
            AST_Expresion cond = (AST_Expresion)Visit(context.expr(0));
            AST_Expresion corpus1 = (AST_Expresion)Visit(context.expr(1));
            return new AST_While(context, cond, corpus1);
        }

        public override AST_Node VisitFeatureprop([NotNull] CoolParser.FeaturepropContext context)
        {
            var v = context.property();
            var formal = v.formal();
            AST_Id id = new AST_Id(formal.ID(), formal.ID().GetText());
            AST_Type_Node type = new AST_Type_Node(formal.TYPE(), formal.TYPE().GetText());
            AST_Expresion exp = null;
            if (v.expr() != null) exp = (AST_Expresion)Visit(v.expr());
            var formald = new AST_FormalDec(v.formal(), id, type);
            return new AST_ClassProperty(context, formald, exp);
        }

        public override AST_Node VisitClassdefine([NotNull] CoolParser.ClassdefineContext context)
        {
            AST_Id id = new AST_Id(context.TYPE(0), context.TYPE(0).GetText());
            AST_Type_Node inh = new AST_Type_Node(context.TYPE(0), "Object");
            if(context.TYPE().Length > 1) inh = new AST_Type_Node(context.TYPE(1), context.TYPE(1).GetText());
            List<AST_ClassProperty> prop = new List<AST_ClassProperty>();
            List<AST_MethodDef> meth = new List<AST_MethodDef>();
            foreach (var item in context.feature())
            {
                var t = Visit(item);
                if (t is AST_ClassProperty) prop.Add((AST_ClassProperty)t);
                else meth.Add((AST_MethodDef)t);
            }
            foreach (var item in prop)
            {
                if (item.exp == null)
                {
                    if (item.decl.type.Type == "Int")
                    {
                        item.exp = new AST_Cte(context, new AST_Token("0", "Int"));
                    }
                    else if (item.decl.type.Type == "Bool")
                    {
                        item.exp = new AST_Cte(context, new AST_Token("false", "false"));
                    }
                    else if (item.decl.type.Type == "String")
                    {
                        item.exp = new AST_StringCte(item.row, item.col, "\"\"");
                    }
                }
            }
            return new AST_ClassDef(context, id, new AST_ListProp(context, prop), new AST_ListMethod(context, meth), inh);
        }

        public override AST_Node VisitClasslist([NotNull] CoolParser.ClasslistContext context)
        {
            List<AST_ClassDef> li = new List<AST_ClassDef>();
            foreach (var item in context.classdefine())
            {
                AST_ClassDef i = (AST_ClassDef)Visit(item);
                li.Add(i);
            }
            return new AST_Root( context, new AST_Node[] { new AST_ListClass(context, li) } );
        }

        public override AST_Node VisitFeaturemet([NotNull] CoolParser.FeaturemetContext context)
        {
            return Visit(context.method());
        }

        public override AST_Node VisitMethod([NotNull] CoolParser.MethodContext context)
        {
            AST_Id id = new AST_Id(context.ID(), context.ID().GetText());
            AST_Type_Node type = new AST_Type_Node(context.TYPE(), context.TYPE().GetText());
            List<AST_ClassProperty> param = new List<AST_ClassProperty>();
            AST_Expresion corpus = (AST_Expresion)Visit(context.expr());
            foreach (var item in context.formal())
            {
                var iid = new AST_Id(item.ID(), item.ID().GetText());
                var ity = new AST_Type_Node(item.TYPE(), item.TYPE().GetText());
                var formald = new AST_FormalDec(item, iid, ity);
                AST_ClassProperty i = new AST_ClassProperty(item, formald, null);
                param.Add(i);
            }
            return new AST_MethodDef(context, id, type, new AST_ListProp(context, param), corpus);
        }

        public override AST_Node VisitNewtype([NotNull] CoolParser.NewtypeContext context)
        {
            var id = new AST_Type_Node(context.TYPE(), context.TYPE().GetText());
            return new AST_New_Type(context, id);
        }

        public override AST_Node VisitIsvoid([NotNull] CoolParser.IsvoidContext context)
        {
            return new AST_Isvoid(context, (AST_Expresion)Visit(context.expr()));
        }

        public override AST_Node VisitArgexpr([NotNull] CoolParser.ArgexprContext context)
        {
            var l = new List<AST_Expresion>();
            foreach (var item in context.expr())
            {
                l.Add((AST_Expresion)Visit(item));
            }
            return new AST_StamentList(context, l);
        }
        

        public override AST_Node VisitExplicitdispatch([NotNull] CoolParser.ExplicitdispatchContext context)
        {
            AST_Expresion expr = (AST_Expresion)Visit(context.expr());
            var typ = (context.TYPE() == null) ? null : new AST_Type_Node(context.TYPE(), context.TYPE().GetText()); ;
            var arg = (AST_StamentList)Visit(context.argexpr());
            var id = new AST_Id(context.ID(), context.ID().GetText());
            return new AST_ExpCall(context, expr, id, typ, arg);
        }

        public override AST_Node VisitDispatch([NotNull] CoolParser.DispatchContext context)
        {
            var arg = (AST_StamentList)Visit(context.argexpr());
            var id = new AST_Id(context.ID(), context.ID().GetText());
            return new AST_Call(context, id, arg);
        }

        public override AST_Node VisitString([NotNull] CoolParser.StringContext context)
        {
            return new AST_StringCte(context, context.STRCTE().GetText());
        }

        public override AST_Node VisitLet([NotNull] CoolParser.LetContext context)
        {
            List<AST_ClassProperty> param = new List<AST_ClassProperty>();
            AST_Expresion corpus = (AST_Expresion)Visit(context.expr());
            foreach (var item in context.property())
            {
                var iid = new AST_Id(item.formal().ID(), item.formal().ID().GetText());
                var ity = new AST_Type_Node(item.formal().TYPE(), item.formal().TYPE().GetText());
                var formald = new AST_FormalDec(item, iid, ity);
                AST_Expresion subexp = null;
                if (item.expr() != null) subexp = (AST_Expresion)Visit(item.expr());
                AST_ClassProperty i = new AST_ClassProperty(item, formald, subexp);
                param.Add(i);
            }
            return new AST_Let(context, new AST_ListProp(context, param), corpus);
        }

        public override AST_Node VisitCaseof([NotNull] CoolParser.CaseofContext context)
        {
            var exp = (AST_Expresion)Visit(context.expr());
            List<AST_ClassProperty> props = new List<AST_ClassProperty>();

            foreach (var item in context.branch())
            {
                var s = new AST_FormalDec(item.formal(), new AST_Id(item.formal().ID(), item.formal().ID().GetText()),
                                           new AST_Type_Node(item.formal().TYPE(), item.formal().TYPE().GetText()));
                AST_ClassProperty x = new AST_ClassProperty(item, s, (AST_Expresion)Visit(item.expr()));
                props.Add(x);
            }

            return new AST_CaseOf(context, new AST_ListProp(context, props), exp);

        }
    }
}
