using Larje.Dialogue.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    public class EventGraphNodePanel : GraphNodePanel
    {
        private EventGraphNode _node;

        public EventGraphNodePanel(EventGraphNode node)
        {
            _node = node;
        }

        public override void Draw(VisualElement root)
        {
            base.Draw(root);
            DrawMainSettings();
        }

        private void DrawMainSettings()
        {
            Box settings = GetBox(_root, "MAIN SETTINGS:");

            TextField eventField = new TextField();
            eventField.label = "Event Name:";
            eventField.value = _node.EventName;
            eventField.RegisterCallback<ChangeEvent<string>>((value) =>
            {
                _node.EventName = value.newValue;
            });
            eventField.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
                _node.UpdateTitle();
            });
            settings.Add(eventField);
        }
    }
}
