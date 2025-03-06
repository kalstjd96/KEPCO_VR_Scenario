<img src="https://capsule-render.vercel.app/api?type=waving&color=auto&height=200&section=header&text=VR-SafetyEducation&fontSize=80" /> 

# 🚧 신고리 5·6호기 VR 안전 교육 시스템

> **VR 환경에서 실감형 안전 교육을 제공하는 프로젝트**  
> Unity 기반의 가상 현실(VR) 시뮬레이션으로 멀티플레이 및 인터랙션 기능을 구현

---

## 📌 프로젝트 개요
- **개발 환경**: `Unity 2021.3 LTS`, `C#`, `Photon`
- **주요 기능**
  - 🎮 **멀티플레이어 적용** (`Photon`)  
  - 🎤 **음성 채팅 기능** (`Photon Voice`)  
  - 📏 **대상 거리 측정 및 안내 시스템**  
  - 🔊 **3D 오디오 시스템 적용**  

---

## 📂 프로젝트 구조
📦 VR-SafetyEducation <br>
├── 📁 Assets │ <br>
  &nbsp;&nbsp;&nbsp;├── 📁 Scripts # 주요 기능을 담당하는 C# 스크립트  <br>
  &nbsp;&nbsp;&nbsp;├── 📁 Prefabs # 사전 제작된 VR 오브젝트  <br>
  &nbsp;&nbsp;&nbsp;├── 📁 Scenes # Unity 씬 파일 <br>
  &nbsp;&nbsp;&nbsp;├── 📁 Audio # 음성 및 효과음 리소스 <br>
  &nbsp;&nbsp;&nbsp;├── 📁 Models # 3D 모델 데이터 <br>
  &nbsp;&nbsp;&nbsp;├── 📁 Textures # 텍스처 및 UI 리소스  <br>
  &nbsp;&nbsp;&nbsp;├── 📄 README.md # 프로젝트 개요 문서 <br>

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

➡ **📂 전체 코드 보기**: [PlayerObjDistance.cs](https://github.com/kalstjd96/KEPCO_VR_Scenario/blob/main/Scripts/PlayerObjDistance.cs#L20-L44)

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

➡ **📂 전체 코드 보기**: [AudioManager.cs](https://github.com/kalstjd96/KEPCO_VR_Scenario/blob/main/Scripts/Manager/AudioManager.cs)

