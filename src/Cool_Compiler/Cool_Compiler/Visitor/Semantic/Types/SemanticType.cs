
using Cool_Compiler.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Visitor.Semantic
{
    public class SemanticType
    {
        public string Name { get; set; }

        // TODO : Poner el level private
        public int Level { get; set; }

        public SemanticType Father { get; set; }

        public List<SemanticAttr> Attrs { get; set; }

        public List<SemanticMethod> Methods { get; set; }

        public override string ToString()
        {
            return string.Format("@({0})", Name);
        }
        public SemanticType(string name, SemanticType semtyp = null)
        {
            Name = name;
            Father = semtyp;
            Attrs = new List<SemanticAttr>();
            Methods = new List<SemanticMethod>();
            Level = 0;
        }

        // Find Methods

        public int _find_method(string name)
        {
            for (int i = 0; i < Methods.Count; i++)
                if (Methods[i].Name == name)
                    return i;
            return -1;
        }
        public SemanticMethod _GetMethod(string name)
        {
            int idx = _find_method(name);
            if (idx != -1)
            {
                var solve = Methods[idx];
                return solve;
            }
            if (Father == null) return null;
            return Father._GetMethod(name);
        }
        public SemanticMethod GetMethod(string name)
        {
            var x = _GetMethod(name);
            if (x == null) return x;
            return x.ChangeType(this);
        }
        public bool HasMethod(string name)
        {
            return GetMethod(name) != null;
        }

        // Find ATTR
        private int _find_attr(string name)
        {
            for (int i = 0; i < Attrs.Count; i++)
                if (Attrs[i].Id == name)
                    return i;
            return -1;
        }
        public SemanticAttr _GetAttr(string name)
        {
            int idx = _find_attr(name);
            if (idx != -1) return Attrs[idx];
            if (Father == null) return null;
            return Father._GetAttr(name);
        }
        public SemanticAttr GetAttr(string name)
        {
            SemanticAttr x = _GetAttr(name);
            if (x == null) return x;
            if (x.Type.Name == "SELF_TYPE")
                return x.ChangeType(this);
            return x;
            
        }
        public bool HasAttr(string name)
        {
            return GetAttr(name) != null;
        }


        // Yield Methods order by level

        public IEnumerable<SemanticMethod> _GetAllMethods(bool my_only = false)
        {
            if (Father != null && !my_only)
            {
                foreach (var item in Father._GetAllMethods())
                {
                    var solve = item;
                    //if (solve.ReturnType.Name == "SELF_TYPE")
                    //    solve = solve.ChangeType(this);
                    yield return solve;
                }
            }
            foreach (var item in Methods)
                yield return item;
        }

        // Yield ATTR order by level

        public IEnumerable<SemanticAttr> _GetAllAttr(bool my_only = false)
        {
            if(Father != null && !my_only)
            {
                foreach (var item in Father._GetAllAttr())
                    yield return item;                
            }
            foreach (var item in Attrs)
                yield return item;
        }

        public IEnumerable<SemanticMethod> GetAllMethods(bool my_only = false)
        {
            foreach (var item in _GetAllMethods(my_only))
            {
                yield return item.ChangeType(this);
            }
        }

        public IEnumerable<SemanticAttr> GetAllAttr(bool my_only = false)
        {
            foreach (var item in _GetAllAttr(my_only))
            {
                if (item.Type.Name == "SELF_TYPE")
                    yield return item.ChangeType(this);
                else yield return item;
            }
        }


        public static Dictionary<string, SemanticType> BuildSystemType()
        {
            Dictionary<string, SemanticType> solve = new Dictionary<string, SemanticType>();

            var Object = new SemanticType("Object");
            var SelfType = new SemanticType("SELF_TYPE");
            var String = new SemanticType("String", Object);
            var Int = new SemanticType("Int", Object);
            var IO = new SemanticType("IO", Object);
            var Bool = new SemanticType("Bool", Object);

            var abort = new SemanticMethod("abort", Object);
            var type_name = new SemanticMethod("type_name", String);
            var copy = new SemanticMethod("copy", SelfType);

            Object.Methods = new List<SemanticMethod> { abort, type_name, copy };


            var length = new SemanticMethod("length", Int);
            var concat = new SemanticMethod("concat", String, new SemanticAttr[] { new SemanticAttr("x", String) });
            var substring = new SemanticMethod("substr", String, new SemanticAttr[] { new SemanticAttr("i", Int), new SemanticAttr("l", Int) });

            String.Methods = new List<SemanticMethod> { length, concat, substring };

            var out_string = new SemanticMethod("out_string", SelfType, new SemanticAttr[] { new SemanticAttr("x", String) });
            var out_int = new SemanticMethod("out_int", SelfType, new SemanticAttr[] { new SemanticAttr("x", Int) });
            var in_string = new SemanticMethod("in_string", String);
            var in_int = new SemanticMethod("in_int", Int);

            IO.Methods = new List<SemanticMethod> { out_string, out_int, in_string, in_int };

            foreach (var item in (new SemanticType[] { Object, Int, IO, SelfType, String, Bool }))
            {
                solve.Add(item.Name, item);
            }

            return solve;
        }

        public static Dictionary<string, SemanticType> BuildAllType(AST_ListClass list)
        {
            // Create SystemTypes
            var solve = BuildSystemType();

            // Create All Types;
            foreach (var item in list.Class_list)
                solve.Add(item.Id.Id, new SemanticType(item.Id.Id));

            // Set Parent
            foreach (var item in list.Class_list)
                solve[item.Id.Id].Father = solve[item.Inherits.Type];

            // Create ATTR
            foreach (var item in list.Class_list)
            {
                string name = item.Id.Id;
                foreach (var attr in item.Property_list.Propertys)
                {
                    solve[name].Attrs.Add(new SemanticAttr(attr.decl.id.Id, solve[attr.decl.type.Type]));
                }

                foreach (var method in item.Method_list.Methods)
                {
                    List<SemanticAttr> v = new List<SemanticAttr>();
                    foreach (var attr in method.Propertys.Propertys)
                    {
                        v.Add(new SemanticAttr(attr.decl.id.Id, solve[attr.decl.type.Type]));
                    }
                    solve[name].Methods.Add(new SemanticMethod(method.Id.Id, solve[method.type.Type], v));
                }              
                
            }
            // Set Self in all methods

            foreach (var item in solve)
            {
                foreach (var method in item.Value.Methods)
                {
                    method.Self = item.Value;
                }
            }

            // Set Self in all attr
            foreach (var item in solve)
            {
                foreach (var method in item.Value.Attrs)
                {
                    method.Self = item.Value;
                }
            }

            // SetLevels
            Dictionary<string, bool> mark = new Dictionary<string, bool>();
            foreach (var item in solve)
                mark.Add(item.Key, false);
            foreach (var item in solve)
                SetLevels(item.Value, mark);
            return solve;
        }

        public static SemanticType LCA(SemanticType ST_A, SemanticType ST_B)
        {
            if(ST_A.Level < ST_B.Level)
            {
                while(ST_A.Level < ST_B.Level)
                {
                    ST_B = ST_B.Father;
                }
            }
            else if(ST_B.Level < ST_A.Level)
            {
                while (ST_B.Level < ST_A.Level)
                {
                    ST_A = ST_A.Father;
                }
            }

            if (ST_A.Name == ST_B.Name)
                return ST_A;

            while(ST_A.Name != ST_B.Name)
            {
                if (ST_A.Father == null || ST_B.Father == null)
                    return null;
                ST_A = ST_A.Father;
                ST_B = ST_B.Father;
            }
            return ST_A;
        }

        public static void SetLevels (SemanticType t, Dictionary<string, bool> mark)
        {
            if (t.Father == null) return;
            if (mark[t.Name]) return;
            mark[t.Name] = true;
            SetLevels(t.Father, mark);
            t.Level = t.Father.Level + 1;
        }

        public static int Distance(SemanticType ST_A, SemanticType ST_B)
        {
            var lca = LCA(ST_A, ST_B);

            if (lca.Name != ST_B.Name)
                return -1;
            return ST_B.Level - ST_A.Level;
        }
    }

    public class SemanticAttr
    {
        public string Id { get; set; }

        public SemanticType Type { get; set; }

        public SemanticType Self { get; set; }

        public SemanticAttr(string id, SemanticType type)
        {
            Id = id;
            Type = type;
        }

        public SemanticAttr ChangeType(SemanticType other)
        {
            var s = new SemanticAttr(Id, Type);
            s.Self = Self;
            return s;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} ({2})", Id, Type.Name, ((Self != null)?Self.Name:""));
        }
    }

    public class SemanticMethod
    {
        public SemanticType ReturnType { get; set; }

        public List<SemanticAttr> AttrParams { get; set; }

        public string Name { get; set; }

        public SemanticType Self { get; set; }

        public SemanticMethod(string name, SemanticType returntype, IEnumerable<SemanticAttr> attrlist = null)
        {
            Name = name;
            if (attrlist == null) AttrParams = new List<SemanticAttr>();
            else AttrParams = new List<SemanticAttr>(attrlist);
            ReturnType = returntype;
        }

        public string Label()
        {
            return Self.Name + "." + Name;
        }

        public SemanticMethod ChangeType(SemanticType t)
        {
            var l = new List<SemanticAttr>();
            foreach (var item in AttrParams)
            {
                if (item.Type.Name == "SELF_TYPE")
                    l.Add(item.ChangeType(t));
                else l.Add(item);
            }
            var solve = new SemanticMethod(Name, ReturnType, AttrParams);
            solve.Self = Self;
            if(solve.ReturnType.Name == "SELF_TYPE") solve.ReturnType = t;
            return solve;
        }
    }
}
