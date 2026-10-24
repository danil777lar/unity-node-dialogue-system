using System;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Dialogue.Runtime
{
    public class DialogueConditionDebug : MonoBehaviour, IDialogueConditionProcessor
    {
        [SerializeField] private List<Condition> values = new List<Condition>();

        public string[] SupportedConditions { get; }

        public bool TryCheckCondition(string condition, out bool result)
        {
            Condition c = values.Find(x => x.Name == condition);
            if (c != null)
            {
                result = c.Value;
                return true;
            }

            result = false;
            return false;
        }

        [Serializable]
        private class Condition
        {
            public string Name;
            public bool Value;
        }        
    }
}
