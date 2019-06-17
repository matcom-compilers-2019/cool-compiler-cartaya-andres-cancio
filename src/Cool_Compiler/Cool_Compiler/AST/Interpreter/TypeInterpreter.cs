using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Iterpreter
{
    public abstract class Base_Object_Type
    {
        public string Name { get; private set; }

        public Base_Object_Type Father { get; private set; }

        public Attribute[] Attributes;

        public IMethod[] Methods;

        public int Size { get; set; }

        public virtual IEnumerable<Attribute> GetAllAttributes()
        {
            foreach (var item in Attributes)
                yield return item;
            if (Father == null) yield break;
            foreach (var item in Father.GetAllAttributes())
                yield return item;
        }

        public virtual Attribute GetAttribute(string attr)
        {
            foreach (var item in GetAllAttributes())
                if (item.Name == attr)
                    return item;
            return null;
        } 

        public virtual bool HasAttr(string attr)
        {
            return GetAttribute(attr) != null;
        }

        public virtual IEnumerable<IMethod> GetAllMethods()
        {
            foreach (var item in Methods)
                yield return item;
            if (Father == null) yield break;
            foreach (var item in Father.GetAllMethods())
                yield return item;
        }

        public virtual IMethod GetMethod(string name)
        {
            foreach (var item in GetAllMethods())
                if (item.Name == name)
                    return item;
            return null;
        }

        public virtual bool HasMethod(string name)
        {
            return GetMethod(name) != null ;
        }

        public virtual Base_Object_Value Instanciate()
        {
            return new Base_Object_Value(Default(), this);
        }

        public abstract object Default();

        public abstract bool IsTrue(Base_Object_Value obj);

        public override string ToString()
        {
            return "<" + Name + ">";
        }        

        public Base_Object_Type(string name, Base_Object_Type father, Attribute[] attr, IMethod[] methods)
        {
            Name = name;
            Father = father;
            Attributes = attr;
            Methods = methods;
        }
    }

    public abstract class IMethod
    {

        public string Name;

        public Base_Object_Type TypeResult;

        public Attribute[] TypeParams;

        public IMethod(string name, Base_Object_Type type_result, Attribute[] type_params)
        {
            Name = name;
            TypeResult = type_result;
            TypeParams = type_params;
        }

        public abstract Base_Object_Value Call(Base_Object_Value[] ObjectParam);
    }

    public class Attribute
    {
        public string Name;        

        public Base_Object_Type Type;

        public int Index;

        public Attribute(string name, Base_Object_Type type, int index)
        {
            Name = name;
            Type = type;
            Index = index;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name + ":" + Type.ToString();
        }

    }

    public class Base_Object_Value
    {
        public Base_Object_Type Type { get; private set; }

        public object Value { get; private set; }

        Dictionary<Attribute, Base_Object_Value> AttributesValues =
            new Dictionary<Attribute, Base_Object_Value>();

        public bool HasAttribute(string attr)
        {
            return Type.HasAttr(attr);
        }

        public Base_Object_Value GetAttributeValue(string attr)
        {
            return AttributesValues[Type.GetAttribute(attr)];
        }

        public void SetAttributeValue(string attr, Base_Object_Value value)
        {
            Attribute a = Type.GetAttribute(attr);
            AttributesValues[a] = value;
        }

        public bool HasMethod(string name)
        {
            return Type.HasMethod(name);
        }

        public Base_Object_Value CallMethod(string name, Base_Object_Value[] param)
        {
            List<Base_Object_Value> p = new List<Base_Object_Value>();
            p.Add(this);
            IMethod m = Type.GetMethod(name);
            return m.Call(p.Union(param).ToArray());
        }

        public Base_Object_Value(object value, Base_Object_Type type)
        {
            Value = value ;
            Type = type   ;
            foreach (var item in Type.GetAllAttributes())
            {
                AttributesValues[item] = item.Type.Instanciate();
            }
        }

        public bool IsTrue()
        {
            return Type.IsTrue(this);
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
}
