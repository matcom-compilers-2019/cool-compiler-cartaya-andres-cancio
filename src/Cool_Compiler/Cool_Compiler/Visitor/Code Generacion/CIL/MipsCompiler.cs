using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.AST.CIL;
using Cool_Compiler.Visitor.Semantic;

namespace Cool_Compiler.Visitor.CIL
{
    public class MipsCompiler : IVisitorCIL<string>
    {
        public Dictionary<string, string> datareference = new Dictionary<string, string>();
        public Dictionary<string, SemanticType> Semtype;
        public Dictionary<string, Dictionary<string, int>> offset_method = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> offset_attrs = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, string> context_function = new Dictionary<string, string>();
        public int ReservLocalArgs = 0;

        public MipsCompiler(CIL_AST_Node root, Dictionary<string, SemanticType> semtype)
        {
            Semtype = semtype;
        }
        public string Visit(CIL_AST_Node node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_AST_Root node)
        {
            string solve = ".data\n";
            // data
            var data = node.Data.Visit(this);
            
            // set types
            var types = Utils.SetMemoryInCIL(Semtype, node.Types);

            // offset_types
            node.Types.Visit(this);
            

            string code = ".text\n\n"+ node.Functions.Visit(this) + @"
        li $v0, 10
        syscall

        exception:
            li $v0, 4
            syscall
            li $v0, 10
            syscall
";
            return solve + data + types +code;
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
            offset_attrs.Add(node.Id, new Dictionary<string, int>());
            int idx = 0;
            foreach (var item in node.Attrs.ListNode)
            {
                offset_attrs[node.Id].Add(item.Id, 4*(idx++));
            }
            offset_method.Add(node.Id, new Dictionary<string, int>());
            idx = 0;
            foreach (var item in node.Methods.ListNode)
            {
                offset_method[node.Id].Add(item.Id, 4 * (idx++));
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
            string solve = "";
            foreach (var item in node.ListNode)
            {
                solve += item.Visit(this) + "\n";
            }
            return solve;
        }

        public string Visit(CIL_DataElement node)
        {
            datareference.Add(node.Id,node.Data);
            return string.Format("\t {0}: .asciiz \"{1}\"", node.Id, node.Data);
        }

        public string Visit(CIL_Label node)
        {
            return $"\t {node.Label}:\n";
        }

        public string Visit(CIL_Goto node)
        {
            return $"\t j {node.Label}\n";
        }

        public string Visit(CIL_If_Goto node)
        {
            string solve = "";
            int val;
            if (int.TryParse(node.Condition, out val))
            {
                solve += $"\t li $t0, {val} \n";
            }
            else
                solve += $"\t lw $t0, { context_function[node.Condition] } \n";
            solve += $"\t bne $t0, $zero, {node.Label}\n";
            return solve;
        }

        public string Visit(CIL_ExpBin node)
        {
            string solve = "";

            int val;
            if (int.TryParse(node.Left, out val))
            {
                solve += $"\t li $t0, {val}\n";
            }
            else
            {
                solve += $"\t lw $t0, {context_function[node.Left]}\n";
            }

            if (int.TryParse(node.Right, out val))
            {
                solve += $"\t li $t1, {val}\n";
            }
            else
            {
                solve += $"\t lw $t1, {context_function[node.Right]}\n";
            }

            if (node.Op == "+") solve += $"\t add $t2, $t0, $t1\n";
            else if (node.Op == "-") solve += $"\t sub $t2, $t0, $t1\n";
            else if (node.Op == "/") solve += $"\t div $t2, $t0, $t1\n";
            else if (node.Op == "*") solve += $"\t mul $t2, $t0, $t1\n";
            else if (node.Op == "=") solve += $"\t seq $t2, $t0, $t1\n";
            else if (node.Op == "<") solve += $"\t sgt $t2, $t1, $t0\n";
            else if (node.Op == "<=") solve += $"\t sge $t2, $t1, $t0\n";

            else throw new Exception($"{node.Op} Not Found Operator");
            solve += $"\t sw $t2 {context_function[node.Result]}\n";




                return solve;
        }

        public string Visit(CIL_ExpUn node)
        {
            string solve = "";

            int val;
            if (int.TryParse(node.Expr, out val))
            {
                solve += $"\t li $t0, {val}\n";
            }
            if(node.Op == "~")
            {
                solve += $"\t neg $t1, $t0\n";
            }
            else if(node.Op.ToLower() == "not") solve += $"\t not $t1, $t0\n";
            else throw new Exception($"{node.Op} Not Found Operator");
            solve += $"\t sw $t1 {context_function[node.Result]}\n";
            return solve;
        }

        public string Visit(CIL_Asig node)
        {
            string solve = "";
            int val;
            if (int.TryParse(node.Expr, out val))
            {
                solve += $"\t li $t0, {val}\n";
                solve += $"\t sw $t0, {context_function[node.Id]}\n";
            }
            else
            {

                solve += $"\t lw $t0, {context_function[node.Expr]}\n";
                solve += $"\t sw $t0, {context_function[node.Id]}\n";
            }
            return solve;
        }

        public string Visit(CIL_Locals node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_TypeOf node)
        {
            string solve = "";
            if(Semtype.ContainsKey(node.Id))
            {
                solve += $"\t la $t0 {node.Id}\n";
                solve += $"\t sw $t0 {context_function[node.Result]}\n";
            }
            else
            {
                solve += $"\t lw $t0 {context_function[node.Id]}\n";
                solve += $"\t lw $t1 4($t0)\n";
                solve += $"\t sw $t1 {context_function[node.Result]}\n";
            }
            return solve;
        }

        public string Visit(CIL_Params node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_StamentList node)
        {
            throw new NotImplementedException();
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
            if (true)
            {
                //if(node.Is_Type_Dir && ! Semtype.ContainsKey(node.Static_Type))
                //{
                //    throw new Exception("Algo se esta partiendo aqui con el vcall");
                //}
                string solve = "";
                int Reserv = 4 * (node.Params.Params.Count + 2);
                // reserv memory to pass the params
                solve += $"\t addiu $sp, $sp, -{Reserv}\n";

                solve += $"\t sw $ra, {Reserv - 4}($sp)\n";

                solve += $"\t sw $fp, {Reserv - 8}($sp)\n";

                // Put The Argument in Memory
                int idx = 0; 
                foreach (var item in node.Params.Params)
                {
                    int val;
                    if(int.TryParse(item, out val))
                    {
                        solve += $"\t li $t0, {val}\n";
                        solve += $"\t sw $t0, {4 * idx}($sp)\n";
                    }
                    else
                    {
                        string result = context_function[item];
                        string[] spl = result.Split('(');
                        string newp = "" + (int.Parse(spl[0]) + Reserv) + result.Substring(spl[0].Length);
                        if (result.Contains("sp")) solve += $"\t lw $t0, {newp}\n";
                        else solve += $"\t lw $t0, {result}\n";
                        solve += $"\t sw $t0, {4 * idx}($sp)\n";
                    }
                    idx++;
                }

                // set fp;
                solve += $"\t move $fp, $sp\n";

                if (node.Static_Type != null)
                {
                    var addr = offset_method[node.Static_Type][node.Name];

                    // find addr type
                    solve += $"\t la $t0, {node.Static_Type}\n";

                    // t1 = addr method
                    solve += $"\t lw $t1, {addr + 16}($t0)\n";

                    solve += $"\t jal $t1\n";

                }
                else
                {
                    solve += $"\t jal {node.Name}\n";
                }
                
                
                solve += $"\t lw $ra, {Reserv - 4}($sp)\n";

                solve += $"\t lw $fp, {Reserv - 8}($sp)\n";

                // release memory
                solve += $"\t addiu $sp, $sp, {Reserv}\n";

                solve += $"\t sw $v0 {context_function[node.Result]}\n";

                return solve;
            }
            //else throw new Exception("Chama no se esta haciendo el polimorfismo");
        }

        public string Visit(CIL_FunctionDef node)
        {
            string solve = "";

            context_function = new Dictionary<string, string>();
            ReservLocalArgs = 4 * node.Locals.Params.Count;
            int idx = 0;
            foreach (var item in node.Params.Params)
            {
                context_function.Add(item, $"{4 * (idx++)}($fp)");
            }

            idx = 0;
            foreach (var item in node.Locals.Params)
            {
                context_function.Add(item, $"{4 * (idx++)}($sp)");
            }
            solve += $"\t {node.Name}:\n";
            // reserv global variable
            solve += $"\t addiu $sp, $sp, -{ ReservLocalArgs }\n";


            foreach (var item in node.Corpus.ListNode)
            {
                var subsolve = item?.Visit(this);
                if (subsolve != null) solve += subsolve;
            }
            
            return solve;
        }

        public string Visit(CIL_SectionFunction node)
        {
            string solve = "";

            foreach (var item in node.ListNode)
            {
                solve += item.Visit(this) + "\n\n";
            }

            return solve;
        }

        public string Visit(CIL_SetAttr node)
        {
            string solve = "";
            int val;
            if(int.TryParse(node.Result, out val))
            {
                solve += $"\t li $t0, {val}\n";
                solve += $"\t lw $t1, {context_function[node.Instanc]}\n";
                solve += $"\t sw $t0, {offset_attrs[node.StaticType][node.Attr] + 8}($t1)\n";
            }
            else
            {

                solve += $"\t lw $t0, {context_function[node.Result]}\n";
                solve += $"\t lw $t1, {context_function[node.Instanc]}\n";
                solve += $"\t sw $t0, {offset_attrs[node.StaticType][node.Attr] + 8}($t1)\n";
            }
            return solve;
        }

        public string Visit(CIL_GetAttr node)
        {
            string solve = "";
            
            solve += $"\t lw $t1, {context_function[node.Instanc]}\n";
            solve += $"\t lw $t0, {offset_attrs[node.StaticType][node.Attr] + 8}($t1)\n";
            solve += $"\t sw $t0 {context_function[node.Result]}\n";
            
            return solve;
        }

        public string Visit(CIL_Allocate node)
        {
            string solve = "";
            if (!node.Is_Type_Dir)
            {
                solve += $"\t la $t0, {node.Type}\n";
                solve += $"\t lw $a0, 4($t0)\n";
                solve += $"\t li $v0, 9\n";
                solve += $"\t syscall\n";
                solve += $"\t sw $v0 {context_function[node.Result]}\n";

                solve += $"\t sw $a0, 0($v0)\n"; // set len
                solve += $"\t sw $t0, 4($v0)\n"; // set type
            }
            else
            {
                solve += $"\t sw $t0, {context_function[node.Type]}\n";
                solve += $"\t lw $a0, 0($t0)\n";
                solve += $"\t li $v0, 9\n";
                solve += $"\t syscall\n";
                solve += $"\t sw $v0 {context_function[node.Result]}\n";

                solve += $"\t sw $a0, 0($v0)\n"; // set len
                solve += $"\t sw $t0, 4($v0)\n"; // set type
            }
            return solve;
        }

        public string Visit(CIL_Return node)
        {
            string solve = "";
            int val;
            if(int.TryParse(node.Result, out val))
            {
                solve += $"\t li $v0 {val}\n";
            }
            else
            {
                solve += $"\t lw $v0 {context_function[node.Result]}\n";
            }
            solve += $"\t addiu $sp, $sp, { ReservLocalArgs }\n";
            solve += $"\t jr $ra\n";
            return solve;
        }

        public string Visit(CIL_LoadStr node)
        {
            
            var string_content = datareference[node.StrId];
            var string_len = string_content.Length;
            var code_result = "";
            //allocate the space in node.result
            code_result += $"\t li $a0 {string_len+8}\n"; //Set $a0 with the param, meaning the size of the allocation
            code_result += $"\t li $v0 9\n";
            code_result += $"\t syscall\n"; // llama a la funcion allocate
            code_result += $"\t sw $v0 {context_function[node.Result]}\n"; //guarda la direccion de la memoria allocada

            //llena la direccion allocada con //tamano,typo,datos
            code_result += $"\t li $t0 {string_len + 8}\n"; //carga en t0 el tamano
            code_result += $"\t lw $t1 {context_function[node.Result]}\n"; //obtiene la direccion en t1
            code_result += $"\t sw $t0 0($t1)\n"; //guarda en la primera posicion el tamano
            code_result += $"\t la $t2 String\n"; //guarda la direccion del tipo String
            code_result += $"\t sw $t2 4($t1)\n";

            code_result += $"\t la $a1 {node.StrId}\n";
            code_result += $"\t lw $t2 {context_function[node.Result]}\n";
            var idx = 0;
            //llena en los bytes restantes el valor del string
            for (int i = 8; i < string_len + 8; i++)
            {
                code_result += $"\t lbu $t1 {idx}($a1)\n";
                code_result += $"\t sb $t1 {i}($t2)\n";
                idx++;
            }
            return code_result;
        }

        public string Visit(CIL_StrStr node)
        {

            var code_result = "";
            var stringref = context_function[node.StrId];
            var destiny = context_function[node.Result];

            code_result += $"\t lw $t0 {stringref}\n";
            code_result += $"\t lw $t0 0($t0)\n";
            code_result += $"\t addi $t0 $t0 -8\n";
            //I
            code_result += $"\t lw $t1 {context_function[node.I]}\n";
            //L
            code_result += $"\t lw $t2 {context_function[node.L]}\n";

            //L+I inside?

            code_result += $"\t lw $t3 {stringref}\n";
            code_result += $"\t move $a0 $t1\n";
            code_result += $"\t add $a0 $a0 $t2\n";
            code_result += $"\t sle $t4, $a0, $t0\n";
            code_result += $"\t la $a0 error_indexout\n";
            code_result += $"\t beq $t4, $zero, exception\n";

            code_result += $"\t lw $t0 {stringref}\n";
            code_result += $"\t lw $t0 0($t0)\n";
            code_result += $"\t lw $t3 {stringref}\n";
            code_result += $"\t li $a0 0\n";
            code_result += $"\t addi $a0 $t2 0\n";
            //branch para la excepttion

            //copy the substring
            //allocate first
            code_result += $"\t addi $a0 $a0 8\n";
            code_result += $"\t li $v0 9\n";
            code_result += $"\t syscall\n"; // allocate
            code_result += $"\t sw $v0 {context_function[node.Result]}\n";

            code_result += $"\t la $t2 String\n";

            code_result += $"\t sw $a0 0($v0)\n"; // save len

            code_result += $"\t sw $t2 4($v0)\n"; // save type

            code_result += $"\t lw $t0 {stringref}\n";

            code_result += $"\t lw $t1 {context_function[node.I]}\n";


            code_result += $"\t addi $t0, $t0, 8\n";//saltate el tipo y tamano
            code_result += $"\t addi $v0, $v0, 8\n";//saltate el tipo y tamano

            //I
            code_result += $"\t lw $t1 {context_function[node.I]}\n";
            //L
            code_result += $"\t lw $t2 {context_function[node.L]}\n";

            code_result += $"\t add $t0, $t0, $t1\n";//parate en I

            code_result += $"\t .sub: \n";
            code_result += $"\t beq $t2 $zero .endsub\n";
            code_result += $"\t lb $t3 0($t0)\n";
            code_result += $"\t sb $t3 0($v0)\n";
            code_result += $"\t addi $t2, $t2, -1\n";
            code_result += $"\t addi $t0, 1\n";
            code_result += $"\t addi $v0, 1\n";
            code_result += $"\t j .sub\n";
            code_result += $"\t .endsub: \n";


            return code_result;
        }

        public string Visit(CIL_LengthStr node)
        {
            var solve = "";
            solve += $"\t lw $v0, {context_function[node.StrId]}\n";
            solve += $"\t lw $v1, 0($v0)\n";
            solve += $"\t addi $v1, $v1, -8\n";
            solve += $"\t sw $v1, {context_function[node.Result]}\n";
            return solve;
        }

        public string Visit(CIL_IORead node)
        {
            throw new NotImplementedException();
        }

        public string Visit(CIL_IOPrint node)
        {
            var code_result = "";

            code_result += $"\t lw $t0, {context_function[node.Obj]}\n";

            code_result += $"\t addiu $a0, $t0 , 8\n";

            code_result += $"\t li $v0, 4\n";
            code_result += $"\t syscall\n";

            return code_result;
        }

        public string Visit(CIL_ExceptionCond node)
        {
            string solve = "";

            solve += $"\n\t la $a0, {node.Sms}\n";
            int val;
            if (!int.TryParse(node.Cond, out val))
                solve += $"\t lw $t0, {context_function[node.Cond] } \n";
            else solve += $"\t li $t0, { val} \n";
            if (!int.TryParse(node.Id, out val))
                solve += $"\t lw $t1, {context_function[node.Id] } \n";
            else
                solve += $"\t li $t1, {val } \n";

            solve += $"\t beq $t0, $t1, exception\n";

            return solve;
        }

        public string Visit(CIL_IOPrintInt node)
        {
            string solve = "";
            int val;
            if(int.TryParse(node.Obj, out val))
            {
                solve += $"\t li $v0 , 1\n";
                solve += $"\t li $a0 , {val}\n";
                solve += $"\t syscall\n";
            }
            else
            {
                solve += $"\t li $v0 , 1\n";
                solve += $"\t lw $a0 , {context_function[node.Obj]}\n";
                solve += $"\t syscall\n";
            }
            return solve;
        }

        public string Visit(CIL_IOReadInt node)
        {
            var s = "";
            s += $"\t li $v0, 5\n";
            s += $"\t syscall\n";
            s += $"\t sw $v0 {context_function[node.Result]}\n";
            return s;
        }

        public string Visit(CIL_TypeName node)
        {
            string solve = "";
            
            solve += $"\t lw $t0, {context_function[node.Type]}\n";
            
            solve += $"\t lw $a0, 12($t0)\n";

            solve += $"\t li $v0, 4\n";

            solve += $"\t syscall\n";

            return solve;
        }

        public string Visit(CIL_FatherType node)
        {
            string solve = "";
            solve += $"\t sw $t1, {context_function[node.Type]}\n";
            solve += "\t lw $t0, 8($t1)\n";
            solve += $"\t sw $t0, {context_function[node.Result]}\n";
            return solve;
        }

        public string Visit(CIL_StrConcat node)
        {
           var code_result = "";

            var string1_len = context_function[node.StrId];
            var string2_len = context_function[node.StrId2];
            //allocate the space in node.result
            code_result += $"\t lw $t0 {string1_len}\n";
            code_result += $"\t lw $t0 0($t0)\n";
            
            code_result += $"\t li $a0 0\n";
            code_result += $"\t lw $t1 {string2_len}\n";
            code_result += $"\t lw $t1 0($t1)\n";
            code_result += $"\t add $a0 $t0 $t1\n";
            code_result += $"\t addi $a0 $a0 -8\n";
            code_result += $"\t li $v0 9\n";
            code_result += $"\t syscall\n"; // allocate
            code_result += $"\t sw $v0 {context_function[node.Result]}\n";
            //guarda la direccion de la memoria allocada

            code_result += $"\t la $t2 String\n";

            code_result += $"\t sw $a0 0($v0)\n"; // save len

            code_result += $"\t sw $t2 4($v0)\n"; // save type

            code_result += $"\t lw $t0 {string1_len}\n";
            code_result += $"\t lw $t1 {string2_len}\n";

            code_result += $"\t lw $t2 0($t0)\n";
            code_result += $"\t addi $t2 $t2 -8\n";            

            code_result += $"\t lw $t4 0($t1)\n";
            code_result += $"\t addi $t4 $t4 -8\n";
            


            code_result += $"\t addi $t0 $t0 8\n";
            code_result += $"\t addi $t1 $t1 8\n";
            code_result += $"\t addi $v0 $v0 8\n";


            // en V0 = newstr, T0 = str1, T1 = str2, t2 = len(str1);

            code_result += $"\t .concat1: \n";
            code_result += $"\t beq $t2 $zero .endconcat1\n";
            code_result += $"\t lb $t3 0($t0)\n";
            code_result += $"\t sb $t3 0($v0)\n";
            code_result += $"\t addi $t2, $t2, -1\n";
            code_result += $"\t addi $t0, 1\n";
            code_result += $"\t addi $v0, 1\n";
            code_result += $"\t j .concat1\n";
            code_result += $"\t .endconcat1: \n";

            code_result += $"\t move $t2 $t4\n";

            code_result += $"\t .concat2: \n";
            code_result += $"\t beq $t2 $zero .endconcat2\n";
            code_result += $"\t lb $t3 0($t1)\n";
            code_result += $"\t sb $t3 0($v0)\n";
            code_result += $"\t addi $t2, $t2, -1\n";
            code_result += $"\t addi $t1, 1\n";
            code_result += $"\t addi $v0, 1\n";
            code_result += $"\t j .concat2\n";
            code_result += $"\t .endconcat2: \n";

            return code_result;
        }

        public string Visit(CIL_Abort node)
        {
            return "\tli $v0, 10\n\t syscall\n";
        }

        public string Visit(CIL_Copy node)
        {
            var solve = "";
            solve += $"\t lw $t0 {context_function["self"]}\n";
            solve += $"\t lw $t2 0($t0)\n";
            solve += $"\t lw $t1 {context_function[node.Result]}\n";
            solve += $"\t .cop: \n";
            solve += $"\t beq $t2 $zero .endcop\n";
            solve += $"\t lb $t3 0($t0)\n";
            solve += $"\t sb $t3 0($t1)\n";
            solve += $"\t addi $t2, $t2, -1\n";
            solve += $"\t addi $t0, 1\n";
            solve += $"\t addi $t1, 1\n";
            solve += $"\t j .cop\n";
            solve += $"\t .endcop: \n";
            return solve;
        }
    }
}
