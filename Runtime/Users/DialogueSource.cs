using Larje.Core.Events;
using Larje.Dialogue.Runtime.Converted;
using Larje.Dialogue.Runtime.Graph;
using UnityEngine;

namespace Larje.Dialogue.Runtime
{
    public class DialogueSource : MonoBehaviour
    {
        [SerializeField] protected DialogueGraphContainer dialogue;

        private IEventBus _events;

        public DialogueGraphContainer Dialogue => dialogue;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IEventBus events)
        {
            _events = events;
        }

        public DialogueStep Begin(string language)
        {
            return dialogue != null ? dialogue.GetFirstStep(SendEvent, SendTrigger, CheckCondition, language) : null;
        }

        public DialogueStep Next(DialogueStep current, int choiceId, string language)
        {
            return dialogue != null && current != null ? dialogue.GetNextStep(current.Id, choiceId, SendEvent, SendTrigger, CheckCondition, language) : null;
        }

        protected virtual void SendEvent(string eventName)
        {
            foreach (IDialogueEventProcessor processor in GetComponentsInChildren<IDialogueEventProcessor>())
            {
                processor.SendEvent(eventName);
            }
        }

        protected virtual void SendTrigger(TriggerId trigger)
        {
            if (_events != null)
            {
                _events.Publish(new TriggerEvent(trigger, 1f, nameof(DialogueSource) + " " + gameObject.name));
            }
        }

        protected virtual bool CheckCondition(string condition)
        {
            foreach (IDialogueConditionProcessor processor in GetComponentsInChildren<IDialogueConditionProcessor>())
            {
                if (processor.TryCheckCondition(condition, out bool result))
                {
                    return result;
                }
            }

            return false;
        }
    }
}
