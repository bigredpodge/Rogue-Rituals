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
                var condition = statuses.ElementAt(i).Key;
                var time = statuses.ElementAt(i).Value;
                statusSlots[i].Setup(condition, time);
            }
            else 
                statusSlots[i].gameObject.SetActive(false);
        }
    }
}
