using Larje.Core.Events;
using Larje.Dialogue.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Larje.Core;

namespace Larje.Dialogue.Editor
{
    public class TriggerGraphNodePanel : GraphNodePanel
    {
        private TriggerGraphNode _node;

        public TriggerGraphNodePanel(TriggerGraphNode node)
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

            ObjectField field = new ObjectField();
            field.label = "Trigger:";
            field.objectType = typeof(TriggerId);
            field.value = _node.Trigger;

            field.RegisterCallback<ChangeEvent<Object>>((value) =>
            {
                _node.Trigger = value.newValue as TriggerId;

                Refresh();
                _node.UpdateTitle();
            });

            field.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
                _node.UpdateTitle();
            });

            settings.Add(field);
        }
    }
}
