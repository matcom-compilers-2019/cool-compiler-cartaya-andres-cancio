using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST.CIL;

namespace Cool_Compiler.Visitor.CIL
{
    public interface IVisitorCIL<T>
    {
        T Visit(CIL_AST_Node node);
        T Visit(CIL_AST_Root node);
        T Visit(CIL_SectionTypes node);
        T Visit(CIL_ClassDef node);
        T Visit(CIL_ClassAttr node);
        T Visit(CIL_ClassMethod node);
        T Visit(CIL_SectionData node);
        T Visit(CIL_DataElement node);
        T Visit(CIL_Label node);
        T Visit(CIL_Goto node);
        T Visit(CIL_If_Goto node);
        T Visit(CIL_ExpBin node);
        T Visit(CIL_ExpUn node);
        T Visit(CIL_Asig node);
        T Visit(CIL_Locals node);
        T Visit(CIL_Params node);
        T Visit(CIL_StamentList node);
        T Visit(CIL_GetId node);
        T Visit(CIL_Constant node);
        T Visit(CIL_FunctionCall node);
        T Visit(CIL_FunctionDef node);
        T Visit(CIL_SectionFunction node);
        T Visit(CIL_SetAttr node);
        T Visit(CIL_GetAttr node);
        T Visit(CIL_Allocate node);
        T Visit(CIL_Return node);
        T Visit(CIL_LoadStr node);
        T Visit(CIL_StrStr node);
        T Visit(CIL_LengthStr node);
        T Visit(CIL_IORead node);
        T Visit(CIL_IOReadInt node);
        T Visit(CIL_IOPrint node);
        T Visit(CIL_TypeOf node);
        T Visit(CIL_ExceptionCond node);
        T Visit(CIL_IOPrintInt node);

        T Visit(CIL_TypeName node);

        T Visit(CIL_FatherType node);

        T Visit(CIL_StrConcat node);

        T Visit(CIL_Abort node);

        T Visit(CIL_Copy node);


    }
}
