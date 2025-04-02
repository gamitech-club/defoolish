using System;
using UnityEngine;
using UnityEngine.UIElements;

public class EndingsMenu : MenuPage
{
    private ScrollView _endingsContainer;

    protected override void Awake()
    {
        base.Awake();
        _endingsContainer = Container.Q<ScrollView>();
    }

    protected override void Start()
    {
        base.Start();
        UpdateEndingList();
    }

    private void UpdateEndingList()
    {
        _endingsContainer.Clear();
        foreach (EndingID ending in (EndingID[])Enum.GetValues(typeof(EndingID)))
        {
            if (ending == EndingID.None)
                continue;
            
            bool isLocked = !SavedGame.Instance.UnlockedEndings.Contains(ending);

            VisualElement endingContainer = new();
            endingContainer.AddToClassList("ending");
            if (isLocked) endingContainer.AddToClassList("locked");

            var (title, desc) = GetEndingTitleDesc(ending);
            Label titleLabel = new(isLocked ? "???" : title);
            Label descLabel = new(isLocked ? "?????? ??? ???? ?" : desc);
            titleLabel.AddToClassList("ending-title");
            descLabel.AddToClassList("ending-desc");

            endingContainer.Add(titleLabel);
            endingContainer.Add(descLabel);
            _endingsContainer.Add(endingContainer);
        }
    }

    private (string, string) GetEndingTitleDesc(EndingID ending)
    {
        return ending switch
        {
            EndingID.BombExploded => ("Bomb Exploded", "The bomb exploded, oof."),
            EndingID.NormalEnding => ("Normal Ending", "You defused the bomb.. kind of."),
            EndingID.MorseCommunityEnding => ("Morse Community Ending", "You teamed up with Morse Code Community."),
            EndingID.PoliceEnding => ("Police Ending", "You both got captured."),
            EndingID.DestroyEverythingEnding => ("Destroy Everything Ending", "You destroyed everything."),
            _ => ("None", "None"),
        };
    }
}
