using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private Animator animator;
    private CameraPan currentCamPan;
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("StaticView");
    }

    // Update is called once per frame
    public void changeCamera(string cameraName) {
        if (currentCamPan != null) 
            currentCamPan.Deactivate();

        animator.Play(cameraName);
        
        Transform child = transform.Find("State-Driven Camera").Find(cameraName);
        if (child != null) {
            var newCamPan = child.GetComponent<CameraPan>();
            
            if (newCamPan != null) {
                currentCamPan = newCamPan;
                currentCamPan.Activate();
            }
        }
    }
}
