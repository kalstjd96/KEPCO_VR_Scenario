using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using System.Linq;

public class InputDeviceController : MonoBehaviour
{
    public static string _actor;
    public static string _j168;
    public static string _modelTag;
    public static string _map_Initials;
    public static string _objid;

    public static List<string> _list_initials;
    public static List<string> _list_tag;
    public static Dictionary<string, string> _list_dic;

    [SerializeField] RectTransform mousePoint; //Cursor
    public bool isMouse;
    Transform targetHighLight;


    public static void KepcoPage(string actor, string map_Initials, string j168, string modelTag, string objid)
    {
        _actor = actor;
        _j168 = j168;
        _map_Initials = map_Initials;
        _modelTag = modelTag;
        _objid = objid;
    }

    public static void CreateHighlightList()
    {
        _list_tag = new List<string>();
        _list_tag = _modelTag.Split('/').ToList();

        _list_initials = new List<string>();
        _list_initials = _map_Initials.Split('&').ToList();

        _list_dic = new Dictionary<string, string>();
        string[] j168_arr = _j168.Split('&');
        for (int i = 0; i < j168_arr.Length; i++)
        {
            // 도면번호와, 도면약어는 1:1 매칭
            _list_dic.Add(j168_arr[i], _list_initials[i]);
        }
    }

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(mousePoint.position, 0.01f);
        //가끔 마우스 커서를 가져다 대도 안뜨는 경우 발생 길이가 1이라서 발생한 것으로 추정 수정 필요

        foreach (Collider col in colliders)
        {
            if (col.transform.GetComponent<Symbol>())
            {
                targetHighLight = col.transform;
                if (SeqManager.instance.symbolHighlight.isSetting)
                {
                    SeqManager.instance.symbolHighlight.OnMouse(targetHighLight, true);
                    if (SteamVR_Actions.default_InteractUI.GetStateUp(SteamVR_Input_Sources.RightHand))
                    {
                        SeqManager.instance.symbolHighlight.OnSymbolClick(targetHighLight);
                        targetHighLight = null;
                        break;
                    }
                }
            }
        }

        if (targetHighLight != null && !colliders.ToList().Contains(targetHighLight.GetComponent<Collider>()))
        {
            SeqManager.instance.symbolHighlight.OnMouse(targetHighLight, false);
            targetHighLight = null;
        }

        if (isMouse)
        {
            if (SteamVR_Actions.default_InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                transform.GetComponent<AudioSource>().Play();
            }
        }
    }

}
