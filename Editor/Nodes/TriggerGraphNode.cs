using Larje.Core.Events;
using System;
using System.Collections.Generic;
using Larje.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    [Serializable]
    public class TriggerGraphNode : GraphNode
    {
        public TriggerId Trigger = null;

        public override string DefaultName => "Trigger";
        protected override string StyleSheetName => "TriggerGraphNode";
        public override GraphNodePanel GetPanelInstance => new TriggerGraphNodePanel(this);
        
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
            title = $"{Trigger?.name ?? "Trigger"}";
        }

        private void DrawUI()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portColor = Color.white;
            inputPort.portName = "In";
            inputContainer.Add(inputPort);
            
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portColor = Color.white;
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);
            
            UpdateTitle();
        }
    }
}
