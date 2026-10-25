using System;
using System.IO;
using Larje.Core.Editor.Cli;
using Larje.Dialogue.Editor.Utility;
using Larje.Dialogue.Runtime.Graph;
using Newtonsoft.Json.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace Larje.Dialogue.Editor.Cli
{
    public static class DialogueCommands
    {
        private const string GROUP = "larje.dialogue";
        private const string READ_DESCRIPTION = "Reads a Larje dialogue graph asset as JSON: nodes {id, type: dialogue|enter|exit|event|trigger|condition, position, enterIndex, exitIndex, eventName, trigger (TriggerId key), conditionName, localizations[{language,title,text,choices[{title,text,condition}]}]} and links {from, port, to}. Dialogue choice ports are '0','1',...; condition ports 'True'/'False'; other outputs 'Out'.";
        private const string WRITE_DESCRIPTION = "Creates or replaces a Larje dialogue graph asset from JSON in the same format as larje.dialogue read, passed inline with --json or as a file path with --file. Every graph needs an enter node with enterIndex 0 linked to the first dialogue node. Node ids are free-form strings used only for links. Returns the saved graph.";

        [CliGroupCommand(GROUP, READ_DESCRIPTION, Tags = new[] { "larje", "assets" })]
        public static object Read(
            [CliArg("path", "DialogueGraphContainer asset path", Required = true)] string path)
        {
            DialogueGraphContainer container = AssetDatabase.LoadAssetAtPath<DialogueGraphContainer>(path);
            if (container == null)
            {
                throw new ArgumentException($"Dialogue graph not found at '{path}'");
            }

            return DialogueGraphJson.Export(container);
        }

        [CliGroupCommand(GROUP, WRITE_DESCRIPTION, Tags = new[] { "larje", "assets" })]
        public static object Write(
            [CliArg("path", "DialogueGraphContainer asset path; created if missing", Required = true)] string path,
            [CliArg("json", "Graph JSON with nodes and links")] string json = null,
            [CliArg("file", "Path to a file with the graph JSON, relative to the project root")] string file = null)
        {
            JObject graph;
            try
            {
                graph = JObject.Parse(CliText.Resolve(json, file, "json"));
            }
            catch (Newtonsoft.Json.JsonException exception)
            {
                throw new ArgumentException("Invalid JSON: " + exception.Message);
            }

            DialogueGraphContainer container = AssetDatabase.LoadAssetAtPath<DialogueGraphContainer>(path);
            bool created = container == null;
            if (created)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                container = ScriptableObject.CreateInstance<DialogueGraphContainer>();
                AssetDatabase.CreateAsset(container, path);
            }

            DialogueGraphJson.Import(container, graph);
            AssetDatabase.SaveAssets();
            return new { created, graph = DialogueGraphJson.Export(container) };
        }
    }
}
