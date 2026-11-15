using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Tutorial 2 on the play screen: a yellow crew card under the crew list says what to do and why, the arrow points at the
    // button or building, and the Objectives panel shows the steps around the current one.
    public sealed partial class GameScreen
    {
        const int TrainingRows = 6;
        const int GuideHeight = 240;

        Image guideBox, guideFace;
        Text guideInitial, guideName, guideText;
        Button guideNext;
        string guideKey = "";
        int shownStep = -1;
        bool trainingEndShown;

        void BuildGuide()
        {
            // Under the crew list, above the selected-building panel: it never covers the map, the bars or the alerts.
            guideBox = UIKit.Panel(root, Palette.Amber, "Crew Guide");
            guideBox.rectTransform.TopLeft(20, LeftTop, Side, GuideHeight);
            var outline = guideBox.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(3, -3);

            guideFace = UIKit.Rect(guideBox.transform, "Face").gameObject.AddComponent<Image>();
            guideFace.sprite = UIKit.CircleSprite;
            guideFace.raycastTarget = false;
            guideFace.rectTransform.TopLeft(16, 14, 64, 64);
            guideInitial = UIKit.Label(guideFace.transform, "", 34, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            guideInitial.rectTransform.Stretch();

            guideName = UIKit.Label(guideBox.transform, "", 22, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            guideName.rectTransform.TopLeft(90, 14, Side - 240, 64);
            guideName.resizeTextForBestFit = true;
            guideName.resizeTextMinSize = 16;
            guideName.resizeTextMaxSize = 22;
            guideText = UIKit.Label(guideBox.transform, "", 26, Palette.Ink, TextAnchor.UpperLeft);
            guideText.rectTransform.Stretch(18, 88, 18, 12);
            guideText.resizeTextForBestFit = true;
            guideText.resizeTextMinSize = 18;
            guideText.resizeTextMaxSize = 26;

            guideNext = UIKit.Button(guideBox.transform, "Next", PressGuideNext, Palette.Ink, Palette.Hull, 26);
            guideNext.GetComponent<RectTransform>().TopLeft(Side - 140, 18, 124, 56);
        }

        public bool GuideShowing => guideBox != null && guideBox.gameObject.activeSelf;

        public void PressGuideNext()
        {
            var step = run.Training?.Current;
            if (step == null || step.done != "next" || ModalOpen) return;
            AudioDirector.Play(Sfx.PickCard);
            run.TrainingAction("next:" + step.id);
        }

        void RefreshGuide()
        {
            if (guideBox == null) return;
            var step = run.Training.Current;
            bool show = step != null && !run.Ended && run.Phase != RunPhase.Briefing;
            guideBox.gameObject.SetActive(show);
            if (!show) return;
            string text = Training.Fill(step.text, run);
            string note = run.Training.Note(run);
            if (note != "") text += "\n<b>" + note + "</b>";
            string key = step.id + text;
            if (key == guideKey) return;
            bool newStep = !guideKey.StartsWith(step.id);
            guideKey = key;

            var member = story.Crew(step.speaker);
            guideFace.color = CrewColor(step.speaker);
            guideInitial.text = member != null ? member.name.Substring(0, 1) : "";
            guideName.text = member != null ? member.name + " · " + member.role : "";
            guideText.text = Keywords.Mark(text, true);
            guideNext.gameObject.SetActive(step.done == "next");
            if (newStep && !UIFx.Calm) UIFx.Pop(guideBox.transform);
        }

        RectTransform TrainingTarget(out string message)
        {
            var step = run.Training.Current;
            message = step != null ? step.goal : "";
            if (step == null || run.BuildChoice.HasValue && step.target == "card:" + run.BuildChoice.Value) return null;
            var parts = step.target.Split(':');
            string arg = parts.Length > 1 ? parts[1] : "";
            switch (parts[0])
            {
                case "card":
                    var card = cards.Find(c => c.type.HasValue && c.type.Value.ToString() == arg);
                    return card != null ? (RectTransform)card.button.transform : null;
                case "crew":
                    var row = crewRows.Find(r => r.id == arg);
                    return row != null ? row.button : null;
                case "chip":
                    var chip = chips.Find(c => c.id == arg);
                    return chip != null ? chip.box : null;
                case "play": return (RectTransform)playButton.transform;
                case "speed": return (RectTransform)speedButton.transform;
                case "eval": return evalText.rectTransform;
                case "wiki": return (RectTransform)wikiButton.transform;
                case "alert":
                    if (alertButtons.TryGetValue(arg, out var button) && button != null) return (RectTransform)button.transform;
                    return System.Enum.TryParse(arg, out BuildingType building) ? BuildingRect(building) : null;
                case "toggle":
                    var type = (BuildingType)System.Enum.Parse(typeof(BuildingType), arg);
                    var selected = run.Selected;
                    if (selected != null && selected.type == type && toggleButton.gameObject.activeInHierarchy)
                        return (RectTransform)toggleButton.transform;
                    message = "Click the " + run.sim.balance.Def(type).displayName;
                    return BuildingRect(type);
                case "building":
                    return BuildingRect((BuildingType)System.Enum.Parse(typeof(BuildingType), arg));
                default: return null;
            }
        }

        RectTransform BuildingRect(BuildingType type)
        {
            var b = run.State.First(type);
            return b != null && views.TryGetValue(b.id, out var view) ? view.body.rectTransform : null;
        }

        // Shows the step before the current one, the current one (yellow box) and the next few.
        void RefreshTrainingObjectives()
        {
            var training = run.Training;
            var steps = training.Steps;
            int first = Mathf.Clamp(training.Index - 1, 0, Mathf.Max(0, steps.Length - objectives.Count));
            for (int i = 0; i < objectives.Count; i++)
            {
                var row = objectives[i];
                int n = first + i;
                bool exists = n < steps.Length;
                row.box.transform.parent.gameObject.SetActive(exists);
                if (!exists) continue;
                bool done = n < training.Index, current = n == training.Index;
                row.text.text = Keywords.Mark(steps[n].goal);
                row.text.color = done ? Palette.Muted : current ? Palette.Hull : Palette.Ice;
                row.text.fontStyle = current ? FontStyle.Bold : FontStyle.Normal;
                row.box.color = done ? Palette.Mint : current ? Palette.Amber : Palette.Space;
                row.check.enabled = done;
            }
            if (training.Index != shownStep && shownStep >= 0) AudioDirector.Play(Sfx.Objective);
            shownStep = training.Index;
            objectivesTitle.text = "TRAINING  " + training.Index + "/" + steps.Length;
        }

        void ShowTrainingEnd()
        {
            trainingEndShown = true;
            Progress.MarkPassed(run.level.id);
            AudioDirector.Play(Sfx.GoodNews);
            var next = LevelCatalog.Load().Next(run.level.id);
            string body = "You met Priya, Yuki and Mateo, and you ran a whole mission with them.";
            var spec = Modal("Training complete!", body, "star");
            spec.buttons.Clear();
            if (next != null)
            {
                spec.body += "\nNext: " + next.Label + ". This time the clock counts down to the inspection.";
                spec.buttons.Add(new ModalButton("Start " + next.Label, () => Flow.StartLevel(next.id)));
            }
            spec.buttons.Add(new ModalButton("Back to levels", () => Flow.ToMenu("story"), false));
            ShowModal(spec);
        }
    }
}
