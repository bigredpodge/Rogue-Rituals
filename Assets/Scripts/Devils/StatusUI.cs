using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, timeText;
    [SerializeField] Image background;
    Dictionary<ConditionID, Color> statusColors;

    public void Setup(Condition condition, int time) {
        statusColors = new Dictionary<ConditionID, Color>() {
            {ConditionID.psn, new Color(0f, .8f, 0f)},
            {ConditionID.brn, new Color(.8f, .4f, 0f)},
            {ConditionID.slp, new Color(.8f, 0f, .8f)},
            {ConditionID.par, new Color(.8f, .8f, 0f)},
            {ConditionID.frz, new Color(0f, .8f, .8f)},
            {ConditionID.dom, new Color(1f, .4f, 0f)}
        };

        nameText.text = condition.Name;
        background.color = statusColors[condition.Id];
        timeText.text = time+"";
    }
}