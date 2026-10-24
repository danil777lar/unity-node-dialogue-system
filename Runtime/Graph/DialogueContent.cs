using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Larje.Dialogue.Runtime.Graph
{
    [Serializable]
    public class DialogueContent
    {
        private const string DEFAULT_LANGUAGE = "en";

        [SerializeField] private List<DialogueLocalization> Localizations;

        public void Init()
        {
            Localizations = new List<DialogueLocalization>();
            AddLocalization(DEFAULT_LANGUAGE);
        }

        public IReadOnlyCollection<DialogueLocalization> GetAll()
        {
            return Localizations.AsReadOnly();
        }

        public void AddLocalization(string languageCode)
        {
            DialogueLocalization localization = new DialogueLocalization();
            localization.PlayerChoices = new List<DialogueChoice>();
            localization.LanguageCode = languageCode;
            Localizations.Add(localization);

            localization.InterlocutorSpeech = new Speech
            {
                Title = $"Interlocutor title",
                Text = $"Interlocutor text"
            };

            for (int i = 0; i < GetChoiceCount(); i++)
            {
                localization.PlayerChoices.Add(new DialogueChoice
                {
                    PlayerSpeech = new Speech
                    {
                        Title = $"Title",
                        Text = $"Text"
                    }
                });
            }
        }

        public void RemoveLocalization(string languageCode)
        {
            if (languageCode == DEFAULT_LANGUAGE)
            {
                return;
            }

            Localizations.Remove(GetLocalization(languageCode));
        }

        public DialogueLocalization GetLocalization(string languageCode)
        {
            DialogueLocalization loc = Localizations.Find(x => x.LanguageCode == languageCode);
            if (loc == null)
            {
                loc = Localizations.Find(x => x.LanguageCode == DEFAULT_LANGUAGE);
            }

            return loc;
        }

        public DialogueLocalization GetLocalization(int index)
        {
            return Localizations[index];
        }

        public void AddChoice()
        {
            foreach (DialogueLocalization localization in Localizations)
            {
                localization.PlayerChoices.Add(new DialogueChoice()
                {
                    PlayerSpeech = new Speech
                    {
                        Title = $"Title",
                        Text = $"Text"
                    }
                });
            }
        }

        public void RemoveChoice(int index)
        {
            foreach (DialogueLocalization localization in Localizations)
            {
                localization.PlayerChoices.RemoveAt(index);
            }
        }

        public int GetChoiceCount()
        {
            return GetLocalization(0).PlayerChoices.Count;
        }

        public bool CanMoveChoice(int index, int direction)
        {
            int target = index + direction;
            return target >= 0 && target <= GetChoiceCount() - 1; 
        }

        public void MoveChoice(int index, int direction)
        {
            if (!CanMoveChoice(index, direction))
            {
                return;
            }

            foreach (DialogueLocalization localization in Localizations)
            {
                DialogueChoice choice = localization.PlayerChoices[index];
                DialogueChoice otherChoice = localization.PlayerChoices[index + direction];

                localization.PlayerChoices[index] = otherChoice;
                localization.PlayerChoices[index + direction] = choice;
            }
        }
    }


    [Serializable]
    public class DialogueLocalization
    {
        public string LanguageCode;
        public Speech InterlocutorSpeech;
        public List<DialogueChoice> PlayerChoices;
    }

    [Serializable]
    public class DialogueChoice
    {
        public Speech PlayerSpeech;
    }

    [Serializable]
    public class Speech
    {
        public string Title;
        public string Text;
        public string Condition;
    }
}
