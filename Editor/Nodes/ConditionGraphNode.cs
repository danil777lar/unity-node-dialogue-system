using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    [Serializable]
    public class ConditionGraphNode : GraphNode
    {
        public string ConditionName = "";
        
        public override string DefaultName => "Condition";
        protected override string StyleSheetName => "ConditionGraphNode";
        public override GraphNodePanel GetPanelInstance => new ConditionGraphNodePanel(this);
        
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
            title = ConditionName;
        }

        private void DrawUI()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portColor = Color.white;
            inputPort.portName = "In";
            inputContainer.Add(inputPort);
            
            Port outputPortTrue = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPortTrue.portColor = Color.white;
            outputPortTrue.portName = "True";
            outputContainer.Add(outputPortTrue);
            
            Port outputPortFalse = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPortFalse.portColor = Color.white;
            outputPortFalse.portName = "False";
            outputContainer.Add(outputPortFalse);
            
            UpdateTitle();
        }
    }
}