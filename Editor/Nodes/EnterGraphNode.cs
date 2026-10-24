using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    [Serializable]
    public class EnterGraphNode : GraphNode
    {
        public int EnterIndex = 0; 
        public override string DefaultName => "Enter";
        protected override string StyleSheetName => "EnterGraphNode";
        public override GraphNodePanel GetPanelInstance => new EnterGraphNodePanel(this);

        public override GraphNode Initialize(Vector2 position, List<Node> allNodes)
        {
            base.Initialize(position, allNodes);
            
            List<EnterGraphNode> otherEnterNodes = allNodes.FindAll(node => node is EnterGraphNode && node != this)
                .ConvertAll(node => (EnterGraphNode) node).OrderBy(node => node.EnterIndex).ToList();
            for (int i = 0; i <= otherEnterNodes.Count; i++)
            {
                if (i >= otherEnterNodes.Count || otherEnterNodes[i].EnterIndex != i)
                {
                    EnterIndex = i;
                    break;
                }
            }

            DrawUI();

            return this;
        }

        public override GraphNode Load(Vector2 position)
        {
            base.Load(position);
            DrawUI();
            return this;
        }

        private void DrawUI()
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portColor = new Color(0.4f, 1f, 0.4f);
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);
            title = $"Enter {EnterIndex}";
        }
    }
}