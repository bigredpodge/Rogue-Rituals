using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] Image background;
    Dictionary<ConditionID, Color> statusColors;

    public void Setup(Condition condition) {
        statusColors = new Dictionary<ConditionID, Color>() {
            {ConditionID.brn, new Color(.8f, .4f, 0f)},
            {ConditionID.slp, new Color(0f, 0.2f, .8f)},
            {ConditionID.dom, new Color(.6f, .4f, .8f)},
            {ConditionID.bsk, new Color(.8f, 0f, .2f)},
            {ConditionID.fbl, new Color(.8f, .6f, .6f)},
            {ConditionID.dsp, new Color(.4f, .6f, .6f)},
            {ConditionID.wth, new Color(.4f, .8f, 0f)},
            {ConditionID.frt, new Color(0f, .8f, .8f)},
            {ConditionID.dnk, new Color(.8f, .8f, 0f)}
        };

        nameText.text = condition.Id.ToString().ToUpper();
        background.color = statusColors[condition.Id];
    }
}