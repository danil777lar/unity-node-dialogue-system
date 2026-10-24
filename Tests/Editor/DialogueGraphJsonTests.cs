using System.Collections.Generic;
using Larje.Core.Events;
using Larje.Dialogue.Editor.Utility;
using Larje.Dialogue.Runtime.Converted;
using Larje.Dialogue.Runtime.Graph;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Larje.Dialogue.Tests
{
    public sealed class DialogueGraphJsonTests
    {
        private const string GRAPH = @"{
            ""nodes"": [
                { ""id"": ""start"", ""type"": ""enter"", ""enterIndex"": 0 },
                { ""id"": ""greet"", ""type"": ""dialogue"", ""localizations"": [
                    { ""language"": ""en"", ""title"": ""Guard"", ""text"": ""Halt!"", ""choices"": [ { ""title"": ""Friend"", ""text"": ""A friend."" }, { ""title"": ""Leave"", ""text"": ""Nobody."" } ] },
                    { ""language"": ""ru"", ""title"": ""Стражник"", ""text"": ""Стой!"", ""choices"": [ { ""title"": ""Друг"", ""text"": ""Друг."" }, { ""title"": ""Уйти"", ""text"": ""Никто."" } ] } ] },
                { ""id"": ""check"", ""type"": ""condition"", ""conditionName"": ""has_pass"" },
                { ""id"": ""open"", ""type"": ""event"", ""eventName"": ""open_gate"" },
                { ""id"": ""pass"", ""type"": ""dialogue"", ""localizations"": [ { ""language"": ""en"", ""title"": ""Guard"", ""text"": ""Pass."", ""choices"": [] } ] },
                { ""id"": ""deny"", ""type"": ""dialogue"", ""localizations"": [ { ""language"": ""en"", ""title"": ""Guard"", ""text"": ""Go away."", ""choices"": [] } ] }
            ],
            ""links"": [
                { ""from"": ""start"", ""port"": ""Out"", ""to"": ""greet"" },
                { ""from"": ""greet"", ""port"": ""0"", ""to"": ""check"" },
                { ""from"": ""check"", ""port"": ""True"", ""to"": ""open"" },
                { ""from"": ""check"", ""port"": ""False"", ""to"": ""deny"" },
                { ""from"": ""open"", ""port"": ""Out"", ""to"": ""pass"" }
            ]
        }";

        [Test]
        public void ImportedGraphWalksWithLocalizationEventsAndConditions()
        {
            DialogueGraphContainer graph = Import(GRAPH);
            List<string> events = new List<string>();

            DialogueStep first = graph.GetFirstStep(events.Add, IgnoreTrigger, x => true, "ru");
            Assert.AreEqual("Стражник", first.Title);
            Assert.AreEqual(1, first.Choices.Count, "choices without an outgoing link are not offered");
            Assert.AreEqual("Друг", first.Choices[0].Title);

            DialogueStep passed = graph.GetNextStep(first.Id, 0, events.Add, IgnoreTrigger, x => x == "has_pass", "en");
            Assert.AreEqual("Pass.", passed.Text);
            CollectionAssert.AreEqual(new[] { "open_gate" }, events);

            DialogueStep denied = graph.GetNextStep(first.Id, 0, events.Add, IgnoreTrigger, x => false, "en");
            Assert.AreEqual("Go away.", denied.Text);

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void ExportRoundTripsNodeCountsAndText()
        {
            DialogueGraphContainer graph = Import(GRAPH);
            JObject exported = DialogueGraphJson.Export(graph);
            DialogueGraphContainer copy = Import(exported.ToString());

            Assert.AreEqual(graph.Nodes.Count, copy.Nodes.Count);
            Assert.AreEqual(graph.Links.Count, copy.Links.Count);
            Assert.AreEqual("Halt!", copy.GetFirstStep(null, IgnoreTrigger, x => true, "en").Text);

            Object.DestroyImmediate(graph);
            Object.DestroyImmediate(copy);
        }

        [Test]
        public void UnknownNodeTypeIsRejected()
        {
            DialogueGraphContainer graph = ScriptableObject.CreateInstance<DialogueGraphContainer>();
            Assert.Throws<System.ArgumentException>(() => DialogueGraphJson.Import(graph, JObject.Parse(@"{ ""nodes"": [ { ""id"": ""a"", ""type"": ""teleport"" } ] }")));
            Object.DestroyImmediate(graph);
        }

        private DialogueGraphContainer Import(string json)
        {
            DialogueGraphContainer graph = ScriptableObject.CreateInstance<DialogueGraphContainer>();
            DialogueGraphJson.Import(graph, JObject.Parse(json));
            return graph;
        }

        private void IgnoreTrigger(TriggerId trigger)
        {
        }
    }
}
