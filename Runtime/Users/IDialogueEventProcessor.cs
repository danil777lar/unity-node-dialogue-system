using UnityEngine;

namespace Larje.Dialogue.Runtime
{
    public interface IDialogueEventProcessor
    {
        public void SendEvent(string eventId);
    }
}
