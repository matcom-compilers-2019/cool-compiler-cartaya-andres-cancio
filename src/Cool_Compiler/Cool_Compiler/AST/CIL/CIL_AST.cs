using Cool_Compiler.Visitor.CIL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.AST.CIL
{
    public abstract class CIL_AST_Node
    {
        public static IEnumerable<CIL_AST_Node> GetAllNode(CIL_AST_Node n)
        {
            if (n != null)
            {
                yield return n;
                foreach (var item in n.Children)
                {
                    foreach (var item2 in GetAllNode(item))
                        yield return item2;
                }
            }
        }

        public List<CIL_AST_Node> Children = new List<CIL_AST_Node>();
        public CIL_AST_Node(IEnumerable<CIL_AST_Node> children)
        {
            if (children != null)
                Children = new List<CIL_AST_Node>(children);
        }

        public virtual T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_AST_Root : CIL_AST_Node
    {
        public CIL_AST_Root(CIL_SectionData data, CIL_SectionTypes types, CIL_SectionFunction functions) : base(new CIL_AST_Node[] { data, types, functions })
        {
            Data = data;
            Types = types;
            Functions = functions;
        }
        public CIL_SectionData Data { get; private set; }
        public CIL_SectionTypes Types { get; private set; }
        public CIL_SectionFunction Functions { get; private set; }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            string solve = "";
            solve += "DATA:\n\n";
            if (Data != null)
                solve += Data.ToString() + "\n";
            solve += "TYPES:\n\n";
            if (Types != null)
                solve += Types.ToString() + "\n";
            solve += "FUNCTION:\n\n";
            if (Functions != null)
                solve += Functions.ToString() + "\n\n";
            return solve;
        }
    }

    #region SectionTypes
    public class CIL_SectionTypes : CIL_ListNode<CIL_ClassDef>
    {
        public CIL_SectionTypes(IEnumerable<CIL_ClassDef> list) : base(list)
        {
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            string solve = "";
            foreach (var item in Children)
            {
                solve += item.ToString() + "\n\n";
            }
            return solve;
        }
    }

    public class CIL_ClassDef : CIL_AST_Node
    {
        public string Id;
        public CIL_ListNode<CIL_ClassAttr> Attrs;
        public CIL_ListNode<CIL_ClassMethod> Methods;
        public CIL_ClassDef(string id, CIL_ListNode<CIL_ClassAttr> attrs, CIL_ListNode<CIL_ClassMethod> methods) : base(new CIL_AST_Node[] { attrs, methods })
        {
            Id = id;
            Attrs = attrs;
            Methods = methods;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            string solve = "type " + Id + " {\n\n";
            foreach (var item in Attrs.ListNode)
            {
                solve += "    attribute " + item.Id + ";\n";
            }
            solve += "\n";
            foreach (var item in Methods.ListNode)
            {
                solve += "    method " + item.Id + " : " + item.Idres + ";\n";
            }
            solve += "\n}";
            return solve;
        }
    }

    public class CIL_ClassAttr : CIL_AST_Node
    {
        public string Id;
        public CIL_ClassAttr(string id) : base(new CIL_AST_Node[] { })
        {
            Id = id;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_ClassMethod : CIL_AST_Node
    {
        public string Id, Idres;
        public CIL_ClassMethod(string id, string idres) : base(new CIL_AST_Node[] { })
        {
            Id = id;
            Idres = idres;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    #endregion

    #region SectionData

    public class CIL_SectionData : CIL_ListNode<CIL_DataElement>
    {
        public CIL_SectionData(IEnumerable<CIL_DataElement> list) : base(list)
        {
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            string solve = "";
            foreach (var item in ListNode)
            {
                solve += item.ToString() + "\n";
            }
            return solve;
        }
    }

    public class CIL_DataElement : CIL_AST_Node
    {
        public string Id, Data;
        public CIL_DataElement(string id, string data) : base(new CIL_AST_Node[] { })
        {
            Id = id;
            Data = data;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} : {1}", Id, Data);
        }
    }

    #endregion

    #region SectionStaments

    public class CIL_Label : CIL_AST_Node
    {
        public string Label;
        public CIL_Label(string label) : base(new CIL_AST_Node[] { })
        {
            Label = label;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("#{0}", Label);
        }

    }

    public class CIL_Goto : CIL_AST_Node
    {
        public string Label;
        public CIL_Goto(string label) : base(new CIL_AST_Node[] { })
        {
            Label = label;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
        public override string ToString()
        {
            return string.Format("jmp {0}", Label);
        }

    }

    public class CIL_If_Goto : CIL_AST_Node
    {
        public string Label;
        public string Condition;
        public CIL_If_Goto(string condition, string label) : base(new CIL_AST_Node[] { })
        {
            Condition = condition;
            Label = label;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("if {0} jmp {1}", Condition.ToString(), Label);
        }
    }

    public class CIL_ListNode<T> : CIL_AST_Node where T : CIL_AST_Node
    {
        public List<T> ListNode;

        public CIL_ListNode(IEnumerable<T> list) : base(list)
        {
            if (list == null) ListNode = new List<T>();
            else this.ListNode = new List<T>(list);
        }
        public override Q Visit<Q>(IVisitorCIL<Q> v) => v.Visit(this);
    }

    public class CIL_ExpBin : CIL_AST_Node
    {
        public string Left, Right;
        public string Op;
        public string Result;
        public CIL_ExpBin(string left, string right, string op, string result) : base(new CIL_AST_Node[] { })
        {
            Left = left; Right = right; Op = op;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = {1} {2} {3}", Result, Left.ToString(), Op, Right.ToString());
        }
    }

    public class CIL_ExpUn : CIL_AST_Node
    {
        public string Expr;
        public string Op, Result;
        public CIL_ExpUn(string expr, string op, string result) : base(new CIL_AST_Node[] { })
        {
            Expr = expr; Op = op;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
        public override string ToString()
        {
            return string.Format("{0} = {1} {2}", Result, Op, Expr.ToString());
        }
    }

    public class CIL_Asig : CIL_AST_Node
    {
        public string Expr;
        public string Id;
        public string TypeE;
        public CIL_Asig(string id, string expr, string typeE) : base(new CIL_AST_Node[] { })
        {
            Id = id;
            Expr = expr;
            TypeE = typeE;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = {1}", Id, Expr);
        }
    }

    public class CIL_Locals : CIL_AST_Node
    {
        public List<string> Locals;
        public CIL_Locals(List<string> locals) : base(new CIL_AST_Node[] { })
        {
            Locals = locals;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_Params : CIL_AST_Node
    {
        public List<string> Params;
        public CIL_Params(List<string> parm) : base(new CIL_AST_Node[] { })
        {
            Params = parm;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_StamentList : CIL_ListNode<CIL_AST_Node>
    {
        public CIL_StamentList(IEnumerable<CIL_AST_Node> list) : base(list) { }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_GetId : CIL_AST_Node
    {
        public CIL_GetId(string id) : base(new CIL_AST_Node[] { })
        {
            Id = id;
        }

        public string Id;

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return Id;
        }
    }

    public class CIL_Constant : CIL_AST_Node
    {
        public int Value;

        public CIL_Constant(int value) : base(new CIL_AST_Node[] { })
        {
            Value = value;
        }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return "" + Value;
        }
    }

    public class CIL_FunctionCall : CIL_AST_Node
    {
        public string Name;
        public CIL_Params Params;
        public string TypeCall;
        public string Result;
        public bool Is_Type_Dir;
        public string Static_Type;
        public CIL_FunctionCall(string name, CIL_Params param, string typecall, string result, bool is_type_dir, string static_type) : base(new CIL_AST_Node[] { param })
        {
            Name = name;
            Params = param;
            TypeCall = typecall;
            Result = result;
            Is_Type_Dir = is_type_dir;
            Static_Type = static_type;
        }

        public override string ToString()
        {
            string s = "\n";
            foreach (var item in Params.Params)
            {
                s += "    ARG " + item + ";\n";
            }
            if (string.IsNullOrEmpty(TypeCall))
            {
                s += "    " + Result + " = " + "call " + " " + Name + "(" + Static_Type + ")";
            }
            else
                s += "    " + Result + " = " + "vcall " + TypeCall + " " + Name + "(" + Static_Type + ")";
            return s;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_GetAttr : CIL_AST_Node
    {
        public string Instanc, Attr, Result;
        public string StaticType;
        // result = instac.attr
        public CIL_GetAttr(string instanc, string attr, string result, string statictype) : base(new CIL_AST_Node[] { })
        {
            Instanc = instanc;
            Attr = attr;
            Result = result;
            StaticType = statictype;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = GETATTR {1} {2} ({3})", Result, Instanc, Attr, StaticType);
        }
    }

    public class CIL_SetAttr : CIL_AST_Node
    {
        public string Instanc, Attr, Result;
        public string StaticType;

        // instac.attr = result
        public CIL_SetAttr(string instanc, string attr, string result, string statictype) : base(new CIL_AST_Node[] { })
        {
            Instanc = instanc;
            Attr = attr;
            Result = result;
            StaticType = statictype;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("SETATTR {0} {1} {2} ({3})", Instanc, Attr, Result, StaticType);
        }
    }

    public class CIL_FatherType : CIL_AST_Node
    {
        public string Type;
        public string Result;
        // result = instac.attr
        public CIL_FatherType(string type, string result) : base(new CIL_AST_Node[] { })
        {
            Type = type;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return Result + " = " + "FATHERTYPE " + Type;
        }
        
    }

    public class CIL_Allocate : CIL_AST_Node
    {
        public string Type;
        public string Result;
        public bool Is_Type_Dir;
        // result = instac.attr
        public CIL_Allocate(string type, string result, bool is_type_dir) : base(new CIL_AST_Node[] { })
        {
            Type = type;
            Result = result;
            Is_Type_Dir = is_type_dir;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return Result + " = " + "ALLOCATE " + Type;
        }
    }


    public class CIL_TypeName : CIL_AST_Node
    {
        public string Type;
        public string Result;
        public bool Is_Type_Dir;
        // result = instac.attr
        public CIL_TypeName(string type, string result, bool is_type_dir) : base(new CIL_AST_Node[] { })
        {
            Type = type;
            Result = result;
            Is_Type_Dir = is_type_dir;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return Result + " = " + "TYPENAME " + Type;
        }
    }

    public class CIL_TypeOf : CIL_AST_Node
    {
        public string Id;
        public string Result;
        // result = instac.attr
        public CIL_TypeOf(string id, string result) : base(new CIL_AST_Node[] { })
        {
            Id = id;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = TYPEOF {1}", Result , Id);
        }
    }

    public class CIL_IsVoid : CIL_AST_Node
    {
        public string Id;
        public string Result;
        // result = instac.attr
        public CIL_IsVoid(string id, string result) : base(new CIL_AST_Node[] { })
        {
            Id = id;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = ISVOID {1}", Result, Id);
        }
    }

    public class CIL_Return : CIL_AST_Node
    {
        public string Result ;
        public string ResultType ;
        // result = instac.attr
        public CIL_Return(string result, string type) : base(new CIL_AST_Node[] { })
        {
            ResultType = type;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return "RETURN " + Result;
        }
    }

    public class CIL_LoadStr : CIL_AST_Node
    {
        public string Result;
        public string StrId;
        // result = LOAD StrId
        public CIL_LoadStr(string strid, string result) : base(new CIL_AST_Node[] { })
        {
            StrId = strid;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = LOAD {1}", Result, StrId);
        }
    }

    public class CIL_LengthStr : CIL_AST_Node
    {
        public string Result;
        public string StrId;
        // result = LOAD StrId
        public CIL_LengthStr(string strid, string result) : base(new CIL_AST_Node[] { })
        {
            StrId = strid;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_StrStr : CIL_AST_Node
    {
        public string Result;
        public string StrId, I, L;
        // result = STR StrId
        public CIL_StrStr(string strid, string i, string l, string result) : base(new CIL_AST_Node[] { })
        {
            StrId = strid;
            Result = result;
            I = i; L = l;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_Abort : CIL_AST_Node
    {
        public CIL_Abort() : base(new CIL_AST_Node[] { })
        {
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }
    public class CIL_Copy : CIL_AST_Node
    {
        public string Id, Result;
        public CIL_Copy(string id, string result) : base(new CIL_AST_Node[] { })
        {
            Id = id;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }


    public class CIL_StrConcat : CIL_AST_Node
    {
        public string Result;
        public string StrId, StrId2;
        // result = STR StrId
        public CIL_StrConcat(string strid, string strid2, string result) : base(new CIL_AST_Node[] { })
        {
            StrId = strid;
            StrId2 = strid2;
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_IORead : CIL_AST_Node
    {
        public string Result;
        // result = READ
        public CIL_IORead(string result) : base(new CIL_AST_Node[] { })
        {
            Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);
    }

    public class CIL_IOPrint : CIL_AST_Node
    {
        public string Obj;
        // result = READ
        public CIL_IOPrint(string obj) : base(new CIL_AST_Node[] { })
        {
            Obj = obj;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("PRINT {0}", Obj);
        }
    }

    public class CIL_IOPrintInt : CIL_AST_Node
    {
        public string Obj;
        // result = READ
        public CIL_IOPrintInt(string obj) : base(new CIL_AST_Node[] { })
        {
            Obj = obj;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("PRINT {0}", Obj);
        }
    }

    public class CIL_IOReadInt : CIL_AST_Node
    {
        public string Result;
        // result = READ
        public CIL_IOReadInt(string obj) : base(new CIL_AST_Node[] { })
        {
            Result = obj;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{0} = READ_INT", Result);
        }
    }

    public class CIL_ExceptionCond : CIL_AST_Node
    {
        public string Cond, Sms, Id;
        // result = READ
        public CIL_ExceptionCond(string id, string cond, string sms) : base(new CIL_AST_Node[] { })
        {
            Cond = cond; Sms = sms; Id = id;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("if {2} = {0} ERROR : [{1}] ", Cond, Sms, Id);
        }
    }

    public class CIL_SetMemory : CIL_AST_Node
    {
        public string Dir, Offset, Value;
        public CIL_SetMemory(string dir, string offset, string value) : base(new CIL_AST_Node[] { })
        {
            Dir = dir; Offset = offset; Value = value;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("[{0} + {1}] = {2} ", Dir, Offset, Value);
        }
    }
    public class CIL_GetMemory : CIL_AST_Node
    {
        public string Dir, Offset, Result;
        public CIL_GetMemory(string dir, string offset, string result) : base(new CIL_AST_Node[] { })
        {
            Dir = dir; Offset = offset; Result = result;
        }
        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            return string.Format("{2} = [{0} + {1}]", Dir, Offset, Result);
        }
    }



    // TODO : Agregar CONCAT y SUBSTRING


    #endregion

    #region SectionFunction
    public class CIL_FunctionDef : CIL_AST_Node
    {
        public CIL_FunctionDef(string name, CIL_Params param, CIL_Params local, CIL_StamentList corpus) : base(new CIL_AST_Node[] { param, local, corpus })
        {
            Name = name;
            Params = param;
            Locals = local;
            Corpus = corpus;
        }

        public string Name { get; private set; }
        public CIL_Params Params { get; private set; }
        public CIL_Params Locals { get; private set; }
        public CIL_StamentList Corpus { get; private set; }

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            string t = "    ";
            string s = "function " + Name + "{\n";

            foreach (var item in Params.Params)
            {
                s += t + "ARG " + item + ";\n";
            }

            s += "\n";

            foreach (var item in Locals.Params)
            {
                s += t + "LOCAL " + item + ";\n";
            }

            s += "\n";

            foreach (var item in Corpus.ListNode)
            {
                s += t + item.ToString() + ";\n";
            }

            return s + "}\n";
        }
    }

    public class CIL_SectionFunction : CIL_ListNode<CIL_FunctionDef>
    {
        public CIL_SectionFunction(IEnumerable<CIL_FunctionDef> list) : base(list) { }       

        public override T Visit<T>(IVisitorCIL<T> v) => v.Visit(this);

        public override string ToString()
        {
            string solve = "";
            foreach (var item in ListNode)
            {
                solve += item + "\n";
            }
            return solve;
        }
    }
    #endregion
}
