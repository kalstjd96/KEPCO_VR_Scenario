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
    public GameObject typeA; //[[TYPE A]] : 이미지 & 텍스트
    public GameObject typeB; //[[TYPE B]] : 텍스트
    public GameObject typeC; //[[TYPE C]] : 이미지

    public GameObject typeD; //[[TYPE C]] : Attention Text
    public GameObject typeE; //[[TYPE C]] : Attention Text + Image
    public GameObject typeNotice; //[[TYPE C]] : Attention Text + Image
    public GameObject completePanel; //시나리오 종료 시 팝업할 UI
    public GameObject startPanel; //시나리오 시작시 팝업할 UI
    public GameObject SeqJumpPanel; //시나리오를 점프하기 위해 필요한 시퀀스
    public GameObject TroubleValve; //시나리오를 점프하기 위해 필요한 시퀀스
    public GameObject UpStair; //계단 올라가고 싶을때 뜰 팝업창
    public GameObject DownStair; //계단 내려가고 싶을때 뜰 팝업창

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

    #region Popup 실행 함수
    /// <summary>
    /// 타입별 PopupUI 띄우기
    /// [[TYPE A]] : 이미지 & 텍스트
    /// [[TYPE B]] : 텍스트
    /// [[TYPE C]] : 이미지
    /// </summary>
    /// <param name="dataRow"></param>
    public void Pop(DataRow dataRow, bool isNotNextSeq = false)
    {
        //////업무 수행 후 분기 (JumpSeq) 칼럼에 값이 있으면 YES NO팝업 띄우기///////////
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

        /*        //음성 나레이션 있으면 재생
                IsNarration(narrationData);*/

        //[[TYPE C]] : 이미지
        if (string.IsNullOrEmpty(imageData) == false && dataRow["OccurConditions"].ToString().Trim().Equals("TypeC"))
            targetUI = typeC;
        //[[TYPE B]] : 텍스트
        else if (string.IsNullOrEmpty(imageData) == true && string.IsNullOrEmpty(textData) == false)
            targetUI = typeB;
        // [[TYPE A]] : 이미지 & 텍스트
        else targetUI = typeA;

        // Attention Data (type D, type E)
        //[[TYPE D]] : 이미지 + 텍스트 메세지
        /*if (string.IsNullOrEmpty(AttimageData) == false)
            AttenUI = typeE;
        //[[TYPE E]] : 텍스트 메세지
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

        //메세지 처리할 부분이 있으면 처리됨.
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
    /// 입력한 text를 popup UI에 띄우기 (시퀀스 진행 도중 주의사항 등 띄워야할 때 사용)
    /// </summary>
    /// <param name="message">popup UI에 띄울 text 전달</param>
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
    /// 주의사항에 대한 팝업만 띄워주기 위해서
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
                //음성 나레이션 있으면 재생
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


        //음성 나레이션 있으면 재생
        IsNarration(narrationData);
    }


    #region 이미지 설정
    /// <summary>
    /// 이미지 저장 및 띄우기
    /// </summary>
    /// <param name="targetUI">대상 popupUI</param>
    /// <param name="data">DB상의 Image data</param>
    void SetImageViewer(GameObject targetUI, string data)
    {
        string[] dataArray = data.Split(',');
        spriteArray = new Sprite[dataArray.Length];

        for (int i = 0; i < dataArray.Length; i++)
            spriteArray[i] = ImagePopupAtlas.GetSprite(dataArray[i].Trim());

        //초기 이미지 설정
        imageContainer = targetUI.transform.Find("Popup Bg/Popup Image").GetComponent<Image>();
        imageContainer.sprite = spriteArray[0];
        //페이지 번호 설정
        totalPage = spriteArray.Length;
        currentPage = 1;
        pageText = imageContainer.transform.Find("PageText").GetComponent<Text>();
        pageText.text = currentPage + " / " + totalPage;
        //페이지 버튼 저장
        PrevBtn = imageContainer.transform.Find("Prev").GetComponent<Button>();
        NextBtn = imageContainer.transform.Find("Next").GetComponent<Button>();
        //페이지 버튼 이벤트 설정
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

        //페이지 버튼 active 설정
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
    /// PopUp 확인 패널
    /// </summary>
    /// <param name="targetUI">버튼이 있는 UI GameObject</param>
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

    #region YES/NO 선택지 버튼 이벤트 설정
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
    /// 시나리오 다 끝나고 완료를 알리는 UI 팝업
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
    /// Oculus 오른쪽A 버튼이나 Vive 에서는 오른쪽 메뉴 버튼을 눌렀을때 시스템을 종료하기 위해 물어보는 UI 팝업 해주는 함수
    /// </summary>
    public void ExitPopupRequest()
    {
        //활성화 안되있을때만 다시 켜주기
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
        else //켜져 있는 상태에서 한번더 누르면 꺼주고
        {
            completePanel.transform.Find("EndPopup Request").gameObject.SetActive(false);
            completePanel.SetActive(false);
        }
    }

    //프로그램 시작할때 실행될 함수.
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

    //프로그램 시작할때 실행될 함수.
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

    //초기화
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
        //음성 나레이션 있으면 재생
        if (string.IsNullOrEmpty(narrationData) == false)
        {
            //경로 :: Resources/Narration/오디오파일명
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
    /// 추후 관리자모드와 연동할때 
    /// </summary>
    /// <param name="com"></param>
    /// <returns></returns>
    IEnumerator SetEndSeq(GameObject com)
    {
        Text txt;

        //시나리오 데이터 받아서 넣어주고
        txt = com.transform.Find("Popup Bg").Find("part1").Find("content").GetComponent<Text>();
        txt.color = Color.blue;
        string temp1 = UIManager.instance.ScenarioData.text;
        AudioManager.instance.PlayEffAudio("Sound/typing");
        yield return StartCoroutine(TimerManager.instance.Typing(txt, temp1, 0.03f));
        yield return new WaitForSeconds(0.5f);

        //사용자가 선택한 직무 데이터 받아서 넣어주고
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

        //사용자가 진행한 초 시간 더한 데이터 계산해서 넣어주고
        int time = 0;
        #region 총 진행 시간 계산
        foreach (var item in TimerManager.instance.timeDic.Values)
            time += item;

        //string formTime = TimerManager.instance.TimeToString(time);
        #endregion
        txt = com.transform.Find("Popup Bg").Find("part3").Find("content").GetComponent<Text>();
        txt.color = Color.blue;
        AudioManager.instance.PlayEffAudio("Sound/typing");
        yield return StartCoroutine(TimerManager.instance.PlusScore(txt, time));
        yield return new WaitForSeconds(2f);

        #region 제시간에 프로젝트를 완성했는지 아닌지 판별하기위한 부분
        //제시간에 프로젝트를 완료했으면 A+, 아니면 F+
        if (SeqManager.instance.TimeAttack >= time)
            com.transform.Find("Popup Bg").Find("A+Score").gameObject.SetActive(true);
        else
            com.transform.Find("Popup Bg").Find("F+Score").gameObject.SetActive(true);

        AudioManager.instance.PlayEffAudio("Sound/doodong");
        yield return new WaitForSeconds(2f);

        //제시간에 프로젝트를 완료했으면 A+, 아니면 F+
        if (SeqManager.instance.TimeAttack >= time)
            AudioManager.instance.PlayEffAudio("Sound/ChildrenLoud");
        else
            AudioManager.instance.PlayEffAudio("Sound/야유");
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

    #region 계단 시스템
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