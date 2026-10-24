using System;
using System.Reflection;
using Larje.Dialogue.Runtime.Graph;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Larje.Dialogue.Editor
{
    [CustomEditor(typeof(DialogueGraphContainer))]
    public class DialogueContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
}
