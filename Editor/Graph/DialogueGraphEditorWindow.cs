using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Larje.Dialogue.Editor.Utility;
using Larje.Dialogue.Runtime.Graph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.Dialogue.Editor
{
    public class DialogueGraphEditorWindow : EditorWindow
    {
        private string _fileName = "New Narrative";
        private string _assetPath = "";

        private Button _undoButton;
        private Button _redoButton;
        
        private TwoPaneSplitView _splitView;
        private VisualElement _splitLeft;
        private VisualElement _splitRight;
        
        private DialogueGraphView _graphView;
        private DialogueGraphContainer _dialogueContainer;
        
        [OnOpenAsset(1)]
        public static bool OpenGraphAsset(int instanceID, int line)
        {
#if UNITY_6000_4_OR_NEWER
            UnityEngine.Object asset = EditorUtility.EntityIdToObject(instanceID);
#else
            UnityEngine.Object asset = EditorUtility.InstanceIDToObject(instanceID);
#endif
            if (!(asset is DialogueGraphContainer))
            {
                return false;
            }
            
            bool windowIsOpen = EditorWindow.HasOpenInstances<DialogueGraphEditorWindow>();
            if (!windowIsOpen)
            {
                EditorWindow.CreateWindow<DialogueGraphEditorWindow>();
            }
            else
            {
                EditorWindow.FocusWindowIfItsOpen<DialogueGraphEditorWindow>();
            }
            
            GetWindow<DialogueGraphEditorWindow>()
                .Initialize(AssetDatabase.GetAssetPath(asset));

            return true;
        }

        private void Initialize(string assetPath)
        {
            _assetPath = assetPath;
            _fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            titleContent = new GUIContent(_fileName);
            
            Draw();
            LoadGraph();
        }

        private void Update()
        {
            _undoButton.SetEnabled(_graphView.CanUndo);
            _redoButton.SetEnabled(_graphView.CanRedo);            
            _graphView?.Update();
        }

        private void Draw()
        {
            ClearVisualElement();
            ConstructGraphView();
            GenerateToolbar();
        }
        
        private void OnDisable()
        {
            ClearVisualElement();
        }

        private void OnEnable()
        {
            Draw();
        }
        
        private void ClearVisualElement()
        {
            rootVisualElement.Clear();
            
            _splitView = new TwoPaneSplitView(1, 250, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(_splitView);
            
            _splitLeft = new VisualElement();
            _splitView.Add(_splitLeft);
            
            _splitRight = new VisualElement();
            _splitView.Add(_splitRight);
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView(this, _assetPath, _splitRight)
            {
                name = _fileName,
            };
            _graphView.StretchToParentSize();
            _splitLeft.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            
            toolbar.Add(new Button(SaveGraph) {text = "Save"});
            
            _undoButton = new Button(_graphView.Undo) {text = "<-"};
            toolbar.Add(_undoButton);
            
            _redoButton = new Button(_graphView.Redo) {text = "->"};
            toolbar.Add(_redoButton);
            
            
            _splitLeft.Add(toolbar);
        }
        
        private void LoadGraph()
        {
            _graphView.Load();
        }

        private void SaveGraph()
        {
            _graphView.Save();
        }

        private void GenerateMiniMap()
        {
            MiniMap miniMap = new MiniMap {anchored = true};
            Vector2 cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }
    }
}