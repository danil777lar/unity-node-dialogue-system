using System;
using System.Linq;
using UnityEngine.Serialization;

namespace Larje.Dialogue.Runtime.Graph.Data
{
    [Serializable]
    public class LinkData
    {
        public string FromPortName;
        public string FromGUID;
        
        public string ToPortName;
        public string ToGUID;
    }
}