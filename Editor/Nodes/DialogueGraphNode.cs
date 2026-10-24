using Larje.Dialogue.Runtime.Graph;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Larje.Dialogue.Runtime.Graph.Data;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    [Serializable]
    public class DialogueGraphNode : GraphNode
    {
        [JsonRequired] public DialogueContent Content; 
            
        
        public override string DefaultName => "Dialogue Step";
        protected override string StyleSheetName => "DialogueGraphNode";
        public override GraphNodePanel GetPanelInstance => new DialogueGraphNodePanel(this);

        public override GraphNode Initialize(Vector2 position, List<Node> allNodes)
        {
            base.Initialize(position, allNodes);

            Content = new DialogueContent();
            Content.Init();
            
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
            title = Content.GetLocalization(0).InterlocutorSpeech.Title;
        }
        
        public void UpdatePorts()
        {
            int index = 0;

            List<DialogueChoice> choices = Content.GetLocalization(0).PlayerChoices;
            while (outputContainer.childCount > choices.Count)
            {
                Port portToRemove = outputContainer[outputContainer.childCount - 1] as Port; 
                RemovePort(portToRemove);
            }

            index = 0;
            while (outputContainer.childCount < choices.Count)
            {
                AddPort("Choice", index);
                index++;
            }

            index = 0;
            foreach (DialogueChoice choice in choices)
            {
                Port port = outputContainer[index] as Port;
                if (port != null)
                {
                    port.name = index.ToString();
                    port.portName = index.ToString();
                    port.portColor = string.IsNullOrEmpty(choice.PlayerSpeech.Condition) ? Color.aliceBlue : Color.lightSlateBlue;
                    
                    RenamePort(port, choice.PlayerSpeech.Title);
                }

                index++;
            }
            
            RefreshPorts();
        }
        
        private void DrawUI()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portColor = Color.white;
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);
            
            UpdateTitle();
            UpdatePorts();
            
            RefreshExpandedState();
        }

        private void AddPort(string choice, int index)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portColor = Color.white;
            
            RenamePort(port, choice);

            int outputPortCount = outputContainer.Query("connector").ToList().Count();
            string outputPortName = index.ToString();
            port.portName = outputPortName;
            
            Label label = new Label(outputPortName);
            port.contentContainer.Add(label);
            outputContainer.Add(port);

            RefreshPorts();
            RefreshExpandedState();
        }
        
        private void RenamePort(Port port, string text)
        {
            List<VisualElement> labels = new List<VisualElement>();
            for (int i = 0; i < port.contentContainer.childCount; i++)
            {
                if (port.contentContainer[i] is Label)
                {
                    labels.Add(port.contentContainer[i]);
                }
            }
            labels.ForEach((x) => port.contentContainer.Remove(x));

            port.contentContainer.Add(new Label(text));
        }
    }
}