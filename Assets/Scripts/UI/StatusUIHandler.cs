using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class StatusUIHandler : MonoBehaviour
{
    StatusUI[] statusSlots;
    public void Init() {
        statusSlots = GetComponentsInChildren<StatusUI>(true);
    }
    public void CheckStatusUI(Devil devil) {
        var statuses = devil.Statuses;
        for (int i = 0; i < statusSlots.Length; i++) {
            if (i < devil.Statuses.Count) {
                statusSlots[i].gameObject.SetActive(true);
                var condition = statuses[i];
                statusSlots[i].Setup(condition);
            }
            else 
                statusSlots[i].gameObject.SetActive(false);
        }
    }
}
