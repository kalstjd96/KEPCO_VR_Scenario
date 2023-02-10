using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// 해당 스크립트는 MultiPagingPhonManager 통해 절차에 있는 전화기에 들어가 
/// 전화기 하이라이트 또는 Player의 손 감지 등의 역할을 한다.
/// 
/// 필요 조건 (기존 MCR 프로젝트 기준으로)
/// (1) 전화기 하위에 receiver라는 오브젝트가 있고 그 하위에 잡는 형태의 손모양이 들어가 있어야 한다.
/// 
/// 주의사항
/// 1. 수신자가 전화기를 드는 순간 포톤 보이스 setting을 호출하기 때문에 발신자가 걸기 전까지 수신자 전화기는 못들게 막아놔야한다!!!!!!!!!!!!
/// 2. Player 인스펙터에 반드시 PhotonVoiceView가 들어가 있어야 한다.
/// </summary>
public class MultiPagingPhone : MonoBehaviour
{
    bool isOn;
    
    Transform gripHand;
    Transform VRHand;
    DataRow dataRow;
    Transform _parent;
    Vector3 oriPosition;
    Quaternion oriRotation;

    private void Awake()
    {
         _parent = transform.parent;
        oriPosition = transform.position;
        oriRotation = transform.rotation;
        VRHand = null;

    }

    /// <summary>
    /// 현재 스크립트는 수화기에 달려 있음 
    /// </summary>
    /// <param name="dataRow"></param>
    public void PhoneSettings(DataRow dataRow)
    {
        this.dataRow = dataRow;

        gripHand = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        #region 전화기 Catch 
        //전화기 잡기
        if (!gripHand.gameObject.activeSelf && SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            Catch();
        }
        else if (gripHand.gameObject.activeSelf && SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand)) //전화기 놓기
        {
            Released();
        }

        #endregion
    }


    void Catch()
    {
        if (isOn)
        {
            //2. 현재 들려있는 전화기가 있는 지 검사
            MultiPagingPhonManager.instance.PV.RPC("PhoneCheck", RpcTarget.All, transform.parent.name);

            if (MultiPagingPhonManager.instance.isPhoneCheck) //발신자 -> 모두 꺼져 있다
                Calling();
            else //수신자
                PickUp();

            VRHand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(false);
            gripHand.gameObject.SetActive(true);
            gripHand.parent.transform.SetParent(VRHand.transform);
        }
    }

    #region 전화거는 사람일 경우 전화 신호음 넣기 및 상대 전화 RingRing
    public void Calling()
    {
        //WaitUntil waitPickUpUntil = new WaitUntil(() => isPickup);
        // yield return waitPickUpUntil;
        //CallManager.instance.SoundPhone(transform.GetComponent<AudioSource>());
        MultiPagingPhonManager.instance.callObj = transform;
        CallManager.instance.SoundPhone(transform.GetComponent<AudioSource>(), CallManager.instance.Riding_Effect, true, true, 0.2f);

        //3. 내가 어떤 전화기인지 확인 => 이건 나만 실행되기 때문에 전부에게 알려줘야함
        if (transform.parent.name.Equals(SeqManager.instance.dataRow["MCR_Phone"].ToString().Trim()))
        {
            MultiPagingPhonManager.instance.PV.RPC("RingRingStart", RpcTarget.Others, true);
        }
        else if (transform.parent.name.Equals(SeqManager.instance.dataRow["Local_Phone"].ToString().Trim()))
            MultiPagingPhonManager.instance.PV.RPC("RingRingStart", RpcTarget.Others, false);
    }

    #endregion


    #region 전화를 받는 사람일 경우 RingRing 끄기 및 PhotonVoice 세팅
    public void PickUp()
    {
        //1. RingRing이 울리고 있을꺼임 -> 소리제거
        MultiPagingPhonManager.instance.PV.RPC("RingRingSoundOff", RpcTarget.All);
        MultiPagingPhonManager.instance.PV.RPC("CallSoundOff", RpcTarget.All);
        //MultiPagingPhonManager.instance.PV.RPC("AllSoundOff", RpcTarget.All);

        //2. 전화받는 소리 => 이건 나만 들려도 됨
        CallManager.instance.SoundPhone(transform.GetComponent<AudioSource>(), CallManager.instance.PickUp_Effect, true, false, 0.2f);

        //3. PhotonVoice Setting 시작
        MultiPagingPhonManager.instance.PV.RPC("PhotonVoiceSet", RpcTarget.All);
    }
    #endregion    


    void Released()
    {
        //StopAllCoroutines();
        MultiPagingPhonManager.instance.PV.RPC("AllSoundOff", RpcTarget.All);
        MultiPagingPhonManager.instance.PV.RPC("CallSoundOff", RpcTarget.All);

        gameObject.transform.SetParent(_parent);    //원래 위치로 넣어주기
        gameObject.transform.position = oriPosition;    //원래위치로 돌려놓기
        gameObject.transform.rotation = oriRotation;

        VRHand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
        gripHand.gameObject.SetActive(false);

        if (MultiPagingPhonManager.instance.isMultiStart)
        {
            if (transform.parent.name.Equals(SeqManager.instance.dataRow["MCR_Phone"].ToString().Trim()))
                MultiPagingPhonManager.instance.PV.RPC("PhotonVoiceEnd", RpcTarget.All, true); //Call
            else
                MultiPagingPhonManager.instance.PV.RPC("PhotonVoiceEnd", RpcTarget.All, false); //Re
        }

        //VRHand = null;
    }


    #region 충돌 및 Trigger 오버라이드 함수들
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("finger"))
        {
            foreach (var playerHand in other.transform.root.GetComponentsInChildren<Hand>())
            {
                if (playerHand.gameObject.name.Equals("RightHand"))
                {
                    VRHand = playerHand.transform;
                    break;
                }
            }

            if (VRHand != null)
                isOn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("finger"))
        {
            isOn = false;
            //VRHand = null;
        }
    }
    #endregion
}
