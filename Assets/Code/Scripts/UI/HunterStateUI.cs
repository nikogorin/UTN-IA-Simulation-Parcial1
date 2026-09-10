using System;
using TMPro;
using UnityEngine;

public class HunterStateUI : StateUI
{
    [SerializeField] private TMP_Text stateText;

    public override void SetState(Enum state)
    {
        if (state is HunterState hunterState)
        {
            stateText.text = hunterState.ToString();

            stateText.color = hunterState switch
            {
                HunterState.Idle => Color.green,
                HunterState.Patrol => Color.yellow,
                HunterState.PlacingBait => Color.cyan,
                _ => Color.white
            };
        }
    }
}
