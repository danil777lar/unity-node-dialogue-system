# Larje Dialogue

`com.larje.dialogue`: node-based dialogue graphs with a graph editor, localized lines and choices, events, triggers and conditions.

## Install

UPM cannot resolve git dependencies, so add UniTask, core and this package to `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.dialogue": "https://github.com/danil777lar/unity-node-dialogue-system.git"
```

## Usage

1. Create a `DialogueGraphContainer` asset and open it in the graph editor. Every graph needs an enter node with index 0 linked to the first dialogue node.
2. Add a `DialogueSource` to a GameObject and assign the graph.
3. Walk the dialogue:

```csharp
DialogueStep step = source.Begin("en");
while (step != null)
{
    Show(step.Title, step.Text, step.Choices);
    int choiceId = await WaitForChoice();
    step = source.Next(step, choiceId, "en");
}
```

- **Event nodes** call `IDialogueEventProcessor.SendEvent` on components under the source.
- **Condition nodes** ask `IDialogueConditionProcessor.TryCheckCondition`. `DialogueConditionDebug` is a simple implementation for testing.
- **Trigger nodes** publish a `TriggerEvent` with a `TriggerId` on the core `IEventBus`. The bus is injected through `Construct`, automatically when Zenject is present.

## Unity CLI commands

With `com.unity.pipeline` in the project, a running editor exposes these commands to the [Unity CLI](https://docs.unity.com/en-us/unity-cli) (`unity command <group> <name>`):

| Command | Purpose |
|---|---|
| `larje.dialogue read --path <asset>` | Export a dialogue graph as JSON (nodes and links) |
| `larje.dialogue write --path <asset> (--json '<graph>' \| --file <graph.json>)` | Create or replace a dialogue graph from the same JSON format |
