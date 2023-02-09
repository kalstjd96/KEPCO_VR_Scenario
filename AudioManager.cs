using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Route")]
    [SerializeField] string resourcesRoute;

    [Header("BGM")]
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] bool isCreateBGMSource;

    [Header("Effect")]
    [SerializeField] AudioSource effectAudioSource;
    [SerializeField] bool isCreateEffectSource;

    List<AudioSource> audioSourceList;
    bool isMute;

    Dictionary<string, AudioClip> audioClipDictionary;
    Dictionary<string, AudioClip> multiClipDictionary;
    Dictionary<string, AudioClip> bgmClipDictionary;
    int multiCount;

    float volumeSize = 1f;
    public void SetVolumeSize(float volumeSize)
    {
        this.volumeSize = volumeSize;
    }

    void Awake()
    {
        instance = this;
        AudioInit();
    }

    void OnDisable()
    {
        if (instance != null)
            instance = null;
    }

    /// <summary>
    /// 오디오매니저 기본 세팅을 해주는 함수
    /// </summary>
    public void AudioInit()
    {
        if (isCreateEffectSource)
        {
            GameObject effectOBJ = new GameObject();
            effectOBJ.transform.parent = this.transform;
            effectOBJ.transform.localPosition = Vector3.zero;
            effectAudioSource = effectOBJ.AddComponent<AudioSource>();
            effectAudioSource.playOnAwake = false;
            effectOBJ.name = "Sound Effect Object";
        }

        if (isCreateBGMSource)
        {
            GameObject bgmObject = new GameObject();
            bgmObject.transform.parent = this.transform;
            bgmObject.transform.position = Vector3.zero;
            bgmAudioSource = bgmObject.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = false;
            bgmObject.name = "Sound BGM Object";
        }

        audioClipDictionary = new Dictionary<string, AudioClip>();
        multiClipDictionary = new Dictionary<string, AudioClip>();
        bgmClipDictionary = new Dictionary<string, AudioClip>();
        audioSourceList = new List<AudioSource>();
    }

    /// <summary>
    /// 특정 오디오클립을 재생해주는 함수
    /// 동시에 여러 사운드를 재생할 수 있으므로 하나의 사운드만 재생될 경우에는 PlayEffAudio 함수 사용 권장
    /// </summary>
    /// <param name="selectedClip">재생할 오디오클립</param>
    /// <param name="volume">오디오 볼륨</param>
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
    /// <summary>
    /// 특정 오디오클립을 재생해주는 함수
    /// 동시에 여러 사운드를 재생할 수 있으므로 하나의 사운드만 재생될 경우에는 PlayEffAudio 함수 사용 권장
    /// </summary>
    /// <param name="fileName">오디오클립 이름(Resources 폴더에 있을 경우에만 가능)</param>
    /// <param name="volume">오디오 볼륨</param>
    public void PlayMultiAudio(string fileName, float volume = 1.0f, bool isLoop = false)
    {
        AudioClip selectedClip = null;
        if (multiClipDictionary.ContainsKey(fileName) == true)
        {
            selectedClip = multiClipDictionary[fileName] as AudioClip;
        }
        else
        {
            selectedClip = Resources.Load<AudioClip>(resourcesRoute + fileName);
            multiClipDictionary.Add(fileName, selectedClip);
        }

        if (audioSourceList.Count < 3)
        {
            GameObject newSoundOBJ = new GameObject();
            newSoundOBJ.transform.parent = this.transform;
            newSoundOBJ.transform.localPosition = Vector3.zero;
            AudioSource audioSrc = newSoundOBJ.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;

            audioSrc.loop = isLoop;

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

    public void PlayMultiAudio(string rootName ,string fileName, float volume = 1.0f)
    {
        AudioClip selectedClip = null;
        if (multiClipDictionary.ContainsKey(fileName) == true)
        {
            selectedClip = multiClipDictionary[fileName] as AudioClip;
        }
        else
        {
            selectedClip = Resources.Load<AudioClip>(rootName + fileName);
            multiClipDictionary.Add(fileName, selectedClip);
        }

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
            audioSourceList[multiCount].loop= false;

            audioSourceList[multiCount].Play(0);

            multiCount++;
            if (multiCount >= 3)
                multiCount = 0;
        }
    }

    /// <summary>
    /// 특정 오디오클립을 재생해주는 함수
    /// 동시에 하나의 사운드만 재생할 수 있으므로 동시 재생을 원할 경우에는 PlayMultiAudio 함수 사용
    /// </summary>
    /// <param name="selectedClip">재생할 오디오클립</param>
    /// <param name="volume">오디오 볼륨</param>
    public void PlayEffAudio(AudioClip selectedClip, float Volume = 1.0f)
    {
        if (selectedClip != null && effectAudioSource != null)
        {
            effectAudioSource.clip = selectedClip;
            effectAudioSource.volume = Volume * volumeSize;
            effectAudioSource.loop = false;
            effectAudioSource.Play(0);
        }
    }
    /// <summary>
    /// 특정 오디오클립을 재생해주는 함수
    /// 동시에 하나의 사운드만 재생할 수 있으므로 동시 재생을 원할 경우에는 PlayMultiAudio 함수 사용
    /// </summary>
    /// <param name="fileName">오디오클립 이름(Resources 폴더에 있을 경우에만 가능)</param>
    /// <param name="volume">오디오 볼륨</param>
    public void PlayEffAudio(string fileName, float volume = 1.0f)
    {
        AudioClip selectedClip = null;

        if (audioClipDictionary.ContainsKey(fileName))
        {
            selectedClip = audioClipDictionary[fileName];
        }
        else
        {
            selectedClip = Resources.Load<AudioClip>(resourcesRoute + fileName);
            audioClipDictionary.Add(fileName, selectedClip);
        }
        
        if (selectedClip != null && effectAudioSource != null)
        {
            effectAudioSource.clip = selectedClip;
            effectAudioSource.volume = volume * volumeSize;
            effectAudioSource.loop = false;
            effectAudioSource.Play(0);
        }
    }

    /// <summary>
    /// 특정 오디오클립을 재생해주는 함수(배경용)
    /// </summary>
    /// <param name="fileName">오디오클립 이름(Resources 폴더에 있을 경우에만 가능)</param>
    /// <param name="volume">오디오 볼륨</param>
    /// <param name="delay">오디오 재생 딜레이</param>
    public void PlayBGM(AudioClip selectedClip, float volume, float delay = 0)
    {
        if (selectedClip != null && bgmAudioSource != null)
        {
            bgmAudioSource.clip = selectedClip;
            bgmAudioSource.volume = volume * volumeSize;
            bgmAudioSource.loop = true;
            bgmAudioSource.PlayDelayed(delay);
        }
    }
    /// <summary>
    /// 특정 오디오클립을 재생해주는 함수(배경용)
    /// </summary>
    /// <param name="fileName">오디오클립 이름(Resources 폴더에 있을 경우에만 가능)</param>
    /// <param name="volume">오디오 볼륨</param>
    /// <param name="delay">오디오 재생 딜레이</param>
    public void PlayBGM(string fileName, float volume, float delay = 0)
    {
        AudioClip selectedClip = null;
        if (bgmClipDictionary.ContainsKey(fileName))
        {
            selectedClip = bgmClipDictionary[fileName];
        }
        else
        {
            selectedClip = Resources.Load<AudioClip>(resourcesRoute + fileName);
            bgmClipDictionary.Add(fileName, selectedClip);
        }

        if (selectedClip != null && bgmAudioSource != null)
        {
            bgmAudioSource.clip = selectedClip;
            bgmAudioSource.volume = volume * volumeSize;
            bgmAudioSource.loop = true;
            bgmAudioSource.PlayDelayed(delay);
        }
    }

    /// <summary>
    /// 배경 오디오를 정지할 때 사용하는 함수
    /// </summary>
    public void StopBGM()
    {
        if (bgmAudioSource != null)
            if (bgmAudioSource.clip != null)
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = null;
            }
    }

    /// <summary>
    /// 이펙트 오디오를 정지할 때 사용하는 함수(싱글용)
    /// </summary>
    public void StopEffAudio()
    {
        if (effectAudioSource != null)
            effectAudioSource.Stop();
    }

    /// <summary>
    /// 이펙트 오디오를 정지할 때 사용하는 함수(멀티용)
    /// </summary>
    public void StopMultiAudio()
    {
        if (audioSourceList.Count > 0)
            for (int i = 0; i < audioSourceList.Count; i++)
                audioSourceList[i].Stop();
    }

    /// <summary>
    /// 보관 중인 오디오 클립을 모두 초기화 할때 사용하는 함수
    /// </summary>
    public void AudioClear()
    {
        if (audioClipDictionary.Count > 0)
            audioClipDictionary.Clear();

        if (multiClipDictionary.Count > 0)
            multiClipDictionary.Clear();

        if (bgmClipDictionary.Count > 0)
            bgmClipDictionary.Clear();
    }
}
