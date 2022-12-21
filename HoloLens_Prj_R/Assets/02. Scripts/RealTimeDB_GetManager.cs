using System.Collections;
using UnityEngine;
using System;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
//using MySql.Data;     //MYSQL함수들을 불러오기 위해서 사용
//using MySql.Data.MySqlClient;    //클라이언트 기능을사용하기 위해서 사용


public class RealTimeDB_GetManager : MonoBehaviour
{
    [SerializeField] Transform photon_dbCommunication;

    public GameObject dbCommunication;
    GameObject[] tagNamesChildren;
    public GameObject tagNames;

    string value;
    string value2;
    Dictionary<string, int> databaseTableName = new Dictionary<string, int>()
        {
            { "hs_7471a"  ,0},
            { "zk_5014h"  ,1},
            { "tx_9933p"  ,2},
            { "lw_4451t"  ,3},
            { "yf_5503e"  ,4},
            { "tag_5111k"  ,5}
        };

    //초기화 같은 기능들은 Awake에서 하는 것을 권장
    private void Awake()
    {
        tagNamesChildren = new GameObject[tagNames.transform.childCount];
        for (int i = 0; i < tagNamesChildren.Length; i++) //5번
        {
            tagNamesChildren[i] = tagNames.transform.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        if (dbCommunication.activeSelf)
        {
            StartCoroutine(DataGet());
        }
    }

    IEnumerator DataGet()
    {
        for (int i = 0; i < tagNamesChildren.Length; i++) //5번
        {
            string targetName = tagNamesChildren[i].ToString().Split(' ')[0];
            
            if (DBCommunication.instance.GetLength() > 0) // Datas의 값이 존재할 때로
            {
                DBCommunication.Datas datas = DBCommunication.instance.GetLastData(databaseTableName[targetName]);

                value = datas.data1;
                value2 = datas.data2;

                int dataLength = DBCommunication.instance.GetLength();

                string text1 = "<color=yellow>온도</color> : ";
                string text2 = "\n\n<color=yellow>압력</color> : ";
                Transform Information = tagNamesChildren[i].transform.Find("Table/Contents");
                Information.GetComponent<Text>().text = text1 + value + text2 + value2;
            }
            //DBCommunication.Datas datas = DBCommunication.instance.GetLastData(databaseTableName[targetName]);
            yield return new WaitForSeconds(0.5f);
        }
    }
    //public void DataCheck()
    //{

    //    tagNamesChildren[i] = tagNames.transform.GetChild(i).gameObject;

    //    string targetName = target;
    //    //여기 에러임 !!!!!!!!!
    //    DBCommunication.Datas datas = DBCommunication.instance.GetLastData(databaseTableName[targetName]);

    //    value = datas.data1;
    //    value2 = datas.data2;

    //    int dataLength = DBCommunication.instance.GetLength();

    //    string text1 = "<color=yellow>온도</color>\n";
    //    string text2 = "\n\n<color=yellow>압력</color>\n";
    //    Transform Information = tagNamesChildren[0].transform.Find("Table/Contents");
    //    Information.GetComponent<Text>().text = text1 + value + text2 + value2;
    //}
}