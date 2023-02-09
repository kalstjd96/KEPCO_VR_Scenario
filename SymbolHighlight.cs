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

    bool isFind; //������ ã�Ҵ� �� Ȯ��
    bool isMainpage; //���� ���������� Ȯ��
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

    #region ������ ���� SET �Լ�
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

        // ������ �����ʴ� PC�� ���̶���Ʈ ���� �ȵǰ�
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

            string[] initialDepth = initialsArray[t].Split('��');

            //0. ���� Tag�� �˶��� ��
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

            //1. ���� �������� �������� �ƴ��� Ȯ��
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
                        // item�� ��ư��ũ��Ʈ�� �پ��ִ� ����������Ʈ�� ���� �θ� �ֽ��� ���ش�.
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

            //2. ���� �������� �����ε� �� ���������� ����� ����
            if (!isFind && isMainpage)
            {
                //(1) �ٸ� ���� ������â���� �ȳ�
                foreach (var panelBtn in pageNavigator.topPanel.GetComponentsInChildren<NA.Control.Button>())
                {
                    if (panelBtn.buttonTitle.Equals(mainGuideBtn))
                    {
                        // panelBtn�� ��ư��ũ��Ʈ�� �پ��ִ� ����������Ʈ�� ���� �θ� �ֽ��� ���ش�.
                        TargetHighLight(panelBtn.transform, false);
                        isFind = true;
                        break;
                    }
                }
            }

            //3. J168ȭ���϶� 
            if (!isAlarm && !isMainpage)
            {
                Page currentShape = currentPage.GetComponent<Page>();

                //(1) �츮�� ���ϴ� J168 ������ ã�Ҵ�
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

                    //(2) �θ� ������ ���� ��
                    else
                    {
                        if (initialDepth.Length > 1)
                        {
                            foreach (var item in currentPage.GetComponentsInChildren<NA.Button>())
                            {
                                if (item.buttonTitle.Equals(initialDepth[1].Trim()))
                                {
                                    // item�� ��ư��ũ��Ʈ�� �پ��ִ� ����������Ʈ�� ���� �θ� �ֽ��� ���ش�.
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
                                    // item�� ��ư��ũ��Ʈ�� �پ��ִ� ����������Ʈ�� ���� �θ� �ֽ��� ���ش�.
                                    TargetHighLight(item.transform.parent, false);
                                    isDepth = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                //(3) �츮�� ���ϴ� ������ �ƴϴ�!
                if (!isFind && !isDepth)
                {
                    foreach (var item in pageNavigator.defaultPage.GetComponentsInChildren<NA.Button>(true))
                    {
                        if (item.buttonTitle.Equals(initialDepth[0].Trim()))
                        {
                            // item�� ��ư��ũ��Ʈ�� �پ��ִ� ����������Ʈ�� ���� �θ� �ֽ��� ���ش�.
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
                            // panelBtn�� ��ư��ũ��Ʈ�� �پ��ִ� ����������Ʈ�� ���� �θ� �ֽ��� ���ش�.
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
                Debug.Log("��ã���� : " + currentPage.name);
        }
    }

    #endregion

    #region ���콺 ���� �Լ�

    public void OnMouse(Transform currentTarget, bool mouseOver)
    {
        // 1. ���콺�� ��Ҵٸ�
        if (mouseOver)
        {
            currentTarget.GetComponentInChildren<Image>().color = Color.blue;
            currentTarget.GetComponent<Symbol>().isHover = true;
        }
        else // (1) ���� �ʾҴٸ�
        {
            currentTarget.GetComponentInChildren<Image>().color = Color.blue;
            currentTarget.GetComponent<Symbol>().isHover = false;
        }
    }

    #endregion

    #region ���̶���Ʈ ����

    public void TargetHighLight(Transform target, bool addList = true, string targetName = null)
    {
        if (!target.GetComponentInChildren<Symbol>())
        {
            //1. ���̶���Ʈ ī��
            GameObject copysymbolHighlight = Instantiate(symbolHighlight.gameObject);

            if (string.IsNullOrEmpty(targetName))
                copysymbolHighlight.transform.name = target.name;
            else
                copysymbolHighlight.transform.name = targetName;

            //2. ����Ʈ ī�� ��� -> �ʱ�ȭ�� ����
            if (addList)
                copyHighLightList.Add(copysymbolHighlight);
            else
            {
                priButtonList.Add(copysymbolHighlight);
                copysymbolHighlight.GetComponent<Symbol>().enabled = false;
            }

            if (copysymbolHighlight.GetComponent<Canvas>().enabled == false)
                copysymbolHighlight.GetComponent<Canvas>().enabled = true;

            //3. ���̶���Ʈ UI �ش� �±׷� �̵���Ű��
            copysymbolHighlight.transform.SetParent(target);
            copysymbolHighlight.transform.localPosition = Vector3.zero;
            copysymbolHighlight.transform.localEulerAngles = Vector3.zero;

            //3. ���̶���Ʈ UI ������ ���߱�
            RectTransform highlgihtRect = copysymbolHighlight.GetComponent<RectTransform>();

            //----------stretch�� ����------------
            highlgihtRect.anchorMin = Vector2.zero;
            highlgihtRect.anchorMax = Vector2.one;
            //------------------------------------
            highlgihtRect.offsetMin = new Vector2(-5f, -5f);  // x : Left  , y : Bottom
            highlgihtRect.offsetMax = new Vector2(5, 5);  // x : Right , y : Top (�����ʰ� ������ ��ȣ�� �ݴ�� ���������)
            //highlgihtRect.offsetMax = new Vector2(5, 55);  // x : Right , y : Top (�����ʰ� ������ ��ȣ�� �ݴ�� ���������)

            highlgihtRect.localScale = Vector3.one;

            //copysymbolHighlight.GetComponent<RectTransform>().sizeDelta =
            //new Vector3(target.GetComponent<RectTransform>().rect.width + 50f, target.GetComponent<RectTransform>().rect.height + 50f, -0.1f);
            //copysymbolHighlight.GetComponent<RectTransform>().localScale = Vector3.one;

            //4. �ڽ��ݶ��̴� ���� �� �ڽ��ݶ��̴� ������ ���߱�
            if (!copysymbolHighlight.GetComponent<BoxCollider>())
                copysymbolHighlight.AddComponent<BoxCollider>();

            copysymbolHighlight.GetComponent<BoxCollider>().size = new Vector3(target.GetComponent<RectTransform>().rect.width,
                target.GetComponent<RectTransform>().rect.height, 5f);

            //5. ���̶���Ʈ ���� ����
            //copysymbolHighlight.GetComponentInChildren<Image>().color = Color.white;
            copysymbolHighlight.GetComponentInChildren<Image>().color = Color.blue;

            //6. ���̶���Ʈ ������Ʈ ���ֱ�
            copysymbolHighlight.SetActive(true);
        }
    }

    #endregion

    #region ���̶���Ʈ �ɺ� Ŭ�� �̺�Ʈ

    public void OnSymbolClick(Transform destoryTarget)
    {
        // �� ������ ���̶���Ʈ �ɺ��� NA.Button OnClick �Լ����� �����Ѵ�.

        if (copyHighLightList.Contains(destoryTarget.gameObject))
        {
            Transform parentDestoryTarget = destoryTarget.transform.parent;

            int templateNo = parentDestoryTarget.GetComponentInChildren<NA.Control.Control>(true).info.templateNo;

            // �˾��� ������ Ȯ��
            if (templateNo == 0)
                SymbolDestroy(destoryTarget);
            else
            {
                // MA Station = No : 999
                // �׿� �������� = No : 302 ~ ...
                this.destoryTarget = destoryTarget;
            }
        }

        //Alarm �̸� �ֽ��� ���� �� ����������
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

    #region ���̶���Ʈ �ɺ� ���� �̺�Ʈ

    public void SymbolDestroy(Transform destoryTarget)
    {
        copyHighLightList.Remove(destoryTarget.gameObject);
        Destroy(destoryTarget.gameObject);

        if (InputDeviceController._list_tag.Contains(destoryTarget.name))
            InputDeviceController._list_tag.Remove(destoryTarget.name);

        // �����ִ� ���鿡�� ���̶���Ʈ �ɺ��� ���� ����������
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

    #region ���� ������ �Ѿ�� ���� ȣ���� �ֽ��� �ʱ�ȭ �Լ�

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

    #region ���� ������ ������ ������ ���� ����

    public void DestroyPage()
    {
        // �ƿ����� �����ֱ�
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
