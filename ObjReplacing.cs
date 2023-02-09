using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// 원본 모델 하위 GetChild(0)번째에 바꾸고자 하는 모델을 넣어놓으면 된다.
/// </summary>
public class ObjReplacing : MonoBehaviour
{
    [NonSerialized] public bool isReplace;
    List<GameObject> targetList;
    List<GameObject> highLightTarget;

    public static ObjReplacing instance { get; set; }

    void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }


    private void Awake()
    {
        if (instance == null)
            instance = this;

        isReplace = false;
        targetList = new List<GameObject>();
    }

    public void Init()
    {
        isReplace = false;
        targetList = new List<GameObject>();

        StopAllCoroutines();
    }

    public void Settings(List<GameObject> targets)
   {
        this.targetList = targets;

        if (targetList.Count == 0)
        {
            SeqManager.instance.IsOn_true();
        }
        else
            StartCoroutine(Process());
   }

    public void CompeleteCheck(bool isReplace)
    {
        this.isReplace = isReplace;
    }

    public IEnumerator Process()
    {
        MinimapManager.instance.Set(SeqManager.instance.dataRow["TagNo"].ToString(), "Valve");

        WaitUntil waitUntil = new WaitUntil(() => isReplace);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);
        for (int i = 0; i < targetList.Count; i++)
        {
            //텔레포트 시 Target 오브젝트 위치로 이동 
            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
            {
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targetList[i].transform));
            }

            highLightTarget = new List<GameObject>();

            foreach (var item in targetList[i].GetComponentsInChildren<MeshRenderer>())
            {
                highLightTarget.Add(item.gameObject);
            }

            Highlight.instance.On(highLightTarget.ToArray());

            LookCheck.instance.Tracker(true, targetList[i].name + "을(를) 조작하세요.", targetList[i].transform);

            // 설비 조작이 시작할때
            MinimapManager.instance.PointState(targetList[i].name, "Start");

            targetList[i].AddComponent<ObjReplacingChild>();
            yield return waitUntil;

            // 설비 조작이 끝났을때
            MinimapManager.instance.PointState(targetList[i].name, "End");

            Destroy(targetList[i].GetComponent<ObjReplacingChild>());
            Highlight.instance.Off();

            Transform changeObj = targetList[i].transform.GetChild(0);

            foreach (var item in targetList[i].GetComponentsInChildren<MeshRenderer>())
            {
                item.enabled = false;
            }

            changeObj.gameObject.SetActive(true);

            yield return waitForSeconds;
            isReplace = false;
            
            if (i == targetList.Count - 1)
            {
                LookCheck.instance.Tracker(false);
                // 모든 설비조작이 끝나고 다음절차로 넘어갈때
                MinimapManager.instance.Initialize();
                SeqManager.instance.IsOn_true();
            }
        }

        
    }
    
   
}
