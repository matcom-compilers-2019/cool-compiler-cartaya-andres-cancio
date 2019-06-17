using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST;

namespace Cool_Compiler.Visitor
{
    public interface IASTVisitor<T>
    {
        T Visit(AST_Node node);
        T Visit(AST_Asignacion node);
        T Visit(AST_BinaryOp node);
        T Visit(AST_Cte node);
        T Visit(AST_Id node);
        T Visit(AST_If node); 
        T Visit(AST_Root node);
        T Visit(AST_StamentList node);
        T Visit(AST_UnaryOp node);
        T Visit(AST_While node);
        T Visit(AST_ClassProperty node);
        T Visit(AST_ListProp node);
        T Visit(AST_MethodDef node);
        T Visit(AST_ListMethod node);
        T Visit(AST_ClassDef node);
        T Visit(AST_ListClass node);
        T Visit(AST_Let node);
        T Visit(AST_Call node);
        T Visit(AST_ExpCall node);
        T Visit(AST_Isvoid node);
        T Visit(AST_New_Type node);
        T Visit(AST_FormalDec node);
        T Visit(AST_Type_Node node);
        T Visit(AST_Token node);
        T Visit(AST_StringCte node);
        T Visit(AST_CaseOf node);
    }
}
