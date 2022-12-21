using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cyber_Plant_Door_Ani : MonoBehaviour
{

    bool doorAniOn;
    Animation doorAni;
    List<string> animArray;
    int index;

    void Awake()
    {
        doorAniOn = false;
        index = 0;
    }
    private void Start()
    {
        doorAni = gameObject.GetComponent<Animation>();
        animArray = new List<string>();
        AnimationArray();
    }

    public void AnimationArray()
    {
        foreach(AnimationState state in doorAni)
        {
            animArray.Add(state.name);
        }
    }
    public void DoorAni()
    {
        if (doorAniOn)
        {
            doorAniOn = false;
            doorAni.Play(animArray[1]);
        }
        else
        {
            doorAniOn = true;
            doorAni.Play(animArray[0]);
        }
    }

}
