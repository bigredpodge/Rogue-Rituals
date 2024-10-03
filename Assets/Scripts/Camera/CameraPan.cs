using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [SerializeField] Transform target;
    private Vector3 originalPos;
    private float orbitRadius;
    private float orbitSpeed;
    private bool activated;
    private float elapsedTime;
    void Awake() {
        originalPos = transform.localPosition;
        orbitRadius = Vector3.Distance(transform.localPosition, target.position);
        orbitSpeed = 1f;
        elapsedTime = 0f;
    }

    public void Activate() {
        transform.localPosition = originalPos;
        elapsedTime = 0f;
        activated = true;
    }

    public void Deactivate() {
        activated = false;
    }

    void Update() {
        if (activated) {
            elapsedTime += Time.deltaTime;
            transform.localPosition = target.position + new Vector3(Mathf.Cos(elapsedTime * orbitSpeed), originalPos.y/8, Mathf.Sin(elapsedTime * orbitSpeed)) * orbitRadius;
        }
    }
}
