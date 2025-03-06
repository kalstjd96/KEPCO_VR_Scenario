<img src="https://capsule-render.vercel.app/api?type=waving&color=auto&height=200&section=header&text=VR-SafetyEducation&fontSize=80" /> 

# 🚧 신고리 5·6호기 VR 안전 교육 시스템

> **VR 환경에서 실감형 안전 교육을 제공하는 프로젝트**  
> Unity 기반의 가상 현실(VR) 시뮬레이션으로 멀티플레이 및 인터랙션 기능을 구현

---

## 📌 프로젝트 개요
- **개발 환경**: `Unity 2021.3 LTS`, `C#`, `Photon`
- **주요 기능**
  - 🎮 **멀티플레이어 지원** (`Photon`)  
  - 🎤 **음성 채팅 기능** (`Photon Voice`)  
  - 📏 **대상 거리 측정 및 안내 시스템**  
  - 🔊 **3D 오디오 시스템 적용**  

---

## 📂 프로젝트 구조
📦 VR-SafetyEducation 
├── 📁 Assets │ 
├── 📁 Scripts # 주요 기능을 담당하는 C# 스크립트 │ 
├── 📁 Prefabs # 사전 제작된 VR 오브젝트 │ 
├── 📁 Scenes # Unity 씬 파일 │ 
├── 📁 Audio # 음성 및 효과음 리소스 │ 
├── 📁 Models # 3D 모델 데이터 │ 
├── 📁 Textures # 텍스처 및 UI 리소스 │ 
├── 📄 README.md # 프로젝트 개요 문서 
└── 📄 .gitignore


---

## 🎮 주요 기능 및 코드 예제

### 🟢 (1) 멀티플레이어 환경 <a id="multiPlayer"></a>

Photon을 활용하여 네트워크 기반 멀티플레이어 환경을 구축했습니다.

#### 🔊 (1-1) 사용자 간 음성 통신 (`Photon Voice`)  
> **특징**: RPC를 사용하여 실시간 음성 통신을 구현  

```c#
[PunRPC]
public void PhotonVoiceSet()
{
    isMultiStart = true;
    recorder = GameObject.Find("Voice").GetComponent<Recorder>().transform;
}
```

➡ **📂 전체 코드 보기**: [MultiPagingPhonManager.cs (L333-L378)](https://github.com/kalstjd96/KEPCO_VR_Scenario/blob/main/Scripts/Voice/MultiPagingPhonManager.cs#L333-L378)


<img src="https://user-images.githubusercontent.com/47016363/218071476-ad655fce-a0c6-4e88-986e-1f27d65ea778.png" width="800"/>  


### 🎯 (2) 인터랙션 시스템 <a id="interaction"></a>
VR 환경에서의 다양한 인터랙션 기능을 개발하였습니다.

#### 📏 (2-1) 대상 간 거리 측정 시스템
특징: 대상과의 거리를 체크하여 적절한 위치로 이동

```c#
public bool DistanceComparison(Transform target, Vector3 targetVector)
{
    float distance = Vector3.Distance(transform.position, targetVector);
    return distance >= maxDistance;
}
```

➡ 📂 전체 코드 보기: DistanceCheck.cs  

<img src="https://user-images.githubusercontent.com/47016363/218258384-517bd295-da59-4b3f-81a1-111d8e61b443.png" width="800"/>


### 🔊 (3) 3D 오디오 시스템 <a id="audio-manager"></a>
게임 내 오디오를 보다 현실감 있게 관리하기 위한 시스템을 구축했습니다.

```c#
public void PlayMultiAudio(AudioClip selectedClip, float volume = 1.0f)
{
    if (selectedClip == null) return;
    audioSourceList[multiCount].clip = selectedClip;
    audioSourceList[multiCount].volume = volume;
    audioSourceList[multiCount].Play(0);

    multiCount = (multiCount + 1) % 3;
}
```

➡ 📂 전체 코드 보기: AudioManager.cs



📢 추가 개선 사항 (추천)
✅ 데모 영상 추가 → 프로젝트 소개를 영상으로 제공
✅ 스크린샷 추가 → 주요 기능을 이미지로 보여주기
✅ 기술 스택 아이콘 추가 → Unity, Photon, C# 아이콘 사용



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

### (1) Photon Voice

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

