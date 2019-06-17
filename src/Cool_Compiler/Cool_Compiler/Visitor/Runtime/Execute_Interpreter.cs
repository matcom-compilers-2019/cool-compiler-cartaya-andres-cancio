using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST;
using Cool_Compiler.Iterpreter;


namespace Cool_Compiler.Visitor.Runtime
{
    public class AST_Execute_Interpreter : IASTVisitor<Base_Object_Value>
    {

        SMemory memo = new SMemory();

        

        public AST_Execute_Interpreter()
        {
            
        }

        public Base_Object_Value Visit(AST_BinaryOp node)
        {
            Base_Object_Value l =  node.Left.Visit(this);
            Base_Object_Value r =  node.Right.Visit(this);
            return l.CallMethod(node.Op.Text, new Base_Object_Value[] { r });
        }

        public Base_Object_Value Visit(AST_Id node)
        {
            return memo.GetValue(node.Id);
        }

        public Base_Object_Value Visit(AST_StamentList node)
        {
            Base_Object_Value result = Type_OBJECT.Singleton().Instanciate();
            foreach (var item in node.Children)
            {
                result = item.Visit(this);
            }
            return result;
        }

        public Base_Object_Value Visit(AST_ClassProperty node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_MethodDef node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_ClassDef node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_ListClass node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_ListMethod node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_ListProp node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_UnaryOp node)
        {
            Base_Object_Value l = Type_INT.Singleton().Instanciate();
            Base_Object_Value r = node.Exp.Visit(this);
            return l.CallMethod(node.Op.Text, new Base_Object_Value[] { r });
        }

        public Base_Object_Value Visit(AST_Root node)
        {
            return node.Children[0].Visit(this);
        }

        public Base_Object_Value Visit(AST_Cte node)
        {
            if (node.Token.Type == "Int")
                return new Base_Object_Value(int.Parse(node.Token.Text), Type_INT.Singleton());
            if (node.Token.Type.ToLower() == "true")
                return new Base_Object_Value(1, Type_INT.Singleton());
            return new Base_Object_Value(0, Type_INT.Singleton());
        }

        public Base_Object_Value Visit(AST_Asignacion node)
        {
            Base_Object_Value y = node.Exp.Visit(this);
            memo.SetValue(node.Id, y);
            return y;
        }

        public Base_Object_Value Visit(AST_Node node)
        {
            throw new Exception("Alguien no sabe como llamarse con el tipo correcto " + node.GetType().ToString());
        }

        public Base_Object_Value Visit(AST_If node)
        {
            Base_Object_Value t = node.Cond.Visit(this);
            if (t.IsTrue())
                return node.IfCorpus.Visit(this);
            if (node.ElseCorpus != null) return node.ElseCorpus.Visit(this);
            return Type_OBJECT.Singleton().Instanciate();
        }

        public Base_Object_Value Visit(AST_While node)
        {
            Base_Object_Value t = node.Cond.Visit(this);
            Base_Object_Value s = Type_OBJECT.Singleton().Instanciate();
            while (t.IsTrue())
            {
                s = node.WhileCorpus.Visit(this);
                t = node.Cond.Visit(this);
            }
            return s;
        }

        public Base_Object_Value Visit(AST_Let node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_Call node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_ExpCall node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_Isvoid node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_New_Type node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_FormalDec node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_Type_Node node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_Token node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_StringCte node)
        {
            throw new NotImplementedException();
        }

        public Base_Object_Value Visit(AST_CaseOf node)
        {
            throw new NotImplementedException();
        }

        Base_Object_Value IASTVisitor<Base_Object_Value>.Visit(AST_Node node)
        {
            throw new NotImplementedException();
        }
        
    }
}
