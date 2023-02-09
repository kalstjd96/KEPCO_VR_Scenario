using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// �ش� ��ũ��Ʈ�� MultiPagingPhonManager ���� ������ �ִ� ��ȭ�⿡ �� 
/// ��ȭ�� ���̶���Ʈ �Ǵ� Player�� �� ���� ���� ������ �Ѵ�.
/// 
/// �ʿ� ���� (���� MCR ������Ʈ ��������)
/// (1) ��ȭ�� ������ receiver��� ������Ʈ�� �ְ� �� ������ ��� ������ �ո���� �� �־�� �Ѵ�.
/// 
/// ���ǻ���
/// 1. �����ڰ� ��ȭ�⸦ ��� ���� ���� ���̽� setting�� ȣ���ϱ� ������ �߽��ڰ� �ɱ� ������ ������ ��ȭ��� ����� ���Ƴ����Ѵ�!!!!!!!!!!!!
/// 2. Player �ν����Ϳ� �ݵ�� PhotonVoiceView�� �� �־�� �Ѵ�.
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
    /// ���� ��ũ��Ʈ�� ��ȭ�⿡ �޷� ���� 
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
        #region ��ȭ�� Catch 
        //��ȭ�� ���
        if (!gripHand.gameObject.activeSelf && SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            Catch();
        }
        else if (gripHand.gameObject.activeSelf && SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand)) //��ȭ�� ����
        {
            Released();
        }

        #endregion
    }


    void Catch()
    {
        if (isOn)
        {
            //2. ���� ����ִ� ��ȭ�Ⱑ �ִ� �� �˻�
            MultiPagingPhonManager.instance.PV.RPC("PhoneCheck", RpcTarget.All, transform.parent.name);

            if (MultiPagingPhonManager.instance.isPhoneCheck) //�߽��� -> ��� ���� �ִ�
                Calling();
            else //������
                PickUp();

            VRHand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(false);
            gripHand.gameObject.SetActive(true);
            gripHand.parent.transform.SetParent(VRHand.transform);
        }
    }

    #region ��ȭ�Ŵ� ����� ��� ��ȭ ��ȣ�� �ֱ� �� ��� ��ȭ RingRing
    public void Calling()
    {
        //WaitUntil waitPickUpUntil = new WaitUntil(() => isPickup);
        // yield return waitPickUpUntil;
        //CallManager.instance.SoundPhone(transform.GetComponent<AudioSource>());
        MultiPagingPhonManager.instance.callObj = transform;
        CallManager.instance.SoundPhone(transform.GetComponent<AudioSource>(), CallManager.instance.Riding_Effect, true, true, 0.2f);

        //3. ���� � ��ȭ������ Ȯ�� => �̰� ���� ����Ǳ� ������ ���ο��� �˷������
        if (transform.parent.name.Equals(SeqManager.instance.dataRow["MCR_Phone"].ToString().Trim()))
        {
            MultiPagingPhonManager.instance.PV.RPC("RingRingStart", RpcTarget.Others, true);
        }
        else if (transform.parent.name.Equals(SeqManager.instance.dataRow["Local_Phone"].ToString().Trim()))
            MultiPagingPhonManager.instance.PV.RPC("RingRingStart", RpcTarget.Others, false);
    }

    #endregion


    #region ��ȭ�� �޴� ����� ��� RingRing ���� �� PhotonVoice ����
    public void PickUp()
    {
        //1. RingRing�� �︮�� �������� -> �Ҹ�����
        MultiPagingPhonManager.instance.PV.RPC("RingRingSoundOff", RpcTarget.All);
        MultiPagingPhonManager.instance.PV.RPC("CallSoundOff", RpcTarget.All);
        //MultiPagingPhonManager.instance.PV.RPC("AllSoundOff", RpcTarget.All);

        //2. ��ȭ�޴� �Ҹ� => �̰� ���� ����� ��
        CallManager.instance.SoundPhone(transform.GetComponent<AudioSource>(), CallManager.instance.PickUp_Effect, true, false, 0.2f);

        //3. PhotonVoice Setting ����
        MultiPagingPhonManager.instance.PV.RPC("PhotonVoiceSet", RpcTarget.All);
    }
    #endregion    


    void Released()
    {
        //StopAllCoroutines();
        MultiPagingPhonManager.instance.PV.RPC("AllSoundOff", RpcTarget.All);
        MultiPagingPhonManager.instance.PV.RPC("CallSoundOff", RpcTarget.All);

        gameObject.transform.SetParent(_parent);    //���� ��ġ�� �־��ֱ�
        gameObject.transform.position = oriPosition;    //������ġ�� ��������
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


    #region �浹 �� Trigger �������̵� �Լ���
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
