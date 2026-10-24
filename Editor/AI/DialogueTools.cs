using System;
using System.IO;
using Larje.Dialogue.Editor.Utility;
using Larje.Dialogue.Runtime.Graph;
using Newtonsoft.Json.Linq;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEditor;
using UnityEngine;

namespace Larje.Dialogue.Editor.AI
{
    public static class DialogueTools
    {
        private const string READ_DESCRIPTION = "Reads a Larje dialogue graph asset as JSON: nodes {id, type: dialogue|enter|exit|event|trigger|condition, position, enterIndex, exitIndex, eventName, trigger (TriggerId key), conditionName, localizations[{language,title,text,choices[{title,text,condition}]}]} and links {from, port, to}. Dialogue choice ports are '0','1',...; condition ports 'True'/'False'; other outputs 'Out'.";
        private const string WRITE_DESCRIPTION = "Creates or replaces a Larje dialogue graph asset from JSON in the same format as Larje.ReadDialogue. Every graph needs an enter node with enterIndex 0 linked to the first dialogue node. Node ids are free-form strings used only for links. Undo-able; returns the saved graph.";

        [McpTool("Larje.ReadDialogue", READ_DESCRIPTION, "Read Larje Dialogue", EnabledByDefault = true, Groups = new[] { "larje", "larje-assets" })]
        public static object Read(ReadParams parameters)
        {
            DialogueGraphContainer container = AssetDatabase.LoadAssetAtPath<DialogueGraphContainer>(parameters.AssetPath);
            if (container == null)
            {
                return Response.Error($"Dialogue graph not found at '{parameters.AssetPath}'");
            }

            return Response.Success(container.name, DialogueGraphJson.Export(container).ToString());
        }

        [McpTool("Larje.WriteDialogue", WRITE_DESCRIPTION, "Write Larje Dialogue", EnabledByDefault = true, Groups = new[] { "larje", "larje-assets" })]
        public static object Write(WriteParams parameters)
        {
            JObject graph;
            try
            {
                graph = JObject.Parse(parameters.Json);
            }
            catch (Exception exception)
            {
                return Response.Error("Invalid JSON: " + exception.Message);
            }

            DialogueGraphContainer container = AssetDatabase.LoadAssetAtPath<DialogueGraphContainer>(parameters.AssetPath);
            bool created = container == null;
            if (created)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(parameters.AssetPath));
                container = ScriptableObject.CreateInstance<DialogueGraphContainer>();
                AssetDatabase.CreateAsset(container, parameters.AssetPath);
            }

            try
            {
                DialogueGraphJson.Import(container, graph);
            }
            catch (ArgumentException exception)
            {
                return Response.Error(exception.Message);
            }

            AssetDatabase.SaveAssets();
            return Response.Success(created ? "Created" : "Updated", DialogueGraphJson.Export(container).ToString());
        }

        public sealed class ReadParams
        {
            [McpDescription("DialogueGraphContainer asset path", Required = true)]
            public string AssetPath { get; set; }
        }

        public sealed class WriteParams
        {
            [McpDescription("DialogueGraphContainer asset path; created if missing", Required = true)]
            public string AssetPath { get; set; }

            [McpDescription("Graph JSON with nodes and links", Required = true)]
            public string Json { get; set; }
        }
    }
}
