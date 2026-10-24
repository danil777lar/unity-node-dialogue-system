using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow _window;
        private DialogueGraphView _graphView;

        private Texture2D _indentationIcon;
        
        public void Configure(EditorWindow window,DialogueGraphView graphView)
        {
            _window = window;
            _graphView = graphView;
            
            _indentationIcon = new Texture2D(1,1);
            _indentationIcon.SetPixel(0,0,new Color(0,0,0,0));
            _indentationIcon.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                
                new SearchTreeEntry(new GUIContent("Dialogue Node", _indentationIcon))
                {
                    level = 1, userData = new DialogueGraphNode()
                },
                
                new SearchTreeEntry(new GUIContent("Enter Node", _indentationIcon))
                {
                    level = 1, userData = new EnterGraphNode()
                },
                
                new SearchTreeEntry(new GUIContent("Trigger Node", _indentationIcon))
                {
                    level = 1, userData = new TriggerGraphNode()
                },

                new SearchTreeEntry(new GUIContent("Exit Node", _indentationIcon))
                {
                    level = 1, userData = new ExitGraphNode()
                },
                
                new SearchTreeEntry(new GUIContent("Event Node", _indentationIcon))
                {
                    level = 1, userData = new EventGraphNode()
                },
                
                new SearchTreeEntry(new GUIContent("Condition Node", _indentationIcon))
                {
                    level = 1, userData = new ConditionGraphNode()
                },
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            
            Vector2 graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);
            
            switch (SearchTreeEntry.userData)
            {
                case GraphNode node:
                    _graphView.AddNode(node.Initialize(graphMousePosition, _graphView.nodes.ToList()));
                    return true;
            }
            
            return false;
        }
    }
}
