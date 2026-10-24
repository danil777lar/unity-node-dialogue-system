using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Larje.Dialogue.Editor;
using Larje.Dialogue.Runtime.Graph;
using Larje.Dialogue.Runtime.Graph.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Larje.Dialogue.Editor.Utility
{
    public static class DialogueGraphSaver
    {
        public static DialogueGraphContainer SaveGraph(DialogueGraphView view, string assetPath)
        {
            DialogueGraphContainer container = AssetDatabase.LoadAssetAtPath<DialogueGraphContainer>(assetPath);

            SaveNodes(container, view);

            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();

            return container;
        }
        
        public static bool TrySaveState(DialogueGraphView view, DialogueGraphContainer prevState, out DialogueGraphContainer newState)
        {
            newState = ScriptableObject.CreateInstance<DialogueGraphContainer>();
            SaveNodes(newState, view);

            return true;
        }

        private static bool SaveNodes(DialogueGraphContainer container, DialogueGraphView view)
        {
            container.Links = new List<LinkData>();
            container.Nodes = new List<NodeData>();

            List<Edge> edges = view.edges.ToList().FindAll(x => x.input.node != null);
            for (int i = 0; i < edges.Count(); i++)
            {
                GraphNode outputNode = (edges[i].output.node as GraphNode);
                GraphNode inputNode = (edges[i].input.node as GraphNode);

                LinkData linkData = new LinkData();
                linkData.FromGUID = outputNode.GUID;
                linkData.FromPortName = edges[i].output.portName;
                linkData.ToGUID = inputNode.GUID;
                linkData.ToPortName = edges[i].input.portName;
                    
                container.Links.Add(linkData);
            }

            foreach (Node node in view.nodes)
            {
                if (node is GraphNode graphNode)
                {
                    container.Nodes.Add(GetData(graphNode));
                }
            }

            return true;
        }

        private static NodeData GetData(GraphNode node)
        {
            NodeData data = new NodeData();

            data.Type = node.GetType().ToString();
            data.Position = node.GetPosition().position;
            
            data.Fields = new List<NodeData.Field>();
            foreach (FieldInfo field in node.GetType().GetFields())
            {
                NodeData.Field fieldData = new NodeData.Field
                {
                    Name = field.Name,
                    Type = field.FieldType.ToString(),
                    Assembly = field.FieldType.Assembly.FullName
                };

                if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    fieldData.UnityObjectValue = field.GetValue(node) as UnityEngine.Object;
                }
                else if (field.CustomAttributes.Any(x => x.AttributeType == typeof(JsonRequiredAttribute)))
                {
                    fieldData.Value = JsonUtility.ToJson(field.GetValue(node));
                    fieldData.IsJson = true;
                }
                else
                {
                    fieldData.Value = field.GetValue(node)?.ToString();
                    fieldData.IsJson = false;
                }
                
                data.Fields.Add(fieldData);
            }

            return data;
        }
    }
}
