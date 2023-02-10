using ExitGames.Client.Photon.StructWrapping;
using Oculus.Platform.Models;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Valve.VR;

/// <summary>
/// �ش� ��ũ��Ʈ�� ��Ƽ ����¡���� �����ϴ� ��ũ��Ʈ
/// </summary>
public class MultiPagingPhonManager : MonoBehaviour
{
    #region �̱��� 
    public static MultiPagingPhonManager instance;
    public bool isCall; //�߽��ڰ� �޾Ҵ� ��
    public bool isRec; //�����ڰ� �޾Ҵ� ��
    public PhotonView PV;
    public DataRow dataRow;



    //��ȭ�Ⱑ ��� �ִ� �� Ȯ��
    public bool isPhoneCheck;
    Transform[] player;
    Transform[] phone;

    //��ȭ ������ �Ҹ� 
    Transform ringPhone;
    bool isPickup;

    //PhotonVoice ����
    public bool isMultiStart;
    Transform recorder;
    Transform targetPhone;
    Transform voice;
    Transform oriMCRSpeakerParent;
    Transform oriLocalSpeakerParent;
    int offPhoneCount;

    public Transform callObj;

    Transform targetReceiver;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        PV = transform.GetComponent<PhotonView>();
        player = new Transform[2];
        phone = new Transform[2];

        ringPhone = null;
        isPickup = false;
        isMultiStart = false;
        recorder = null;
        targetPhone = null;
        voice = null;
        offPhoneCount = 0;
        oriMCRSpeakerParent = null;
        oriLocalSpeakerParent = null;
        callObj = null;
        targetReceiver = null;
    }
    private void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }
    #endregion

    //[PunRPC]
    public void MultiPhoneSetting()
    {
        Debug.Log("��Ƽ ��ȭ ����");
        PV.RPC("Settings", RpcTarget.All);
    }


    #region ��ȭ�⿡ MultiPagingPhone ��ũ��Ʈ �־��ֱ�, ��Ƽ ��ȭ�� ���� �⺻ ���� 

    /// <summary>
    /// phone, player [0]�� ������ MCR ��ȭ��
    /// phone, player [1]�� ������ Local ��ȭ��
    /// </summary>
    /// <param name="dataRow"></param>
    [PunRPC]
    public void Settings()
    {
        this.dataRow = SeqManager.instance.dataRow;
        BoothDoor currentDoor;

        #region 1. ��ȭ�� Transform�� ��� ���� ����
        for (int i = 0; i < SeqManager.phoneList.Count; i++)
        {
            if (SeqManager.phoneList[i].name.Equals(dataRow["MCR_Phone"].ToString().Trim()))
            {
                if (SeqManager.phoneList[i].GetComponentInChildren<PagingPhoneManager>() != null)
                    targetReceiver = SeqManager.phoneList[i].GetComponentInChildren<PagingPhoneManager>().transform;
                else
                {
                    SeqManager.instance.player.transform.GetComponentInChildren<PagingPhoneManager>().Grab(true);
                    targetReceiver = SeqManager.phoneList[i].GetComponentInChildren<PagingPhoneManager>().transform; //Recevier
                }

                phone[0] = targetReceiver;
                if (!targetReceiver.GetComponent<MultiPagingPhone>())
                    targetReceiver.gameObject.AddComponent<MultiPagingPhone>().PhoneSettings(dataRow);

                targetReceiver.GetComponent<PagingPhoneManager>().enabled = false;
            }
            else if (SeqManager.phoneList[i].name.Equals(dataRow["Local_Phone"].ToString().Trim()))
            {
                if (SeqManager.phoneList[i].GetComponentInChildren<PagingPhoneManager>() != null)
                    targetReceiver = SeqManager.phoneList[i].GetComponentInChildren<PagingPhoneManager>().transform;
                else
                {
                    SeqManager.instance.player.transform.GetComponentInChildren<PagingPhoneManager>().Grab(true);
                    targetReceiver = SeqManager.phoneList[i].GetComponentInChildren<PagingPhoneManager>().transform; //Recevier
                }
                phone[1] = targetReceiver;
                if (!targetReceiver.GetComponent<MultiPagingPhone>())
                    targetReceiver.gameObject.AddComponent<MultiPagingPhone>().PhoneSettings(dataRow);
                targetReceiver.GetComponent<PagingPhoneManager>().enabled = false;
            }

            if (phone[0] != null&& phone[1] != null)
                break;
        }
        #endregion

        #region 2. Player Transform�� ��� ���� ����
        bool callPlayer = SeqManager.instance.dataRow["isFade"].ToString().Trim().Equals("MCR");
        int nextIndex = SeqManager.instance.rowIndex + 1;
        DataRow dataRo = SeqManager.instance.dataTable.Rows[nextIndex];

        if (callPlayer) //MCR
        {
            for (int i = 0; i < SeqManager.instance.PlayerList.Count; i++)
            {
                string[] playerActor = SeqManager.instance.PlayerList[i].name.ToString().Split('/');
                for (int j = 0; j < playerActor.Length; j++)
                {
                    if (playerActor[j].ToString().Trim().Equals(dataRow["Actor"].ToString().Trim()))
                        player[0] = SeqManager.instance.PlayerList[i].transform;
                    else if (playerActor[j].ToString().Trim().Equals(dataRo["Actor"].ToString().Trim()))
                        player[1] = SeqManager.instance.PlayerList[i].transform;
                }

                if (player[0] != null && player[1] != null)
                    break;
            }

        }
        else
        {
            for (int i = 0; i < SeqManager.instance.PlayerList.Count; i++)
            {
                string[] playerActor = SeqManager.instance.PlayerList[i].name.ToString().Split('/');
                for (int j = 0; j < playerActor.Length; j++)
                {
                    if (playerActor[j].ToString().Trim().Equals(dataRow["Actor"].ToString().Trim()))
                        player[1] = SeqManager.instance.PlayerList[i].transform;
                    else if (playerActor[j].ToString().Trim().Equals(dataRo["Actor"].ToString().Trim()))
                        player[0] = SeqManager.instance.PlayerList[i].transform;
                }

                if (player[0] != null && player[1] != null)
                    break;
            }
        }

        if (player[0] != null && player[1] != null)
        {
            if (player[0] == player[1])
            {
                for (int i = 0; i < phone.Length; i++)
                {
                    if (phone[i].GetComponent<MultiPagingPhone>())
                    {
                        Destroy(phone[i].GetComponent<MultiPagingPhone>());
                        phone[i].GetComponent<PagingPhoneManager>().enabled = true;
                    }
                }
                
                if (player[0].gameObject == SeqManager.instance.ItsMe)
                    SeqManager.instance.StartCoroutine(SeqManager.instance.Actor_Check2());

                PV.RPC("Init", RpcTarget.All);
            }
            else
            {
                #region isBuilding 
                List<Role> myActorList = ConnManager.instance.myActor.ToList<Role>();
                if (myActorList.Contains(Role.LO))
                    SeqManager.instance.BuildingOn(dataRow["IsBuilding"].ToString());
                #endregion

                if (player[1].GetComponent<PhotonView>().IsMine)
                {
                    Vector3 targetBound;
                    Vector3 destination;

                    //Player ��ġ �̵� + LOCAL ����¡���϶�, 
                    if (phone[1].parent.parent != null && phone[1].parent.parent.name.Contains("Booth_Box"))
                    {
                        try
                        {
                            targetBound = phone[1].GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(phone[1]) : phone[1].position;
                            destination = targetBound + Vector3.forward * 1.5f;

                            GameObject boothDoor = phone[1].parent.parent.Find("Door/Booth_Door/HotSpot_Area").gameObject;
                            boothDoor.SetActive(true);
                            boothDoor.GetComponent<CapsuleCollider>().enabled = true;
                            boothDoor.AddComponent<BoothDoor>();
                            boothDoor.GetComponent<BoothDoor>().CatchHotspot();
                            currentDoor = boothDoor.GetComponent<BoothDoor>();
                            currentDoor.init_door();
                        }
                        catch (System.Exception)
                        {
                            targetBound = phone[1].GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(phone[1]) : phone[1].position;
                            destination = targetBound + Vector3.forward * 1.5f;
                        }
                    }
                    else
                    {
                        targetBound = phone[1].GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(phone[1]) : phone[1].position;
                        destination = targetBound + Vector3.forward * 1.5f;
                    }

                    Transform direction_obj = null;
                    //YH - �߰��� �κ� 2022-08-29
                    for (int j = 0; j < phone[1].parent.childCount; j++)
                    {
                        if (phone[1].parent.GetChild(j).tag == "Direction")
                        {
                            direction_obj = phone[1].parent.GetChild(j);
                            break;
                        }
                    }

                    if (direction_obj != null)
                        destination = targetBound + direction_obj.forward * 2f;

                    //����¡���� �����̿� ������ �ڷ���Ʈ�� �Ƚ����ְ� �; § �ڵ�(2022-10-12 YH)
                    //if(SeqManager.instance.playerObjDistance.DistanceComparison(phone[1], phone[1].position))
                        StartCoroutine(PhoneFade(destination, targetBound));
                }
            }
        }
        #endregion

    }

    public IEnumerator PhoneFade(Vector3 destination, Vector3 targetBound)
    {
        yield return StartCoroutine(FadeEffect.instance.OUT());

        SeqManager.instance.player.transform.eulerAngles = Vector3.zero;
        SeqManager.instance.player.transform.position = targetBound;
        SeqManager.instance.VRCameraMoveTo(destination, targetBound);

        SeqManager.instance.playerObjDistance.SetPlayerHeight(targetBound);

        yield return StartCoroutine(FadeEffect.instance.IN());
    }
    #endregion

    #region ��ȭ�Ⱑ ��� �ִ� �� Ȯ��

    [PunRPC]
    public void PhoneCheck(string phoneName)
    {
        if (!isCall && !isRec)
            isPhoneCheck = true;
        else
            isPhoneCheck = false;
    }


    #endregion

    #region ��ȭ ������ �Ҹ� 
    [PunRPC]
    public void RingRingStart(bool phone)
    {
        if (phone) //MCR ��ȭ���̱⿡ Local ��ȭ�⿡ RingRing
        {
            ringPhone = this.phone[1];
            isCall = true;
        }
        else //Local ��ȭ��
        {
            ringPhone = this.phone[0];
            isRec = true;
        }

        StartCoroutine(RingRing(ringPhone));
    }

    [PunRPC]
    public void RingRingSoundOff()
    {
        isPickup = true;
    }

    public IEnumerator RingRing(Transform recPhone)
    {
        WaitUntil waitPickUpUntil = new WaitUntil(() => isPickup);

        //1. ��ȭ �޴� �Ҹ�
        CallManager.instance.SoundPhone(recPhone.GetComponent<AudioSource>(), CallManager.instance.RingRing_Effect, true, true, 0.7f);
        yield return waitPickUpUntil;

        //2. �޾����� ����, ��ȣ���� �Ҹ� ����
        CallManager.instance.SoundPhone(recPhone.GetComponent<AudioSource>());
        //CallManager.instance.SoundPhone(this.phone[0].GetComponent<AudioSource>());
        //CallManager.instance.SoundPhone(this.phone[1].GetComponent<AudioSource>());

        isPickup = false;
    }

    #endregion

    #region PhotonVoice ����
    [PunRPC]
    public void PhotonVoiceSet()
    {
        isMultiStart = true;

        //1. ���ڴ��� ������Ʈ�� ���
        voice = GameObject.Find("Voice").transform;

        recorder = voice.GetComponent<Recorder>().transform;
        //recorder.GetComponent<Recorder>().DebugEchoMode = true; // �̰��� �׽�Ʈ�� ������ �ּ� -> ����Ҹ� �����

        for (int i = 0; i < player.Length; i++)
        {
            targetPhone = phone[i]; //0 : �߽��� , 1 : ������

            //4. Player���� �ִ� PhotonVoiceView�� ���ڴ� ����, ����Ŀ �����ϰ� ������ ���ش�.
            if (i == 0)
            {
                player[i].GetComponent<PhotonVoiceView>().RecorderInUse = recorder.GetComponent<Recorder>();
                //player[i].GetComponent<PhotonVoiceView>().SpeakerInUse = phone[1].GetComponent<Speaker>();
                Transform speakerObj = player[i].GetComponentInChildren<Speaker>(true).transform;
                oriMCRSpeakerParent = player[i];

                speakerObj.GetComponent<Speaker>().enabled = true;
                speakerObj.SetParent(phone[1]);
            }
            else if (i == 1)
            {
                //player[i].GetComponent<PhotonVoiceView>().SpeakerInUse = phone[0].GetComponent<Speaker>();
                Transform speakerObj = player[i].GetComponentInChildren<Speaker>(true).transform;
                oriLocalSpeakerParent = player[i];

                speakerObj.GetComponent<Speaker>().enabled = true;
                speakerObj.SetParent(phone[0]);
            }

            if (!targetPhone.GetComponent<AudioSource>())
                targetPhone.gameObject.AddComponent<AudioSource>();

            targetPhone.gameObject.GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Linear; //�Ÿ��� ���� ��Ҹ��� ũ�Ⱑ �޶�����.
            targetPhone.gameObject.GetComponent<AudioSource>().minDistance = 1f;
            targetPhone.gameObject.GetComponent<AudioSource>().maxDistance = 500f;

            targetPhone.GetComponent<AudioSource>().playOnAwake = true;
            targetPhone.GetComponent<AudioSource>().loop = true;
        }
    }


    [PunRPC]
    public void PhotonVoiceEnd(bool mcr_phone)
    {
        if (mcr_phone)
            phone[0].GetComponent<MultiPagingPhone>().enabled = false;
        else
            phone[1].GetComponent<MultiPagingPhone>().enabled = false;

        ++offPhoneCount;

        if (offPhoneCount == 1)
        {
            Speaker mcrSpeaker = phone[0].GetComponentInChildren<Speaker>(true);
            mcrSpeaker.PlaybackOnlyWhenEnabled = true;
            mcrSpeaker.enabled = false;
            mcrSpeaker.transform.SetParent(oriMCRSpeakerParent);

            Speaker localSpeaker = phone[1].GetComponentInChildren<Speaker>(true);
            localSpeaker.PlaybackOnlyWhenEnabled = true;
            localSpeaker.enabled = false;
            localSpeaker.transform.SetParent(oriLocalSpeakerParent);
        }

        if (offPhoneCount == 2)
        {
            for (int i = 0; i < phone.Length; i++)
            {
                if (phone[i].GetComponentsInChildren<MultiPagingPhone>().Length > 0)
                {
                    foreach (var item in phone[i].GetComponentsInChildren<MultiPagingPhone>())
                    {
                        Destroy(item.GetComponent<MultiPagingPhone>());
                        item.GetComponent<PagingPhoneManager>().enabled = true;
                    }
                }
            }

            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("Init", RpcTarget.All);
                SeqManager.instance.RPC_FinishPaging();
                SeqManager.instance.PV.RPC("RPC_IsCall_T", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void Init()
    {
        player = new Transform[2];
        phone = new Transform[2];
        ringPhone = null;
        isPickup = false;
        isMultiStart = false;
        recorder = null;
        targetPhone = null;
        voice = null;
        offPhoneCount = 0;
        oriMCRSpeakerParent = null;
        oriLocalSpeakerParent = null;
        offPhoneCount = 0;
        isCall = false;
        isRec = false;
    }
    #endregion

    #region �߽��� ��ȣ�� ����
    [PunRPC]
    public void CallSoundOff() //��ȣ���� ���� ����
    {
        if (callObj != null)
            CallManager.instance.SoundPhone(callObj.GetComponent<AudioSource>());
    }
    #endregion

    #region ��� ���� Off
    [PunRPC]
    public void AllSoundOff()
    {
        for (int i = 0; i < SeqManager.phoneList.Count; i++)
        {
            foreach (var item in SeqManager.phoneList[i].GetComponentsInChildren<AudioSource>())
            {
                CallManager.instance.SoundPhone(item.transform.GetComponent<AudioSource>());
                break;
            }
        }

        if (!isMultiStart) //��Ƽ�� ������� �ʾҴٸ� 
        {
            isCall = false;
            isRec = false;
        }
    }

    #endregion
}
