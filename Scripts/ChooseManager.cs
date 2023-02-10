using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ChooseManager : MonoBehaviour
{
    public static ChooseManager instance;
    public bool isJumpCheck;
    public string clickString;
    public PhotonView PV;
    public string[] seqArray { get;  private set; }

    [System.NonSerialized] public int targetIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        seqArray = new string[2];
        isJumpCheck = false;
        PV = transform.GetComponent<PhotonView>();
    }
    private void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }

    public void JumpSeq(DataRow dataRow)
    {
        seqArray = new string[2];
        seqArray[0] = dataRow["JumpSeq"].ToString().Split('/')[0].Trim();
        seqArray[1] = dataRow["JumpSeq"].ToString().Split('/')[1].Trim();

        PopupManager.instance.PopJumpSeq(dataRow);
    }

    /// <summary>
    /// 시퀀스 점프 시, 이전 시퀀스 버튼들의 시간 처리
    /// </summary>
    /// <param name="beforeNextNum">기준 시간</param>
    /// <param name="targetIndex"></param>
    public void CheckJumpSeq_Timer(int targetIndex)
    {
        int beforeNextNum = SeqManager.instance.rowIndex + 2;
        int jumpNum = targetIndex + 1;

        TimerManager.instance.NoteTime(SeqManager.instance.SequenceTime, SeqManager.instance.SequenceTime.GetComponent<Text>().text);
        TimerManager.instance.ResetTimer(SeqManager.instance.SequenceTime);

        if (beforeNextNum < jumpNum)
        {
            //점프뛴 Operator버튼들 전부 시간계산때려주기
            for (int i = beforeNextNum; i < jumpNum; i++)
                TimerManager.instance.NoteTime(SeqListSetting.instance.Oper_Dic[i], "00 : 00 : 00");

            for (int i = beforeNextNum; i < jumpNum; i++)
            {
                if (SeqListSetting.instance.Stage_Dic[i] != SeqListSetting.instance.Stage_Dic[i + 1])
                    if (SeqListSetting.instance.Stage_Dic[i] != SeqListSetting.instance.Stage_Dic[jumpNum])
                    {
                        try
                        {
                            TimerManager.instance.NoteStateTime(SeqListSetting.instance.Stage_Dic[i]);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.Log(ex);
                            Debug.Log("i : " + i);
                            Debug.Log(SeqListSetting.instance.Stage_Dic[i].name, SeqListSetting.instance.Stage_Dic[i]);
                        }
                    }
            }
        }
    }

    #region YES/NO 버튼 이벤트
    public bool isExit = false;
    [PunRPC]
    public void OnClick_YN(bool yesBtn)
    {
        PopupManager.instance.SeqJumpPanel.SetActive(false);
        int index;
        if (yesBtn)
            index = 1;
        else
            index = 0;

        if (seqArray[index].Equals("exit"))
            isExit = true;
        else
            targetIndex = int.Parse(seqArray[index]) - 1;

        if (!ConnManager.instance.UseNetwork)
            SeqManager.instance.IsOn_true();
        else
            SeqManager.instance.PV.RPC("RPC_IsJump_T", RpcTarget.All);
    }

    //[PunRPC]
    //public void OnClick_YES()
    //{
    //    PopupManager.instance.SeqJumpPanel.SetActive(false);

    //    if (seqArray[1].Equals("end"))
    //        targetIndex = SeqManager.instance.dataTable.Rows.Count - 1;
    //    else
    //        targetIndex = int.Parse(seqArray[1]) - 1;


    //    if (!ConnManager.instance.UseNetwork)
    //        SeqManager.instance.IsOn_true();
    //    else 
    //        SeqManager.instance.PV.RPC("RPC_IsJump_T", RpcTarget.All);
    //}
    //[PunRPC]
    //public void OnClick_NO()
    //{
    //    PopupManager.instance.SeqJumpPanel.SetActive(false);
    //    if (seqArray[0].Equals("end"))
    //        targetIndex = SeqManager.instance.dataTable.Rows.Count - 1;
    //    else
    //        targetIndex = int.Parse(seqArray[0]) - 1;


    //    if (!ConnManager.instance.UseNetwork) 
    //        SeqManager.instance.IsOn_true();
    //    else 
    //        SeqManager.instance.PV.RPC("RPC_IsJump_T", RpcTarget.All);
    //}
    #endregion
    public void Initialize()
    {
        isJumpCheck = false;
    }
}
