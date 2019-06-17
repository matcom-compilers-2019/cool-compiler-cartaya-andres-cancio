using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST.CIL;
using Cool_Compiler.Visitor.Semantic;

namespace Cool_Compiler.Visitor.CIL
{
    public interface ICIL_Memory
    {
        int GetValue(int idx);

        int SetValue(int idx, int value);

        int Push(int value);

        void Pop();

        int Allocate(int t);

        void ClearStack(int k);

        int GetDirType(string id);

        int GetDirMethod(string id);

        int SetDirType(string id, int value);

        int SetDirmethod(string id, int value);

        int GetRegisterValue(string reg);

        int SetRegisterValue(string reg, int val);

        int GetESP();
    }
    public class CIL_Memory : ICIL_Memory
    {
        public const int MAX = 100000;
        public int[] RAM = new int[MAX];
        public int esp = MAX - 1;
        public int len = 1;

        public Dictionary<string, int> Register = new Dictionary<string, int>();

        public Dictionary<string, int> funcref = new Dictionary<string, int>();

        public Dictionary<string, int> typeref = new Dictionary<string, int>();

        public CIL_Memory()
        {
            Register.Add("eax", 0);
        }
        public int GetValue(int idx)
        {
            return RAM[idx];
        }

        public int SetValue(int idx, int value)
        {
            return RAM[idx] = value;
        }

        
        public int Push(int value) { RAM[esp] = value; return esp--; }

        public void Pop() => ClearStack(1);

        public int Allocate(int t)
        {
            int s = len;
            len += t;
            return s;
        }
            
        public void ClearStack(int k)
        {
            esp += k;
        }

        public int GetDirType(string id) => typeref[id];

        public int GetDirMethod(string id) => funcref[id];

        public int SetDirType(string id, int value)
        {
            if (!typeref.ContainsKey(id)) typeref.Add(id, value);
            return typeref[id] = value;
        }

        public int SetDirmethod(string id, int value)
        {
            if (!funcref.ContainsKey(id)) funcref.Add(id, value);
            return funcref[id] = value;
        }

        public int GetRegisterValue(string reg)
        {
            return Register[reg];
        }

        public int SetRegisterValue(string reg, int val)
        {
            return Register[reg] = val;
        }

        public int GetESP()
        {
            return esp;
        }
    }

    class Utils
    {
        public static void BuildGraphDFS(SemanticType t, Dictionary<string, List<string>> graph)
        {
            if (t.Father == null) return;
            BuildGraphDFS(t.Father, graph);
            if(!graph[t.Father.Name].Contains(t.Name))
                graph[t.Father.Name].Add(t.Name);
        }
        public static Dictionary<string, List<string>> BuildGraph(Dictionary<string, SemanticType> types)
        {
            Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();
            foreach (var item in types)
                graph.Add(item.Key, new List<string>());

            foreach (var item in types)
            {
                BuildGraphDFS(item.Value, graph);
            }

            return graph;
        }

        public static void SetMemory(ICIL_Memory mem, Dictionary<string, SemanticType> types, CIL_SectionTypes secttype)
        {
            Queue<string> Zeros = new Queue<string>();
            var graph = BuildGraph(types);
            Dictionary<string, int> ind = new Dictionary<string, int>();
            foreach (var item in graph)
                ind.Add(item.Key, 0);
            foreach (var item in graph)                            
                foreach (var w in item.Value)                
                    ind[w]++;
            foreach (var item in ind)
                if (item.Value == 0)
                    Zeros.Enqueue(item.Key);

            // Section Type
            Dictionary<string, CIL_ClassDef> mapped = new Dictionary<string, CIL_ClassDef>();
            foreach (var item in secttype.ListNode)
                mapped.Add(item.Id, item);           

            while(Zeros.Count > 0)
            {
                string k = Zeros.Dequeue();

                foreach (var item in graph[k])
                {
                    ind[item]--;
                    if (ind[item] == 0) Zeros.Enqueue(item);
                }

                if (!mapped.ContainsKey(k)) continue;

                var classdef = mapped[k];

                int idx = mem.Allocate(classdef.Methods.ListNode.Count + 2);

                mem.SetDirType(k, idx);

                mem.SetValue(idx++, classdef.Methods.ListNode.Count + 1);

                var sem = types[k];

                if(sem.Father != null)
                    mem.SetValue(idx, mem.GetDirType(sem.Father.Name));

                idx++;

                foreach (var item in classdef.Methods.ListNode)
                    mem.SetDirmethod(item.Idres, idx++);
            }
        }
        public static string SetMemoryInCIL(Dictionary<string, SemanticType> types, CIL_SectionTypes secttype)
        {
            Queue<string> Zeros = new Queue<string>();
            var graph = BuildGraph(types);
            Dictionary<string, int> ind = new Dictionary<string, int>();
            foreach (var item in graph)
                ind.Add(item.Key, 0);
            foreach (var item in graph)
                foreach (var w in item.Value)
                    ind[w]++;
            foreach (var item in ind)
                if (item.Value == 0)
                    Zeros.Enqueue(item.Key);

            // Section Type
            Dictionary<string, CIL_ClassDef> mapped = new Dictionary<string, CIL_ClassDef>();
            foreach (var item in secttype.ListNode)
                mapped.Add(item.Id, item);

            string solve = "";

            while (Zeros.Count > 0)
            {
                string k = Zeros.Dequeue();

                foreach (var item in graph[k])
                {
                    ind[item]--;
                    if (ind[item] == 0) Zeros.Enqueue(item);
                }

                if (!mapped.ContainsKey(k)) continue;

                var classdef = mapped[k];
                var semt = types[k];

                solve += string.Format("\t {0}: .word ", k);

                // Len Del Type
                solve += (classdef.Methods.ListNode.Count + 4);

                // Len del Objeto
                solve += ", " + (4*(classdef.Attrs.ListNode.Count + 2));

                // Father
                solve += ", " + ((semt.Father == null) ? "0" : semt.Father.Name);

                // Type Name
                solve += string.Format(", type_{0}", k);

                for (int i = 0; i < classdef.Methods.ListNode.Count; i++)
                {
                    var s = classdef.Methods.ListNode[i];
                    solve += ", " + s.Idres;
                }
                solve += "\n";               
                
            }

            return solve;
        }
    }

    public interface IIO
    {
        void print(string x);
        void print(int x);

        int read_int();

        string read_string();

        string get_all_text();
    }

    public class BasicIO : IIO
    {
        string solve = "";
        public string get_all_text()
        {
            return solve;
        }

        public void print(string x)
        {
            //Console.WriteLine(x);
            solve += x;
        }

        public void print(int x)
        {
            throw new NotImplementedException();
        }

        public int read_int()
        {
            throw new NotImplementedException();
        }

        public string read_string()
        {
            throw new NotImplementedException();
        }
    }

    public class CIL_Execute : IVisitorCIL<string>
    {
        public static string Execute(CIL_AST_Root root, Dictionary<string, SemanticType> semtype)
        {
            return new CIL_Execute(new CIL_Memory(), semtype).Visit(root);
        }

        public ICIL_Memory mem;
        public Dictionary<string, SemanticType> semtype;
        public IIO io = new BasicIO();
        public int PC = 0;

        public Dictionary<string, int> resolvaddr = new Dictionary<string, int>();
        List<CIL_AST_Node> allnode = new List<CIL_AST_Node>();

        public Stack<Dictionary<string, int>> variableaddr = new Stack<Dictionary<string, int>>();

        public Dictionary<string, int> dataaddr = new Dictionary<string, int>();

        public Dictionary<string, Dictionary<string, int>> OffSetAttr = new Dictionary<string, Dictionary<string, int>>();

        public Dictionary<string, Dictionary<string, int>> OffSetMethod = new Dictionary<string, Dictionary<string, int>>();

        public string GetTypeFromDir(int pos)
        {
            foreach (var item in semtype)
            {
                if (item.Key == "SELF_TYPE") continue;
                int idx = mem.GetDirType(item.Key);
                if (pos == idx) return item.Key;
            }
            throw new Exception($"La posicion {pos} no pertenece a ningun tipo");
        }

        public void PrintObject(int idx)
        {
            int len = mem.GetValue(idx);
            idx++;
            int typeint = mem.GetValue(idx++);
            Console.Write($"[{len}]");
            if (typeint != 0)
            {
                Console.Write($"<{GetTypeFromDir(typeint)}> ");
            }
            else
            {
                Console.Write("<void> ");
            }
            while (--len > 0)
            {
                Console.Write("{0} ", mem.GetValue(idx++));
            }
            Console.WriteLine();
        }

        public CIL_Execute(ICIL_Memory mem, Dictionary<string, SemanticType> semtype)
        {
            this.semtype = semtype;
            this.mem = mem;
        }
        public string Visit(CIL_AST_Node node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_AST_Root node)
        {
            Utils.SetMemory(mem, semtype, node.Types);
            allnode = new List<CIL_AST_Node>(CIL_AST_Node.GetAllNode(node));
            int main = -1;
            for (int i = 0; i < allnode.Count; i++)
            {
                if (allnode[i] is CIL_Label) {
                    resolvaddr[((CIL_Label)allnode[i]).Label] = i;
                }
                else if (allnode[i] is CIL_FunctionDef)
                {
                    resolvaddr[((CIL_FunctionDef)allnode[i]).Name] = i;
                    if (main == -1) main = i;
                    //mem.SetValue(mem.GetDirMethod(((CIL_ClassMethod)allnode[i]).Idres), i);
                }
            }

            foreach (var item in semtype)
            {
                if (item.Key == "SELF_TYPE") continue;
                int idx = mem.GetDirType(item.Key);
                idx += 2;
                foreach (var item2 in item.Value.GetAllMethods())
                {
                    if (resolvaddr.ContainsKey(item2.Label()))
                        mem.SetValue(idx++, resolvaddr[item2.Label()]);
                    else
                    {
                        Console.WriteLine($"Method {item2.Label()} not found in CIL");
                        idx++;
                    }
                }
                //Console.Write($"{item.Key} :");
                //PrintObject(mem.GetDirType(item.Key));
            }

           

            // Build DataElement
            node.Data.Visit(this);

            // Debug DataElement
            foreach (var d in dataaddr)
            {
                //PrintObject(d.Value);
            }

            // Build OffSetAttr
            node.Types.Visit(this);

            // Init Simulator
            variableaddr.Push(new Dictionary<string, int>());

            allnode[resolvaddr["entry"]].Visit(this);

            return io.get_all_text();
        }

        public string Visit(CIL_SectionTypes node)
        {
            foreach (var item in node.ListNode)
            {
                item.Visit(this);
            }
            return "";
        }

        public string Visit(CIL_ClassDef node)
        {
            OffSetAttr.Add(node.Id, new Dictionary<string, int>());
            OffSetMethod.Add(node.Id, new Dictionary<string, int>());
            for (int i = 0; i < node.Attrs.ListNode.Count; i++)
            {
                OffSetAttr[node.Id].Add(node.Attrs.ListNode[i].Id, i);
            }
            for (int i = 0; i < node.Methods.ListNode.Count; i++)
            {
                OffSetMethod[node.Id].Add(node.Methods.ListNode[i].Id, i);
            }

            return "";
        }

        public string Visit(CIL_ClassAttr node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_ClassMethod node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_SectionData node)
        {
            foreach (var item in node.ListNode)
            {
                item.Visit(this);
            }
            return "";
        }

        public string Visit(CIL_DataElement node)
        {
            int idx = mem.Allocate(node.Data.Length + 1 + 2);

            dataaddr.Add(node.Id, idx);

            mem.SetValue(idx++, node.Data.Length + 2);

            mem.SetValue(idx++, mem.GetDirType("String"));

            foreach (var item in node.Data)
                mem.SetValue(idx++, item);

            return "";
        }

        public int GetValueFromId(string x)
        {
            // verify if result its a cte
            int s;
            if (!int.TryParse(x, out s))
                s = mem.GetValue(variableaddr.Peek()[x]);
            return s;
        }

        public string Visit(CIL_Label node)
        {
            return "";
        }

        public string Visit(CIL_Goto node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_If_Goto node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_ExpBin node)
        {
            int value1 = GetValueFromId(node.Left);
            int value2 = GetValueFromId(node.Right);
            int solve = 0;
            if (node.Op == "+") solve = value1 + value2;
            if (node.Op == "-") solve = value1 - value2;
            if (node.Op == "*") solve = value1 * value2;
            if (node.Op == "/") solve = value1 / value2;
            if (node.Op == "<") solve = (value1 < value2) ? 1 : 0;
            if (node.Op == "<=") solve = (value1 <= value2) ? 1 : 0;
            if (node.Op == "=") solve = (value1 == value2) ? 1 : 0;
            int dir = variableaddr.Peek()[node.Result];
            mem.SetValue(dir, solve);
            return "";
        }

        public string Visit(CIL_ExpUn node)
        {
            int value = GetValueFromId(node.Expr);
            if(node.Op == "~") value = ~value;
            if (node.Op == "not") value = (value != 0) ? 0 : 1;
            int dir = variableaddr.Peek()[node.Result];
            mem.SetValue(dir, value);
            return "";
        }

        public string Visit(CIL_Asig node)
        {
            int value = GetValueFromId(node.Expr);
            int dir = variableaddr.Peek()[node.Id];
            mem.SetValue(dir, value);
            return "";
        }

        public string Visit(CIL_Locals node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_Params node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_StamentList node)
        {
            foreach (var item in node.ListNode)
            {
                item.Visit(this);
            }
            return "";
        }

        public string Visit(CIL_GetId node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_Constant node)
        { 
            throw new NotImplementedException();
        }

        public string Visit(CIL_FunctionCall node)
        {
            if(node.TypeCall != null)
            {
                foreach (var item in node.Params.Params)
                {
                    //int pos = variableaddr.Peek()[item];
                    mem.Push(GetValueFromId(item));
                }
                int offset = OffSetMethod[node.TypeCall][node.Name];
                int dir = mem.GetDirType(node.TypeCall);
                dir += 2 + offset;
                int reff = mem.GetValue(dir);
                allnode[reff].Visit(this);

                dir = variableaddr.Peek()[node.Result];
                mem.SetValue(dir, mem.GetRegisterValue("eax"));
            }
            return "";
            
        }

        public string Visit(CIL_FunctionDef node)
        {
            Dictionary<string, int> PosMem = new Dictionary<string, int>();
            int esp = mem.GetESP();
            esp += node.Params.Params.Count;
            for (int i = 0; i < node.Params.Params.Count; i++)
                PosMem.Add(node.Params.Params[i], esp--);
            foreach (var item in node.Locals.Params)
            {
                PosMem.Add(item, mem.Push(0));
            }
            Dictionary<string, int> LabelsDir = new Dictionary<string, int>();
            for(int i = 0; i < node.Corpus.ListNode.Count; i++)
            {
                if(node.Corpus.ListNode[i] is CIL_Label)
                {
                    LabelsDir.Add(((CIL_Label)node.Corpus.ListNode[i]).Label, i+1);
                }
            }
            variableaddr.Push(PosMem);
            int idx = 0;
            while ( true )
            {
                var st = node.Corpus.ListNode[idx];
                if(st is CIL_Goto)
                {
                    idx = LabelsDir[((CIL_Goto)st).Label];
                }
                else if (st is CIL_If_Goto)
                {
                    var n = (CIL_If_Goto)st;
                    var idx1 = LabelsDir[((CIL_If_Goto)st).Label];
                    int v = GetValueFromId(n.Condition);
                    if (v != 0) idx = idx1;
                    else idx++;
                }
                else if( st is CIL_Return)
                {
                    st.Visit(this);
                    break;
                }
                else
                {
                    st.Visit(this);
                    idx++;
                }
            }
            variableaddr.Pop();
            return "";
        }

        public string Visit(CIL_SectionFunction node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_SetAttr node)
        {
            int idx = mem.GetValue(variableaddr.Peek()[node.Instanc]);
            idx++;
            int typedir = mem.GetValue(idx++);
            string type = GetTypeFromDir(typedir);
            
            idx += OffSetAttr[type][node.Attr];

            // verify if result its a cte
            int s;
            if (!int.TryParse(node.Result, out s))
                s = mem.GetValue(variableaddr.Peek()[node.Result]);
            

            mem.SetValue(idx, s);

            //PrintObject(idx);
            return "";
        }

        public string Visit(CIL_GetAttr node)
        {
            int idx = mem.GetValue(variableaddr.Peek()[node.Instanc]);
            idx++;
            int typedir = mem.GetValue(idx++);
            string type = GetTypeFromDir(typedir);

            idx += OffSetAttr[type][node.Attr];

            int s = mem.GetValue(idx);

            int valorreturn = variableaddr.Peek()[node.Result];
            mem.SetValue(valorreturn, s);

            return "";
        }

        public string Visit(CIL_Allocate node)
        {
            int idx = mem.Allocate(OffSetAttr[node.Type].Count + 3);
            int idxsalva = idx;
            mem.SetValue(idx, OffSetAttr[node.Type].Count + 1);
            mem.SetValue(++idx, mem.GetDirType(node.Type));

            mem.SetValue(variableaddr.Peek()[node.Result], idx - 1);
            //PrintObject(idxsalva);
            return "";
        }

        public string Visit(CIL_Return node)
        {
            mem.SetRegisterValue("eax", GetValueFromId(node.Result));
            return "";
        }

        public string Visit(CIL_LoadStr node)
        {
            int idx = dataaddr[node.StrId];
            int size = mem.GetValue(idx);
            int all = mem.Allocate(size + 1);
            for (int i = 0; i < size + 1; i++)
            {
                mem.SetValue(all + i, mem.GetValue(idx + i));
            }
            mem.SetValue(variableaddr.Peek()[node.Result], all);
            //(new CIL_IOPrint(node.Result)).Visit(this);
            return "";

        }

        public string Visit(CIL_StrStr node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_LengthStr node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_IORead node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_IOPrint node)
        {
            string solve = "";
            int idx = variableaddr.Peek()[node.Obj];
            idx = mem.GetValue(idx);
            PrintObject(idx);
            int len = mem.GetValue(idx++);
            idx++;
            for(int i = 0; i < len - 2; i++) 
            {
                int x = mem.GetValue(idx + i);
                char c = (char)x;
                if (c == '\\')
                {
                    x = mem.GetValue(idx + ++i);
                    c = (char)x;
                    if (c == 'n') solve += '\n';
                    if (c == 'b') solve += '\b';
                    if (c == 't') solve += '\t';
                    if (c == 'r') solve += '\r';

                }
                else solve += c;
            }
            io.print(solve);
            return "";
        }

        public string Visit(CIL_TypeOf node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_ExceptionCond node)
        {
            return "";
        }

        public string Visit(CIL_IOPrintInt node)
        {
            int s = GetValueFromId(node.Obj);
            io.print(s.ToString());
            return "";
        }

        public string Visit(CIL_IOReadInt node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_TypeName node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_FatherType node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_StrConcat node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_Abort node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_Copy node)
        {
            throw new NotImplementedException();
        }
    }
}
