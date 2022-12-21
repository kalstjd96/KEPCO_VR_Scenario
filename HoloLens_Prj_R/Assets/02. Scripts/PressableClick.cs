using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class PressableClick : MonoBehaviour
{
    public GameObject roverExplorer; //율 모델 
    public GameObject size1;
    public GameObject size2;
    public GameObject size3;
    public GameObject lockObject;
    public GameObject playerPos;

    public GameObject kepcoModel;
    Vector3 scaleChange;
    Vector3 orignalscaleChange;
    Vector3 orignalPosChange;

    public Transform mainModel;
    public GameObject pressableBtn;
    Vector3 targetPos;
    bool isLock;


    // Start is called before the first frame update
    void Start()
    {
        orignalscaleChange = new Vector3();
        orignalscaleChange = roverExplorer.transform.localScale;
        orignalPosChange = new Vector3();
        
        isLock = false;
    }

    public float distance;

    #region 모델 사이즈 조정 및 모델 잠금 On/Off
    public void PressableSizeBtn(float aa)
   {
        orignalPosChange = roverExplorer.transform.position;

        if (aa != 0)
        {
            if (aa == 0.25 || aa == 0.5)
            {
                distance = 5f;
                //플레이어가 바라보는 방향 
                Vector3 dir = playerPos.transform.forward;
                //dir.x = 0;
                dir.y = 0;
                dir.Normalize();

                //모델의 높이 조정
                //Vector3 targetPos = playerPos.transform.position + dir;// * distance;
                if (aa == 0.5)
                {
                    targetPos = playerPos.transform.position + dir + playerPos.transform.right * -1.6f + playerPos.transform.up * 1.1f + (playerPos.transform.forward * -0.2f);    
                }
                else
                {
                    targetPos = playerPos.transform.position + dir + playerPos.transform.right * -0.9f + playerPos.transform.up * 0.3f + playerPos.transform.forward * -0.7f;
                }
                roverExplorer.transform.position = targetPos;

                //플레이어 방향으로 모델도 바라보게 설정
                roverExplorer.transform.localRotation = playerPos.transform.localRotation;
                roverExplorer.transform.localRotation = new Quaternion(0, roverExplorer.transform.localRotation.y, 0, roverExplorer.transform.localRotation.w);

                //roverExplorer.transform.localScale= orignalscaleChange;
                roverExplorer.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                scaleChange = new Vector3(roverExplorer.transform.localScale.x * aa, roverExplorer.transform.localScale.y * aa, roverExplorer.transform.localScale.z * aa);
                roverExplorer.transform.localScale = scaleChange;

            }
            else if(aa == 1)
            {
                //플레이어가 바라보는 방향 
                Vector3 dir = playerPos.transform.forward;
                //dir.x = 0;
                dir.y = 0;
                dir.Normalize();

                //모델의 높이 조정
                Vector3 targetPos = playerPos.transform.position + dir  + playerPos.transform.right *-4f + playerPos.transform.up * 2f;
                roverExplorer.transform.position = targetPos;

                //플레이어 방향으로 모델도 바라보게 설정

                //roverExplorer.transform.rotation = Quaternion.Euler(roverExplorer.transform.localRotation.x, roverExplorer.transform.localRotation.y * -3f, roverExplorer.transform.localRotation.z);
                roverExplorer.transform.localRotation = playerPos.transform.localRotation;
                roverExplorer.transform.localRotation = new Quaternion(0, roverExplorer.transform.localRotation.y, 0, roverExplorer.transform.localRotation.w);
                //roverExplorer.transform.localRotation = new Quaternion(0, roverExplorer.transform.localRotation.y, 0, roverExplorer.transform.localRotation.w);

                //roverExplorer.transform.localScale= orignalscaleChange;
                roverExplorer.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                scaleChange = new Vector3(roverExplorer.transform.localScale.x * aa, roverExplorer.transform.localScale.y * aa, roverExplorer.transform.localScale.z * aa);
                roverExplorer.transform.localScale = scaleChange;

                //playerPos.transform.LookAt(roverExplorer.transform.Find("V1322_ALL/1-542-M-PP01A1"));
            }
            else if(aa == 2)
            {
                if (pressableBtn.transform.GetChild(2).gameObject.activeSelf)
                {
                    pressableBtn.transform.GetChild(2).gameObject.SetActive(false);
                    pressableBtn.transform.GetChild(3).gameObject.SetActive(false);
                }
                else
                {
                    pressableBtn.transform.GetChild(2).gameObject.SetActive(true);
                    pressableBtn.transform.GetChild(3).gameObject.SetActive(true);
                }
            }
           
        }
        else if(aa == 0)//Lock
        {
            if (isLock) //락이 걸려 있다면 콜라이더가 꺼져 있다면
            {
                Debug.Log("락풀림");
                roverExplorer.GetComponent<Collider>().enabled = true;
                //콜라이더를 끄면 락이 걸림
                //lockObject.transform.Find("/IconAndText/TextMeshPro").GetComponent<TextMesh>().text = "Lock";
                lockObject.GetComponent<TMP_Text>().text = "Lock";
                isLock = false;
            }
            else
            {
                Debug.Log("락걸림");
                roverExplorer.GetComponent<Collider>().enabled = false;
                lockObject.GetComponent<TMP_Text>().text = "UnLock";
                isLock = true;
            }

        }
    }

    #endregion

    #region 한기 율 모델 양쪽에 띄우는 코드 
    public void BothModel()
    {
        float size = 0.1f;

        kepcoModel.SetActive(true);

        //율 모델 크기 조정 및 모델 방향 지정 
        roverExplorer.transform.localRotation = playerPos.transform.localRotation;
        roverExplorer.transform.localRotation = new Quaternion(0, roverExplorer.transform.localRotation.y, 0, roverExplorer.transform.localRotation.w);
       
        roverExplorer.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        scaleChange = new Vector3(roverExplorer.transform.localScale.x * size, roverExplorer.transform.localScale.y * size, roverExplorer.transform.localScale.z * size);
        roverExplorer.transform.localScale = scaleChange;

        //한기 모델 크기 조정 및 모델 방향 지정 
        kepcoModel.transform.localRotation = playerPos.transform.localRotation;
        kepcoModel.transform.localRotation = new Quaternion(0, roverExplorer.transform.localRotation.y, 0, roverExplorer.transform.localRotation.w);

        kepcoModel.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        scaleChange = new Vector3(kepcoModel.transform.localScale.x * size, kepcoModel.transform.localScale.y * size, kepcoModel.transform.localScale.z * size);
        kepcoModel.transform.localScale = scaleChange;

        //위치 지정 
        Vector3 playerforward = playerPos.transform.forward;
        playerforward.y = 0;
        playerforward.Normalize();

        Vector3 modelPos = playerPos.transform.position + playerPos.transform.right * -0.5f + playerPos.transform.up * 0.3f + playerPos.transform.forward * -0.5f;
        roverExplorer.transform.position = modelPos;
        kepcoModel.transform.position = modelPos + roverExplorer.transform.right * -0.01f;


    }
    #endregion

    #region 율에서 만든 거 껏다 키는 코드 
    public void YoulModelOnOff()
    {
        if (roverExplorer.activeSelf)
        {
            roverExplorer.SetActive(false);
        }
        else
        {
            roverExplorer.SetActive(true);
        }
    }
    #endregion

    #region 한기 모델만 별도로 띄우기 ??그냥 율 모델을 둔채 자그만하게 띄우는 거??
    public void KepcoModelOnOff()
    {

    }
    #endregion
}
