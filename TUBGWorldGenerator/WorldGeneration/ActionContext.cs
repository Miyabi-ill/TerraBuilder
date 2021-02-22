namespace TUBGWorldGenerator.WorldGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class ActionContext
    {
        private Dictionary<string, PropertyInfo> PropertyDictionary { get; } = new Dictionary<string, PropertyInfo>();

        private Dictionary<string, object> OuterPropertyDictionary { get; } = new Dictionary<string, object>();

        public ActionContext()
        {
            foreach (var property in this.GetType().GetProperties())
            {
                PropertyDictionary.Add(property.Name, property);
            }
        }

        public object this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new KeyNotFoundException(name);
                }

                if (PropertyDictionary.ContainsKey(name))
                {
                    return PropertyDictionary[name].GetValue(this);
                }
                else if (OuterPropertyDictionary.ContainsKey(name))
                {
                    return OuterPropertyDictionary[name];
                }

                throw new KeyNotFoundException(name);
            }

            set
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new KeyNotFoundException(name);
                }

                if (PropertyDictionary.ContainsKey(name))
                {
                    try
                    {
                        var setValue = Convert.ChangeType(value, PropertyDictionary[name].PropertyType);
                        PropertyDictionary[name].SetValue(this, setValue);
                    }
                    catch
                    {
                        throw new TypeMismatchException(
                            string.Format("Type is mismatched during conversion. Expected type: {0}, Real type: {1}", PropertyDictionary[name].PropertyType.Name, value.GetType()));
                    }
                }
                else if (OuterPropertyDictionary.ContainsKey(name))
                {
                    OuterPropertyDictionary[name] = value;
                }
                else
                {
                    OuterPropertyDictionary.Add(name, value);
                }
            }
        }
    }
}
