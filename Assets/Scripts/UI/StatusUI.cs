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
            {ConditionID.slp, new Color(.8f, 0f, .8f)},
            {ConditionID.dom, new Color(1f, .4f, 0f)}
        };

        nameText.text = condition.Id.ToString().ToUpper();
        background.color = statusColors[condition.Id];
    }
}