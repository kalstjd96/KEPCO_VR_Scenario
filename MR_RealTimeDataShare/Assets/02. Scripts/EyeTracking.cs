using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using System;

public class EyeTracking : MonoBehaviour
{
    public Image aim;
    public GameObject aimLookingName;
    public GameObject graphTemaimLookingInfo;
    public GameObject graphPreaimLookingInfo;
    public GameObject circieGraphName;
    public GameObject tagInfo;
    
    public GameObject AimLookedDataInfo;

    public GameObject temGraph;
    public GameObject preGraph;

    public GameObject aimLookinggraph;
    public GameObject aimCircleTemgraph;
    public GameObject aimCirclePregraph;

    public GameObject reconnecrBtn;
    public string dbtargetName;
    public string targetName;
    string orginaltargetName;

    public GameObject[] targetList;
    public GameObject aimPos;
    GameObject orginalPointObject;

    GameObject pointObject;
    GameObject orginalPointObjects;
    public GameObject tagTextInfo;
    public GameObject tagName;
    public bool eyeTrackingStart;
    public Dictionary<string, int> databaseTableName = new Dictionary<string, int>();

    public GameObject reconnectBtn;
    //private void Update()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay();
    //    RaycastHit hitInfo;
    //    if (Physics.Raycast(ray, out hitInfo))
    //    {
    //        Debug.Log(hitInfo.collider.name);
    //    }
    //}


    private void Awake()
    {
        eyeTrackingStart = false;
    }

    private void Start()
    {
        pointObject = null;
    }

    private void Update()
    {
        if (reconnecrBtn.activeSelf == false)
        {
            bool isTarget = false;
            for (int i = 0; i < targetList.Length; i++)
            {
                Vector3 target3DPos = targetList[i].transform.position;
                Vector3 aim3Dpos = aimPos.transform.position;

                Vector3 targetPos = Camera.main.WorldToScreenPoint(targetList[i].transform.position);
                Vector3 aimCheckPos = Camera.main.WorldToScreenPoint(aimPos.transform.position);
                
                if ((targetPos - aimCheckPos).magnitude < 30f && Vector3.Distance(target3DPos , aim3Dpos) <5f)
                {
                    pointObject = targetList[i].transform.GetChild(0).gameObject;
                    pointObject.GetComponent<Image>().color = Color.blue;
                    //에임 색깔 변경 
                    
                    //에임이 커짐
                    aim.transform.localScale = Vector3.Slerp(aim.transform.localScale, Vector3.one * 1.2f, 0.2f);
                    
                    //targetName = EyeTrackingTarget.LookedAtTarget.ToString().Split('(')[0].Trim();
                    //aimLookingInfoName.GetComponent<TMP_Text>().text = targetName;
                    aimLookingName.GetComponent<Text>().text = targetName;
                    circieGraphName.GetComponent<TMP_Text>().text = targetName;

                    isTarget = true;

                    targetName = targetList[i].name;
                    orginaltargetName = targetName;
                    if (orginaltargetName != null)
                    {
                        for (int j = 0; j < targetList.Length; j++)
                        {
                            if (targetList[j].name != targetName)
                            {
                                targetList[j].transform.GetChild(0).GetComponent<Image>().color = Color.green;
                            }
                        }
                    }
                }
            }

            if (!isTarget)
            {
                LookingAway();
            }

            if (targetName != String.Empty)
            {
                //태그들의 정보가 전부 채워졌다면 
                //if (tagInfo.activeSelf)
                //{
                string tableName = null;
                //if (DBCommunication.instance.GetLength() > 0) // Datas의 값이 존재할 때로
                //{
                    if (targetName == "보조급수펌프_진동")
                    {
                        tableName = "hs_7471a";
                    }
                    else if (targetName == "보조급수_펌프_베어링_온도")
                    {
                        tableName = "lw_4451t";
                    }
                    else if (targetName == "보조급수_펌프_후단_유량")
                    {
                        tableName = "tag_5111k";
                    }
                    else if (targetName == "보조급수_펌프_후단_유량2")
                    {
                        tableName = "tx_9933p";
                    }
                    else if (targetName == "보조급수_펌프_후단_압력")
                    {
                        tableName = "yf_5503e";
                    }
                    else if (targetName == "계기별_운전변수")
                    {
                        tableName = "zk_5014h";
                    }

                if (!reconnectBtn.activeSelf)
                {

                    //에임에 잡힌 태그 정보 실시간 출력
                    DBCommunication.Datas datas = DBCommunication.instance.GetLastData(DB_Manager.instance.databaseTableName[tableName]);
                    //DBCommunication.Datas datas = DBCommunication.instance.GetLastData(DB_Manager.instance.databaseTableName[targetName]);


                        string tem = datas.data1;
                        string press = datas.data2;
                       
                        //그래프 옆 텍스트 문자
                        string text1 = "<color=yellow>온도</color> : ";
                        string text2 = " , <color=yellow>압력</color> : ";
                        Transform information = tagTextInfo.transform.Find("AimLookingInfo");
                        information.GetComponent<TMP_Text>().text = text1 + tem + text2 + press;
                        tagTextInfo.transform.Find("Image/Text (TMP)").GetComponent<TMP_Text>().text = targetName;
                        //tagInfo.SetActive(true);

                        //포인트 부분도 콜라이더를 킨다.
                        for (int j = 0; j < tagName.transform.childCount; j++)
                        {
                            tagName.transform.GetChild(j).GetComponent<BoxCollider>().enabled = true;
                        }

                        graphTemaimLookingInfo.GetComponent<TMP_Text>().text = tem;
                        graphPreaimLookingInfo.GetComponent<TMP_Text>().text = press;

                        //그래프 항목 
                        float temValue = float.Parse(tem);
                        RectTransform temGraphRect = temGraph.GetComponent<RectTransform>();
                        
                        float preValue = float.Parse(press);
                        RectTransform preGraphRect = preGraph.GetComponent<RectTransform>();

                        temGraphRect.localScale = Vector3.Slerp(temGraphRect.localScale, new Vector3(1.0f, temValue * 0.015f, 1.0f), 0.2f);
                        preGraphRect.localScale = Vector3.Slerp(preGraphRect.localScale, new Vector3(1.0f, preValue * 0.015f, 1.0f), 0.2f);

                        //원형 그래프
                        aimCircleTemgraph.GetComponent<Image>().fillAmount = Mathf.SmoothStep(aimCircleTemgraph.GetComponent<Image>().fillAmount, temValue * 0.01f, 0.2f);
                        aimCirclePregraph.GetComponent<Image>().fillAmount = Mathf.Lerp(aimCirclePregraph.GetComponent<Image>().fillAmount, preValue * 0.01f, 0.2f);

                }
                    //}
                //}
            }
        }

    }

    public void LookingInfoPrint() //내가 보고 있는 모델에 대한 정보 출력
    {
        // 텍스트 정보 출력창
        AimLookedDataInfo.SetActive(true);

        // 그래프 정보 출력창
        aimLookinggraph.SetActive(true);
        //Debug.Log(EyeTrackingTarget.LookedAtTarget.ToString());
    }

    public void LookingAway() //보고 있는 것이 눈에서 벗어날 때
    {
        //에임 텍스트 이름 되돌림
        aimLookingName.GetComponent<Text>().text = "";

        aim.transform.localScale = Vector3.one;
    }

    public void LookingModelPoint() //내가 보고 있는 모델에 포인트 주기
    {
        Debug.Log("포인트 주기");
    }
}
