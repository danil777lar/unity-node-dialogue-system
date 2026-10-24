using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    [Serializable]
    public class ExitGraphNode : GraphNode
    {
        public int ExitIndex = 0;
        public override string DefaultName => "Exit";
        protected override string StyleSheetName => "ExitGraphNode";
        public override GraphNodePanel GetPanelInstance => new ExitGraphNodePanel(this);


        public override GraphNode Initialize(Vector2 position, List<Node> allNodes)
        {
            base.Initialize(position, allNodes);
            DrawUI();
            return this;
        }

        public override GraphNode Load(Vector2 position)
        {
            base.Load(position);
            DrawUI();
            return this;
        }

        public void UpdateTitle()
        {
            title = $"Exit {ExitIndex}";
        }

        private void DrawUI()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portColor = new Color(1f, 0.4f, 0.4f);
            inputPort.portName = "In";
            inputContainer.Add(inputPort);
            
            UpdateTitle();
        }
    }
}