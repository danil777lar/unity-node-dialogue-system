using System;
using System.Collections.Generic;
using System.Linq;
using Larje.Dialogue.Runtime.Graph.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    [Serializable]
    public abstract class GraphNode : Node
    {
        public const int DEFAULT_NODE_WIDTH = 200;
        public const int DEFAULT_NODE_HEIGHT = 150;
        public const string STYLESHEET_PATH = "DialogueGraph/";
        
        public string GUID;

        protected GraphNodePanel _panel;

        public event Action<Port> EventRemovePort;
        
        public abstract string DefaultName { get; }
        protected abstract string StyleSheetName { get; }
        public virtual GraphNodePanel GetPanelInstance => null;

        public virtual GraphNode Initialize(Vector2 position, List<Node> allNodes)
        {
            LoadStyleSheet();

            _panel = GetPanelInstance;
            title = DefaultName;
            
            GUID = Guid.NewGuid().ToString();
            SetPosition(new Rect(position, new Vector2(DEFAULT_NODE_WIDTH, DEFAULT_NODE_HEIGHT)));
            
            return this;
        }

        public virtual GraphNode Load(Vector2 position)
        {
            _panel = GetPanelInstance;
            
            LoadStyleSheet();
            
            SetPosition(new Rect(position, new Vector2(DEFAULT_NODE_WIDTH, DEFAULT_NODE_HEIGHT)));
            return this;
        }

        public virtual void DrawPanelUI(VisualElement root)
        {
            root.Clear();
            _panel?.Draw(root);
        }
        
        protected void RemovePort(Port port)
        {
            EventRemovePort?.Invoke(port);  
            outputContainer.Remove(port);
            RefreshPorts();
            RefreshExpandedState();
        }

        protected void LoadStyleSheet()
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>(STYLESHEET_PATH + StyleSheetName);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
                Debug.LogWarning($"Stylesheet {STYLESHEET_PATH + StyleSheetName} loaded!");
            }
            else
            {
                Debug.LogWarning($"Stylesheet {STYLESHEET_PATH + StyleSheetName} was not found!");
            }
        }
    }
}
