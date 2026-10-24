using Larje.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Larje.Core;
using Larje.Dialogue.Runtime.Converted;
using Larje.Dialogue.Runtime.Graph.Data;
using UnityEngine;

namespace Larje.Dialogue.Runtime.Graph
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Larje/Dialogue/Dialogue Graph")]
    public class DialogueGraphContainer : ScriptableObject
    {
        private const string TYPE_DIALOGUE = "DialogueGraphNode";
        private const string TYPE_ENTER = "EnterGraphNode";
        private const string TYPE_EXIT = "ExitGraphNode";
        private const string TYPE_EVENT = "EventGraphNode";
        private const string TYPE_TRIGGER = "TriggerGraphNode";
        private const string TYPE_CONDITION = "ConditionGraphNode";
        
        public List<LinkData> Links = new List<LinkData>();
        public List<NodeData> Nodes = new List<NodeData>();
        
        public DialogueStep GetFirstStep(Action<string> sendEvent, Action<TriggerId> sendTrigger, Func<string, bool> checkCondition, string language)
        {
            NodeData enter0 = Nodes.Find(x => 
                x.IsTypeOf(TYPE_ENTER) && x.GetField<int>("EnterIndex") == 0);
            
            LinkData link = Links.Find(x => x.FromGUID == enter0.GetField<string>("GUID"));
            
            string nextNodeGuid = FindNextStep(link.ToGUID, sendEvent, sendTrigger, checkCondition);
            NodeData nextNode = GuidToNode(nextNodeGuid);
            
            return NodeToStep(nextNode, language);
        }
        
        public DialogueStep GetNextStep(string id, int choiceId, Action<string> sendEvent, Action<TriggerId> sendTrigger, Func<string, bool> checkCondition, string language)
        {
            NodeData node = Nodes[int.Parse(id)];
            LinkData link = Links.Find(x => 
                x.FromGUID == node.GetField<string>("GUID") && x.FromPortName == choiceId.ToString());
            
            string nextNodeGuid = FindNextStep(link.ToGUID, sendEvent, sendTrigger, checkCondition);
            NodeData nextNode = GuidToNode(nextNodeGuid);
            
            return NodeToStep(nextNode, language);
        }

        public bool Compare(DialogueGraphContainer other)
        {
            return Nodes.SequenceEqual(other.Nodes) && Links.SequenceEqual(other.Links);
        }

        private string FindNextStep(string guid, Action<string> sendEvent, Action<TriggerId> sendTrigger, Func<string, bool> checkCondition)
        {
            NodeData node = GuidToNode(guid);

            if (string.IsNullOrEmpty(guid))
            {
                return "";
            }
            
            if (node.IsTypeOf(TYPE_DIALOGUE))
            {
                return guid;
            }

            string next = "";
            
            if (node.IsTypeOf(TYPE_EVENT))
            {
                sendEvent?.Invoke(node.GetField<string>("EventName"));
                
                LinkData link = Links.Find(x => x.FromGUID == guid);
                next = link != null ? link.ToGUID : "";
            }

            if (node.IsTypeOf(TYPE_TRIGGER))
            {
                sendTrigger?.Invoke(node.GetField<TriggerId>("Trigger"));
                
                LinkData link = Links.Find(x => x.FromGUID == guid);
                next = link != null ? link.ToGUID : "";
            }
            
            if (node.IsTypeOf(TYPE_CONDITION))
            {
                LinkData linkTrue = Links.Find(x => x.FromGUID == guid && x.FromPortName == "True");
                LinkData linkFalse = Links.Find(x => x.FromGUID == guid && x.FromPortName == "False");

                string conditionName = node.GetField<string>("ConditionName");
                next = checkCondition.Invoke(conditionName) ? linkTrue.ToGUID : linkFalse.ToGUID; 
            }
            
            if (node.IsTypeOf(TYPE_EXIT))
            {
                int exitIndex = node.GetField<int>("ExitIndex");
                NodeData enterNode = Nodes.Find(x => 
                    x.IsTypeOf(TYPE_ENTER) && x.GetField<int>("EnterIndex") == exitIndex);

                next = enterNode != null ? enterNode.GetField<string>("GUID") : ""; 
            }
            
            if (node.IsTypeOf(TYPE_ENTER))
            {
                LinkData link = Links.Find(x => x.FromGUID == guid);
                next = link != null ? link.ToGUID : ""; 
            }
            
            return FindNextStep(next, sendEvent, sendTrigger, checkCondition);
        }
        
        private string GuidToId(string guid)
        {
            foreach (NodeData nodeData in Nodes)
            {
                if (nodeData.GetField<string>("GUID") == guid)
                {
                    return Nodes.IndexOf(nodeData).ToString();
                }
            }

            return null;
        }
        
        private NodeData GuidToNode(string guid)
        {
            return Nodes.Find(x => x.GetField<string>("GUID") == guid);
        }

        private DialogueStep NodeToStep(NodeData node, string language)
        {
            if (node != null && node.IsTypeOf(TYPE_DIALOGUE))
            {
                DialogueStep step = new DialogueStep();
                DialogueContent content = node.GetField("Content") as DialogueContent;

                step.Id = GuidToId(node.GetField<string>("GUID"));
                step.Title = content.GetLocalization(language).InterlocutorSpeech.Title;
                step.Text = content.GetLocalization(language).InterlocutorSpeech.Text;

                step.Choices = new List<DialogueStep.Choice>();
                foreach (LinkData nodeLink in Links)
                {
                    if (nodeLink.FromGUID == node.GetField<string>("GUID"))
                    {
                        int index = int.Parse(nodeLink.FromPortName);
                        DialogueStep.Choice choice = new DialogueStep.Choice
                        {
                            Id = index,
                            Title = content.GetLocalization(language).PlayerChoices[index].PlayerSpeech.Title,
                            Text = content.GetLocalization(language).PlayerChoices[index].PlayerSpeech.Text,
                            Condition = content.GetLocalization(language).PlayerChoices[index].PlayerSpeech.Condition
                        };
                        step.Choices.Add(choice);
                    }
                }
                step.Choices = step.Choices.OrderBy(x => x.Id).ToList();

                return step;
            }

            return null;
        }
    }
}
