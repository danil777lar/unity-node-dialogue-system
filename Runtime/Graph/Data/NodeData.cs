using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Larje.Dialogue.Runtime.Graph.Data
{
    [Serializable]
    public class NodeData
    {
        public string Type;
        public Vector2 Position;
        public List<Field> Fields;
        
        public T GetField<T>(string name)
        {
            object value = GetField(name);
            
            if (value is T)
            {
                return (T)value;
            }
            return default;
        }
        
        public object GetField(string name)
        {
            Field field = Fields.Find(x => x.Name == name);
            if (field != null)
            {
                Assembly asm = field.Assembly == "" ? Assembly.GetExecutingAssembly() : Assembly.Load(field.Assembly);
                Type type = asm.GetType(field.Type);
                if (field.UnityObjectValue != null)
                {
                    return field.UnityObjectValue;
                }
                else if (field.IsJson)
                {
                    object data = JsonUtility.FromJson(field.Value, type);
                    return data;
                }
                else
                {
                    return Convert.ChangeType(field.Value, type);   
                }
            }
            
            return null;
        }

        public bool IsTypeOf(string type)
        {
            return Type.Split(".").Last() == type.Split(".").Last();
        }
        
        [Serializable]
        public class Field
        {
            public string Name;
            public string Type;
            public string Assembly;
            public bool IsJson;
            public string Value;
            public UnityEngine.Object UnityObjectValue;
        }
    }
}
