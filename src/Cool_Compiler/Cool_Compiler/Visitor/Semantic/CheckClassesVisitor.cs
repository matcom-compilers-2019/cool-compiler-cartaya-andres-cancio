using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.Visitor.Semantic.Context;
using Cool_Compiler.AST;
using Cool_Compiler.Visitor.Semantic.Errors;
using Cool_Compiler.Visitor.Semantic;
using Cool_Compiler.Visitor.CIL;

namespace Cool_Compiler.Visitor.Semantic
{
    public class CheckClassesVisitor : IASTVisitor<bool>
    {
        public ErrorLoger CurrErrorLoger;
        public Dictionary<string, SemanticType> All_Types;
        private HashSet<string> hs;

        private bool ACycle(Dictionary<string, List<string>> graph)
        {
            return ACycle("Object", graph);
        }

        private bool ACycle(string node, Dictionary<string, List<string>> graph, string last = "~")
        {
            if (hs.Contains(node))
            {
                CurrErrorLoger.LogError(0, 0, "Dependencia ciclica entre " + node + " y " + last);
                return false;
            }
            hs.Add(node);

            foreach (var ady in graph[node])
            {
                if (!ACycle(ady, graph, node))
                    return false;
            }
            return true;
        }
        public bool Visit(AST_Node node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Asignacion node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_BinaryOp node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Cte node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Id node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_If node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Root node)
        {
            All_Types = SemanticType.BuildAllType(node.class_list);
            CurrErrorLoger = new ErrorLoger();
            hs = new HashSet<string>();
            var graph = Utils.BuildGraph(All_Types);
            if (!ACycle(graph))
            {
                return false;
            }
            return node.class_list.Visit(this);
        }

        public bool Visit(AST_StamentList node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_UnaryOp node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_While node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_ClassProperty node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_ListProp node)
        {
            bool solve = true;

            HashSet<string> all_props = new HashSet<string>();

            foreach (var attr in node.Propertys)
            {
                if (!all_props.Add(attr.decl.id.Id))
                {
                    CurrErrorLoger.LogError(attr.decl.row, attr.decl.col, "El atributo " + attr.decl.id.Id + " ya esta definido en la clase");
                    solve = false;
                }
            }

            return solve;
        }

        public bool Visit(AST_MethodDef node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_ListMethod node)
        {
            bool solve = true;

            HashSet<string> all_methods = new HashSet<string>();

            foreach (var method in node.Methods)
            {
                if (!all_methods.Add(method.Id.Id))
                {
                    solve = false;
                    CurrErrorLoger.LogError(method.Id.row, method.Id.col, "El metodo " + method.Id.Id + " ya esta definido en la clase");
                }
            }

            return solve;
        }

        public bool Visit(AST_ClassDef node)
        {

            if(node.Id.Id == "Main")
            {
                if (!All_Types["Main"].HasMethod("main"))
                {
                    CurrErrorLoger.LogError(node.row, node.col, "Tiene que tener un metodo main");
                    return false;
                }
            }

            bool solve = true;

            solve &= node.Property_list.Visit(this);

            solve &= node.Method_list.Visit(this);

            SemanticType curr = All_Types[node.Id.Id];

            var my_methods = curr.Methods;

            while(curr.Father != null)
            {
                var curr_father = curr.Father;

                if (curr_father.Name == "Bool" || curr_father.Name == "String" || curr_father.Name == "Int")
                {
                    CurrErrorLoger.LogError(node.row, node.col, "No se puede heredar de los tipos built_int");
                    return false;
                }
                var father_method = curr_father.Methods;

                foreach (var mine in my_methods)
                {
                    foreach (var him in father_method)
                    {
                        if(mine.Name == him.Name && mine.Name != "init")
                        {
                            if(mine.AttrParams.Count != him.AttrParams.Count)
                            {
                                CurrErrorLoger.LogError(node.row, node.col, "Para sobresicribir metodos deben tener la misma signatura");
                                solve = false;
                            }
                            else
                            {
                                var x = All_Types[node.Id.Id].GetMethod(mine.Name);
                                var y = curr_father.GetMethod(him.Name);
                                if (x.Name != y.Name || x.ReturnType.Name != y.ReturnType.Name)
                                {
                                    solve = false;
                                    CurrErrorLoger.LogError(node.row, node.col, "Tiene que ser el mismo tipo de retorno para la redefinicion de metodos");
                                }
                                else
                                {
                                    for (int i = 0; i < mine.AttrParams.Count; i++)
                                    {
                                        if (mine.AttrParams[i].Type.Name != him.AttrParams[i].Type.Name)
                                        {
                                            solve = false;
                                            CurrErrorLoger.LogError(node.row, node.col, "Tienen que ser del mismo tipo los parametros para la redefinicion de metodos");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                curr = curr.Father;
            }



            return solve;
        }

        public bool Visit(AST_ListClass node)
        {
            bool main = false;
            foreach (var item in node.Class_list)
            {
                if(item.Id.Id == "Main")
                {
                    main = true;
                    break;
                }
            }

            if (!main)
            {
                CurrErrorLoger.LogError(node.row, node.col, "Tiene que exisitir una clase Main");
                return false;
            }

            HashSet<string> hs = new HashSet<string>();
            bool visit_result = true;
            foreach (var class_def in node.Class_list)
            {
                if (!hs.Add(class_def.Id.Id))
                    visit_result = false;
                visit_result &= class_def.Visit(this);
            }
            return visit_result;
        }

        public bool Visit(AST_Let node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Call node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_ExpCall node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Isvoid node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_New_Type node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_FormalDec node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Type_Node node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_Token node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_StringCte node)
        {
            throw new NotImplementedException();
        }

        public bool Visit(AST_CaseOf node)
        {
            throw new NotImplementedException();
        }
    }
}
