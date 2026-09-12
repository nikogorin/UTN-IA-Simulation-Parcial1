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
                HunterState.Idle => Color.black,
                HunterState.Patrol => Color.yellow,
                HunterState.PlacingBait => Color.cyan,
                HunterState.Attacking => Color.red,
                HunterState.GoingToGather => Color.green,
                HunterState.Gathering => Color.violet,
                _ => Color.white
            };
        }
    }
}
