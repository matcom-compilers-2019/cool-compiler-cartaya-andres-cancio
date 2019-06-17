using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.Visitor.Semantic.Context;
using Cool_Compiler.AST;
using Cool_Compiler.Visitor.Semantic.Errors;
using Cool_Compiler.Visitor.Semantic;

namespace Cool_Compiler.Visitor.Semantic
{
    public class CheckIDSandTypeDecVisitor : IASTVisitor<bool>
    {

        public ErrorLoger CurrErrorLoger;
        public HashSet<string> ReservedWords;
        public HashSet<string> Types;

        public bool Visit(AST_Node node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Asignacion node)
        {
            return true;
        }

        public bool Visit(AST_BinaryOp node)
        {
            return node.Left.Visit(this) && node.Right.Visit(this);
        }

        public bool Visit(AST_Cte node)
        {
            return true;
        }

        public bool Visit(AST_Id node)
        {
            return !Types.Contains(node.Id) && !ReservedWords.Contains(node.Id);
        }

        public bool Visit(AST_If node)
        {
            return node.Cond.Visit(this) && node.IfCorpus.Visit(this) && node.ElseCorpus.Visit(this);
        }
        
        public bool Visit(AST_Root node)
        {
            CurrErrorLoger = new ErrorLoger();
            Types = new HashSet<string>();
            ReservedWords = new HashSet<string>(new List<string>() {
                "class", "else", "fi", "if", "in", "inherits",
                "isvoid", "let", "loop", "pool", "then", "while",
                "case", "esac", "new", "of", "not",
                 "Class", "Else", "Fi", "If", "In", "Inherits",
                "Isvoid", "Let", "Loop", "Pool", "Then", "While",
                "Case", "Esac", "New", "Of", "Not", "true", "false",
                "Int", "Bool", "Object", "String", "IO"
            });
            return node.class_list.Visit(this);
        }

        public bool Visit(AST_StamentList node)
        {
            bool solve = true;
            foreach (var item in node.stament_list)
            {
                solve &= item.Visit(this);
            }
            return true;
        }

        public bool Visit(AST_UnaryOp node)
        {
            return node.Exp.Visit(this);
        }

        public bool Visit(AST_While node)
        {
            return node.Cond.Visit(this) && node.WhileCorpus.Visit(this);
        }

        public bool Visit(AST_ClassProperty node)
        {
            if (ReservedWords.Contains(node.decl.id.Id))
            {
                CurrErrorLoger.LogError(node.decl.row, node.decl.col, "El id de la variable es una palabra reservada");
                return false;
            }
            if (Types.Contains(node.decl.id.Id))
            {
                CurrErrorLoger.LogError(node.decl.row, node.decl.col, "El id de la variable es una tipo definido");
                return false;
            }
            if (!Types.Contains(node.decl.type.Type))
            {
                CurrErrorLoger.LogError(node.decl.row, node.decl.col, "El tipo de la variable no esta definido");
                return false;
            }
            if (node.exp != null)
                if (!node.exp.Visit(this))
                    return false;
            return true;
        }

        public bool Visit(AST_ListProp node)
        {
            bool solve = true;

            foreach (var item in node.Propertys)
            {
                solve &= item.Visit(this);
            }

            return solve;
        }

        public bool Visit(AST_MethodDef node)
        {
            bool solve = true;

            foreach (var item in node.Propertys.Propertys)
            {
                solve &= item.Visit(this);
            }

            return solve & node.Statament.Visit(this);
        }

        public bool Visit(AST_ListMethod node)
        {
            bool solve = true;

            foreach (var item in node.Methods)
            {
                solve &= item.Visit(this);
            }

            return solve;
        }

        public bool Visit(AST_ClassDef node)
        {
            bool solve = true;

            solve &= node.Property_list.Visit(this);

            solve &= node.Method_list.Visit(this);

            return solve;
        }

        public bool Visit(AST_ListClass node)
        {
            bool solve = true;

            Types.Add("Object");
            Types.Add("String");
            Types.Add("IO");
            Types.Add("Int");
            Types.Add("Bool");

            foreach (var item in node.Class_list)
            {
                if (!Types.Add(item.Id.Id))
                    return false;
            }

            foreach (var item in node.Class_list)
            {
                solve &= item.Visit(this);
            }

            return solve;
        }

        public bool Visit(AST_Let node)
        {
            bool solve = true;
            foreach (var item in node.props.Propertys)
            {
                solve &= item.Visit(this);
            }
            return solve & node.expr.Visit(this);
        }

        public bool Visit(AST_Call node)
        {
            return node.arg.Visit(this);
        }

        public bool Visit(AST_ExpCall node)
        {
            bool solve = true;
            if(node.expr != null)
                solve &= node.expr.Visit(this);
            if (node.explicittype != null)
                solve &= node.explicittype.Visit(this);
            solve &= node.arg.Visit(this);

            return solve;
        }

        public bool Visit(AST_Isvoid node)
        {
            return true;
        }

        public bool Visit(AST_New_Type node)
        {
            return true;
        }

        public bool Visit(AST_FormalDec node)
        {
            if (ReservedWords.Contains(node.id.Id))
            {
                CurrErrorLoger.LogError(node.id.row, node.id.col, "El id de la variable es una palabra reservada");
                return false;
            }
            if (Types.Contains(node.id.Id))
            {
                CurrErrorLoger.LogError(node.id.row, node.id.col, "El id de la variable es un tipo definido");
                return false;
            }
            if (!Types.Contains(node.type.Type))
            {
                CurrErrorLoger.LogError(node.id.row, node.id.col, "El tipo de la variable no esta definido");
                return false;
            }
            return true;
        }

        public bool Visit(AST_Type_Node node)
        {
            return Types.Contains(node.Type);
        }

        public bool Visit(AST_Token node)
        {
            return !Types.Contains(node.Text) && !ReservedWords.Contains(node.Text);
        }

        public bool Visit(AST_StringCte node)
        {
            return true;
        }

        public bool Visit(AST_CaseOf node)
        {
            if (!node.expr.Visit(this))
                return false;
            foreach (var item in node.props.Propertys)
            {
                if (!item.exp.Visit(this))
                    return false;
                if (!item.decl.id.Visit(this))
                    return false;
            }

            return true;
        }
    }
}
