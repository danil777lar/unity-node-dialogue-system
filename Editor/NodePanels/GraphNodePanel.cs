using Larje.Dialogue.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    public class GraphNodePanel
    {
        protected VisualElement _root;

        protected virtual GraphNode Node { get; set; }

        public virtual void Draw(VisualElement root)
        {
            ScrollView scroll = new ScrollView();
            scroll.mode = ScrollViewMode.Vertical;

            root.Add(scroll);

            _root = scroll;
            _root.Clear();

            _root.Add(new Label($"{this.GetType()}"));
        }

        protected void Refresh()
        {
            _root.Clear();
            Draw(_root);
        }

        protected Box GetBox(VisualElement root, string label)
        {
            float padding = 5f;
            float width = 0.75f;
            StyleColor color = new StyleColor(Color.gray);

            Box rootBox = new Box();
            rootBox.Add(new Label(){ text = label });
            rootBox.style.paddingBottom = 5f;
            rootBox.style.paddingTop = 5f;
            rootBox.style.paddingLeft = 5f;
            rootBox.style.paddingRight = 5f;
            root.Add(rootBox);

            Box box = new Box();
            rootBox.Add(box);

            box.style.borderBottomColor = color;
            box.style.borderTopColor = color;
            box.style.borderLeftColor = color;
            box.style.borderRightColor = color;

            box.style.borderBottomWidth = width;
            box.style.borderTopWidth = width;
            box.style.borderLeftWidth = width;
            box.style.borderRightWidth = width;

            box.style.paddingBottom = padding;
            box.style.paddingTop = padding;
            box.style.paddingLeft = padding;
            box.style.paddingRight = padding;

            return box;
        }
    }
}
