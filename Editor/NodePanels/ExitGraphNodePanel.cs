using Larje.Dialogue.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    public class ExitGraphNodePanel : GraphNodePanel
    {
        private ExitGraphNode _node;

        public ExitGraphNodePanel(ExitGraphNode node)
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

            IntegerField conditionField = new IntegerField();
            conditionField.label = "Exit Number:";
            conditionField.value = _node.ExitIndex;
            conditionField.RegisterCallback<ChangeEvent<int>>((value) =>
            {
                _node.ExitIndex = value.newValue;
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
