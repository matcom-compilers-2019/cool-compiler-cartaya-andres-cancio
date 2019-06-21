using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cool_Compiler.Transpiler;
using Cool_Compiler.Visitor.Semantic;
using Cool_Compiler.Visitor.Semantic.Errors;
using Cool_Compiler.Visitor;
using Cool_Compiler.Visitor.CIL;
using System.IO;
using Antlr4.Runtime;
using Cool_Compiler.Visitor.Runtime;

namespace Runtime
{
    class Program
    {
        public static void Debug_AST(string text)
        {
            Travels.DFS(BuildAST_Cool.BUILD(text));
        }

        public static void Debug_SemanticTypes(string text)
        {
            var root = BuildAST_Cool.BUILD(text);
            foreach (var item in SemanticType.BuildAllType(root.class_list))
            {
                Console.WriteLine("-----------------------");
                Console.WriteLine("Type : {0} <- {1} ( lvl : {2} )", item.Value.Name, item.Value.Father, item.Value.Level);
                Console.WriteLine("> ATTRS:");

                foreach (var attr in item.Value.GetAllAttr())
                {
                    Console.WriteLine(attr);
                }

                Console.WriteLine("> METHODS:");
                foreach (var meth in item.Value.GetAllMethods())
                {
                    Console.Write("{0} : {1} (", meth.Name, meth.ReturnType.Name);
                    foreach (var attr in meth.AttrParams)
                    {
                        Console.Write("{0}, ", attr);
                    }
                    Console.WriteLine(")");
                }

                Console.WriteLine("-----------------------");
            }
        }

        public static void Debug_ASTCIL(string text, string path = "cil.il")
        {
            var root = BuildAST_Cool.BUILD(text);
            var cil_root = CILCompiler.Build(root);
            string s = cil_root.ToString();
            var w = new StreamWriter(path);
            w.Write(s);
            w.Close();
        }

        public static void Debug_Mips(string text, string path = "mips.s")
        {
            var root = BuildAST_Cool.BUILD(text);
            var cil_root = CILCompiler.Build(root);
            var sem = SemanticType.BuildAllType(root.class_list);
            //var solve = CIL_Execute.Execute(cil_root, sem);
            var prog = new MipsCompiler(cil_root, sem);
            string s = (prog.Visit(cil_root));
            //Console.WriteLine(s);
            var w = new StreamWriter(path);
            w.Write(s);
            w.Close();

        }

        public static Tuple<bool, bool, bool> Debug_Semantic(string text)
        {
            bool ans;
            var root = BuildAST_Cool.BUILD(text);

            //check variables

            var v1 = new CheckVariablesVisitor();
            //try
            //{
            //    ans = v1.Visit(root);
            //    Console.WriteLine("Checkeo de variables: {0}", ans);
            //    if (!ans)
            //    {
            //        foreach (var item in v1.CurrErrorLoger.Log)
            //        {
            //            Console.WriteLine(item);
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("Se partio el checkeo de variables");
            //}

            //check types
            var v2 = new CheckTypesVisitor();
            //ans = v2.Visit(root);
            //try
            //{
            //    ans = v2.Visit(root);
            //    Console.WriteLine("Checkeo de tipos: {0}", ans);
            //    if (!ans)
            //    {
            //        foreach (var item in v2.CurrErrorLoger.Log)
            //        {
            //            Console.WriteLine(item);
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("Se partio el checkeo de tipos");
            //}

            ////check classes
            var v3 = new CheckClassesVisitor();
            //try
            //{
            //    ans = v3.Visit(root);
            //    Console.WriteLine("Checkeo de classes: {0}", ans);
            //    if (!ans)
            //    {
            //        foreach (var item in v3.CurrErrorLoger.Log)
            //        {
            //            Console.WriteLine(item);
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("Se partio el checkeo de classes");
            //}

            return new Tuple<bool, bool, bool>(v1.Visit(root), v2.Visit(root), v3.Visit(root));
        }

        public static void Debug_Semantic_Files_Success()
        {
            string success_path = "../../../cooltestcases/Semantics/success";
            var dir = new DirectoryInfo(success_path);
            int count = 0;
            int total = 0;
            foreach (var item in dir.GetFiles())
            {
                if (item.Extension == ".cl")
                {
                    var f = new StreamReader(item.FullName);

                    string text = f.ReadToEnd();

                    try
                    {
                        var solve = Debug_Semantic(text);
                        var lastadded = debug_new_shit(text);
                        if ((lastadded && solve.Item1 && solve.Item2 && solve.Item3))
                        {

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("OK: ");
                            count++;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write("Wrong: {0} {1} {2} {3}  ", lastadded, solve.Item1, solve.Item2, solve.Item3);
                            //Debug_Semantic(text);
                        }
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Broken: ");
                    }

                    Console.WriteLine(item.Name);
                    Console.ResetColor();
                    total++;
                }
            }
            Console.WriteLine($"A: {count}, T: {total}, Acc: {(double)(1.0*count/total) * 100.0}");
        }

        public static void Debug_Semantic_Files_Fail()
        {
            string success_path = "../../../cooltestcases/Semantics/fail";
            var dir = new DirectoryInfo(success_path);
            int count = 0;
            int total = 0;
            foreach (var item in dir.GetFiles())
            {
                if (item.Extension == ".cl")
                {
                    var f = new StreamReader(item.FullName);

                    string text = f.ReadToEnd();

                    try
                    {
                        var shit = debug_new_shit(text);
                        if (shit)
                        {
                            var vars = debug_new_vars(text);
                            if (vars)
                            {
                                var types = debug_new_types(text);
                                if (types)
                                {
                                    var classes = debug_new_class(text);
                                    if (classes)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Magenta;
                                        Console.Write("Wrong: ");
                                        Debug_Semantic(text);
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write("OK: ");
                                        count++;
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write("OK: ");
                                    count++;
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("OK: ");
                                count++;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("OK: ");
                            count++;
                        }
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Broken: ");
                    }

                    Console.WriteLine(item.Name);
                    Console.ResetColor();
                    total++;
                }
            }
            Console.WriteLine($"A: {count}, T: {total}, Acc: {(double)(1.0 * count / total) * 100.0}");
        }

        public static void Debug_CILProgram(string text)
        {
            var root = BuildAST_Cool.BUILD(text);
            var cil_root = CILCompiler.Build(root);
            var sem = SemanticType.BuildAllType(root.class_list);
            var solve = CIL_Execute.Execute(cil_root, sem);
            Console.WriteLine(solve);
        }

        public static Tuple<bool , ErrorLoger> SemanticChecking(Cool_Compiler.AST.AST_Root root)
        {
            var v0 = new CheckIDSandTypeDecVisitor();

            if (v0.Visit(root))
            {
                var v1 = new CheckClassesVisitor();
                if (v1.Visit(root))
                {
                    var v2 = new CheckVariablesVisitor();
                    if (v2.Visit(root))
                    {
                        var v3 = new CheckTypesVisitor();
                        if (v3.Visit(root))
                        {
                            return new Tuple<bool, ErrorLoger>(true, null);
                        }
                        else
                        {
                            return new Tuple<bool, ErrorLoger>(false, v3.CurrErrorLoger);
                        }
                    }
                    else
                    {
                        return new Tuple<bool, ErrorLoger>(false, v2.CurrErrorLoger);
                    }
                }
                else
                {
                    return new Tuple<bool, ErrorLoger>(false, v1.CurrErrorLoger);
                }
            }
            else
            {
                return new Tuple<bool, ErrorLoger>(false, v0.CurrErrorLoger);
            }
        }

        static void OldMain()
        {
            var f = new StreamReader("main.cl");

            string text = f.ReadToEnd();

            Console.WriteLine(text);
            //Debug_CILProgram(text);
            var root = BuildAST_Cool.BUILD(text);


            //Debug_AST(text);
            //Debug_SemanticTypes(text);
            //Debug_ASTCIL(text);
            //Debug_Mips(text);
            //Console.WriteLine(Debug_Semantic(text));

            #region Debug Folders

            Console.WriteLine("*****Testing Success Files*****");
            Debug_Semantic_Files_Success();
            Console.WriteLine("*****End Success Files*****");
            Console.WriteLine("\n*****Testing Fail Files*****");
            Debug_Semantic_Files_Fail();
            Console.WriteLine("*****End Success Files*****");
            #endregion
            //asd
            #region debug file

            //debug_new_shit(text);
            //debug_new_class(text);
            //debug_new_vars(text);
            //debug_new_types(text);

            #endregion

        }

        static void Compilator(string path, string dest, string mips)
        {
            var r = new StreamReader(path);

            var text = r.ReadToEnd();

            var root = BuildAST_Cool.BUILD(text);

            if(root == null)
            {
                return;
            }

            var result = SemanticChecking(root);

            if (result.Item1)
            {
                var cil_root = CILCompiler.Build(root);
                string s = cil_root.ToString();
                var w = new StreamWriter(dest);
                //Console.WriteLine(s);
                w.Write(s);
                w.Close();
                var sem = SemanticType.BuildAllType(root.class_list);
                //var solve = CIL_Execute.Execute(cil_root, sem);
                var prog = new MipsCompiler(cil_root, sem);
                s = (prog.Visit(cil_root));
                //Console.WriteLine(s);
                var t = new StreamWriter(mips);
                //Console.WriteLine(s);
                t.Write(s);
                t.Close();
            }
            else
            {
                Console.WriteLine("There are some errors!!!");
                foreach (var item in result.Item2.Log)
                {
                    Console.WriteLine(item);
                }
            }
        }

        static void Main(string[] args)
        {
            //OldMain();string t = Console.ReadLine();
            string path = "main.cl";
            if (args.Length > 0) path = args[0];
            string tmp = path;
            tmp = tmp.Remove(tmp.Length - 3);
            //Console.WriteLine(path + " " + tmp + ".il" + " " + tmp + ".s");
            Compilator(path, tmp + ".il", tmp + ".s");
        }

        static bool debug_new_shit(string text)
        {
            var root = BuildAST_Cool.BUILD(text);
            var v0 = new CheckIDSandTypeDecVisitor();
            return v0.Visit(root);
            bool r = true;
            try
            {
                r = v0.Visit(root);
                Console.WriteLine(r);
            }
            catch (Exception)
            {
                Console.WriteLine("Se partio debug shit");
            }
            Console.WriteLine(r + "shit");
            return r;
        }
        static bool debug_new_vars(string text)
        {
            var root = BuildAST_Cool.BUILD(text);
            var v0 = new CheckVariablesVisitor();
            return v0.Visit(root);
            bool r = true;
            try
            {
                r = v0.Visit(root);
                Console.WriteLine(r);
            }
            catch (Exception)
            {
                //Console.WriteLine("Se partio debug shit vars");
            }
            Console.WriteLine(r + "vars");
            return r;
        }
        static bool debug_new_types(string text)
        {
            var root = BuildAST_Cool.BUILD(text);
            var v0 = new CheckTypesVisitor();
            return v0.Visit(root);
            bool r = true;
            try
            {
                r = v0.Visit(root);
                Console.WriteLine(r);
            }
            catch (Exception)
            {
                Console.WriteLine("Se partio debug types");
            }
            Console.WriteLine(r + "types");
            if (!r)
            {
                foreach (var item in v0.CurrErrorLoger.Log)
                {
                    Console.WriteLine(item);
                }
            }
            return r;
        }
        static bool debug_new_class(string text)
        {
            var root = BuildAST_Cool.BUILD(text);
            var v0 = new CheckClassesVisitor();
            bool r = true;
            return v0.Visit(root);
            try
            {
                r = v0.Visit(root);
                Console.WriteLine(r);
            }
            catch (Exception)
            {
                Console.WriteLine("Se partio debug class");
            }
            
            Console.WriteLine(r + "class");
            return r;

            //Debug_ITree(text);
            //Debug_AST(text);
            //Debug_SemanticTypes(text);
            Debug_ASTCIL(text);
            //Debug_CILProgram(text);            
        }

        private static void Debug_ITree(string text)
        {
            AntlrInputStream input = new AntlrInputStream(text);
            CoolLexer lexer = new CoolLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            CoolParser parser = new CoolParser(tokens);
            Console.WriteLine(parser.program().GetText());
        }

    }
}
