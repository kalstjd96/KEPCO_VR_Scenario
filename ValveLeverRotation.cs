using System.Collections;
using UnityEngine;
using Valve.VR;

public class ValveLeverRotation : MonoBehaviour
{
    Function callback;

    [SerializeField] GameObject leverhHand;
    [SerializeField] int count = 3;
    [SerializeField] AudioClip clip;
    public Transform hand;
    Transform msadv;

    bool isOn;
    bool isCatch;
    bool isCount;
    public bool isActive { get; set; }

    public void Init()
    {
        time = 0f;
        isActive = false;
        isCatch = false;
        isCount = false;
        isOn = false;
        callback = null;

        if (msadv)
            msadv.localEulerAngles = Vector3.zero;

        if (hand)
            hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
        leverhHand.SetActive(false);
    }

  
    float sensitivity;
    float dot = 0f;
    Vector3 vec3;
    float time;

    void Update()
    {
        if (!isActive) return;
   
        if (isOn && SteamVR_Actions.default_InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            msadv = transform.parent.parent;
            vec3 = Vector3.zero;
            leverhHand.SetActive(true);
            hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(false);
            isCatch = true;
        }

        if (isCatch)
        {
            if(vec3 == Vector3.zero)
                vec3 = hand.transform.position;

            time += Time.deltaTime;

            if (time > 0.5f)
            {
                if (Angle(msadv.localEulerAngles.y) < -50) //위로 올릴 때
                {
                    Vector3 vec3 = msadv.localEulerAngles;
                    vec3.y = -50f;
                    msadv.localEulerAngles = vec3;

                    if (!isCount)
                    {
                        isCount = true;
                        count--;

                        Debug.Log("남은 횟수 :" + count);

                        //완료되면 소리 나게 하기 위함
                        //AudioManager.instance.PlayEffAudio(clip);

                        if (count == 0)
                        {
                            isActive = false;
                            SeqManager.instance.IsOn_true();
                            //StartCoroutine(Retrun());
                        }
                    }
                }
                else if (Angle(msadv.localEulerAngles.y) > 60f)
                {
                    Vector3 vec3 = msadv.localEulerAngles;
                    vec3.y = 60f;
                    msadv.localEulerAngles = vec3;

                    if (isCount)   
                        isCount = false;
                }

                if (Angle(msadv.localEulerAngles.y) < 70 && Angle(msadv.localEulerAngles.y) > -60)
                {

                    if (vec3 != Vector3.zero && (Mathf.Abs(vec3.y - hand.transform.position.y) > 0.002f))
                    {
                        if (hand.transform.position.y > vec3.y)
                        {
                            msadv.localEulerAngles = new Vector3(0f, msadv.localEulerAngles.y - (Mathf.Abs(vec3.y - hand.transform.position.y) * 200f), 0f);
                            vec3 = hand.transform.position;
                        }
                        else if (hand.transform.position.y < vec3.y)
                        {
                            msadv.localEulerAngles = new Vector3(0f, msadv.localEulerAngles.y + (Mathf.Abs(vec3.y - hand.transform.position.y) * 200f), 0f);
                            vec3 = hand.transform.position;
                        }
                    }
                }
            }

            //펌프 상하 시 민감도 설정하기 위한 코드
            //if (Mathf.Abs(Angle(msadv.localEulerAngles.y)) <= 20)
            //    sensitivity = 60f;
            //else if (Mathf.Abs(Angle(msadv.localEulerAngles.y)) > 20)
            //    sensitivity = 300f;

        }



        if (SteamVR_Actions.default_InteractUI.GetStateUp(SteamVR_Input_Sources.RightHand))
        {
            time = 0f;
            vec3 = Vector3.zero;
            dot = 0f;
            isCatch = false;
            leverhHand.SetActive(false);
            hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
            if (isActive)
                isActive = false;
        }
    }

    IEnumerator Retrun()
    {
        yield return new WaitForSeconds(2f);

        hand.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.SetActive(true);
        if (leverhHand.activeSelf)
            leverhHand.SetActive(false);
        SeqManager.instance.IsOn_true();
    }

    public float Angle(float angle)
    {
        if (angle < -180F)
            angle += 360F;
        if (angle > 180F)
            angle -= 360F;
        return angle;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("finger"))
        {
            isActive = true;
            isOn = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("finger"))
        {
            isOn = false;
        }
    }
}
