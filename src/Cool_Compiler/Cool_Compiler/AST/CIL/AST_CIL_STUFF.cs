using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.Transpiler;
using Cool_Compiler.Visitor.Semantic;
using Cool_Compiler.Visitor;
using Cool_Compiler.Visitor.CIL;
using System.IO;
using Antlr4.Runtime;
using Cool_Compiler.Visitor.Runtime;


namespace Cool_Compiler.AST.CIL
{
    public static class AST_CIL_STUFF
    {
        public static CIL_AST_Node AST_BUILDER(string statament)
        {
            string[] stat_split = statament.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(stat_split.Length == 1)
            {
                return new CIL_Constant(int.Parse(stat_split[0]));
            }
            else if(stat_split.Length == 2)
            {
                if (stat_split[0] == "LABEL")
                    return new CIL_Label(stat_split[1]);
                else if (stat_split[0] == "GOTO")
                    return new CIL_Goto(stat_split[1]);
                else if (stat_split[0] == "RETURN")
                    return new CIL_Return(stat_split[1], null); // TODO : Ver lo del null este
                else if (stat_split[0] == "PRINT")
                    return new CIL_IOPrint(stat_split[1]);
                else if (stat_split[0] == "PRINT_INT")
                    return new CIL_IOPrint(stat_split[1]);
            }
            else if(stat_split.Length == 3)
            {
                if (stat_split[1] == "=")
                    return new CIL_Asig(stat_split[0], stat_split[2], null);
                if (stat_split[2] == "READ" && stat_split[1] == "=")
                    return new CIL_IORead(stat_split[1]);
                if (stat_split[2] == "READ_INT" && stat_split[1] == "=")
                    return new CIL_IORead(stat_split[1]);
            }
            else if(stat_split.Length == 4)
            {
                if (stat_split[0] == "IF" && stat_split[2] == "GOTO")
                    return new CIL_If_Goto(stat_split[1], stat_split[3]);
                else if (stat_split[1] == "=")
                    return new CIL_ExpUn(stat_split[3], stat_split[2], stat_split[0]);
                else if (stat_split[0] == "SETATTR")
                    return new CIL_SetAttr(stat_split[1], stat_split[2], stat_split[3], stat_split[4]);
                else if (stat_split[1] == "=" && stat_split[2] == "ALLOCATE")
                    return new CIL_Allocate(stat_split[0], stat_split[3], true); // TODO : ver lo de boolsito este
                else if (stat_split[1] == "=" && stat_split[2] == "TYPEOF")
                    return new CIL_TypeOf(stat_split[0], stat_split[3]);
                else if (stat_split[1] == "=" && stat_split[2] == "LOAD")
                    return new CIL_LoadStr(stat_split[0], stat_split[3]);
                else if (stat_split[2] == "LENGTH" && stat_split[1] == "=")
                    return new CIL_LengthStr(stat_split[3], stat_split[0]);
                else if (stat_split[2] == "SUBSTRING" && stat_split[1] == "=")
                    return new CIL_StrStr(stat_split[3], stat_split[4], stat_split[5], stat_split[0]);
            }
            else if(stat_split.Length == 5)
            {
                if (stat_split[1] == "=" && stat_split[2] == "GETATTR")
                    return new CIL_GetAttr(stat_split[0], stat_split[3], stat_split[4], stat_split[4]);
                else if (stat_split[1] == "=")
                    return new CIL_ExpBin(stat_split[2], stat_split[3], stat_split[4], stat_split[0]);
            }


            return null;
        }
    }
}
