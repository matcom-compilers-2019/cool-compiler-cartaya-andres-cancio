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
    public class CheckVariablesVisitor : IASTVisitor<bool>
    {
        public IContext CurrContext;
        public ErrorLoger CurrErrorLoger;
        Dictionary<string, SemanticType> All_Types;

        public bool Visit(AST_Node node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Asignacion node)
        {
            bool solve = true;
            bool visit_result = node.Exp.Visit(this);
            if (!visit_result)
                solve = false;
            if (!CurrContext.IsDefine(node.Id))
            {
                solve = false;
                CurrErrorLoger.LogError(node.row, node.col, "Variable " + node.Id + " no definida");
            }
            CurrContext.Define(node.Id);
            return solve;
        }

        public bool Visit(AST_BinaryOp node)
        {
            bool visit_left = node.Left.Visit(this);
            bool visit_right = node.Right.Visit(this);
            return visit_right && visit_left;
        }

        public bool Visit(AST_Cte node)
        {
            return true;
        }

        public bool Visit(AST_Id node)
        {
            return CurrContext.IsDefine(node.Id) || node.Id == "self";
        }

        public bool Visit(AST_If node)
        {
            bool visit_result = true;
            foreach (var child in node.Children)
            {
                visit_result &= child.Visit(this);
            }
            return visit_result;
        }

        public bool Visit(AST_Root node)
        {
            CurrContext = new Contexts();            
            CurrErrorLoger = new ErrorLoger();
            All_Types = SemanticType.BuildAllType(node.class_list);
            return node.class_list.Visit(this);
        }

        public bool Visit(AST_StamentList node)
        {
            bool visit_result = true;
            foreach (var child in node.stament_list)
            {
                visit_result &= child.Visit(this);
            }
            return visit_result;
        }

        public bool Visit(AST_UnaryOp node)
        {
            return node.Exp.Visit(this);
        }

        public bool Visit(AST_While node)
        {
            bool visit_result = true;
            foreach (var child in node.Children)
            {
                visit_result &= child.Visit(this);
            }
            return visit_result;
        }

        public bool Visit(AST_ClassProperty node)
        {
            bool solve = true;
            if (CurrContext.IsDefineInMe(node.decl.id.Id))
            {
                solve = false;
                CurrErrorLoger.LogError(node.decl.row, node.decl.col, "La propiedad " + node.decl.id.Id + " ya pertenece a la clase");
            }
            if (node.exp != null && !node.exp.Visit(this))
                solve = false;
            CurrContext.Define(node.decl.id.Id);
            return solve;
        }

        public bool Visit(AST_ListProp node)
        {
            bool visit_result = true;
            foreach (var property in node.Propertys)
            {
                visit_result &= property.Visit(this);
            }
            return visit_result;
        }

        public bool Visit(AST_MethodDef node)
        {
            CurrContext = CurrContext.CreateChild();
            foreach (var arg in node.Propertys.Propertys)
            {
                CurrContext.Define(arg.decl.id.Id);
            }
            bool visit_result = node.Statament.Visit(this);
            if(!CurrContext.NullFather())
                CurrContext = CurrContext.GetParent();
            return visit_result;
        }

        public bool Visit(AST_ListMethod node)
        {
            bool visit_result = true;
            foreach (var method in node.Methods)
            {
                visit_result &= method.Visit(this); 
            }
            return visit_result;
        }

        public bool Visit(AST_ClassDef node)
        {
            bool solve = true;
            CurrContext = CurrContext.CreateChild();
            SemanticType father = All_Types[node.Id.Id].Father;
            while (true)
            {             
                var my_atributes = father.GetAllAttr();
                foreach (var attr in my_atributes)
                {
                    if (CurrContext.IsDefine(attr.Id))
                    {
                        solve = false;
                        CurrErrorLoger.LogError(node.row, node.col, "Propiedad " + attr.Id + " previamente definida");
                    }
                    CurrContext.Define(attr.Id);
                }
                father = father.Father;
                if (father == null)
                    break;
            }

            foreach (var prop in node.Property_list.Propertys)
            {
                if (CurrContext.IsDefine(prop.decl.id.Id))
                {
                    solve = false;
                    CurrErrorLoger.LogError(prop.decl.row, prop.decl.col, "Propiedad " + node.Id + " previamente definida");
                }
                else CurrContext.Define(prop.decl.id.Id);
            }

            solve = node.Method_list.Visit(this);
            if (!CurrContext.NullFather())
                CurrContext = CurrContext.GetParent();
            return solve;
        }

        public bool Visit(AST_ListClass node)
        {
            bool visit_result = true;
            foreach (var class_def in node.Class_list)
            {
                visit_result &= class_def.Visit(this);
            }
            
            return visit_result;
        }

        public bool Visit(AST_Let node)
        {
            CurrContext = CurrContext.CreateChild();
            if (node.props.Propertys.Count == 0) return false;
            foreach (var prop in node.props.Propertys)
            {
                if (prop.exp != null &&  !prop.exp.Visit(this))
                    return false;
                CurrContext.Define(prop.decl.id.Id);
            }
            bool flag = node.expr.Visit(this);
            if (!CurrContext.NullFather())
                CurrContext = CurrContext.GetParent();
            return flag;
        }

        public bool Visit(AST_Call node)
        {
            bool solve = true;
            if (node.expr != null && !node.expr.Visit(this))
                solve = false;
            if (!node.Args.Visit(this))
                solve = false;
            return solve;
        }

        public bool Visit(AST_ExpCall node)
        {
            return node.arg.Visit(this);
        }

        public bool Visit(AST_Isvoid node)
        {
            return node.expr.Visit(this);
        }

        public bool Visit(AST_New_Type node)
        {
            return true;
        }

        public bool Visit(AST_FormalDec node)
        {
            if (CurrContext.IsDefineInMe(node.id.Id))
            {
                CurrErrorLoger.LogError(node.row, node.col, "Variable " + node.id.Id + " previamente definida");
                return false;
            }
            CurrContext.Define(node.id.Id);
            return true;
        }

        public bool Visit(AST_Type_Node node)
        {
            return true;
        }

        public bool Visit(AST_Token node)
        {
            return true;
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
            }

            return true;
        }
    }
}
