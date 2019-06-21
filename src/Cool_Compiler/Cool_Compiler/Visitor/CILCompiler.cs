using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST;
using Cool_Compiler.AST.CIL;
using Cool_Compiler.Visitor.Semantic;
using Cool_Compiler.Visitor.Code_Generacion.CIL;

namespace Cool_Compiler.Visitor
{
    public class GenerateContinuosLabel
    {
        private string label;
        private int ind;
        public GenerateContinuosLabel(string label, int init = 0)
        {
            this.label = label; ind = init;
        }

        public string Next (string id = null) { return string.Format("{0}{1}", ((id == null)?label:id), ind++);  }
    }

    public class ContentCILMethod
    {
        public ContentCILMethod father;

        public string name;

        public GenerateContinuosLabel gen;

        public Dictionary<string, string> mappedid = new Dictionary<string, string>();

        public ContentCILMethod(string name, ContentCILMethod father = null)
        {
            this.name = name; this.father = father; gen = new GenerateContinuosLabel(name);
        }

        public ContentCILMethod CreateChild(string name)
        {
            return new ContentCILMethod(name, this);
        }

        public string Gen(string n = "")
        {
            return gen.Next(FullName() + "." + n);
        }

        private string FullName()
        {
            if (father == null) return name;
            return father.FullName() + "." + name;
        }

        public string GetId(string id)
        {
            if (mappedid.ContainsKey(id))
                return mappedid[id];
            if (father == null) return null;
            return father.GetId(id);
        }
        public void SetId(string id, string val)
        {
            mappedid.Add(id, val);
        }
    }

    public class ContextMethodCIL
    {
        public List<CIL_AST_Node> Staments = new List<CIL_AST_Node>();
        public List<string> Locals = new List<string>();
        public SemanticType actual_type;
        public AST_ListProp propertys_class;
        public ContentCILMethod actual_content;
        public ContextMethodCIL()
        {
            actual_content = new ContentCILMethod("class");
        }

        public void Reset(string name)
        {
            Locals.Clear();
            Staments.Clear();
            actual_content = new ContentCILMethod(name);
        }

        public string GenLocal(string id, bool exp)
        {
            string s = actual_content.Gen(id);
            if (!exp)
            {
                Locals.Add(s);
                actual_content.SetId(id, s);
            }
            else Locals.Add(s);
            return s;
        }

        public string GetId(string id)
        {
            return actual_content.GetId(id);
        }

        public string GenLabel(string id)
        {
            return actual_content.Gen(id);
        }

        public void AddContext(string name)
        {
            actual_content = actual_content.CreateChild(name);
        }

        public void ClearContext()
        {
            actual_content = actual_content.father;
        }
        public List<string> arg = new List<string>();
    }
    public class CILCompiler : IASTVisitor<string>
    {
        private Dictionary<string, SemanticType> semantictypes = new Dictionary<string, SemanticType>();
        public Dictionary<string, string> data = new Dictionary<string, string>();
        public List<CIL_ClassDef> listtypes = new List<CIL_ClassDef>();
        public GenerateContinuosLabel label_data_gen;
        public ContextMethodCIL methodcontext = new ContextMethodCIL();
        public List<CIL_FunctionDef> methods = new List<CIL_FunctionDef>();
        public static CIL_AST_Root Build(AST_Root root)
        {
            // SectionTypes
            var inst = new CILCompiler();
            inst.semantictypes = SemanticType.BuildAllType(root.class_list);

            // SectionData
            inst.label_data_gen = new GenerateContinuosLabel("str");

            inst.Visit(root.class_list);

            List<CIL_DataElement> listdata = new List<CIL_DataElement>(inst.data.Select(x => new CIL_DataElement(x.Value, x.Key)));
            
            foreach (var typ in inst.semantictypes)
            {
                var t = typ.Value;
                bool isbasic = typ.Key != "SELF_TYPE";
                foreach (var item in inst.listtypes)
                {
                    isbasic &= item.Id != typ.Key;
                }
                if (!isbasic) continue;
                List<CIL_ClassAttr> lattr = new List<CIL_ClassAttr>();
                foreach (var item in t.GetAllAttr())
                {
                    lattr.Add(new CIL_ClassAttr(item.Id));
                }
                List<CIL_ClassMethod> lmeth = new List<CIL_ClassMethod>();
                if (t.Father != null)
                {
                    foreach (var item in t.Father.GetAllMethods())
                    {
                        if (t._find_method(item.Name) == -1)
                            lmeth.Add(new CIL_ClassMethod(item.Name, item.Label()));
                    }
                }
                foreach (var item in t.Methods)
                    lmeth.Add(new CIL_ClassMethod(item.Name, item.Label()));
                inst.listtypes.Add(new CIL_ClassDef(typ.Key, new CIL_ListNode<CIL_ClassAttr>(lattr), new CIL_ListNode<CIL_ClassMethod>(lmeth)));
            }

            // agregar los str types al data
            foreach (var item in inst.listtypes)
            {
                listdata.Add(new CIL_DataElement("type_" + item.Id, item.Id));
            }
            
            listdata.Add(new CIL_DataElement("error_null", "Null Reference Exception"));
            listdata.Add(new CIL_DataElement("error_div0", "Divition By Zero Exception"));
            listdata.Add(new CIL_DataElement("error_indexout", "Index Out Range Exception"));
            
            inst.methods.Add(SystemCallGenerator.GeneratePrint());
            inst.methods.Add(SystemCallGenerator.GeneratePrintInt());
            inst.methods.Add(SystemCallGenerator.Descend());
            inst.methods.Add(SystemCallGenerator.Abort());
            inst.methods.Add(SystemCallGenerator.TypeName());
            inst.methods.Add(SystemCallGenerator.StrLenght());
            inst.methods.Add(SystemCallGenerator.StrConcat());
            inst.methods.Add(SystemCallGenerator.Copy());
            inst.methods.Add(SystemCallGenerator.StrSubstr());
            inst.methods.Add(SystemCallGenerator.GenerateReadInt());

            inst.methods.Add(SystemCallGenerator.Entry());
            return new CIL_AST_Root(new CIL_SectionData(listdata), new CIL_SectionTypes(inst.listtypes), new CIL_SectionFunction(inst.methods));
        }

        public string Visit(AST_ClassDef node)
        {
            SemanticType t = semantictypes[node.Id.Id];
            List<CIL_ClassAttr> lattr = new List<CIL_ClassAttr>();
            foreach (var item in t.GetAllAttr())
            {
                lattr.Add(new CIL_ClassAttr(item.Id));
            }
            List<CIL_ClassMethod> lmeth = new List<CIL_ClassMethod>();
            foreach (var item in t.Father.GetAllMethods())
            {
                if(t._find_method(item.Name) == -1)
                    lmeth.Add(new CIL_ClassMethod(item.Name, item.Label()));
            }
            foreach (var item in t.Methods)
                lmeth.Add(new CIL_ClassMethod(item.Name, item.Label()));

            methodcontext.actual_type = semantictypes[node.Id.Id];
            methodcontext.propertys_class = node.Property_list;
            foreach (var item in node.Method_list.Methods)
            {
                methodcontext.Reset(methodcontext.actual_type.GetMethod(item.Id.Id).Label());
                methodcontext.Staments.Add(new CIL_ExceptionCond("self", "0", "error_null"));
                // poniendo los valores por defecto en el constructor
                if (item.Id.Id == "init" || (methodcontext.actual_type.Name == "Main" && item.Id.Id == "main"))
                {
                    foreach (var prop in methodcontext.propertys_class.Propertys)
                    {
                        if(prop.exp != null)
                        {
                            var s = prop.exp.Visit(this);
                            if(data.ContainsValue(s))
                            {
                                // Load STR
                                var exp2 = methodcontext.GenLocal("exp", true);
                                methodcontext.Staments.Add(new CIL_LoadStr(s, exp2));
                                s = exp2;
                            }
                            methodcontext.Staments.Add(new CIL_SetAttr("self", prop.decl.id.Id, s, methodcontext.actual_type.Name));
                        }
                    }
                }
                Visit(item);
            }

            listtypes.Add(new CIL_ClassDef(node.Id.Id, new CIL_ListNode<CIL_ClassAttr>(lattr), new CIL_ListNode<CIL_ClassMethod>(lmeth)));
            return "";
        }

        public string Visit(AST_ListClass node)
        {
            foreach (var item in node.Class_list)
                Visit(item);
            return "";
        }

        public string Visit(AST_Node node)
        {
            foreach (var item in node.Children)
            {
                item?.Visit(this);
            }
            return "";
        }

        public string Visit(AST_Asignacion node)
        {
            string exp = node.Exp.Visit(this);
            if(data.ContainsValue(exp))
            {
                // Load STR
                var exp2 = methodcontext.GenLocal("exp", true);
                methodcontext.Staments.Add(new CIL_LoadStr(exp, exp2));
                exp = exp2;
                return exp;
            }
            else if (methodcontext.GetId(node.Id) == null && methodcontext.arg.Contains(node.Id))
            {
                // arg
                methodcontext.Staments.Add(new CIL_Asig(node.Id, exp, "var"));
                return exp;
            }
            else if (methodcontext.GetId(node.Id) == null && methodcontext.actual_type.HasAttr(node.Id))
            {
                // SetAttr
                //methodcontext.Staments.Add(new CIL_ExceptionCond("self", "0", "error_null"));
                methodcontext.Staments.Add(new CIL_SetAttr("self", node.Id, exp, methodcontext.actual_type.Name));
                return exp;
            }
            else
            {
                methodcontext.Staments.Add(new CIL_Asig(methodcontext.GetId(node.Id), exp, "var"));
                return exp;
            }
        }

        public string Visit(AST_BinaryOp node)
        {
            string l = node.Left.Visit(this);
            string r = node.Right.Visit(this);
            string s = methodcontext.GenLocal("exp", true);
            if(node.Op.Text == "/") methodcontext.Staments.Add(new CIL_ExceptionCond(r, "0", "error_div0"));
            methodcontext.Staments.Add(new CIL_ExpBin(l, r, node.Op.Text, s));
            return s;
        }

        public string Visit(AST_Cte node)
        {
            if (node.Token.Type == "Int")
            {
                return node.Token.Text;
            }
            else if (node.Token.Type.ToLower() == "true")
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        public string Visit(AST_Id node)
        {
            if(methodcontext.GetId(node.Id) == null && methodcontext.arg.Contains(node.Id))
            {
                return node.Id;
            }
            else if (methodcontext.GetId(node.Id) == null && methodcontext.actual_type.HasAttr(node.Id))
            {
                //methodcontext.Staments.Add(new CIL_ExceptionCond("self", "0", "error_null"));
                var exp = methodcontext.GenLocal("get_attr", true);
                methodcontext.Staments.Add(new CIL_GetAttr("self", node.Id, exp, methodcontext.actual_type.Name));
                return exp;
            }
            else
            {
                return methodcontext.GetId(node.Id);
            }
        }

        public string Visit(AST_If node)
        {
            var x = node.Cond.Visit(this);
            var s = methodcontext.GenLocal("resultif", true);
            var If = methodcontext.GenLabel("if");
            var endif = methodcontext.GenLabel("endif");
            methodcontext.Staments.Add(new CIL_If_Goto(x, If));
            var y = "";
            if (node.ElseCorpus != null)
            {
                y = node.ElseCorpus.Visit(this);
                methodcontext.Staments.Add(new CIL_Asig(s, y, "var"));
                methodcontext.Staments.Add(new CIL_Goto(endif));
            }

            methodcontext.Staments.Add(new CIL_Label(If));

            y = node.IfCorpus.Visit(this);
            methodcontext.Staments.Add(new CIL_Asig(s, y, "var"));

            methodcontext.Staments.Add(new CIL_Label(endif));

            return s;
        }

        public string Visit(AST_Root node)
        {
            foreach (var item in node.Children)
                item?.Visit(this);
            return "";
        }

        public string Visit(AST_StamentList node)
        {
            string s = "";
            foreach (var item in node.Children)
                s = item?.Visit(this);
            return s;
        }

        public string Visit(AST_UnaryOp node)
        {            
            var e = node.Exp.Visit(this);
            var s = methodcontext.GenLocal("exp", true);

            methodcontext.Staments.Add(new CIL_ExpUn(e, node.Op.Text, s));
            return s;
        }

        public string Visit(AST_While node)
        {
            var whil = methodcontext.GenLabel("while");
            var s = methodcontext.GenLocal("resultwhile", true);

            var dowhile = methodcontext.GenLabel("dowhile");
            var endwhile = methodcontext.GenLabel("endwhile");

            methodcontext.Staments.Add(new CIL_Label(whil));
            var x = node.Cond.Visit(this);
            methodcontext.Staments.Add(new CIL_If_Goto(x, dowhile));
            methodcontext.Staments.Add(new CIL_Goto(endwhile));
            methodcontext.Staments.Add(new CIL_Label(dowhile));
                        
            var y = node.WhileCorpus.Visit(this);

            methodcontext.Staments.Add(new CIL_Asig(s, y, "var"));

            methodcontext.Staments.Add(new CIL_Goto(whil));

            methodcontext.Staments.Add(new CIL_Label(endwhile));

            return s;
        }

        public string Visit(AST_ClassProperty node)
        {
            string id = methodcontext.GenLocal(node.decl.id.Id, false);

            if(node.exp != null)
            {
                var s = node.exp.Visit(this);
                methodcontext.Staments.Add(new CIL_Asig(id, s, "var"));
            }

            return "";
        }

        public string Visit(AST_ListProp node)
        {
            foreach (var item in node.Children)
                item?.Visit(this);
            return "";
        }

        public string Visit(AST_MethodDef node)
        {
            var method = methodcontext.actual_type.GetMethod(node.Id.Id);
            var tags = method.Label();

            //List<string> par = new List<string>(method.AttrParams.Select(x => x.Id));
            methodcontext.arg = new List<string>(method.AttrParams.Select(x => x.Id));
            methodcontext.arg.Insert(0, "self");
            
            var solve = node.Statament.Visit(this);
            
            methodcontext.Staments.Add(new CIL_Return(solve, "var"));

            methods.Add(new CIL_FunctionDef(tags, new CIL_Params(methodcontext.arg), 
                            new CIL_Params(new List<string>(methodcontext.Locals)),
                                new CIL_StamentList(methodcontext.Staments)));

            return "";
        }

        public string Visit(AST_ListMethod node)
        {
            foreach (var item in node.Methods)
            {
                methodcontext.Reset(item.Id.Id);
                item?.Visit(this);
            }
            return "";
        }

        public string Visit(AST_Let node)
        {
            List<string> exprresult = new List<string>();
            foreach (var item in node.props.Propertys)
            {

                if (item.exp != null)
                {
                    var s1 = item.exp.Visit(this);
                    exprresult.Add(s1);
                }
                else exprresult.Add(null);
            }

            methodcontext.AddContext("Let");

            for (int i = 0; i < node.props.Propertys.Count; i++)
            {
                string id = methodcontext.GenLocal(node.props.Propertys[i].decl.id.Id, false);
                if (exprresult[i] != null)
                    methodcontext.Staments.Add(new CIL_Asig(id, exprresult[i], "var"));
            }

            var s = methodcontext.GenLocal("exp", true);

            var x = node.expr.Visit(this);

            methodcontext.Staments.Add(new CIL_Asig(s, x, "var"));

            methodcontext.ClearContext();

            return s;
        }

        public string Visit(AST_Call node)
        {
            var typ = methodcontext.actual_type;

            //methodcontext.Staments.Add(new CIL_ExceptionCond("self", "0", "error_null"));
            var s = methodcontext.GenLocal("exp", true);

            List<string> parm = new List<string>();
            parm.Add("self");
            foreach (var item in node.Args.stament_list)
            {
                parm.Add(item.Visit(this));
            }
            methodcontext.Staments.Add(new CIL_FunctionCall(node.ID.Id, new CIL_Params(parm), typ.Name, s, false, typ.Name));
            return s;

        }

        public string Visit(AST_ExpCall node)
        {
            
            var s = methodcontext.GenLocal("exp", true);

            var e0 = node.expr.Visit(this);

            methodcontext.Staments.Add(new CIL_ExceptionCond(e0, "0", "error_null"));

            var typ = "";

            List<string> parm = new List<string>();
            parm.Add(e0);
            foreach (var item in node.arg.stament_list)
            {
                parm.Add(item.Visit(this));
            }

            if(node.explicittype != null)
            {
                typ = node.explicittype.Type;
                methodcontext.Staments.Add(new CIL_FunctionCall(node.id.Id, new CIL_Params(parm), typ, s, false, typ));
            }
            else
            {
                typ = methodcontext.GenLocal("get_typ", true);
                methodcontext.Staments.Add(new CIL_TypeOf(e0, typ));
                methodcontext.Staments.Add(new CIL_FunctionCall(node.id.Id, new CIL_Params(parm), typ, s, true, (node.expr.MyType == null) ? "NULL" : node.expr.MyType.Name ));
            }

            return s;
        }

        public string Visit(AST_Isvoid node)
        {
            var s = methodcontext.GenLocal("exp", true);

            var exp0 = node.expr.Visit(this);

            methodcontext.Staments.Add(new CIL_IsVoid(exp0, s));

            return s;
        }

        public string Visit(AST_New_Type node)
        {
            var s = methodcontext.GenLocal("exp", true);
            if (node.type.Type == "SELF_TYPE")
            {
                var s2 = methodcontext.GenLocal("get_type", true);
                methodcontext.Staments.Add(new CIL_TypeOf("self", s2));
                methodcontext.Staments.Add(new CIL_Allocate(s2, s, true));
            }
            else
            {
                methodcontext.Staments.Add(new CIL_Allocate(node.type.Type, s, false));
            }
            return s;
        }

        public string Visit(AST_FormalDec node)
        {
            foreach (var item in node.Children)
                item?.Visit(this);
            return "";
        }

        public string Visit(AST_Type_Node node)
        {
            foreach (var item in node.Children)
                item?.Visit(this);
            return "";
        }

        public string Visit(AST_Token node)
        {
            return "";
        }

        public string Visit(AST_StringCte node)
        {
            if (!data.ContainsKey(node.Token))
                data.Add(node.Token, label_data_gen.Next());

            var s = methodcontext.GenLocal("exp", true);

            methodcontext.Staments.Add(new CIL_LoadStr(data[node.Token], s));
            return s;
        }

        public string Visit(AST_CaseOf node)
        {
            var s = node.expr.Visit(this);
            var solve = methodcontext.GenLocal("exp", true);
            var typexpr = methodcontext.GenLocal("get_type", true);
            var besti = methodcontext.GenLocal("best_type", true);
            
            var cmp = methodcontext.GenLocal("comp", true);
            methodcontext.Staments.Add(new CIL_TypeOf(s, typexpr));

            var endlab = methodcontext.GenLabel("end");

            methodcontext.Staments.Add(new CIL_ExceptionCond(typexpr, "0", "error_null"));

            methodcontext.Staments.Add(new CIL_Asig(besti, "10000", "Int"));

            List<string> typeval = new List<string>();
            foreach (var item in node.props.Propertys)
            {
                typeval.Add(methodcontext.GenLocal("type", true));
            }

            for (int i = 0; i < typeval.Count; i++)
            {
                methodcontext.Staments.Add(new CIL_TypeOf(node.props.Propertys[i].decl.type.Type, typeval[i]));
                methodcontext.Staments.Add(new CIL_FunctionCall("descend", new CIL_Params(new List<string> { typeval[i], typexpr }), null, typeval[i], false, null));
                methodcontext.Staments.Add(new CIL_ExpBin(besti, typeval[i], "<", cmp));
                var iff = methodcontext.GenLabel("if");
                methodcontext.Staments.Add(new CIL_If_Goto(cmp, iff));
                methodcontext.Staments.Add(new CIL_Asig(besti, typeval[i], "var"));
                methodcontext.Staments.Add(new CIL_Label(iff));
            }

            List<string> labelsok = new List<string>();
            for (int i = 0; i < typeval.Count; i++)
                labelsok.Add(methodcontext.GenLabel("ok"));

            methodcontext.Staments.Add(new CIL_ExceptionCond(besti, "10000", "error_null"));

            for (int i = 0; i < typeval.Count; i++)
            {
                methodcontext.Staments.Add(new CIL_ExpBin(besti, typeval[i], "=", cmp));
                methodcontext.Staments.Add(new CIL_If_Goto(cmp, labelsok[i]));
            }

            for (int i = 0; i < typeval.Count; i++)
            {
                methodcontext.Staments.Add(new CIL_Label(labelsok[i]));

                methodcontext.AddContext("case");

                
                string id = methodcontext.GenLocal(node.props.Propertys[i].decl.id.Id, false);
                methodcontext.Staments.Add(new CIL_Asig(id, s, "var"));
                string idexp = node.props.Propertys[i].exp.Visit(this);
                methodcontext.Staments.Add(new CIL_Asig(solve, idexp, "var"));

                methodcontext.ClearContext();
                

                methodcontext.Staments.Add(new CIL_Goto(endlab));
            }

            methodcontext.Staments.Add(new CIL_Label(endlab));


            return solve;
        }
    }
}
