using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Larje.Dialogue.Editor.Utility;
using Larje.Dialogue.Runtime.Graph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Larje.Dialogue.Editor
{
    public class DialogueGraphView : GraphView
    {
        private string _assetPath;
        private VisualElement _nodeDataPanel;
        private NodeSearchWindow _searchWindow;
        private DialogueGraphContainer _savedRecord;
        private DialogueGraphContainer _currentRecord;
        private GraphNode _selectedNode;
        private Dictionary<DialogueGraphContainer, string> _undoStack = new Dictionary<DialogueGraphContainer, string>();
        
        public int CurrentRecordIndex => _undoStack.Keys.ToList().IndexOf(_currentRecord);
        public bool CanUndo => _undoStack.Count > 0 && _currentRecord != null && _currentRecord != _savedRecord;
        public bool CanRedo => _currentRecord != null && CurrentRecordIndex < _undoStack.Count - 1;

        public DialogueGraphView(DialogueGraphEditorWindow editorWindow, string assetPath, VisualElement nodeDataPanel)
        {
            _assetPath = assetPath;
            _nodeDataPanel = nodeDataPanel;
            
            StyleSheet styleSheet = Resources.Load<StyleSheet>("DialogueGraph/GraphView");
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow(editorWindow);

            this.graphViewChanged += Record;
        }

        public void AddNode(GraphNode node)
        {
            node.EventRemovePort += OnPortRemoved;
            AddElement(node);
            
            node.RefreshPorts();
            node.RefreshExpandedState();
        }
        
        public void AddEdge(Edge edge)
        {
            Add(edge);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            Port startPortView = startPort;

            ports.ForEach((port) =>
            {
                Port portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }
        
        public void Save()
        {
            _savedRecord = DialogueGraphSaver.SaveGraph(this, _assetPath);
            _undoStack.Clear();
            _currentRecord = _savedRecord;
        }

        public void Load()
        {
            _savedRecord = DialogueGraphLoader.LoadGraph(this, _assetPath);
            _undoStack.Clear();
            _currentRecord = _savedRecord;
        }

        public GraphViewChange Record(GraphViewChange change)
        {
            DialogueGraphContainer newState;
            if (DialogueGraphSaver.TrySaveState(this, _currentRecord, out newState))
            {
                while (CurrentRecordIndex < _undoStack.Count - 1)
                {
                    _undoStack.Remove(_undoStack.Keys.Last());
                }
                
                _undoStack.Add(newState, "change");
                _currentRecord = newState;
            }

            return change;
        }
        
        public void Undo()
        {
            if (CanUndo)
            {
                if (CurrentRecordIndex > 0)
                {
                    _currentRecord = _undoStack.Keys.ToList()[CurrentRecordIndex - 1];
                }
                else
                {
                    _currentRecord = _savedRecord;
                }
                DialogueGraphLoader.LoadState(this, _currentRecord);
            }            
        }

        public void Redo()
        {
            if (CanRedo)
            {
                _currentRecord = _undoStack.Keys.ToList()[CurrentRecordIndex + 1];
                DialogueGraphLoader.LoadState(this, _currentRecord);
            }
        }

        public void Update()
        {
            List<GraphNode> selectedNodes = selection.FindAll(x => x is GraphNode).Cast<GraphNode>().ToList();
            if (selectedNodes != null && selectedNodes.Count == 1)
            {
                if (_selectedNode != selectedNodes.First())
                {
                    _selectedNode = selectedNodes.First();
                    _selectedNode.DrawPanelUI(_nodeDataPanel);
                }
            }
            else
            {
                _selectedNode = null;
                _nodeDataPanel.Clear();
            }
        }

        private void OnPortRemoved(Port port)
        {
            IEnumerable<Edge> targetEdge = edges.ToList()
                .Where(x => x.output.portName == port.portName && x.output.node == port.node);
            
            if (targetEdge.Any())
            {
                Edge edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }
        }

        private void AddSearchWindow(DialogueGraphEditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }
    }
}