using System;
using System.Collections.Generic;
using System.Reflection;
using Larje.Core.Events;
using Larje.Dialogue.Runtime.Graph;
using Larje.Dialogue.Runtime.Graph.Data;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Larje.Dialogue.Editor.Utility
{
    public static class DialogueGraphJson
    {
        private const string SUFFIX = "GraphNode";
        private const float COLUMN_WIDTH = 320f;
        private const float ROW_HEIGHT = 220f;

        public static JObject Export(DialogueGraphContainer container)
        {
            JArray nodes = new JArray();
            foreach (NodeData node in container.Nodes)
            {
                JObject entry = new JObject
                {
                    ["id"] = node.GetField<string>("GUID"),
                    ["type"] = ShortType(node.Type),
                    ["position"] = new JArray(node.Position.x, node.Position.y)
                };

                foreach (NodeData.Field field in node.Fields)
                {
                    if (field.Name == "GUID")
                    {
                        entry["id"] = field.Value;
                    }
                    else if (field.Name == "Content")
                    {
                        entry["localizations"] = ExportContent(node.GetField("Content") as DialogueContent);
                    }
                    else if (field.UnityObjectValue is TriggerId trigger)
                    {
                        entry[ToCamel(field.Name)] = trigger.Key;
                    }
                    else if (!field.IsJson && field.UnityObjectValue == null)
                    {
                        entry[ToCamel(field.Name)] = field.Value;
                    }
                }

                nodes.Add(entry);
            }

            JArray links = new JArray();
            foreach (LinkData link in container.Links)
            {
                links.Add(new JObject { ["from"] = link.FromGUID, ["port"] = link.FromPortName, ["to"] = link.ToGUID });
            }

            return new JObject { ["nodes"] = nodes, ["links"] = links };
        }

        public static void Import(DialogueGraphContainer container, JObject graph)
        {
            Dictionary<string, string> guids = new Dictionary<string, string>();
            Dictionary<string, Type> types = new Dictionary<string, Type>();
            List<NodeData> nodes = new List<NodeData>();
            JArray sourceNodes = (JArray)graph["nodes"] ?? new JArray();

            for (int i = 0; i < sourceNodes.Count; i++)
            {
                JObject source = (JObject)sourceNodes[i];
                string id = (string)source["id"] ?? ("node" + i);
                Type nodeType = ResolveNodeType((string)source["type"]);
                if (nodeType == null)
                {
                    throw new ArgumentException($"Unknown node type '{source["type"]}'. Use: dialogue, enter, exit, event, trigger, condition");
                }

                string guid = Guid.NewGuid().ToString();
                guids[id] = guid;
                types[id] = nodeType;
                nodes.Add(CreateNode(nodeType, guid, source, i));
            }

            List<LinkData> links = new List<LinkData>();
            foreach (JObject link in (JArray)graph["links"] ?? new JArray())
            {
                string from = (string)link["from"];
                string to = (string)link["to"];
                if (!guids.ContainsKey(from) || !guids.ContainsKey(to))
                {
                    throw new ArgumentException($"Link {from} -> {to} references an unknown node id");
                }

                links.Add(new LinkData
                {
                    FromGUID = guids[from],
                    FromPortName = (string)link["port"] ?? "Out",
                    ToGUID = guids[to],
                    ToPortName = ShortType(types[to].Name) == "dialogue" ? "Input" : "In"
                });
            }

            Undo.RecordObject(container, "Import Dialogue Graph");
            container.Nodes = nodes;
            container.Links = links;
            EditorUtility.SetDirty(container);
        }

        private static NodeData CreateNode(Type nodeType, string guid, JObject source, int index)
        {
            JArray position = source["position"] as JArray;
            NodeData data = new NodeData
            {
                Type = nodeType.ToString(),
                Position = position != null && position.Count >= 2
                    ? new Vector2((float)position[0], (float)position[1])
                    : new Vector2(index % 4 * COLUMN_WIDTH, index / 4 * ROW_HEIGHT),
                Fields = new List<NodeData.Field>()
            };

            foreach (FieldInfo field in nodeType.GetFields())
            {
                NodeData.Field fieldData = new NodeData.Field
                {
                    Name = field.Name,
                    Type = field.FieldType.ToString(),
                    Assembly = field.FieldType.Assembly.FullName
                };

                if (field.Name == "GUID")
                {
                    fieldData.Value = guid;
                }
                else if (field.FieldType == typeof(DialogueContent))
                {
                    fieldData.IsJson = true;
                    fieldData.Value = BuildContentJson(source["localizations"] as JArray);
                }
                else if (typeof(TriggerId).IsAssignableFrom(field.FieldType))
                {
                    fieldData.UnityObjectValue = FindTrigger((string)source[ToCamel(field.Name)]);
                }
                else
                {
                    JToken token = source[ToCamel(field.Name)] ?? source[field.Name];
                    fieldData.Value = token != null ? token.ToString() : DefaultValue(field.FieldType);
                }

                data.Fields.Add(fieldData);
            }

            return data;
        }

        private static JArray ExportContent(DialogueContent content)
        {
            JArray result = new JArray();
            if (content == null)
            {
                return result;
            }

            foreach (DialogueLocalization localization in content.GetAll())
            {
                JArray choices = new JArray();
                foreach (DialogueChoice choice in localization.PlayerChoices)
                {
                    choices.Add(new JObject { ["title"] = choice.PlayerSpeech.Title, ["text"] = choice.PlayerSpeech.Text, ["condition"] = choice.PlayerSpeech.Condition });
                }

                result.Add(new JObject
                {
                    ["language"] = localization.LanguageCode,
                    ["title"] = localization.InterlocutorSpeech.Title,
                    ["text"] = localization.InterlocutorSpeech.Text,
                    ["choices"] = choices
                });
            }

            return result;
        }

        private static string BuildContentJson(JArray localizations)
        {
            JArray result = new JArray();
            foreach (JObject localization in localizations ?? new JArray { new JObject { ["language"] = "en" } })
            {
                JArray choices = new JArray();
                foreach (JObject choice in (JArray)localization["choices"] ?? new JArray())
                {
                    choices.Add(new JObject { ["PlayerSpeech"] = Speech(choice) });
                }

                result.Add(new JObject
                {
                    ["LanguageCode"] = (string)localization["language"] ?? "en",
                    ["InterlocutorSpeech"] = Speech(localization),
                    ["PlayerChoices"] = choices
                });
            }

            return new JObject { ["Localizations"] = result }.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static JObject Speech(JObject source)
        {
            return new JObject
            {
                ["Title"] = (string)source["title"] ?? string.Empty,
                ["Text"] = (string)source["text"] ?? string.Empty,
                ["Condition"] = (string)source["condition"] ?? string.Empty
            };
        }

        private static TriggerId FindTrigger(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(TriggerId)))
            {
                TriggerId trigger = AssetDatabase.LoadAssetAtPath<TriggerId>(AssetDatabase.GUIDToAssetPath(guid));
                if (trigger != null && (trigger.Key == key || trigger.name == key))
                {
                    return trigger;
                }
            }

            throw new ArgumentException($"TriggerId '{key}' not found");
        }

        private static Type ResolveNodeType(string shortName)
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<GraphNode>())
            {
                if (!type.IsAbstract && string.Equals(ShortType(type.Name), shortName, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }

            return null;
        }

        private static string ShortType(string typeName)
        {
            string name = typeName.Substring(typeName.LastIndexOf('.') + 1);
            if (name.EndsWith(SUFFIX))
            {
                name = name.Substring(0, name.Length - SUFFIX.Length);
            }

            return name.ToLowerInvariant();
        }

        private static string ToCamel(string name)
        {
            return string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private static string DefaultValue(Type type)
        {
            return type == typeof(int) ? "0" : string.Empty;
        }
    }
}
