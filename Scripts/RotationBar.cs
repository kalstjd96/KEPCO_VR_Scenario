using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class RotationBar : MonoBehaviour
{
    public static RotationBar instance;
    int[] gauge;
    public Transform LIC;
    List<GameObject> targets;
    List<GameObject> highLightTarget;
    List<GameObject> target;
    bool isCompelete;

    Transform targetBar;
    Dictionary<string, string> gaugeInfo;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        targets = new List<GameObject>();
        target = new List<GameObject>();
        isCompelete = false;
    }
    private void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }

    public void GaugeSetting(DataRow dataRow)
    {
        gaugeInfo = new Dictionary<string, string>();
        targets = new List<GameObject>();
        for (int j = 0; j < dataRow["TagNo"].ToString().Split('/').Length; j++)
        {
            for (int i = 0; i < SeqManager.tagNoList.Count; i++)
            {
                if (SeqManager.tagNoList[i].name.Equals(dataRow["TagNo"].ToString().Split('/')[j].Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    targets.Add(SeqManager.tagNoList[i].gameObject);
                    if (!gaugeInfo.ContainsKey(SeqManager.tagNoList[i].ToString()))
                        gaugeInfo.Add(SeqManager.tagNoList[i].gameObject.name, dataRow["Standard"].ToString().Split('/')[j].Trim());
                    break;
                }
            }
        }


        StartCoroutine(GaugeStart(targets));
    }

    public IEnumerator GaugeStart(List<GameObject> currentTaget)
    {
        WaitUntil waitUntil = new WaitUntil(() => isCompelete);

        for (int i = 0; i < currentTaget.Count; i++)
        {
            //1. 한번 쳐다보면 온도계 변화 시작
            gauge = new int[2];

            string value = gaugeInfo[currentTaget[i].name];
            gauge[0] = int.Parse(value.ToString().Split(',')[0]);
            gauge[1] = int.Parse(value.ToString().Split(',')[1]);

            targetBar = currentTaget[i].transform.GetChild(0).GetChild(0);
            for (int k = gauge[0]; k < targetBar.childCount; k++)
            {
                targetBar.GetChild(k).localRotation =
                    Quaternion.Euler(-90f, targetBar.GetChild(k).localRotation.y, targetBar.GetChild(k).localRotation.z);
            }

            for (int j = 0; j < gauge[0]; j++)
            {
                targetBar.GetChild(j).localRotation =
                    Quaternion.Euler(90f, targetBar.GetChild(j).localRotation.y, targetBar.GetChild(j).localRotation.z);
            }

            highLightTarget = new List<GameObject>();
            foreach (var highLight in currentTaget[i].GetComponentsInChildren<MeshFilter>())
                highLightTarget.Add(highLight.gameObject);

            Highlight.instance.On(highLightTarget.ToArray());

            target = new List<GameObject>();
            target.Add(currentTaget[i]);

            LookCheck.instance.Settings(targets);

            yield return new WaitUntil(() => LookCheck.instance.loadingBar.gameObject.activeSelf);

            Highlight.instance.Off();

            if (gauge[0] > gauge[1]) //감소한다 
            {
                for (int z = gauge[0]; z >= gauge[1]; z--)
                {
                    BarState(z, false);
                    yield return new WaitForSeconds(0.2f);

                    if (z == gauge[1])
                        isCompelete = true;
                }
            }
            else //증가한다
            {
                for (int g = gauge[0]; g <= gauge[1]; g++)
                {
                    BarState(g, true);
                    yield return new WaitForSeconds(0.2f);

                    if (g == gauge[1])
                        isCompelete = true;
                }
            }

            yield return waitUntil;
            yield return new WaitUntil(() => LookCheck.instance.isCheck);
        }
    }

    private void BarState(int barIndex, bool up = false)
    {
        if (up)
        {
            targetBar.GetChild(barIndex).localRotation =
            Quaternion.Euler(90f, targetBar.GetChild(barIndex).localRotation.y, targetBar.GetChild(barIndex).localRotation.z);
        }
        else
        {
            targetBar.GetChild(barIndex).localRotation =
            Quaternion.Euler(-90f, targetBar.GetChild(barIndex).localRotation.y, targetBar.GetChild(barIndex).localRotation.z);
        }
    }

}
