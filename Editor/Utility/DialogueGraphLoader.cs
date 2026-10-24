using System;
using System.Linq;
using System.Reflection;
using Larje.Dialogue.Editor;
using Larje.Dialogue.Runtime.Graph;
using Larje.Dialogue.Runtime.Graph.Data;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor.Utility
{
    public static class DialogueGraphLoader
    {
        public static DialogueGraphContainer LoadGraph(DialogueGraphView view, string assetPath)
        {
            DialogueGraphContainer container = AssetDatabase.LoadAssetAtPath<DialogueGraphContainer>(assetPath);

            ClearView(view);
            LoadNodes(container, view);
            LoadEdges(container, view);

            AssetDatabase.SaveAssets();

            return container;
        }
        
        public static void LoadState(DialogueGraphView view, DialogueGraphContainer container)
        {
            ClearView(view);
            LoadNodes(container, view);
            LoadEdges(container, view);
        }

        private static void ClearView(DialogueGraphView view)
        {
            view.nodes.ToList().ForEach(view.RemoveElement);
            view.edges.ToList().ForEach(view.RemoveElement);            
        }

        private static void LoadNodes(DialogueGraphContainer container, DialogueGraphView view)
        {
            foreach (NodeData nodeData in container.Nodes)
            {
                GraphNode node = Activator.CreateInstance(Type.GetType(nodeData.Type)) as GraphNode;

                foreach (NodeData.Field field in nodeData.Fields)
                {
                    FieldInfo fieldInfo = node.GetType().GetField(field.Name);
                    if (fieldInfo != null)
                    {
                        object value = nodeData.GetField(field.Name);
                        fieldInfo.SetValue(node, value);   
                    }
                }

                view.AddNode(node.Load(nodeData.Position));
            }
        }

        private static void LoadEdges(DialogueGraphContainer container, DialogueGraphView view)
        {
            foreach (LinkData link in container.Links)
            {
                GraphNode outputNode = view.nodes.ToList().Find(x => ((GraphNode)x).GUID == link.FromGUID) as GraphNode;
                GraphNode inputNode = view.nodes.ToList().Find(x => ((GraphNode)x).GUID == link.ToGUID) as GraphNode;

                Edge edge = new Edge();
                
                edge.output = outputNode.outputContainer.Query<Port>().ToList().Find(x => x.portName == link.FromPortName);
                edge.input = inputNode.inputContainer.Query<Port>().ToList().Find(x => x.portName == link.ToPortName);
                
                if (edge.output != null && edge.input != null)
                {
                    edge.output.Connect(edge);
                    edge.input.Connect(edge);
                    
                    view.AddEdge(edge);
                }
            }
        }
    }
}