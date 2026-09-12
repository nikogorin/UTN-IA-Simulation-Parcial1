using System;
using TMPro;
using UnityEngine;

public class PreyStateUI : StateUI
{
    [SerializeField] private TMP_Text stateText;

    public override void SetState(Enum state)
    {
        if (state is PreyState preyState)
        {
            stateText.text = preyState.ToString();

            stateText.color = preyState switch
            {
                PreyState.Flocking => Color.green,
                PreyState.GoingToBait => Color.yellow,
                PreyState.Eating => Color.cyan,
                PreyState.Evading => Color.red,
                PreyState.Dead => Color.black,
                _ => Color.white
            };
        }
    }
}
