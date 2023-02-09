using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// ���� �� ���� GetChild(0)��°�� �ٲٰ��� �ϴ� ���� �־������ �ȴ�.
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
            //�ڷ���Ʈ �� Target ������Ʈ ��ġ�� �̵� 
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

            LookCheck.instance.Tracker(true, targetList[i].name + "��(��) �����ϼ���.", targetList[i].transform);

            // ���� ������ �����Ҷ�
            MinimapManager.instance.PointState(targetList[i].name, "Start");

            targetList[i].AddComponent<ObjReplacingChild>();
            yield return waitUntil;

            // ���� ������ ��������
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
                // ��� ���������� ������ ���������� �Ѿ��
                MinimapManager.instance.Initialize();
                SeqManager.instance.IsOn_true();
            }
        }

        
    }
    
   
}
