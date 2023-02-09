using NA.SGTR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using ZLibNet;

public class PCController : MonoBehaviour
{
    //public static PCController instance { get; set; }
    [SerializeField] RectTransform _mousePoint;
    [SerializeField] GameObject windowList; //화면들 리스트 넣기
    //public GameObject[] mainWindowList;
    public GameObject currentWindow;
    public Transform screenTrans;
    public List<GameObject> higlights { get; set; }
    public RectTransform mousePoint { get { return _mousePoint; } }
    GameObject modelObj;
    public Image loadingBar;
    public TextMeshProUGUI progressIndicator;

    float currentValue = 0f;
    float speed = 120f;
    Vector3 locP;
    Quaternion locQ;
    Vector3 locS;
    bool mainWindowCheck;

    bool isStart;
    bool pageFind;
    bool isCheck = false;

    List<Transform> boxCollList;
    public Transform symbolHighlight; //마우스 오버 시 나타나는 심볼 하이라이트
    public int index; // Target 버튼의 하이라이트를 넣어주기 위함
    List<GameObject> instantObjList;
    Transform orisymbolHighlight;
    public bool currentPC; //현재 PC에서만 PCController를 동작시키기 위해
    Color color;

    private void Awake()
    {
        instantObjList = new List<GameObject>();
        boxCollList = new List<Transform>();
        locP = Vector3.zero;
        pageFind = false;
        isStart = false;
        loadingBar.fillAmount = 0f;
        loadingBar.gameObject.SetActive(false);
        orisymbolHighlight = symbolHighlight.transform.parent;
        index = 0;
        mainWindowCheck = true;
        currentPC = false;// 내가 잡은 PC에서만 target을 추적하기 위해서
        //Init();
    }
    /*
    void Start()
    {
        //MousePoint 세팅 
        for (int i = 0; i < transform.childCount; i++)
        {
            //스크립트에 껴놓은 것들이 빠졌을 때를 방지하기 위함
            if (transform.GetChild(i).name.Contains("MouseCursor"))
            {
                loadingBar = transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>();
                progressIndicator = transform.GetChild(i).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                break;
            }
        }
    }*/
    public void Init()
    {
        if (boxCollList != null && boxCollList.Count > 0)
            for (int j = 0; j < boxCollList.Count; j++)
                Destroy(boxCollList[j].GetComponent<BoxCollider>());

        boxCollList = new List<Transform>();
        locP = Vector3.zero;
        pageFind = false;
        isStart = false;
        loadingBar.fillAmount = 0f;
        loadingBar.gameObject.SetActive(false);

        index = 0; //tagNo의 순서를 알기 위해서
        mainWindowCheck = true; //Main Page가 열린 것인지 J168도면이 열린 것인지 알기 위해서


        if (instantObjList.Count > 0)
        {
            for (int e = 0; e < instantObjList.Count; e++)
                Destroy(instantObjList[e]);
        }
        instantObjList = new List<GameObject>();

        if (symbolHighlight)
            symbolHighlight.gameObject.SetActive(false);

        //메인 첫 페이지 열어두기
        /*if (!mainWindowList[0].transform.parent.parent.gameObject.activeSelf)
            mainWindowList[0].transform.parent.parent.gameObject.SetActive(true);
        */
        /*currentWindow = mainWindowList[0].transform.parent.parent.gameObject;*/
    }

    public void Settings()
    {
        pageFind = false; //페이지를 찾았는지 확인하기 위함\
        var NAData = NA.ButtonToPageLink.data;
        string buttonName = NAData.FirstOrDefault(entry => EqualityComparer<string>.Default.Equals(entry.Value, InputDeviceController._j168)).Key; // 버튼 Name
        var mainWindowList = new Transform[2];
        currentWindow = GetComponentInChildren<NA.PageNavigator>().currentPage.gameObject;
        mainWindowList[0] = currentWindow.transform.GetChild(0).GetChild(1);
        mainWindowList[1] = currentWindow.transform.GetChild(0).GetChild(2);
        if (buttonName == null)
        {
            return;
        }
        //하드로 맞춰논 Primary 화면의 위치, 회전 값 저장
        /*locP = mainWindowList[0].transform.parent.parent.transform.position;
         locQ = mainWindowList[0].transform.parent.parent.transform.rotation;
         locS = mainWindowList[0].transform.parent.parent.transform.localScale;*/
        for (int i = 0; i < mainWindowList.Length; i++)
        {

            for (int j = 0; j < mainWindowList[i].childCount; j++)
            {
                Transform item = mainWindowList[i].transform.GetChild(j);
                if (item.name.Split('-').Length > 1)
                {
                    if (item.name.Split('-')[1].ToString().Trim().Contains(buttonName))//Page를 찾았다!!
                    {
                        pageFind = true; //찾았음을 표시
                        mainWindowCheck = true;

                        //1. 박스 콜라이더 만들기
                        item.gameObject.AddComponent<BoxCollider>().enabled = true;
                        item.gameObject.GetComponent<BoxCollider>().size =
                            new Vector3(item.GetComponent<RectTransform>().rect.width,
                            item.GetComponent<RectTransform>().rect.height, 1.5f);

                        //Loading이 끝나면 해당하는 item의 콜라이더를 제거하기 위해
                        boxCollList.Add(item);

                        TargetHighLight(index, true, false); //심볼 하이라이트 위치 이동 
                        break;
                    }
                }
            }
            //Main 페이지는 복사가 아닌 위치 조정
            if (pageFind) //페이지를 찾았다면 찾은 페이지의 Position, Rotation 값 지정
            {
                /*
                 currentWindow.transform.position = locP; //Page-Primary 
                 currentWindow.transform.rotation = locQ;
                 currentWindow.transform.localScale = locS;
                */
                break;
            }
        }

        isStart = true;
    }

    private void Update()
    {
        if (isStart && currentPC)
            TagetLookCheck();
    }
    public void LoadingOn()
    {
        //(1). 마우스 고정
        //InputDeviceController.isMouse = false;

        loadingBar.gameObject.SetActive(true);
        //(2). 게이지 증가
        if (currentValue < 100)
        {
            currentValue += speed * Time.deltaTime;
            progressIndicator.text = ((int)currentValue).ToString() + "%";
        }
        else
        {
            progressIndicator.text = "Done";
            isCheck = true;

            //InputDeviceController.isMouse = true; //고정 해제
            AudioManager.instance.PlayMultiAudio("Sound/", "MP_Pling");
            index++;
            LoadingOff();

        }
        loadingBar.fillAmount = currentValue / 100;
    }

    public void LoadingOff()
    {
        currentValue = 0f;
        loadingBar.fillAmount = 0f;
        progressIndicator.text = "";
        loadingBar.gameObject.SetActive(false);
        //InputDeviceController.isMouse = true; //고정 해제
    }

    public void TargetHighLight(int index, bool isLoc, bool mouseOver) //하이라이트 순서 , 위치이동 여부, 색상
    {
        if (mouseOver)
        {
            ColorUtility.TryParseHtmlString("#00F0FF", out color);
            symbolHighlight.GetComponentInChildren<Image>().color = color;
        }
        else
            symbolHighlight.GetComponentInChildren<Image>().color = Color.white;

        if (isLoc)
        {
            symbolHighlight.SetParent(boxCollList[index]);
        }
        else if (boxCollList.Count > 0)
        {
            //1. 하이라이트 UI 켜기.
            symbolHighlight.gameObject.SetActive(true);
            symbolHighlight.localPosition = new Vector3(0, 0, -5f);
            symbolHighlight.localEulerAngles = Vector3.zero;
            if (symbolHighlight.GetComponent<Canvas>().enabled == false)
                symbolHighlight.GetComponent<Canvas>().enabled = true;
            //2. 하이라이트 UI 해당 태그로 이동시키기
            symbolHighlight.SetParent(boxCollList[index]);
            //3. 하이라이트 UI 사이즈 맞추기
            symbolHighlight.GetComponent<RectTransform>().sizeDelta =
            new Vector3(boxCollList[index].GetComponent<RectTransform>().rect.width + 50f, boxCollList[index].GetComponent<RectTransform>().rect.height + 50f, -0.1f);

            symbolHighlight.GetComponent<RectTransform>().localScale = Vector3.one;
        }

    }

    #region 해당 부분은 마우스 커서로 꾹누르고 있어 게이지 체크로 확인 절차를 진행하기 위함
    public void TagetLookCheck()
    {
        //헤당 PC 하위에 심볼 하이라이트가 있을 경우에만 동작되게 하기

        Collider[] colliders = Physics.OverlapSphere(mousePoint.position, 0.008f);
        //가끔 마우스 커서를 가져다 대도 안뜨는 경우 발생 길이가 1이라서 발생한 것으로 추정 수정 필요
        if (colliders.Length >= 1)
        {
            if (symbolHighlight)
            {
                //마우스 오버된 상태
                TargetHighLight(index, false, true);
            }

            if (SteamVR_Actions.default_InteractUI.GetState(SteamVR_Input_Sources.RightHand))
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].name.Contains(InputDeviceController._map_Initials)) // 도면약어
                    {
                        Debug.Log("Lookgo : " + InputDeviceController._map_Initials);
                        AudioManager.instance.PlayMultiAudio("Sound/", "MouseClick");
                        StartCoroutine(DeskTopProcess());
                    }
                    else if (!mainWindowCheck && modelObj.name.Contains(colliders[i].name)) //ModelTag
                    {
                        //AudioManager.instance.PlayMultiAudio("Sound/", "MouseClick");
                        LoadingOn();
                    }
                    else if (!mainWindowCheck && colliders[i].name.Contains("Back")) //BackBtn 클릭 시
                    {
                        AudioManager.instance.PlayMultiAudio("Sound/", "MouseClick");
                        currentWindow.SetActive(false);

                        //다시 해당 절차의 메인을 찾는다.
                        isStart = false;
                        Init();
                        Settings();
                    }
                }
            }
        }
        else
        {
            //실전모드일 때 -> 끄기
            //symbolHighlight.gameObject.SetActive(false);

            //연습모드일 때 -> 하얀색으로 표기
            TargetHighLight(index, false, false);
        }

        if (SteamVR_Actions.default_InteractUI.GetStateUp(SteamVR_Input_Sources.RightHand))
        {
            LoadingOff();
        }
    }
    #endregion

    public IEnumerator DeskTopProcess() //J168 도면 페이지
    {
        mainWindowCheck = false;

        WaitUntil waitUntil = new WaitUntil(() => isCheck);

        #region 초기 세팅 
        List<Dictionary<string, List<string>>> j168ListDic = new List<Dictionary<string, List<string>>>();
        Dictionary<string, List<string>> targetList = new Dictionary<string, List<string>>();

        for (int z = 0; z < InputDeviceController._j168.Split('&').Length; z++)
        {
            string key = InputDeviceController._j168.Split('&')[z].ToString().Trim();
            Debug.Log(key);
            if (!targetList.ContainsKey(key))
            {
                targetList.Add(key, new List<string>());
            }
            for (int i = 0; i < InputDeviceController._modelTag.Split('&').Length; i++)
            {
                string valueList = InputDeviceController._modelTag.Split('&')[i].ToString().Trim();
                for (int d = 0; d < valueList.Split('/').Length; d++)
                {
                    string value = valueList.Split('/')[d].ToString().Trim();
                    targetList[key].Add(value);
                }
            }
            j168ListDic.Add(targetList);
        }
        #endregion

        int finalCheck = 0;
        var pageNavi = GetComponentInChildren<NA.PageNavigator>();
        foreach (var item in j168ListDic[0].Keys)
        {
            finalCheck++;
            if (NA.Manager.Instance.pageList.Find(go => go.name == item) != null)//(item.Equals(windowList.transform.GetChild(i).name)) //Key의 이름 나와야 함 9-461-J168-101
            {
                #region 콜라이더 제거 (MainWindow 화면에 존재하는 콜라이더를 제거하기 위함)
                //mainPage RG 같은 도면약어의 콜라이더를 지운다.
                for (int j = 0; j < boxCollList.Count; j++)
                    Destroy(boxCollList[j].GetComponent<BoxCollider>());
                boxCollList = new List<Transform>();
                #endregion

                #region J168도면 PC에 띄우기
                //현재 떠있는 화면 끄기
                currentWindow.SetActive(false);

                //J168 페이지 복사하여 해당 PC에 지정 후 현재 페이지 수정
                //GameObject instantCurrentWindow = Instantiate(windowList.transform.GetChild(i).gameObject, screenTrans);
                //currentWindow = instantCurrentWindow;
                pageNavi.GotoPage(item);
                //pageNavi.currentPage = instantCurrentWindow.GetComponent<NA.Page>();
                //currentWindow.transform.localScale = locS;
                currentWindow = pageNavi.currentPage.gameObject;
                currentWindow.SetActive(true);

                instantObjList.Add(currentWindow); //복제된 Obj들을 관리하기 위함
                #endregion

                #region J168별로 나누고 해당 J168의 ModelTag값 나누기
                foreach (var keyValue in j168ListDic[0][item].ToArray()) //9-461-J168-101
                {
                    Debug.Log(keyValue);
                    for (int s = 0; s < currentWindow.transform.childCount; s++)
                    {
                        if (currentWindow.transform.GetChild(s).name.Split(':').Length > 1)
                        {
                            if (currentWindow.transform.GetChild(s).name.Split(':')[1].ToString().Trim().Contains(keyValue.Substring(2)))
                            {
                                boxCollList.Add(currentWindow.transform.GetChild(s));
                                break;
                            }

                        }
                    }
                }
                if (boxCollList.Count == 0)
                {
                    Debug.Log("BoxCollList.Count = 0");
                }

                #endregion

                #region j168ListDic[0][item]에 해당하는 modelTag의 Transform 정보 콜라이더 씌우기
                for (int j = 0; j < boxCollList.Count; j++)
                {
                    isCheck = false;

                    boxCollList[j].gameObject.SetActive(true);
                    index = j;
                    TargetHighLight(index, true, false); //심볼 하이라이트 위치 이동 

                    modelObj = boxCollList[j].gameObject;

                    //1. 박스 콜라이더 만들기
                    if (!boxCollList[j].gameObject.GetComponent<BoxCollider>())
                        boxCollList[j].gameObject.AddComponent<BoxCollider>().enabled = true;
                    else
                        boxCollList[j].gameObject.GetComponent<BoxCollider>().enabled = true;

                    boxCollList[j].gameObject.GetComponent<BoxCollider>().size =
                    new Vector3(boxCollList[j].GetComponent<RectTransform>().rect.width,
                    boxCollList[j].GetComponent<RectTransform>().rect.height, 3.0f);

                    yield return waitUntil;

                    //해당 심볼 박스 콜라이더 OFF  아님 삭제로 할까?? 
                    boxCollList[j].gameObject.GetComponent<BoxCollider>().enabled = false;

                    if (j == boxCollList.Count - 1)
                    {
                        for (int e = 0; e < instantObjList.Count; e++)
                            instantObjList[e].SetActive(false);
                        instantObjList = new List<GameObject>();
                        symbolHighlight.SetParent(orisymbolHighlight);
                        if (finalCheck == j168ListDic[0].Count) //마지막 절차
                        {
                            Debug.Log("마지막 절차완료");
                            //마우스를 뗀다 
                            //transform.GetComponentInChildren<InputDeviceController>().MouseDetach();
                            index = 0;
                            currentPC = false;
                            //InputDeviceController.isSettings = true;
                            Init();
                            SeqManager.instance.IsOn_true();
                        }
                    }
                }
                break;
                #endregion
            }
            else
            {
                Debug.Log("WindowList에 해당 모델이 없음 : " + item);
            }

        }
    }

}
