using Larje.Dialogue.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    public class ConditionGraphNodePanel : GraphNodePanel
    {
        private ConditionGraphNode _node;

        public ConditionGraphNodePanel(ConditionGraphNode node)
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

            TextField conditionField = new TextField();
            conditionField.label = "Condition Name:";
            conditionField.value = _node.ConditionName;
            conditionField.RegisterCallback<ChangeEvent<string>>((value) =>
            {
                _node.ConditionName = value.newValue;
            });
            conditionField.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
                _node.UpdateTitle();
            });
            settings.Add(conditionField);
        }
    }
}
