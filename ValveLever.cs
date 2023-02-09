using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ValveLever : MonoBehaviour
{
    Function callback;

    [SerializeField] GameObject leverhHand;
    [SerializeField] Transform hand;

    bool isOn;
    bool isCatch;
    public bool isActive { get; set; }
    Vector3 originalPosition;
    Vector3 wearingPosition = new Vector3(0.0159f, 0.121f, 0.0049f);
    Vector3 wearingAngle = new Vector3(48.897f, 183.705f, 196.456f);
    Transform originalParent;

    public void Init()
    {
        isActive = false;
        isCatch = false;
        isOn = false;
        callback = null;
        
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.localEulerAngles = Vector3.zero;

        if (hand)
            hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
        leverhHand.SetActive(false);
    }

    void Start()
    {
        originalPosition = transform.position;
        originalParent = transform.parent;
    }

    void Update()
    {
        if (!isActive) return;

        if (isOn && SteamVR_Actions.default_InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            isCatch = true;

            leverhHand.SetActive(true);
            hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(false);
        }

        if (isCatch)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 0.01f);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].name.Contains("MSADV_1_00"))
                {
                    transform.GetComponent<ValveLeverRotation>().enabled = true;
                    transform.SetParent(colliders[i].transform.GetChild(0));
                    transform.localPosition = Vector3.zero;
                    transform.localEulerAngles = new Vector3(0f, 0f, -90f);

                    hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
                    leverhHand.SetActive(false);

                    isCatch = false;
                    isActive = false;
                    //transform.GetComponent<ValveLeverRotation>().enabled = false;
                    return;
                }
            }

            if (originalParent == transform.parent)
            {
                transform.SetParent(hand);
                transform.localPosition = wearingPosition;
                transform.localEulerAngles = wearingAngle;
            }
            
            if (SteamVR_Actions.default_InteractUI.GetStateUp(SteamVR_Input_Sources.RightHand))
            {
                hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
                leverhHand.SetActive(false);

                isCatch = false;

                transform.SetParent(originalParent);
                transform.position = originalPosition;
                transform.localEulerAngles = Vector3.zero;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("finger"))
        {
            isActive = true;
            isOn = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("finger"))
        {
            isActive = false;
            isOn = false;
        }
    }
}
