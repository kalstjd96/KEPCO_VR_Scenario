<img src="https://capsule-render.vercel.app/api?type=waving&color=auto&height=200&section=header&text=VR-SafetyEducation&fontSize=80" /> 

# 신고리 5,6호기 VR 안전 교육 시스템

## Features (담당 기능)

-   [멀티 환경 구성](#multiPlayer)
    -  [사용자 간 음성통신](#photon-voice)
    -  RPC를 통한 정보공유
    -  Photon Local Server Setting
-   [인터렉션, 가상 현실 기능 개발](#interaction)
    -  [대상 간 거리 계산](#distance-check)
    -  Player가 행하는 행위
    -  [Sound 조작 기능](#audio-manager)
    -  Target 위치 가이드
    
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
            player[i].GetComponent<PhotonVoiceView>().RecorderInUse = recorder.GetComponent<Recorder>();
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
<img src="https://user-images.githubusercontent.com/47016363/218258384-517bd295-da59-4b3f-81a1-111d8e61b443.png"  width="800" height="250"/>

### (2) Audio Manager

```c#

public void PlayMultiAudio(AudioClip selectedClip, float volume = 1.0f)
{
    if (audioSourceList.Count < 3)
    {
        GameObject newSoundOBJ = new GameObject();
        newSoundOBJ.transform.parent = this.transform;
        newSoundOBJ.transform.localPosition = Vector3.zero;
        AudioSource audioSrc = newSoundOBJ.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        newSoundOBJ.name = "Sound EffObj";

        audioSourceList.Add(audioSrc);
    }

    if (selectedClip != null && audioSourceList[multiCount])
    {
        audioSourceList[multiCount].clip = selectedClip;
        audioSourceList[multiCount].volume = volume * volumeSize;
        audioSourceList[multiCount].Play(0);

        multiCount++;
        if (multiCount >= 3)
            multiCount = 0;
    }
}


```

