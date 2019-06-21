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
    public class CheckTypesVisitor : IASTVisitor<bool>
    {
        public IContext CurrContext;
        public ErrorLoger CurrErrorLoger;
        public Dictionary<string, SemanticType> All_Types;
        SemanticType CurrType;
        public bool Visit(AST_Node node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Asignacion node)
        {
            if (!node.Exp.Visit(this))
                return false;
            var x = CurrContext.GetType(node.Id);
            //if (x.Name != node.Exp.MyType.Name)
            if (SemanticType.LCA(x, node.Exp.MyType).Name != x.Name)
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo de " + node.Exp.MyType.Name + " no se conforma a " + x.Name);
                return false;
            }
            node.MyType = node.Exp.MyType;
            return true;
        }

        public bool Visit(AST_BinaryOp node)
        {
            bool solve = true;
            bool visit_right = node.Right.Visit(this);
            bool visit_left = node.Left.Visit(this);
            if (!visit_left || !visit_right)
                return false;
            if (!All_Types.ContainsKey(node.Right.MyType.Name))
            {
                CurrErrorLoger.LogError(node.Right.row, node.Right.col, "Tipo inexistente");
                return false;
            }
            if (!All_Types.ContainsKey(node.Left.MyType.Name))
            {
                CurrErrorLoger.LogError(node.Left.row, node.Left.col, "Tipo inexistente");
                return false;
            }
            if (node.Op.Text == "=")
            {
                bool ok = true;
                if (node.Right.MyType.Name == "String" || node.Right.MyType.Name == "Int" ||
                    node.Right.MyType.Name == "Bool" || node.Left.MyType.Name == "String" || node.Left.MyType.Name == "Int" ||
                    node.Left.MyType.Name == "Bool" || node.Left.MyType.Name == "Object" || node.Right .MyType.Name == "Object")
                    ok = node.Right.MyType.Name == node.Left.MyType.Name;
                //else solve &= false;
                node.MyType = All_Types["Bool"];
                if (node.Right.MyType == null || node.Left.MyType == null || !ok)
                    return false;
                var lca = SemanticType.LCA(node.Right.MyType, node.Left.MyType);
                if (lca.Name != node.Right.MyType.Name && lca.Name != node.Left.MyType.Name)
                {
                    CurrErrorLoger.LogError(node.row, node.col, "No se encuentran en la misma rama del arbol de herencia");
                    return false;
                }
                return true;
            }
            else
            {
                if (node.Right.MyType.Name != node.Left.MyType.Name)
                {
                    CurrErrorLoger.LogError(node.row, node.col, "Los tipos de la expresion deben ser iguales");
                    return false;
                }
                if (node.Right.MyType.Name != "Int")
                {
                    CurrErrorLoger.LogError(node.Right.row, node.Right.col, "El tipo de la expresion debe ser INT");
                    return false;
                }
                if(node.Left.MyType.Name != "Int")
                {
                    CurrErrorLoger.LogError(node.Left.row, node.Left.col, "El tipo de la expresion debe ser INT");
                    return false;
                }
                if (node.Op.Text == "<" || node.Op.Text == "<=")
                    node.MyType = All_Types["Bool"];
                else node.MyType = All_Types["Int"];
                return true;
            }
        }

        public bool Visit(AST_Cte node)
        {
            if (node.Token.Text == "true" || node.Token.Text == "false")
                node.MyType = All_Types["Bool"];
            else
            {
                int x;
                if (int.TryParse(node.Token.Text, out x))
                    node.MyType = All_Types["Int"];
                else node.MyType = All_Types["String"];
            }
            return true;
        }

        public bool Visit(AST_Id node)
        {
            SemanticType my_type;
            if (node.Id == "self")
                my_type = CurrType;
            else if ((my_type = CurrContext.GetType(node.Id)) == null)
            {
                CurrErrorLoger.LogError(node.row, node.col, "La variable " + node.Id + "no tiene tipo");
                return false;
            }
            node.MyType = my_type;
            return true;
        }

        public bool Visit(AST_If node)
        {
            bool solve = true;
            bool visit_if = node.IfCorpus.Visit(this);
            bool visit_else = node.ElseCorpus.Visit(this);
            bool visit_cond = node.Cond.Visit(this);
            if (!visit_cond)
                return false;
            if(node.Cond.MyType.Name != "Bool")
            {
                CurrErrorLoger.LogError(node.Cond.row, node.Cond.col, "La condicion debe ser Bool");
                return false;
            }
            if (!visit_if || !visit_else || !visit_cond)
                return false;
            var if_type = SemanticType.LCA(node.IfCorpus.MyType, node.ElseCorpus.MyType);
            node.MyType = if_type;
            if(if_type == null)
            {
                CurrErrorLoger.LogError(node.row, node.col, "No se puede asignar el tipo");
                return false;
            }
            return true;
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
            foreach (var stat in node.stament_list)
            {
                visit_result &= stat.Visit(this);
                node.MyType = stat.MyType;
            }

            return visit_result & node.MyType != null;
        }

        public bool Visit(AST_UnaryOp node)
        {
            bool solve = true;
            if (!node.Exp.Visit(this))
                return false;
            node.MyType = node.Exp.MyType;
            
            if(node.Op.Text == "not")
            {
                if(node.Exp.MyType.Name != "Bool")
                {
                    CurrErrorLoger.LogError(node.row, node.col, "No se puede aplicar not a una expresion no booleana");
                    return false;
                }
            }
            else
            {
                if(node.Op.Text == "~" || node.Op.Text == "-")
                {
                    if (node.Exp.MyType.Name == "Bool")
                    {
                        CurrErrorLoger.LogError(node.row, node.col, "Operacion invalida para una expresion booleana");
                        return false;
                    }
                    else if (node.Exp.MyType.Name != "Int")
                    {
                        CurrErrorLoger.LogError(node.row, node.col, "Operacion invalida solo aplicables para tipos Int");
                        return false;
                    }
                }
            }
            return true;
        }

        public bool Visit(AST_While node)
        {
            bool solve = true;
            if (!node.Cond.Visit(this))
                return false;
            if (!node.WhileCorpus.Visit(this))
                return false;
            node.MyType = All_Types["Object"];

            return solve;
        }

        public bool Visit(AST_ClassProperty node)
        {
            bool solve = true;

            var type = CurrContext.GetType(node.decl.id.Id);

            bool visit_tuple = true;

            if(node.exp != null)
                visit_tuple = node.exp.Visit(this);

            if (!visit_tuple || type != null)
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo " + node.decl.id.Id + " ya posee un tipo");
                return false;
            }

            if(type == null)
            {
                if (!All_Types.ContainsKey(node.decl.type.Type))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "Tipo no definido");
                    return false;
                }
                else
                {
                    //if (node.decl.type.Type == "int")
                    //    node.decl.type.Type = "Int";
                    //if (node.decl.type.Type == "string")
                    //    node.decl.type.Type = "String";
                    //if (node.decl.type.Type == "bool")
                    //    node.decl.type.Type = "Bool";
                    node.decl.id.MyType = All_Types[node.decl.type.Type];
                    CurrContext.SetType(node.decl.id.Id, All_Types[node.decl.type.Type]);
                }
            }

            if (node.decl.id.MyType != null && node.exp != null)
            {
                SemanticType lca = SemanticType.LCA(All_Types[node.decl.type.Type], node.exp.MyType);

                if (lca.Name != node.decl.id.MyType.Name)
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo " + node.exp.MyType.Name + " no se conforma a " + node.decl.type.Type);
                    return false;
                }
            }
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
            bool solve = true;
            if (!CurrType.HasMethod(node.Id.Id))
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo de retorno del metodo no existe");
                return false;
            }
            CurrContext = CurrContext.CreateChild();
            //if (node.type.Type == "int")
            //    node.type.Type = "Int";
            //if (node.type.Type == "string")
            //    node.type.Type = "String";
            //if (node.type.Type == "bool")
            //    node.type.Type = "Bool";
            if (!All_Types.ContainsKey(node.type.Type))
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo de retorno del metodo no existe");
                if (!CurrContext.NullFather())
                    CurrContext = CurrContext.GetParent();
                return false;
            }
            foreach (var arg in node.Propertys.Propertys)
            {
                //if (arg.decl.type.Type == "int")
                //    arg.decl.type.Type = "Int";
                //if (arg.decl.type.Type == "string")
                //    arg.decl.type.Type = "String";
                //if (arg.decl.type.Type == "bool")
                //    node.type.Type = "Bool";
                if (!All_Types.ContainsKey(arg.decl.type.Type))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo de la declaracion del argumento no existe");
                    if (!CurrContext.NullFather())
                        CurrContext = CurrContext.GetParent();
                    return false;
                }
                if (CurrContext.GetType(arg.decl.id.Id) == null)
                    CurrContext.SetType(arg.decl.id.Id, All_Types[arg.decl.type.Type]);
                else CurrContext.ChangeType(arg.decl.id.Id, All_Types[arg.decl.type.Type]);
            }
            var visit = node.Statament.Visit(this);

            if (!visit)
            {
                if (!CurrContext.NullFather())
                    CurrContext = CurrContext.GetParent();
                return false;
            }

            SemanticType lca = SemanticType.LCA(All_Types[node.type.Type], node.Statament.MyType);
            if (lca.Name == node.type.Type)
                solve &= true;
            else
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo " + node.Statament.MyType + " no se conforma a " + node.type.Type);
                if (!CurrContext.NullFather())
                    CurrContext = CurrContext.GetParent();
                return false;
            }
            if (!CurrContext.NullFather())
                CurrContext = CurrContext.GetParent();
            return solve;
        }

        public bool Visit(AST_ListMethod node)
        {
            bool visit_result = true;
            foreach (var method_def in node.Methods)
            {
                visit_result &= method_def.Visit(this);
            }
            return visit_result;
        }

        public bool Visit(AST_ClassDef node)
        {
            bool solve = true;
            if (!All_Types.ContainsKey(node.Id.Id))
                return false;

            CurrType = All_Types[node.Id.Id];

            CurrContext = CurrContext.CreateChild();

            SemanticType father = All_Types[node.Id.Id].Father;
            while (true)
            {
                var my_atributes = father.GetAllAttr();
                foreach (var attr in my_atributes)
                {
                    //if (attr.Type.Name == "int")
                    //    attr.Type.Name = "Int";
                    //if (attr.Type.Name == "string")
                    //    attr.Type.Name = "String";
                    //if (attr.Type.Name == "bool")
                    //    attr.Type.Name = "Bool";
                    if (!All_Types.ContainsKey(attr.Type.Name))
                    {
                        CurrErrorLoger.LogError(node.row, node.col, "El tipo de la declaracion del atributo no existe");
                        if (!CurrContext.NullFather())
                            CurrContext = CurrContext.GetParent();
                        return false;
                    }
                    CurrContext.SetType(attr.Id, attr.Type);
                }
                father = father.Father;
                if (father == null)
                    break;
            }

            bool visit_result = node.Property_list.Visit(this) &&
                                node.Method_list.Visit(this);
            if (!CurrContext.NullFather())
                CurrContext = CurrContext.GetParent();

            CurrType = null;

            return visit_result;
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
            bool solve = true;
            
            CurrContext = CurrContext.CreateChild();

            foreach (var arg in node.props.Propertys)
            {
                //if (arg.decl.type.Type == "int")
                //    arg.decl.type.Type = "Int";
                //if (arg.decl.type.Type == "string")
                //    arg.decl.type.Type = "String";
                //if (arg.decl.type.Type == "bool")
                //    arg.decl.type.Type = "Bool";
                if (!All_Types.ContainsKey(arg.decl.type.Type))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo de la declaracion de la variable no existe");
                    if (!CurrContext.NullFather())
                        CurrContext = CurrContext.GetParent();
                    return false;
                }
                if (arg.exp != null)
                {
                    if (!arg.exp.Visit(this))
                    {
                        if (!CurrContext.NullFather())
                            CurrContext = CurrContext.GetParent();
                        return false;
                    }
                    if (SemanticType.LCA(arg.exp.MyType, All_Types[arg.decl.type.Type]).Name != All_Types[arg.decl.type.Type].Name)
                    {
                        CurrErrorLoger.LogError(node.row, node.col, "El tipo de la expresion no es un subtipo del tipo de la declaracion");
                        if (!CurrContext.NullFather())
                            CurrContext = CurrContext.GetParent();
                        return false;
                    }
                }

                
                if (CurrContext.GetTypeInMe(arg.decl.id.Id) == null)
                    CurrContext.SetType(arg.decl.id.Id, All_Types[arg.decl.type.Type]);
                else CurrContext.ChangeType(arg.decl.id.Id, All_Types[arg.decl.type.Type]);
                
            }
            if (!node.expr.Visit(this))
            {
                if (!CurrContext.NullFather())
                    CurrContext = CurrContext.GetParent();
                return false;
            }
            node.MyType = node.expr.MyType;
            if (!CurrContext.NullFather())
                CurrContext = CurrContext.GetParent();
            return true;            
        }

        public bool Visit(AST_Call node)
        {
            bool solve = true;
            if (!CurrType.HasMethod(node.id.Id))
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo " + CurrType.Name + " no posee el metodo " + node.id.Id);
                return false;
            }
            var my_method = CurrType.GetMethod(node.id.Id);

            var invok_params = node.arg.stament_list;

            if (my_method.AttrParams.Count != invok_params.Count)
            {
                CurrErrorLoger.LogError(node.row, node.col, "No coinciden la cantidad de parametros");
                return false;
            }

            for (int i = 0; i < my_method.AttrParams.Count; i++)
            {
                if (!invok_params[i].Visit(this))
                {
                    return false;
                }
                if (SemanticType.LCA(my_method.AttrParams[i].Type, invok_params[i].MyType).Name != my_method.AttrParams[i].Type.Name)
                {
                    CurrErrorLoger.LogError(node.row, node.col, "No coinciden los tipos de los parametros");
                    return false;
                }
            }

            node.MyType = my_method.ReturnType;
            
            return true;
        }

        public bool Visit(AST_ExpCall node)
        {
            bool solve = true;
            bool visit_left = node.expr.Visit(this);
            
            if (node.explicittype == null)
            {
                if (node.expr.MyType == null)
                    return false;

                var my_type = node.expr.MyType;

                if (!my_type.HasMethod(node.id.Id))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo " + my_type.Name + " no posee el metodo " + node.id.Id);
                    return false;
                }
                var my_method = my_type.GetMethod(node.id.Id);

                var invok_params = node.arg.stament_list;

                if (my_method.AttrParams.Count != invok_params.Count)
                {
                    CurrErrorLoger.LogError(node.row, node.col, "La cantidad de parametros no coinciden");
                    return false;
                }

                for (int i = 0; i < my_method.AttrParams.Count; i++)
                {
                    if (!invok_params[i].Visit(this))
                        return false;
                    if (SemanticType.LCA(my_method.AttrParams[i].Type, invok_params[i].MyType).Name != my_method.AttrParams[i].Type.Name)
                    //if (my_method.AttrParams[i].Type.Name != invok_params[i].MyType.Name)
                    {
                        CurrErrorLoger.LogError(node.row, node.col, "No coinciden los tipos de los parametros");
                        return false;
                    }
                }
                node.MyType = my_method.ReturnType;
                return true;
            }
            else
            {
                string cast_type_in_str = node.explicittype.Type;

                if (!All_Types.ContainsKey(cast_type_in_str))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo al que se esta casteando no esta definido");
                    return false;
                }
                var cast_type = All_Types[cast_type_in_str];

                var my_type = node.expr.MyType;

                if (!cast_type.HasMethod(node.id.Id))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo de casteo no posee ese metodo");
                    return false;
                }
                var my_method = cast_type.GetMethod(node.id.Id);

                var invok_params = node.arg.stament_list;

                if (my_method.AttrParams.Count != invok_params.Count)
                {
                    CurrErrorLoger.LogError(node.row, node.col, "La cantidad de parametros no coinciden");
                    return false;
                }

                for (int i = 0; i < my_method.AttrParams.Count; i++)
                {
                    if (!invok_params[i].Visit(this))
                        return false;
                    if(SemanticType.LCA(my_method.AttrParams[i].Type, invok_params[i].MyType).Name != my_method.AttrParams[i].Type.Name)
                    //if (my_method.AttrParams[i].Type.Name != invok_params[i].MyType.Name)
                    {
                        CurrErrorLoger.LogError(node.row, node.col, "No coinciden los tipos de los parametros");
                        return false;
                    }
                }

                var lca = SemanticType.LCA(my_type, cast_type);

                node.MyType = my_method.ReturnType;

                if (lca.Name == cast_type.Name)
                    return true;
                else
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo " + my_type.Name + "no se conforma" + cast_type.Name);
                    return false;
                }
            }
        }

        public bool Visit(AST_Isvoid node)
        {
            node.MyType = All_Types["Bool"];
            return true;
        }

        public bool Visit(AST_New_Type node)
        {
            if(node.type.Type == "SELF_TYPE")
            {
                node.MyType = CurrType;
                return true;
            }
            //if (node.type.Type == "Int" || node.type.Type == "Bool" || node.type.Type == "String")
                if (!All_Types.ContainsKey(node.type.Type))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "El tipo definido no existe");
                    return false;
                }
            if (!All_Types.ContainsKey(node.type.Type))
            {
                CurrErrorLoger.LogError(node.row, node.col, "Tipo no definido");
                return false;
            }
            node.MyType = All_Types[node.type.Type];
            return true;
        }

        public bool Visit(AST_FormalDec node)
        {
            bool solve = true;
            if (CurrContext.GetTypeInMe(node.id.Id) != null)
            {
                CurrErrorLoger.LogError(node.row, node.col, "La variable ya es otro tipo");
                return false;
            }
            //if (node.type.Type == "int")
            //    node.type.Type = "Int";
            //if (node.type.Type == "string")
            //    node.type.Type = "String";
            //if (node.type.Type == "bool")
            //    node.type.Type = "Bool";
            if (!All_Types.ContainsKey(node.type.Type))
            {
                CurrErrorLoger.LogError(node.row, node.col, "El tipo de la variable no existe");
                return false;
            }
            CurrContext.SetType(node.id.Id, All_Types[node.type.Type]);
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
            node.MyType = All_Types["String"];
            return true;
        }

        public bool Visit(AST_CaseOf node)
        {
            if(!node.expr.Visit(this))
                return false;

            var types = new List<SemanticType>();

            foreach (var item in node.props.Propertys)
            {
                CurrContext = CurrContext.CreateChild();

                if (!item.decl.Visit(this))
                {
                    if (!CurrContext.NullFather())
                        CurrContext = CurrContext.GetParent();
                    return false;
                }
                if (!item.exp.Visit(this))
                {
                    if (!CurrContext.NullFather())
                        CurrContext = CurrContext.GetParent();
                    return false;
                }
                CurrContext = CurrContext.GetParent();
                types.Add(item.exp.MyType);
            }


            SemanticType lca = types[0];

            for (int i = 1; i < types.Count; i++)
            {
                lca = SemanticType.LCA(lca, types[i]);
            }

            node.MyType = lca;

            return true;
        }
    }
}
