using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Cool_Compiler.Visitor;
using Cool_Compiler.Visitor.Semantic;

namespace Cool_Compiler.AST
{
    public abstract class AST_Node
    {
        public List<AST_Node> Children;
        public int row, col;
        public AST_Node(ParserRuleContext parser_node, IEnumerable<AST_Node> children)
        {
            if (children != null) Children = new List<AST_Node>(children);
            else Children = new List<AST_Node>();
            row = parser_node.Start.Line;
            col = parser_node.Start.Column;
        }
        public AST_Node(ITerminalNode parser_node, IEnumerable<AST_Node> children)
        {
            if (children != null) Children = new List<AST_Node>(children);
            else Children = new List<AST_Node>();
            row = parser_node.Symbol.Line;
            col = parser_node.Symbol.Column;
        }

        public AST_Node(int r, int c, IEnumerable<AST_Node> children)
        {
            if (children != null) Children = new List<AST_Node>(children);
            else Children = new List<AST_Node>();
            row = r;
            col = c;
        }

        public virtual T Visit<T>(IASTVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class AST_Expresion : AST_Node
    {
        public SemanticType MyType { get; set; }
        public AST_Expresion(ParserRuleContext parser_node, IEnumerable<AST_Node> children) : base(parser_node, children) { }
        public AST_Expresion(int row, int col, IEnumerable<AST_Node> children) : base(row, col, children) { }
        public AST_Expresion(ITerminalNode parser_node, IEnumerable<AST_Node> children) : base(parser_node, children) { }
    }

    public class AST_Root : AST_Node
    {
        public AST_ListClass class_list;
        public AST_Root(ParserRuleContext parser_node, IEnumerable<AST_Node> children) : base(parser_node, children)
        { 
            class_list = new AST_ListClass(parser_node, new List<AST_ClassDef> { });
            if (Children.Count > 0 && Children[0] is AST_ListClass)
                class_list = (AST_ListClass)Children[0];
            

        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return "Root";
        }
    }

    public class AST_ListClass : AST_Node
    {
        public List<AST_ClassDef> Class_list;
        public AST_ListClass(ParserRuleContext context, IEnumerable<AST_ClassDef> children) : base(context, children)
        {
            Class_list = new List<AST_ClassDef>(children);
        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

    }

    public class AST_ClassDef : AST_Node
    {
        public AST_Id Id;
        public AST_Type_Node Inherits;
        public AST_ListProp Property_list;
        public AST_ListMethod Method_list;
        public AST_ClassDef(ParserRuleContext parser_node, AST_Id id, AST_ListProp property_list, AST_ListMethod method_list, AST_Type_Node inherits) : base(parser_node, new AST_Node[] { id, inherits, property_list, method_list })
        {
            this.Id = id;
            this.Property_list = property_list;
            this.Method_list = method_list;
            this.Inherits = inherits;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }
    public class AST_ClassProperty : AST_Node
    {
        public AST_FormalDec decl;
        public AST_Expresion exp;
        public AST_ClassProperty(ParserRuleContext parser_node, AST_FormalDec decl, AST_Expresion exp) : base(parser_node, new AST_Node[2] { decl, exp })
        {
            this.decl = decl;
            this.exp = exp;
        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_ListProp : AST_Node
    {
        public List<AST_ClassProperty> Propertys;
        public AST_ListProp(ParserRuleContext parser_node, IEnumerable<AST_ClassProperty> children) : base(parser_node, children)
        {
            this.Propertys = new List<AST_ClassProperty>(children);
        }

        public AST_ListProp(int r, int c, IEnumerable<AST_ClassProperty> children) : base(r, c, children)
        {
            this.Propertys = new List<AST_ClassProperty>(children);
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_ListMethod : AST_Node
    {
        public List<AST_MethodDef> Methods;
        public AST_ListMethod(ParserRuleContext parser_node, IEnumerable<AST_MethodDef> children) : base(parser_node, children)
        {
            this.Methods = new List<AST_MethodDef>(children);
        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }
    public class AST_MethodDef : AST_Node
    {
        public AST_Id Id;
        public AST_Type_Node type;
        public AST_ListProp Propertys;
        public AST_Expresion Statament;
        public AST_MethodDef(ParserRuleContext parser_node, AST_Id id, AST_Type_Node type, AST_ListProp propertys, AST_Expresion corpus) : base(parser_node, new AST_Node[] { id, type, propertys, corpus })
        {
            this.Id = id;
            this.type = type;
            this.Propertys = propertys;
            this.Statament = corpus;
        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_BinaryOp : AST_Expresion
    {
        public AST_Expresion Left, Right;
        public IToken Op;
        public AST_BinaryOp(ParserRuleContext parser_node, AST_Expresion left, AST_Expresion right, IToken op) : base(parser_node, new AST_Node[] { left, right })
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return "Pending";
        }
    }

    public class AST_UnaryOp : AST_Expresion
    {
        public AST_Expresion Exp;
        public IToken Op;
        public AST_UnaryOp(ParserRuleContext parser_node, AST_Expresion exp, IToken op) : base(parser_node, new AST_Node[] { exp })
        {
            Exp = exp;
            Op = op;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return Op.Text + " " + Exp.ToString();
        }
    }

    public class AST_StamentList : AST_Expresion
    {
        public List<AST_Expresion> stament_list;
        public AST_StamentList(ParserRuleContext parser_node, IEnumerable<AST_Expresion> children) : base(parser_node, children)
        {
            this.stament_list = new List<AST_Expresion>(children);
        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return "Stament_List";
        }
    }

    public class AST_Cte : AST_Expresion
    {
        public AST_Token Token;
        public AST_Cte(ITerminalNode parse_node, AST_Token token) : base(parse_node, null)
        { this.Token = token; }
        public AST_Cte(ParserRuleContext parse_node, AST_Token token) : base(parse_node, null)
        { this.Token = token; }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return Token.Text;
        }
    }
    public class AST_StringCte : AST_Expresion
    {
        public string Token;
        public AST_StringCte(ParserRuleContext parse_node, string token) : base(parse_node, null)
        { this.Token = token.Substring(1, token.Length - 2); }

        public AST_StringCte(int row, int col, string token) : base(row, col, null)
        { this.Token = token.Substring(1, token.Length - 2); }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return Token;
        }
    }
    public class AST_Asignacion : AST_Expresion
    {
        public string Id;

        public AST_Expresion Exp;

        public AST_Asignacion(ParserRuleContext parse_node, string id, AST_Expresion exp) : base(parse_node, new AST_Node[] { exp })
        {
            Id = id;
            Exp = exp;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return Id + " <- " + Exp.ToString();
        }
    }

    public class AST_Token
    {
        public string Text;
        public string Type;
        public AST_Token(string text, string type_token)
        {
            Text = text; Type = type_token;
        }

    }

    public class AST_Id : AST_Expresion
    {
        public string Id;

        public AST_Id(ITerminalNode parse_node, string id) : base(parse_node, null)
        {
            Id = id;
        }
        public AST_Id(ParserRuleContext parse_node, string id) : base(parse_node, null)
        {
            Id = id;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return Id;
        }
    }

    public class AST_Type_Node : AST_Node
    {
        public string Type;

        public AST_Type_Node(ITerminalNode parse_node, string type) : base(parse_node, null)
        {
            Type = type;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return string.Format("@({0})", Type);
        }
    }

    public class AST_FormalDec : AST_Node
    {
        public AST_Id id;
        public AST_Type_Node type;
        public AST_FormalDec(ParserRuleContext parser_node, AST_Id id, AST_Type_Node type) : base(parser_node, new AST_Node[] { id, type })
        {
            this.id = id; this.type = type;
        }
        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_If : AST_Expresion
    {
        public AST_Expresion Cond;
        public AST_Expresion IfCorpus;
        public AST_Expresion ElseCorpus;

        public AST_If(ParserRuleContext parse_node, AST_Expresion cond, AST_Expresion ifcorpus, AST_Expresion elsecorpus) : base(parse_node, new AST_Node[] { cond, ifcorpus, elsecorpus })
        {
            Cond = cond;
            IfCorpus = ifcorpus;
            ElseCorpus = elsecorpus;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

        public override string ToString()
        {
            return "Pending";
        }
    }

    public class AST_While : AST_Expresion
    {
        public AST_Expresion Cond;
        public AST_Expresion WhileCorpus;


        public AST_While(ParserRuleContext parse_node, AST_Expresion cond, AST_Expresion whilecorpus) : base(parse_node, new AST_Node[] { cond, whilecorpus })
        {
            Cond = cond;
            WhileCorpus = whilecorpus;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

    }

    public class AST_New_Type : AST_Expresion
    {
        public AST_Type_Node type;
        public AST_New_Type(ParserRuleContext parser_node, AST_Type_Node type) : base(parser_node, new AST_Node[] { type })
        {
            this.type = type;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_Isvoid : AST_Expresion
    {
        public AST_Expresion expr;
        public AST_Isvoid(ParserRuleContext parser_node, AST_Expresion expr) : base(parser_node, new AST_Node[] { expr })
        {
            this.expr = expr;
        }


        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_ExpCall : AST_Expresion
    {
        public AST_Expresion expr;
        public AST_Type_Node explicittype;
        public AST_Id id;
        public AST_StamentList arg;
        public AST_ExpCall(ParserRuleContext parser_node, AST_Expresion expr, AST_Id id, AST_Type_Node expltype, AST_StamentList arg) : base(parser_node, new AST_Node[] { expr, id, expltype, arg })
        {
            this.expr = expr;
            this.explicittype = expltype;
            this.id = id;
            this.arg = arg;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_Call : AST_ExpCall
    {
        public AST_Id ID;
        public AST_StamentList Args;
        public AST_Call(ParserRuleContext parser_node, AST_Id id, AST_StamentList arg) : base(parser_node, null, id, null, arg)
        {
            this.ID = id;
            this.Args = arg;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);

    }

    public class AST_Let : AST_Expresion
    {
        public AST_ListProp props;
        public AST_Expresion expr;
        public AST_Let(ParserRuleContext parser_node, AST_ListProp props, AST_Expresion expr) : base(parser_node, new AST_Node[] { props, expr })
        {
            this.props = props; this.expr = expr;
        }
        public AST_Let(int r, int c, AST_ListProp props, AST_Expresion expr) : base(r, c, new AST_Node[] { props, expr })
        {
            this.props = props; this.expr = expr;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AST_CaseOf : AST_Expresion
    {
        public AST_Expresion expr;
        public AST_ListProp props;
        public AST_CaseOf(ParserRuleContext parser_node, AST_ListProp props, AST_Expresion expr) : base(parser_node, new AST_Node[] { expr, props })
        {
            this.props = props; this.expr = expr;
        }

        public override T Visit<T>(IASTVisitor<T> visitor) => visitor.Visit(this);
    }

}
