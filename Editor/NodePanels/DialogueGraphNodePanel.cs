using Larje.Dialogue.Runtime.Graph;
using System;
using System.Collections.Generic;
using Larje.Dialogue.Editor;
using Larje.Dialogue.Runtime.Converted;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Larje.Dialogue.Editor
{
    public class DialogueGraphNodePanel : GraphNodePanel
    {
        private int _selectedLangIndex;
        private DialogueGraphNode _node;

        public DialogueGraphNodePanel(DialogueGraphNode node)
        {
            _node = node;
        }

        public override void Draw(VisualElement root)
        {
            base.Draw(root);

            DrawToolbar();
            DrawSettings();
            DrawMainSpeech();
            DrawChoices();

            _node.UpdatePorts();
            _node.UpdateTitle();
        }

        private void DrawToolbar()
        {
            ToolbarMenu toolbar = new ToolbarMenu();
            toolbar.text = $"Language: {_node.Content.GetLocalization(_selectedLangIndex).LanguageCode.ToUpper()}";

            int index = 0; 
            foreach (DialogueLocalization localization in _node.Content.GetAll())
            {
                bool selected = index == _selectedLangIndex;
                int indexCopy = index;
                toolbar.menu.AppendAction(localization.LanguageCode.ToUpper(), (action) =>
                {
                    _selectedLangIndex = indexCopy;
                    Refresh();
                });

                index++;
            }

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new Button(() =>
            {
                _node.Content.AddLocalization("new");
                _selectedLangIndex = _node.Content.GetAll().Count - 1;
                Refresh();
            })
            {
                text = "+"
            });

            _root.Add(toolbar);
        }

        private void DrawSettings()
        {
            DialogueLocalization localization = _node.Content.GetLocalization(_selectedLangIndex);
            Box settings = GetBox(_root, "MAIN SETTINGS:");

            TextField languageCodeField = new TextField();
            languageCodeField.SetEnabled(_selectedLangIndex != 0);
            languageCodeField.label = "Language Code:";
            languageCodeField.value = localization.LanguageCode;
            languageCodeField.RegisterCallback<ChangeEvent<string>>((value) =>
            {
                localization.LanguageCode = value.newValue;
            });
            languageCodeField.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
            });
            settings.Add(languageCodeField);

            Button deleteButton = new Button(() =>
            {
                _node.Content.RemoveLocalization(localization.LanguageCode);
                _selectedLangIndex = Mathf.Max(0, _selectedLangIndex - 1);
                Refresh();
            });
            deleteButton.SetEnabled(_selectedLangIndex != 0);
            deleteButton.text = "Delete localization";
            settings.Add(deleteButton);

            Button addChoiceButton =  new Button(() =>
            {
                _node.Content.AddChoice();
                Refresh();
            });
            addChoiceButton.text = "Add choice";
            settings.Add(addChoiceButton);
        }

        private void DrawMainSpeech()
        {
            DialogueLocalization localization = _node.Content.GetLocalization(_selectedLangIndex);
            Box box = GetBox(_root, "MAIN SPEECH:");
            DrawSpeech(box, localization.InterlocutorSpeech);
        }

        private void DrawChoices()
        {
            Box choicesRoot = GetBox(_root, "CHOICES:");

            DialogueLocalization localization = _node.Content.GetLocalization(_selectedLangIndex);
            int index = 0;
            foreach (DialogueChoice choice in localization.PlayerChoices)
            {
                int indexCopy = index;
                Box box = GetBox(choicesRoot, $"choice {index}:");
                DrawSpeech(box, choice.PlayerSpeech);

                Box toolbarMain = new Box();
                toolbarMain.style.flexDirection = FlexDirection.Row;
                box.Add(toolbarMain);

                Box toolbarMove = new Box();
                toolbarMove.style.flexDirection = FlexDirection.Row;
                toolbarMove.style.flexGrow = 1f;
                toolbarMain.Add(toolbarMove);

                Box toolbarDelete = new Box();
                toolbarDelete.style.flexDirection = FlexDirection.Row;
                toolbarDelete.style.flexGrow = 0.01f;
                toolbarMain.Add(toolbarDelete);


                Button upButton = new Button(() =>
                {
                    _node.Content.MoveChoice(indexCopy, -1);
                    Refresh();
                });
                upButton.SetEnabled(_node.Content.CanMoveChoice(indexCopy, -1));
                upButton.text = "\u2191";
                toolbarMove.Add(upButton);

                Button downButton = new Button(() =>
                {
                    _node.Content.MoveChoice(indexCopy, 1);
                    Refresh();
                });
                downButton.SetEnabled(_node.Content.CanMoveChoice(indexCopy, 1));
                downButton.text = "\u2193";
                toolbarMove.Add(downButton);

                Button deleteButton = new Button(() =>
                {
                    _node.Content.RemoveChoice(indexCopy);
                    Refresh();
                });
                deleteButton.text = "X";
                toolbarDelete.Add(deleteButton);

                index++;
            }
        }

        private void DrawSpeech(VisualElement root, Speech speech)
        {
            TextField titleField = new TextField();
            titleField.label = "Title:";
            titleField.value = speech.Title;
            titleField.RegisterCallback<ChangeEvent<string>>((value) =>
            {
                speech.Title = value.newValue;
            });
            titleField.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
            });
            root.Add(titleField);

            TextField textField = new TextField();
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.multiline = true;
            textField.label = "Text:";
            textField.value = speech.Text;
            textField.RegisterCallback<ChangeEvent<string>>((value) =>
            {
                speech.Text = value.newValue;
            });
            textField.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
            });
            root.Add(textField);

            TextField conditionField = new TextField();
            conditionField.style.whiteSpace = WhiteSpace.Normal;
            conditionField.multiline = true;
            conditionField.label = "Condition:";
            conditionField.value = speech.Condition;
            conditionField.RegisterCallback<ChangeEvent<string>>((value) =>
            {
                speech.Condition = value.newValue;
            });
            conditionField.RegisterCallback<FocusOutEvent>((value) =>
            {
                Refresh();
            });
            root.Add(conditionField);
        }
    }
}
