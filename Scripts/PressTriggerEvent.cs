using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR;

public class PressTriggerEvent : MonoBehaviour
{
    public UnityEvent Event;
    Function OnEnterEvent;
    Function OnExitEvent;
    Function OnStayEvent;
    public Color highlightColor;
    [System.NonSerialized] public Color originalColor;
    [System.NonSerialized] public Transform target;

    void Awake()
    {
        if (GetComponent<Image>())
            originalColor = GetComponent<Image>().color;
    }

    void OnEnable()
    {
        isEntered = false;
    }

    void OnDisable()
    {
        target = null;
        isEntered = false;

        if (gameObject.name.Contains("OK"))
            GetComponent<Image>().color = originalColor;
    }

    /// <summary>
    /// 해당 Obejct를 눌렀을때 실행시킬 함수와 Obejct를 떼었을때 실행시킬 함수를 넣어주면된다.
    /// </summary>
    /// <param name="OnEnterEvent">Obejct를 터치했을때 실행시킬 함수</param>
    /// <param name="OnExitEvent">Obejct를 떼었을때 실행시킬 함수</param>
    public void AddHanddPressListner(Function OnEnterEvent, Function OnExitEvent = null)
    {
        if (this.OnEnterEvent == null)
            this.OnEnterEvent = OnEnterEvent;
        if (OnExitEvent != null)
            this.OnExitEvent = OnExitEvent;
    }

    public void OnStayTriggerEvent(Function OnStayEvent)
    {
        this.OnStayEvent = OnStayEvent;
    }

    bool isTriggered;
    private void OnTriggerStay(Collider other)
    {
        OnStayEvent?.Invoke();
        //if (SteamVR_Actions.default_InteractUI.GetStateUp(SteamVR_Input_Sources.RightHand))
        //{
        //    if (enabled && !isTriggered)
        //    {
        //        isTriggered = true;
        //        OnStayEvent?.Invoke();
        //    }
        //}
    }

    public bool isEntered;
    private void OnTriggerEnter(Collider other)
    {
        target = other.transform;

        if (enabled && !isEntered)
        {
            if (LayerMask.Equals(other.gameObject.layer, LayerMask.NameToLayer("Interactive")))
            {
                if (other.transform.parent.parent != null && other.transform.parent.parent.name.Contains("Left"))
                    return;

                //AudioManager.instance.PlayMultiAudio("Sound/Select");
                isEntered = true;
                OnEnterEvent?.Invoke();
                Event?.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        target = null;

        if (LayerMask.Equals(other.gameObject.layer, LayerMask.NameToLayer("Interactive")))
        {
            isEntered = false;
            OnExitEvent?.Invoke();
        }
/*        isEntered = false;
        OnExitEvent?.Invoke();*/
        if (enabled)
        {
            //isEntered = false;

        }
    }


    ///// Test
    [Button]
    public void Click()
    {
        OnEnterEvent?.Invoke();
        Event?.Invoke();
    }

    public void NextSequence()
    {

    }
}
