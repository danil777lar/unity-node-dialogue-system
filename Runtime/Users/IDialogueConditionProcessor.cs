using UnityEngine;

namespace Larje.Dialogue.Runtime
{
    public interface IDialogueConditionProcessor
    {
        public bool TryCheckCondition(string condition, out bool result);
    }
}
