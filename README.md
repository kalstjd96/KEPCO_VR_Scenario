<img src="https://capsule-render.vercel.app/api?type=waving&color=auto&height=200&section=header&text=VR-SafetyEducation&fontSize=80" /> 

# 신고리 5,6호기 VR 안전 교육 시스템

## Features (담당 기능)

-   [멀티 환경 구성](#multiPlayer)
    -  [사용자 간 음성통신](#multiPlayer)
    -  [RPC를 통한 정보공유](#multiPlayer)
    -  [Photon Local Server Setting](#multiPlayer)
-   [Panel 리스트 뷰어 기능](#panel-list-viewer)
-   [절차별 Target에 대한 상태](#target-state)
-   [그래프로 표현된 Data 정보 열람 기능](#graph-data-viewer)
-   [HoloLens2 Motion 적용(MRTK)](#mrtk)
    
## Panel Procedure DB

>사용된 스크립트<br/>
> DB_Manager.cs

Control Panel 진행에 대한 절차를 SQLite 기반으로 DB 구성하였습니다.

## Panel List Viewer

>사용된 스크립트<br/>
> ProcedureList.cs

진행해야할 절차를 리스트 형태로 확인하는 기능을 구현하였습니다.

```c#

// List 생성
public void Create()
{
    buttonList = new List<GameObject>();

    for (int i = 0; i < procedureRowDic.Count; i++)
    {
        int rowIndex = procedureRowDic.ElementAt(i).Key;
        if (!ProjectManager.instance.GetData(rowIndex, "SubList").Equals("TRUE"))
        {
            GameObject listObj = Instantiate(listPrefab, listParent.transform);
            listObj.GetComponent<Interactable>().OnClick.AddListener(delegate
            {
                SkipTo(rowIndex);
            });

            //리스트 속성 설정
            listObj.transform.Find("Number").GetComponent<TMP_Text>().text = (i + 1).ToString("000") + ". ";
            listObj.transform.Find("Local").GetComponent<TMP_Text>().text = "[ " + ProjectManager.instance.GetData(rowIndex, "Location") + " ]";
            listObj.transform.Find("Main_Text").GetComponent<TMP_Text>().text = procedureRowDic.ElementAt(i).Value;

            buttonList.Add(listObj);
        }
    }
    listParent.GetComponent<GridObjectCollection>().UpdateCollection(); //Grid 정렬 갱신
}
```
<img src="https://user-images.githubusercontent.com/47016363/217998078-331fba74-9df0-4c51-ac18-9ff4d9780b5e.png"  width="400" height="250"/>

## Target State

>사용된 스크립트<br/>
> Highlight.cs , GuideMessage.cs 

해당 절차에 대한 Target 상태를 표현하였습니다. ex. 애니메이션, 가이드 등 

```c#

//하이라이트 색상으로 깜빡임 
while (true)
{
    for (int i = 0; i < targets.Length; i++)
    {
        Material targeMat = highlightMaterial;
        Color targetColor = targeMat.color;
        //Color targetColor = new Color(0f, 30f, 255f, 255f) * 10f : Color.white * 200f;
        targetColor.a = 1.5f;

        for (int j = 0; j < targets[i].materials.Length; j++)
        {
            // Target의 RenderMode 변경 
            ChangeRenderMode(targets[i].materials[j], BlendMode.Transparent);
            targets[i].materials[j] = targeMat;
            targets[i].materials[j].SetColor("_Color",
                Color.Lerp(targeMat.color, targetColor, Mathf.PingPong(Time.time, 1.5f)));
        }

    }
    yield return null;
}


```
<img src="https://user-images.githubusercontent.com/47016363/217998187-0a5727b9-833d-4189-af01-abc630d038c0.png"  width="400" height="250"/>

## Graph Data Viewer

DB에 저장된 데이터를 기반으로 그래프 뷰어를 구현하였습니다. (오픈소스 )

<img src="https://user-images.githubusercontent.com/47016363/217997541-07d916e2-a315-4baa-97a3-63c46751ec48.png"  width="400" height="250"/>

## MRTK

MRTK를 통해 가상 물체의 크기, 위치, 회전 등 조작 기능을 적용하였습니다.

<img src="https://user-images.githubusercontent.com/47016363/217986555-00894438-ebaa-4e50-9ef7-49df1b70e041.png"  width="400" height="250"/>
<img src="https://user-images.githubusercontent.com/47016363/217989203-7a7d481d-4426-46e0-8399-3153e20877ce.png"  width="400" height="250"/>

