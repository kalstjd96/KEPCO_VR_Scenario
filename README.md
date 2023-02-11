<img src="https://capsule-render.vercel.app/api?type=waving&color=auto&height=200&section=header&text=VR-SafetyEducation&fontSize=80" /> 

# 신고리 5,6호기 VR 안전 교육 시스템

## Features (담당 기능)

-   [멀티 환경 구성](#multiPlayer)
    -  [사용자 간 음성통신](#photon-voice)
    -  RPC를 통한 정보공유
    -  Photon Local Server Setting
-   [인터렉션 기능 개발](#interaction)
    -  [대상 간 거리 계산](#distance-check)
    -  Player가 행하는 행위
-   [가상 현실 기능 개발](#target-state)
    -  [Sound 조작 기능](#multiPlayer)
    -  Target 위치 가이드(#multiPlayer)
    
## MultiPlayer

Photon을 사용하여 멀티 기능을 구현하였습니다.

### (1) photon Voice

```c#

[PunRPC]
public void PhotonVoiceSet()
{
    isMultiStart = true;

    //1. 레코더가 오브젝트에 담기
    voice = GameObject.Find("Voice").transform;

    recorder = voice.GetComponent<Recorder>().transform;
    //recorder.GetComponent<Recorder>().DebugEchoMode = true;

    for (int i = 0; i < player.Length; i++)
    {
        targetPhone = phone[i]; //0 : 발신자 , 1 : 수신자

        //4. Player에게 있는 PhotonVoiceView에 레코더 설정, 스피커 연결하고 설정을 해준다.
        if (i == 0)
        {
            player[iresult].GetComponent<PhotonVoiceView>().RecorderInUse = recorder.GetComponent<Recorder>();
            Transform speakerObj = player[i].GetComponentInChildren<Speaker>(true).transform;
            oriMCRSpeakerParent = player[i];

            speakerObj.GetComponent<Speaker>().enabled = true;
            speakerObj.SetParent(phone[1]);
        }
        else if (i == 1)
        {
            Transform speakerObj = player[i].GetComponentInChildren<Speaker>(true).transform;
            oriLocalSpeakerParent = player[i];

            speakerObj.GetComponent<Speaker>().enabled = true;
            speakerObj.SetParent(phone[0]);
        }

        if (!targetPhone.GetComponent<AudioSource>())
            targetPhone.gameObject.AddComponent<AudioSource>();

        AudioSource targetAudio = targetPhone.gameObject.GetComponent<AudioSource>();
        targetAudio.rolloffMode = AudioRolloffMode.Linear; //거리에 따라 목소리의 크기가 달라진다.
        targetAudio.minDistance = 1f;
        targetAudio.maxDistance = 500f;

        targetPhone.GetComponent<AudioSource>().playOnAwake = true;
        targetPhone.GetComponent<AudioSource>().loop = true;
    }
}
```
<img src="https://user-images.githubusercontent.com/47016363/218071476-ad655fce-a0c6-4e88-986e-1f27d65ea778.png"  width="800" height="250"/>

## Interaction

시나리오를 진행하는데 있어 필요한 기능을 개발하였습니다.
ex. 대상 간의 거리 계산, Target 방향 가이드, 장비 

### (1) Distance Check

```c#

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjDistance : MonoBehaviour
{
    float distance; //거리
    Vector3 direction; // 방향
    public const float maxDistance = 7f; 

    private RaycastHit hit;
    private float maxRaycast = 300f;

    /// <summary>
    /// Player와 Target 간의 거리 비교 후 대상 위치로 텔레포트
    /// </summary>
    /// <param name="target">해당 절차에 처리해야할 대상</param>
    /// <param name="targetVector">처리해야할 대상의 RendererCenter</param>
    /// <returns>텔레포트 해야할 지에 대한 여부 </returns>
    public bool DistanceComparison(Transform target, Vector3 targetVector)
    {
        //Transform playerCamera = transform.Find("SteamVRObjects/VRCamera"); ;
        Transform playerCamera = transform;
        Vector3 reSetting = playerCamera.position;
        distance = Vector3.Distance(playerCamera.position, targetVector);

        if (distance >= maxDistance)
            return true;
        else
        {
            if ((playerCamera.position - target.position).y <= 0f)
                direction = playerCamera.up;
            else
                direction = -playerCamera.up; 

            reSetting.y += 0.1f;
            if (Physics.Raycast(reSetting, direction, out hit, maxRaycast, 1 << LayerMask.NameToLayer("Floor")))
            {
                if (hit.distance < Mathf.Abs((playerCamera.position - target.position).y))
                    return true;
            }
        }
        return false;
    }

    //감지된 Floor의 월드좌표y를 Player 좌표로 대입, 키에 따른 높이 상이함을 보정
    public float playerHeight;
    public float floorHeight;
    public Transform currentFloor;
    public void SetPlayerHeight()
    {
        Transform floor = currentFloor = MinimapManager.instance.FloorCheck();

        floorHeight = floor.position.y;

        Vector3 playerPos = transform.position;
        playerPos.y = floorHeight;
        transform.position = playerPos;

        playerHeight = transform.position.y;
    }
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

