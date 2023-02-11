using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PopupManager : MonoBehaviour
{
    #region PROPERTIES
    public static PopupManager instance;

    [Header("UI Objects")]
    public GameObject typeA; //[[TYPE A]] : �̹��� & �ؽ�Ʈ
    public GameObject typeB; //[[TYPE B]] : �ؽ�Ʈ
    public GameObject typeC; //[[TYPE C]] : �̹���

    public GameObject typeD; //[[TYPE C]] : Attention Text
    public GameObject typeE; //[[TYPE C]] : Attention Text + Image
    public GameObject typeNotice; //[[TYPE C]] : Attention Text + Image
    public GameObject completePanel; //�ó����� ���� �� �˾��� UI
    public GameObject startPanel; //�ó����� ���۽� �˾��� UI
    public GameObject SeqJumpPanel; //�ó������� �����ϱ� ���� �ʿ��� ������
    public GameObject TroubleValve; //�ó������� �����ϱ� ���� �ʿ��� ������
    public GameObject UpStair; //��� �ö󰡰� ������ �� �˾�â
    public GameObject DownStair; //��� �������� ������ �� �˾�â

    [Header("Atlas")]
    public SpriteAtlas ImagePopupAtlas;

    int currentPage = 1;
    int totalPage;
    Text pageText;
    Image imageContainer;
    Sprite[] spriteArray;
    Button PrevBtn;
    Button NextBtn;
    #endregion
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }

    #region Popup ���� �Լ�
    /// <summary>
    /// Ÿ�Ժ� PopupUI ����
    /// [[TYPE A]] : �̹��� & �ؽ�Ʈ
    /// [[TYPE B]] : �ؽ�Ʈ
    /// [[TYPE C]] : �̹���
    /// </summary>
    /// <param name="dataRow"></param>
    public void Pop(DataRow dataRow, bool isNotNextSeq = false)
    {
        //////���� ���� �� �б� (JumpSeq) Į���� ���� ������ YES NO�˾� ����///////////
        if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()) && !dataRow["JumpSeq"].ToString().Trim().ToLower().Contains("exit"))
        {
            SeqManager.instance.IsOn_true();
            return;
        }

        string imageData = dataRow["OP_ImageFile"].ToString();
        string textData = dataRow["Operator"].ToString();
        string narrationData = dataRow["Narration"].ToString();
        //string AttentionData = dataRow["Attention"].ToString();
        //string AttimageData = dataRow["Att_ImageFile"].ToString();

        GameObject targetUI = null;
        //GameObject AttenUI = null;

        /*        //���� �����̼� ������ ���
                IsNarration(narrationData);*/

        //[[TYPE C]] : �̹���
        if (string.IsNullOrEmpty(imageData) == false && dataRow["OccurConditions"].ToString().Trim().Equals("TypeC"))
            targetUI = typeC;
        //[[TYPE B]] : �ؽ�Ʈ
        else if (string.IsNullOrEmpty(imageData) == true && string.IsNullOrEmpty(textData) == false)
            targetUI = typeB;
        // [[TYPE A]] : �̹��� & �ؽ�Ʈ
        else targetUI = typeA;

        // Attention Data (type D, type E)
        //[[TYPE D]] : �̹��� + �ؽ�Ʈ �޼���
        /*if (string.IsNullOrEmpty(AttimageData) == false)
            AttenUI = typeE;
        //[[TYPE E]] : �ؽ�Ʈ �޼���
        else if (string.IsNullOrEmpty(AttimageData) == true && string.IsNullOrEmpty(AttentionData) == false)
            AttenUI = typeD;
        else AttenUI = null;*/

        targetUI.SetActive(true);
        //0914-MS
        if (targetUI.GetComponentInChildren<Hand_Interaction_Item>())
            targetUI.GetComponentInChildren<Hand_Interaction_Item>().enabled = true;
        //

        if (string.IsNullOrEmpty(imageData) == false)
            SetImageViewer(targetUI, imageData);
        if (string.IsNullOrEmpty(textData) == false && targetUI != typeC)
            targetUI.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = textData;
        SetVerifyBT(targetUI, isNotNextSeq);

        //�޼��� ó���� �κ��� ������ ó����.
/*        if (AttenUI != null)
        {
            AttenUI.SetActive(true);
            if (string.IsNullOrEmpty(AttimageData) == false)
                SetImageViewer(AttenUI, AttimageData);
            if (string.IsNullOrEmpty(AttentionData) == false)
                AttenUI.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = AttentionData;

            SetAttentionBT(AttenUI);
        }*/

        AudioManager.instance.PlayMultiAudio("Sound/Popup");
    }

    /// <summary>
    /// �Է��� text�� popup UI�� ���� (������ ���� ���� ���ǻ��� �� ������� �� ���)
    /// </summary>
    /// <param name="message">popup UI�� ��� text ����</param>
    public void Pop(string message, string Audioname = null, Function onPressed = null)
    {
        AudioManager.instance.PlayMultiAudio("Sound/Popup");

        typeB.SetActive(true);
        typeB.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = message;
        SetAttentionBT(typeB, onPressed);

        if(Audioname != null)
        {
            AudioManager.instance.PlayEffAudio("Narration/" + Audioname);
        }

    }
    #endregion

    /// <summary>
    /// ���ǻ��׿� ���� �˾��� ����ֱ� ���ؼ�
    /// </summary>
    public void PopMessage(DataRow dataRow)
    {
        string narrationData = dataRow["Narration"].ToString();
        string AttentionData = dataRow["Attention"].ToString();

        if (!string.IsNullOrEmpty(AttentionData))
        {
            AudioManager.instance.PlayMultiAudio("Sound/Popup");
            GameObject targetUI = typeD;

            targetUI.SetActive(true);
            targetUI.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = AttentionData;
            SetAttentionBT(targetUI);

            if (!string.IsNullOrEmpty(narrationData))
            {
                //���� �����̼� ������ ���
                IsNarration(narrationData);
            }
        }

    }

    public void PopNoticeSeq(DataRow dataRow, bool isNotNextSeq = false)
    {
        //string imageData = dataRow["OP_ImageFile"].ToString();
        string textData = dataRow["Operator"].ToString();
        GameObject targetUI = typeNotice;
        //string narrationData = dataRow["Narration"].ToString();

        targetUI.SetActive(true);
        if (string.IsNullOrEmpty(textData) == false)
            targetUI.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = textData;
        SetVerifyBT(targetUI, isNotNextSeq);

        AudioManager.instance.PlayMultiAudio("Sound/Popup");
    }


    public void PopJumpSeq(DataRow dataRow)
    {
        AudioManager.instance.PlayMultiAudio("Sound/MP_MessageAlarm");

        string Jumpseq = dataRow["Operator"].ToString().Trim();
        string narrationData = dataRow["Narration"].ToString();

        //GameObject targetUI = SeqJumpPanel;
        SeqJumpPanel.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = Jumpseq;
        SeqJumpPanel.SetActive(true);
        SetJumpBT(SeqJumpPanel);


        //���� �����̼� ������ ���
        IsNarration(narrationData);
    }


    #region �̹��� ����
    /// <summary>
    /// �̹��� ���� �� ����
    /// </summary>
    /// <param name="targetUI">��� popupUI</param>
    /// <param name="data">DB���� Image data</param>
    void SetImageViewer(GameObject targetUI, string data)
    {
        string[] dataArray = data.Split(',');
        spriteArray = new Sprite[dataArray.Length];

        for (int i = 0; i < dataArray.Length; i++)
            spriteArray[i] = ImagePopupAtlas.GetSprite(dataArray[i].Trim());

        //�ʱ� �̹��� ����
        imageContainer = targetUI.transform.Find("Popup Bg/Popup Image").GetComponent<Image>();
        imageContainer.sprite = spriteArray[0];
        //������ ��ȣ ����
        totalPage = spriteArray.Length;
        currentPage = 1;
        pageText = imageContainer.transform.Find("PageText").GetComponent<Text>();
        pageText.text = currentPage + " / " + totalPage;
        //������ ��ư ����
        PrevBtn = imageContainer.transform.Find("Prev").GetComponent<Button>();
        NextBtn = imageContainer.transform.Find("Next").GetComponent<Button>();
        //������ ��ư �̺�Ʈ ����
        //PrevBtn.GetComponent<PressTriggerEvent>().onClick.RemoveAllListeners();
        PrevBtn.GetComponent<PressTriggerEvent>().AddHanddPressListner(
            delegate
            {
                NextBtn.gameObject.SetActive(true);
                if (currentPage == 1) return;

                currentPage -= 1;

                imageContainer.sprite = spriteArray[currentPage - 1];
                pageText.text = currentPage + " / " + totalPage;

                if (currentPage == 1)
                    PrevBtn.gameObject.SetActive(false);
                else
                    NextBtn.gameObject.SetActive(true);
            });
        //NextBtn.GetComponent<PressTriggerEvent>().onClick.RemoveAllListeners();
        NextBtn.GetComponent<PressTriggerEvent>().AddHanddPressListner(
            delegate
            {
                PrevBtn.gameObject.SetActive(true);
                if (currentPage == totalPage) return;

                currentPage++;
                imageContainer.sprite = spriteArray[currentPage - 1];
                pageText.text = currentPage + " / " + totalPage;

                if (currentPage == totalPage)
                    NextBtn.gameObject.SetActive(false);
                else 
                    PrevBtn.gameObject.SetActive(true);
            });

        //������ ��ư active ����
        if (spriteArray.Length <= 1)
        {
            PrevBtn.gameObject.SetActive(false);
            NextBtn.gameObject.SetActive(false);
        }
        else
        {
            PrevBtn.gameObject.SetActive(false);
            NextBtn.gameObject.SetActive(true);
        }
    }
    #endregion

    /// <summary>
    /// PopUp Ȯ�� �г�
    /// </summary>
    /// <param name="targetUI">��ư�� �ִ� UI GameObject</param>
    void SetVerifyBT(GameObject targetUI, bool notNextSeq = false)
    {
        GameObject btn = targetUI.transform.Find("Popup Bg").Find("Popup Button").gameObject;
        btn.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().originalColor; 

        }, delegate {
            //if (!string.IsNullOrEmpty(SeqManager.instance.dataRow["TagNo"].ToString()))
            //{
            //    string[] array = null;
            //    array = SeqManager.instance.dataRow["TagNo"].ToString().Split('/');
            //    for (int i = 0; i < SeqManager.tagNoList.Count; i++)
            //    {
            //        for (int j = 0; j < array.Length; j++)
            //        {
            //            if (SeqManager.tagNoList[i].name.Equals(SeqManager.instance.dataRow["TagNo"].ToString().Split('/')[j].Trim(), System.StringComparison.OrdinalIgnoreCase))
            //            {
            //                foreach (Animation anim in SeqManager.tagNoList[i].GetComponentsInChildren<Animation>())
            //                {
            //                    anim.Play();
            //                }
            //                break;
            //            }
            //        }
            //    }
            //}

            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
            if (!notNextSeq)
                SeqManager.instance.IsOn_true();
            targetUI.SetActive(false);

            //0914-MS
            if (targetUI.GetComponentInChildren<Hand_Interaction_Item>())
                targetUI.GetComponentInChildren<Hand_Interaction_Item>().enabled = false;
            //

            AudioManager.instance.StopEffAudio();
        });
        btn.GetComponent<PressTriggerEvent>().OnStayTriggerEvent(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
        });
    }

    void SetAttentionBT(GameObject targetUI, Function onPressed = null)
    {
        GameObject btn = targetUI.transform.Find("Popup Bg").Find("Popup Button").gameObject;
        btn.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().originalColor;

        }, delegate {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
            //targetUI.SetActive(false);
            AudioManager.instance.StopEffAudio();
            closeAnim(targetUI);

            onPressed?.Invoke();
        });
        btn.GetComponent<PressTriggerEvent>().OnStayTriggerEvent(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
        });
    }

    #region YES/NO ������ ��ư �̺�Ʈ ����
    void SetJumpBT(GameObject targetUI)
    {
        GameObject btn_Yes = targetUI.transform.Find("Popup Bg/YES Button").gameObject;
        GameObject btn_NO = targetUI.transform.Find("Popup Bg/NO Button").gameObject;
        btn_Yes.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            btn_Yes.GetComponent<Image>().color = btn_Yes.GetComponent<PressTriggerEvent>().originalColor;

            if (!ConnManager.instance.UseNetwork)
                ChooseManager.instance.OnClick_YN(true);
            else
                ChooseManager.instance.PV.RPC("OnClick_YN", RpcTarget.All, true);

        }, delegate {

            btn_Yes.GetComponent<Image>().color = btn_Yes.GetComponent<PressTriggerEvent>().originalColor;

        });
        btn_Yes.GetComponent<PressTriggerEvent>().OnStayTriggerEvent(delegate
        {
            btn_Yes.GetComponent<Image>().color = btn_Yes.GetComponent<PressTriggerEvent>().originalColor;
        });


        
        btn_NO.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            
            btn_NO.GetComponent<Image>().color = btn_NO.GetComponent<PressTriggerEvent>().originalColor;
            if (!ConnManager.instance.UseNetwork)
                ChooseManager.instance.OnClick_YN(false);
            else
                ChooseManager.instance.PV.RPC("OnClick_YN", RpcTarget.All, false);

        }, delegate {
            btn_NO.GetComponent<Image>().color = btn_NO.GetComponent<PressTriggerEvent>().originalColor;
        });
        btn_NO.GetComponent<PressTriggerEvent>().OnStayTriggerEvent(delegate
        {
            btn_NO.GetComponent<Image>().color = btn_NO.GetComponent<PressTriggerEvent>().originalColor;
        });

        if (ConnManager.instance.UseNetwork)
        {
            if (!SeqManager.instance.CheckMyTurn(SeqManager.instance.dataRow))
            {
                btn_Yes.SetActive(false);
                btn_NO.SetActive(false);
            }
            else
            {
                btn_Yes.SetActive(true);
                btn_NO.SetActive(true);
            }
        }
    }
    #endregion

    /// <summary>
    /// �ó����� �� ������ �ϷḦ �˸��� UI �˾�
    /// </summary>
    public void PopupEnd()
    {
        AudioManager.instance.PlayMultiAudio("Sound/Popup");
        completePanel.SetActive(true);
        completePanel.transform.Find("Popup Bg").gameObject.SetActive(true);
        completePanel.transform.Find("Popup Bg/Popup Button").GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate{
            completePanel.transform.Find("Popup Bg").gameObject.SetActive(false);
            //completePanel.transform.Find("EndSystem Bg").gameObject.SetActive(true);
            //AudioManager.instance.PlayEffAudio("Narration/Ending");
            //StartCoroutine(ExitSystem(completePanel.transform.Find("EndSystem Bg/Title BG/second").GetComponent<Text>()));
            ExitPopupRequest();
        });
        StartCoroutine(SetEndSeq(completePanel));
    }

    /// <summary>
    /// Oculus ������A ��ư�̳� Vive ������ ������ �޴� ��ư�� �������� �ý����� �����ϱ� ���� ����� UI �˾� ���ִ� �Լ�
    /// </summary>
    public void ExitPopupRequest()
    {
        //Ȱ��ȭ �ȵ��������� �ٽ� ���ֱ�
        if (!completePanel.transform.Find("EndPopup Request").gameObject.activeSelf)
        {
            AudioManager.instance.PlayMultiAudio("Sound/Popup");

            completePanel.transform.Find("EndPopup Request/No Button").GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate {
                completePanel.transform.Find("EndPopup Request").gameObject.SetActive(false);
                completePanel.SetActive(false);
            });

            completePanel.transform.Find("EndPopup Request/Yes Button").GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate {
                completePanel.transform.Find("EndSystem Bg").gameObject.SetActive(true);
                completePanel.transform.Find("EndPopup Request").gameObject.SetActive(false);
                AudioManager.instance.PlayEffAudio("Narration/Ending");
                StartCoroutine(ExitSystem(completePanel.transform.Find("EndSystem Bg/Title BG/second").GetComponent<Text>()));
            });

            completePanel.transform.Find("EndPopup Request").gameObject.SetActive(true);
            completePanel.SetActive(true);
        }
        else //���� �ִ� ���¿��� �ѹ��� ������ ���ְ�
        {
            completePanel.transform.Find("EndPopup Request").gameObject.SetActive(false);
            completePanel.SetActive(false);
        }
    }

    //���α׷� �����Ҷ� ����� �Լ�.
    public void PopipStart()
    {
        startPanel.SetActive(true);
        AudioManager.instance.PlayEffAudio("Narration/" + "intro");
        GameObject btn = startPanel.transform.Find("Popup Bg").Find("Popup Button").gameObject;

        btn.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().originalColor;

        }, delegate {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
            SeqManager.instance.isStart = true;
            startPanel.SetActive(false);
        });
        btn.GetComponent<PressTriggerEvent>().OnStayTriggerEvent(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
        });
    }

    //���α׷� �����Ҷ� ����� �Լ�.
    public void PopipStart_Multi()
    {
        startPanel.SetActive(true);
        AudioManager.instance.PlayEffAudio("Narration/" + "intro");
        GameObject btn = startPanel.transform.Find("Popup Bg").Find("Popup Button").gameObject;

        if (PhotonNetwork.IsMasterClient)
        {
            btn.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
            {
                btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().originalColor;

            }, delegate {
                btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
                SeqManager.instance.PV.RPC("RPC_StartButton", RpcTarget.All);
            });
            btn.GetComponent<PressTriggerEvent>().OnStayTriggerEvent(delegate
            {
                btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
            });
        }
        else
        {
            btn.SetActive(false);
        }

    }

    public void Popup_TroubleValve()
    {
        if (!TroubleValve.activeSelf)
        {
            AudioManager.instance.PlayMultiAudio("Sound/MP_MessageAlarm");
            TroubleValve.SetActive(true);
        }
    }

    //�ʱ�ȭ
    public void Initialize()
    {
        AudioManager.instance.StopMultiAudio();

        if (NextBtn)
            NextBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        if (PrevBtn)
            PrevBtn.GetComponent<Button>().onClick.RemoveAllListeners();

        currentPage = 1;

        typeC.SetActive(false);
        typeB.SetActive(false);
        typeA.SetActive(false);
        SeqJumpPanel.SetActive(false);
        completePanel.SetActive(false);
    }

    public void IsNarration(string narrationData)
    {
        //���� �����̼� ������ ���
        if (string.IsNullOrEmpty(narrationData) == false)
        {
            //��� :: Resources/Narration/��������ϸ�
            AudioClip audioClip = Resources.Load<AudioClip>("Narration/" + narrationData);
            if (audioClip)
                AudioManager.instance.PlayEffAudio(audioClip);
        }
    }

    void closeAnim(GameObject target)
    {
        StartCoroutine(CloseAfterDelay(target));
    }

    IEnumerator CloseAfterDelay(GameObject target)
    {
        Animator anim =  target.transform.GetComponent<Animator>();

        anim.SetTrigger("close");
        yield return new WaitForSeconds(0.5f);
        target.SetActive(false);
        anim.ResetTrigger("close");
    }


    /// <summary>
    /// ���� �����ڸ��� �����Ҷ� 
    /// </summary>
    /// <param name="com"></param>
    /// <returns></returns>
    IEnumerator SetEndSeq(GameObject com)
    {
        Text txt;

        //�ó����� ������ �޾Ƽ� �־��ְ�
        txt = com.transform.Find("Popup Bg").Find("part1").Find("content").GetComponent<Text>();
        txt.color = Color.blue;
        string temp1 = UIManager.instance.ScenarioData.text;
        AudioManager.instance.PlayEffAudio("Sound/typing");
        yield return StartCoroutine(TimerManager.instance.Typing(txt, temp1, 0.03f));
        yield return new WaitForSeconds(0.5f);

        //����ڰ� ������ ���� ������ �޾Ƽ� �־��ְ�
        txt = com.transform.Find("Popup Bg").Find("part2").Find("content").GetComponent<Text>();
        txt.color = Color.blue;
        string tmep2 = ConnManager.instance.myActor[0].ToString();
        for (int i = 1; i < ConnManager.instance.myActor.Length; i++)
        {
            tmep2 += ", ";
            tmep2 += ConnManager.instance.myActor[i].ToString();
        }
        AudioManager.instance.PlayEffAudio("Sound/typing");
        yield return StartCoroutine(TimerManager.instance.Typing(txt, tmep2, 0.03f));
        yield return new WaitForSeconds(0.5f);

        //����ڰ� ������ �� �ð� ���� ������ ����ؼ� �־��ְ�
        int time = 0;
        #region �� ���� �ð� ���
        foreach (var item in TimerManager.instance.timeDic.Values)
            time += item;

        //string formTime = TimerManager.instance.TimeToString(time);
        #endregion
        txt = com.transform.Find("Popup Bg").Find("part3").Find("content").GetComponent<Text>();
        txt.color = Color.blue;
        AudioManager.instance.PlayEffAudio("Sound/typing");
        yield return StartCoroutine(TimerManager.instance.PlusScore(txt, time));
        yield return new WaitForSeconds(2f);

        #region ���ð��� ������Ʈ�� �ϼ��ߴ��� �ƴ��� �Ǻ��ϱ����� �κ�
        //���ð��� ������Ʈ�� �Ϸ������� A+, �ƴϸ� F+
        if (SeqManager.instance.TimeAttack >= time)
            com.transform.Find("Popup Bg").Find("A+Score").gameObject.SetActive(true);
        else
            com.transform.Find("Popup Bg").Find("F+Score").gameObject.SetActive(true);

        AudioManager.instance.PlayEffAudio("Sound/doodong");
        yield return new WaitForSeconds(2f);

        //���ð��� ������Ʈ�� �Ϸ������� A+, �ƴϸ� F+
        if (SeqManager.instance.TimeAttack >= time)
            AudioManager.instance.PlayEffAudio("Sound/ChildrenLoud");
        else
            AudioManager.instance.PlayEffAudio("Sound/����");
        #endregion

        yield return new WaitForSeconds(1f);
        com.transform.Find("Popup Bg").Find("Popup Text").gameObject.SetActive(true);
        com.transform.Find("Popup Bg").Find("Popup Button").gameObject.SetActive(true);

    }

    IEnumerator ExitSystem(Text second)
    {
        float time = 5f;
        second.color = Color.red;

        while (time >= 0)
        {
            time -= Time.deltaTime;
            second.text = ((int)time).ToString();

            yield return null;
        }

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();

    }

    #region ��� �ý���
    public void RequestStair(UpDownStair.StairType type, UpDownStair target)
    {
        GameObject targetPopup;
        if(type == UpDownStair.StairType.UpperFloor) targetPopup = DownStair;
        else targetPopup = UpStair;

        targetPopup.SetActive(true);

        GameObject btn = targetPopup.transform.Find("Popup Bg").Find("YES Button").gameObject;
        btn.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            btn.GetComponent<Image>().color = btn.GetComponent<PressTriggerEvent>().highlightColor;
            target.StartCoroutineCycle(type);
            targetPopup.SetActive(false);
        });

        btn = targetPopup.transform.Find("Popup Bg").Find("No Button").gameObject;
        btn.GetComponent<PressTriggerEvent>().AddHanddPressListner(delegate
        {
            targetPopup.SetActive(false);
        });
    }

    public void ExitStair(UpDownStair.StairType type, UpDownStair target)
    {
        GameObject targetPopup;
        if (type == UpDownStair.StairType.UpperFloor) targetPopup = DownStair;
        else targetPopup = UpStair;

        if(targetPopup.activeSelf)
            targetPopup.SetActive(false);
    }
    #endregion


}