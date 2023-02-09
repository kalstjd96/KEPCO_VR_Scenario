using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NA;
using Button = UnityEngine.UI.Button;
using System.IO;

public class SymbolHighlight : MonoBehaviour
{
    [System.NonSerialized] public Transform currentPage;
    [System.NonSerialized] public Transform destoryTarget;

    public Transform symbolHighlight;

    [Header("RO")]
    public PageNavigator ro_pageNavigator;

    [Header("TO")]
    public PageNavigator to_pageNavigator;

    [Header("EO")]
    public PageNavigator eo_pageNavigator;

    PageNavigator navigator;

    bool isFind; //도면을 찾았는 지 확인
    bool isMainpage; //메인 페이지인지 확인
    bool isDepth;
    bool isAlarm;
    string mainGuideBtn;

    List<GameObject> copyHighLightList;
    List<GameObject> priButtonList;

    [Space(10f)]
    public bool isSetting;

    StreamWriter sw;

    private void Awake()
    {
        isFind = false;
        isMainpage = false;
        isSetting = false;
        isDepth = false;
        isAlarm = false;
        copyHighLightList = new List<GameObject>();
        priButtonList = new List<GameObject>();
    }

    #region 페이지 변경 SET 함수
    public void Set(PageNavigator pageNavigator, Transform current)
    {
        if (ConnManager.instance.PlayMode == RoomData.Mode.Training)
        {
            foreach (var item in symbolHighlight.GetComponentsInChildren<Image>(true))
            {
                item.enabled = false;
            }
            if (symbolHighlight.transform.Find("HotspotEffect") != null)
                symbolHighlight.transform.Find("HotspotEffect").gameObject.SetActive(false);
        }

        // 직무에 맞지않는 PC는 하이라이트 생성 안되게
        if (!pageNavigator.actor.ToUpper().Equals(InputDeviceController._actor.ToUpper().Trim()))
        return;

        currentPage = current;
        navigator = pageNavigator;

        copyHighLightList.Clear();
        priButtonList.Clear();
        //DestroyPage();

        string[] initialsArray = InputDeviceController._list_initials.ToArray();
        for (int t = 0; t < initialsArray.Length; t++)
        {
            isFind = false;
            isMainpage = false;
            isDepth = false;
            isAlarm = false;

            string[] initialDepth = initialsArray[t].Split('§');

            //0. 현재 Tag가 알람일 때
            if (InputDeviceController._list_tag.Contains("Alarm"))
            {
                foreach (var panelBtn in pageNavigator.topPanel.GetComponentsInChildren<NA.Control.Button>())
                {
                    if (panelBtn.buttonTitle.Equals("Alarm"))
                    {
                        TargetHighLight(panelBtn.transform, false);
                        isFind = true;
                        break;
                    }
                }

                isAlarm = true;
            }

            //1. 현재 페이지가 메인인지 아닌지 확인
            if (!isAlarm && (currentPage.name.Contains("Primary") || currentPage.name.Contains("Secondary")))
            {
                foreach (var symbolObj in pageNavigator.topPanel.GetComponentsInChildren<Symbol>())
                {
                    Destroy(symbolObj.gameObject);
                }

                isMainpage = true;

                foreach (var item in currentPage.GetComponentsInChildren<NA.Button>())
                {
                    if (item.buttonTitle.Equals(initialDepth[0].Trim()))
                    {
                        // item은 버튼스크립트가 붙어있는 하위오브젝트니 상위 부모를 핫스팟 해준다.
                        TargetHighLight(item.transform.parent, false);
                        isFind = true;
                        break;
                    }
                }

                if (currentPage.name.Contains("Primary"))
                    mainGuideBtn = "SECD";
                else if (currentPage.name.Contains("Secondary"))
                    mainGuideBtn = "PRIM";
            }

            //2. 현재 페이지가 메인인데 이 페이지에는 도면약어가 없어
            if (!isFind && isMainpage)
            {
                //(1) 다른 메인 페이지창으로 안내
                foreach (var panelBtn in pageNavigator.topPanel.GetComponentsInChildren<NA.Control.Button>())
                {
                    if (panelBtn.buttonTitle.Equals(mainGuideBtn))
                    {
                        // panelBtn은 버튼스크립트가 붙어있는 하위오브젝트니 상위 부모를 핫스팟 해준다.
                        TargetHighLight(panelBtn.transform, false);
                        isFind = true;
                        break;
                    }
                }
            }

            //3. J168화면일때 
            if (!isAlarm && !isMainpage)
            {
                Page currentShape = currentPage.GetComponent<Page>();

                //(1) 우리가 원하는 J168 도면을 찾았다
                string[] j168Array = InputDeviceController._j168.Split('&');
                string[] j168TagArray = InputDeviceController._list_tag.ToArray();

                for (int i = 0; i < j168Array.Length; i++)
                {
                    if (currentPage.name.Contains(j168Array[i].ToString().Trim()))
                    {
                        isFind = true;
                        //string[] j168TagArray = dbTagArray[i].Split('/');
                        for (int j = 0; j < currentShape.ShapeList.Count; j++)
                        {
                            for (int k = 0; k < j168TagArray.Length; k++)
                            {
                                if (currentShape.ShapeList[j].Value.ContainsKey("Tag No.") &&
                                    currentShape.ShapeList[j].Value["Tag No."].Equals(j168TagArray[k].ToString().Trim()))
                                    TargetHighLight(currentShape.ShapeList[j].transform, true, j168TagArray[k].ToString().Trim());
                            }
                        }
                    }

                    //(2) 부모 도면이 있을 때
                    else
                    {
                        if (initialDepth.Length > 1)
                        {
                            foreach (var item in currentPage.GetComponentsInChildren<NA.Button>())
                            {
                                if (item.buttonTitle.Equals(initialDepth[1].Trim()))
                                {
                                    // item은 버튼스크립트가 붙어있는 하위오브젝트니 상위 부모를 핫스팟 해준다.
                                    TargetHighLight(item.transform.parent, false);
                                    isDepth = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in currentPage.GetComponentsInChildren<NA.Button>())
                            {
                                if (item.buttonTitle.Equals(initialDepth[0].Trim()))
                                {
                                    // item은 버튼스크립트가 붙어있는 하위오브젝트니 상위 부모를 핫스팟 해준다.
                                    TargetHighLight(item.transform.parent, false);
                                    isDepth = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                //(3) 우리가 원하는 도면이 아니다!
                if (!isFind && !isDepth)
                {
                    foreach (var item in pageNavigator.defaultPage.GetComponentsInChildren<NA.Button>(true))
                    {
                        if (item.buttonTitle.Equals(initialDepth[0].Trim()))
                        {
                            // item은 버튼스크립트가 붙어있는 하위오브젝트니 상위 부모를 핫스팟 해준다.
                            isFind = true;
                            mainGuideBtn = "PRIM";
                            break;
                        }
                    }

                    if (!isFind)
                        mainGuideBtn = "SECD";

                    foreach (var panelBtn in pageNavigator.topPanel.GetComponentsInChildren<NA.Control.Button>())
                    {
                        if (panelBtn.buttonTitle.Equals(mainGuideBtn))
                        {
                            // panelBtn은 버튼스크립트가 붙어있는 하위오브젝트니 상위 부모를 핫스팟 해준다.
                            TargetHighLight(panelBtn.transform, false);
                            isFind = true;
                            break;
                        }
                    }
                }
            }

            if (isFind)
                isSetting = true;
            else
                Debug.Log("못찾앗음 : " + currentPage.name);
        }
    }

    #endregion

    #region 마우스 오버 함수

    public void OnMouse(Transform currentTarget, bool mouseOver)
    {
        // 1. 마우스가 닿았다면
        if (mouseOver)
        {
            currentTarget.GetComponentInChildren<Image>().color = Color.blue;
            currentTarget.GetComponent<Symbol>().isHover = true;
        }
        else // (1) 닿지 않았다면
        {
            currentTarget.GetComponentInChildren<Image>().color = Color.blue;
            currentTarget.GetComponent<Symbol>().isHover = false;
        }
    }

    #endregion

    #region 하이라이트 생성

    public void TargetHighLight(Transform target, bool addList = true, string targetName = null)
    {
        if (!target.GetComponentInChildren<Symbol>())
        {
            //1. 하이라이트 카피
            GameObject copysymbolHighlight = Instantiate(symbolHighlight.gameObject);

            if (string.IsNullOrEmpty(targetName))
                copysymbolHighlight.transform.name = target.name;
            else
                copysymbolHighlight.transform.name = targetName;

            //2. 리스트 카피 담기 -> 초기화시 삭제
            if (addList)
                copyHighLightList.Add(copysymbolHighlight);
            else
            {
                priButtonList.Add(copysymbolHighlight);
                copysymbolHighlight.GetComponent<Symbol>().enabled = false;
            }

            if (copysymbolHighlight.GetComponent<Canvas>().enabled == false)
                copysymbolHighlight.GetComponent<Canvas>().enabled = true;

            //3. 하이라이트 UI 해당 태그로 이동시키기
            copysymbolHighlight.transform.SetParent(target);
            copysymbolHighlight.transform.localPosition = Vector3.zero;
            copysymbolHighlight.transform.localEulerAngles = Vector3.zero;

            //3. 하이라이트 UI 사이즈 맞추기
            RectTransform highlgihtRect = copysymbolHighlight.GetComponent<RectTransform>();

            //----------stretch로 변경------------
            highlgihtRect.anchorMin = Vector2.zero;
            highlgihtRect.anchorMax = Vector2.one;
            //------------------------------------
            highlgihtRect.offsetMin = new Vector2(-5f, -5f);  // x : Left  , y : Bottom
            highlgihtRect.offsetMax = new Vector2(5, 5);  // x : Right , y : Top (오른쪽과 위쪽은 부호를 반대로 적어줘야함)
            //highlgihtRect.offsetMax = new Vector2(5, 55);  // x : Right , y : Top (오른쪽과 위쪽은 부호를 반대로 적어줘야함)

            highlgihtRect.localScale = Vector3.one;

            //copysymbolHighlight.GetComponent<RectTransform>().sizeDelta =
            //new Vector3(target.GetComponent<RectTransform>().rect.width + 50f, target.GetComponent<RectTransform>().rect.height + 50f, -0.1f);
            //copysymbolHighlight.GetComponent<RectTransform>().localScale = Vector3.one;

            //4. 박스콜라이더 생성 및 박스콜라이더 사이즈 맞추기
            if (!copysymbolHighlight.GetComponent<BoxCollider>())
                copysymbolHighlight.AddComponent<BoxCollider>();

            copysymbolHighlight.GetComponent<BoxCollider>().size = new Vector3(target.GetComponent<RectTransform>().rect.width,
                target.GetComponent<RectTransform>().rect.height, 5f);

            //5. 하이라이트 색상 변경
            //copysymbolHighlight.GetComponentInChildren<Image>().color = Color.white;
            copysymbolHighlight.GetComponentInChildren<Image>().color = Color.blue;

            //6. 하이라이트 오브젝트 켜주기
            copysymbolHighlight.SetActive(true);
        }
    }

    #endregion

    #region 하이라이트 심볼 클릭 이벤트

    public void OnSymbolClick(Transform destoryTarget)
    {
        // ※ 도면약어 하이라이트 심볼은 NA.Button OnClick 함수에서 삭제한다.

        if (copyHighLightList.Contains(destoryTarget.gameObject))
        {
            Transform parentDestoryTarget = destoryTarget.transform.parent;

            int templateNo = parentDestoryTarget.GetComponentInChildren<NA.Control.Control>(true).info.templateNo;

            // 팝업이 없는지 확인
            if (templateNo == 0)
                SymbolDestroy(destoryTarget);
            else
            {
                // MA Station = No : 999
                // 그외 나머지는 = No : 302 ~ ...
                this.destoryTarget = destoryTarget;
            }
        }

        //Alarm 이면 핫스팟 제거 후 다음절차로
        else if (destoryTarget.parent.GetComponent<NA.Control.Button>())
        {
            Transform parentDestoryTarget = destoryTarget.transform.parent;
            string buttonTitle = parentDestoryTarget.GetComponent<NA.Control.Button>().buttonTitle;
            if (buttonTitle.Equals("Alarm"))
            {
                Destroy(destoryTarget.gameObject);

                SeqManager.instance.IsOn_true();
                InitializeState();
            }
        }
    }

    #endregion

    #region 하이라이트 심볼 삭제 이벤트

    public void SymbolDestroy(Transform destoryTarget)
    {
        copyHighLightList.Remove(destoryTarget.gameObject);
        Destroy(destoryTarget.gameObject);

        if (InputDeviceController._list_tag.Contains(destoryTarget.name))
            InputDeviceController._list_tag.Remove(destoryTarget.name);

        // 열려있는 도면에서 하이라이트 심볼을 전부 수행했을때
        if (copyHighLightList.Count == 0)
        {
            string initial = InputDeviceController._list_dic[currentPage.name.Split('(')[0].Trim()];
            InputDeviceController._list_initials.Remove(initial);
        }

        if (InputDeviceController._list_tag.Count == 0)
        {
            SeqManager.instance.IsOn_true();
            InitializeState();
        }
    }

    #endregion

    #region 다음 시퀀스 넘어가기 직전 호출할 핫스팟 초기화 함수

    public void InitializeState()
    {
        if (copyHighLightList.Count > 0)
        {
            for (int i = 0; i < copyHighLightList.Count; i++)
            {
                Destroy(copyHighLightList[i].gameObject);
            }
        }

        if (priButtonList.Count > 0)
        {
            for (int i = 0; i < priButtonList.Count; i++)
            {
                Destroy(priButtonList[i].gameObject);
            }
        }

        if (navigator)
        {
            foreach (var symbolObj in navigator.topPanel.GetComponentsInChildren<Symbol>())
            {
                Destroy(symbolObj.gameObject);
            }
        }
        copyHighLightList = new List<GameObject>();
        isMainpage = false;
        isFind = false;
        isAlarm = false;
    }

    #endregion

    #region 현재 도면을 제외한 나머지 도면 삭제

    public void DestroyPage()
    {
        // 아웃라인 돌려주기
        navigator.outlinePanel.SetParent(navigator.controlPanel);
        navigator.outlinePanel.localPosition = new Vector3(0f, 0f, -3f);
        navigator.outlinePanel.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        navigator.outlinePanel.GetComponent<RectTransform>().anchorMax = Vector2.zero;

        for (int i = 0; i < navigator.screenPanel.childCount; i++)
        {
            if (navigator.screenPanel.GetChild(i) != currentPage)
                Destroy(navigator.screenPanel.GetChild(i).gameObject);
        }
    }

    #endregion

}
