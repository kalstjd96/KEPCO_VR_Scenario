using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DBCommunication : MonoBehaviourPunCallbacks
{
    public struct Datas
    {
        public Datas(string data1, string data2)
        {
            this.data1 = data1;
            this.data2 = data2;
        }
        public string data1;
        public string data2;
    }

    public static DBCommunication instance;

    public List<Datas>[] datas; 

    void Awake()
    {
        datas = new List<Datas>[6]; //6개의 Datas 틀의 리스트
        for (int i = 0; i < datas.Length; i++)
            datas[i] = new List<Datas>();

        if (instance == null)
            instance = this;
        else
        {
            DestroyImmediate(this);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!photonView.IsMine)
            photonView.RPC("Synchronization", RpcTarget.MasterClient);
    }

    [PunRPC]
    void SetDataRPC(string data1, string data2,int index)
    {
        this.datas[index].Add(new Datas(data1, data2));
    }

    [PunRPC]
    void RemoveDataRPC(string data1 ,string data2 , int index)
    {
        this.datas[index].Remove(new Datas(data1, data2));
    }
    [PunRPC]
    void RemoveAtDataRPC(int index)
    {
        for (int i = 0; i < datas.Length; i++)
            this.datas[i].RemoveAt(index);
    }

    [PunRPC]
    void Synchronization()
    {
        for (int i = 0; i < datas[0].Count; i++) //data2동일하기 때문에 같이 적용
        {
            Datas[] datas = new Datas[6];
            for (int j = 0; j < datas.Length; j++)
            {
                datas[j] = this.datas[j][i];
                photonView.RPC("SetDataRPC", RpcTarget.Others, datas[j].data1, datas[j].data2, j);
            }
        }
    }

    #region 데이터 수정
    //서버
    public void SetData(string data1, string data2, int index)
    {
        photonView.RPC("SetDataRPC", RpcTarget.All, data1, data2, index);
    }

    public void RemoveData(string data1, string data2, int index)
    {
        photonView.RPC("RemoveDataRPC", RpcTarget.All, data1, data2, index);
    }

    public void RemoveAtData(int index)
    {
        photonView.RPC("RemoveAtDataRPC", RpcTarget.All, index);
    }
    //서버
    

    //모바일
    public Datas[] GetData(int index)
    {
        Datas[] datas = new Datas[6];
        for (int i = 0; i < datas.Length; i++)
            datas[i] = this.datas[i][index];
        return datas;
    }

    public int GetLength()
    {
        return datas[0].Count;
    }

    public Datas[] GetLastData()
    {
        Datas[] datas = new Datas[6];
        for (int i = 0; i < datas.Length; i++)
            datas[i] = this.datas[i][this.datas[i].Count-1];
        return datas;
    }

    public Datas GetLastData(int index)
    {
        //Debug.Log(this.datas[index].Count - 1);
        return this.datas[index][this.datas[index].Count - 1];
    }

    public Datas[][] GetDatas()
    {
        Datas[][] datas = new Datas[6][];
        for (int i = 0; i < datas.Length; i++)
        {
            datas[i] = new Datas[this.datas[i].Count];
            for (int j = 0; j < this.datas[i].Count; j++)
                datas[i][j] = this.datas[i][j];
        }

        return datas;
    }
    //모바일
    #endregion
}
