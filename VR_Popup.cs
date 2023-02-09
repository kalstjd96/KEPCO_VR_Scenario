using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VR_Popup : MonoBehaviour
{
    public bool alwaysLook = true;
    public bool lerpFollow = false;
    public bool IsValve = false;
    public float distance;
    GameObject vr_camera;

    private void Update()
    {
        if (alwaysLook)
            OnEnable();

        if (IsValve)
            ValveInfo();
    }

    public void OnEnable()
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y =0f;
        forward.Normalize();
        Vector3 destination = Camera.main.transform.position + forward * distance;
        if (lerpFollow)
            transform.position = Vector3.Lerp(transform.position, destination, 0.4f);
        else transform.position = destination;
        transform.LookAt(Camera.main.transform);
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y + 180f, 0f);
    }

    public void ValveInfo()
    {
        transform.LookAt(Camera.main.transform);
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y + 180, 0f);

    }
}