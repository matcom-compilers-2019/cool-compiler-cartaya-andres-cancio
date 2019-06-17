using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Visitor.Semantic.Context
{
    public interface IContext
    {
        bool IsDefine(string var);
        bool Define(string var);
        bool IsDefineInMe(string var);
        IContext CreateChild();
        HashSet<string> GetScope();
        IContext GetParent();
        SemanticType GetType(string field);
        bool SetType(string var, SemanticType type);
        bool ChangeType(string var, SemanticType type);
        SemanticType GetTypeInMe(string var);
        bool NullFather();
    }
    public class Contexts : IContext
    {
        public IContext Parent;
        public HashSet<string> Vars;
        public Dictionary<string, SemanticType> AsociedType;
        public Contexts()
        {
            this.AsociedType = new Dictionary<string, SemanticType>();
            Parent = null;
            Vars = new HashSet<string>();
        }
        public Contexts(IContext Parent = null)
        {
            this.Parent = Parent;
            this.Vars = new HashSet<string>();
            this.AsociedType = new Dictionary<string, SemanticType>();
        }
        public bool Define(string var)
        {
            return Vars.Add(var);
        }

        public bool IsDefine(string var)
        {
            return Vars.Contains(var) || (Parent != null && Parent.IsDefine(var));
        }
        public IContext CreateChild()
        {
            var r = new Contexts(this);
            return r;
        }
        public HashSet<string> GetScope()
        {
            return this.Vars;
        }

        public IContext GetParent()
        {
            return this.Parent;
        }

        public SemanticType GetType(string field)
        {
            if (!AsociedType.ContainsKey(field))
            {
                if (Parent != null)
                    return Parent.GetType(field);
                return null;
            }
            return AsociedType[field];
        }

        public bool SetType(string var, SemanticType type)
        {
            if (AsociedType.ContainsKey(var))
                return false;
            AsociedType.Add(var, type);
            return true;
        }

        public bool ChangeType(string var, SemanticType type)
        {
            if (!this.AsociedType.ContainsKey(var))
                return false;
            this.AsociedType[var] = new SemanticType(type.Name);
            return true;
        }

        public SemanticType GetTypeInMe(string var)
        {
            if (!AsociedType.ContainsKey(var))
                return null;
            return AsociedType[var];
        }

        public bool IsDefineInMe(string var)
        {
            return Vars.Contains(var);
        }

        public bool NullFather()
        {
            return this.Parent == null;
        }
    }
}
