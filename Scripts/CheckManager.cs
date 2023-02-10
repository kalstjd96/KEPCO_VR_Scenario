using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class CheckManager : MonoBehaviour
{
    public static CheckManager instance; // 3번 타입
    List<GameObject> targets;

    #region 해당 오브젝트 교체하는 절차

    Transform leftHand;
    Transform rightHand;
    #endregion

    private void Awake()
    {
        if (instance == null)
            instance = this;
        targets = new List<GameObject>();

        #region 해당 오브젝트 교체하는 절차
        leftHand = null;
        rightHand = null;
        #endregion
    }
    private void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }

    public void LookTarget(DataRow dataRow, Function onCompelte = null)
    {
        targets = new List<GameObject>();
        for (int j = 0; j < dataRow["TagNo"].ToString().Split('/').Length; j++)
        {
            for (int i = 0; i < SeqManager.tagNoList.Count; i++)
            {
                if (SeqManager.tagNoList[i].name.Equals(dataRow["TagNo"].ToString().Split('/')[j].Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    targets.Add(SeqManager.tagNoList[i].gameObject);
                    break;
                }
            }
        }
        LookCheck.instance.Settings(targets, onCompelte);
    }

    public void ControlMMI(DataRow dataRow)
    {
        InputDeviceController.KepcoPage(dataRow["Actor"].ToString(), dataRow["Pribtn"].ToString(), dataRow["J168"].ToString(), dataRow["TagNo"].ToString(), dataRow["ObjectID"].ToString());
        InputDeviceController.CreateHighlightList();

        switch (InputDeviceController._actor.Trim())
        {
            case "RO":
                SeqManager.instance.symbolHighlight.ro_pageNavigator.Refresh();
                break;
            case "TO":
                SeqManager.instance.symbolHighlight.to_pageNavigator.Refresh();
                break;
            case "EO":
                SeqManager.instance.symbolHighlight.eo_pageNavigator.Refresh();
                break;
        }
    }


    public void Replacing(DataRow dataRow)
    {
        //MinimapManager.instance.Set(dataRow["TagNo"].ToString());

        targets = new List<GameObject>();
        Debug.Log(SeqManager.tagNoList.Count);
        for (int i = 0; i < dataRow["TagNo"].ToString().Split('/').Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].transform.name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    targets.Add(SeqManager.tagNoList[j].gameObject);
                    break;
                }
            }
        }

        ObjReplacing.instance.Settings(targets);
    }

    public void SetObjectEffect(DataRow dataRow)
    {
        targets = new List<GameObject>();
        for (int i = 0; i < dataRow["TagNo"].ToString().Split('/').Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].transform.name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    targets.Add(SeqManager.tagNoList[j].gameObject);
                    break;
                }
            }
        }

        GameObject particle = targets[0].transform.parent.Find("WaterSpread").gameObject;

        for (int i = 0; i < dataRow["TagNo"].ToString().Split('/').Length; i++)
        {
            if (dataRow["Standard"].ToString().Split('/')[i].Trim().Equals("OPEN"))
                particle.SetActive(true);
            else
                particle.SetActive(false);
        }

        LookCheck.instance.Settings(targets);
    }


    public void Initialize()
    {
        if (ObjReplacing.instance)
            ObjReplacing.instance.Init();
        LookCheck.instance.Init();
    }
}
