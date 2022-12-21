using System.Collections;
using UnityEngine;
using System;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
//using MySql.Data;     //MYSQL함수들을 불러오기 위해서 사용
//using MySql.Data.MySqlClient;    //클라이언트 기능을사용하기 위해서 사용


public class DB_Manager : MonoBehaviour
{
    [SerializeField] Transform photon_dbCommunication;
    MySqlConnection sqlconn = null;
    public GameObject dbCommunication;
    GameObject[] tagNamesChildren;
    public GameObject tagName;
    public GameObject tagInfo;
    public GameObject eyeTracking;
    string value;
    string value2;
    string dbTagName;

    //DB 관련 변수
    string sqlDBip;
    string port;
    string sqlDBname;
    string userName;
    string password;
    int tableNameListCount;

    DBCommunication.Datas datas;
    //public Dictionary<string, int> databaseTableName = new Dictionary<string, int>();
    public Dictionary<string, int> databaseTableName = new Dictionary<string, int>()
        {
            { "hs_7471a"  ,0},
            { "zk_5014h"  ,1},
            { "tx_9933p"  ,2},
            { "lw_4451t"  ,3},
            { "yf_5503e"  ,4},
            { "tag_5111k"  ,5}
        };
    public static DB_Manager instance;

    //초기화 같은 기능들은 Awake에서 하는 것을 권장
    private void Awake()
    {
        //tableNameListCount = 0;
        //tagNamesChildren = new GameObject[tagInfo.transform.childCount];
        //for (int i = 0; i < tagNamesChildren.Length; i++) //5번
        //{
        //    tagNamesChildren[i] = tagInfo.transform.GetChild(i).gameObject;
        //}

        //sqlDBip = "127.0.0.1";
        //port = "3307";
        //sqlDBname = "realtimeinform";
        //userName = "root";
        //password = "youlsys";

        //if (instance == null)
        //    instance = this;
        //else
        //{
        //    DestroyImmediate(this);
        //    Destroy(gameObject);
        //}

    }

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            DestroyImmediate(this);
            Destroy(gameObject);
        }

        tableNameListCount = 0;
        tagNamesChildren = new GameObject[tagInfo.transform.childCount];
        for (int i = 0; i < tagNamesChildren.Length; i++) //5번
        {
            tagNamesChildren[i] = tagInfo.transform.GetChild(i).gameObject;
        }
        //SqlRead();
    }
    #region 안쓰는 코드 
    //private void Update()
    //{
    //    if (dbCommunication.activeSelf)
    //    {
    //        StartCoroutine(DataGet());
    //    }
    //}

    //IEnumerator DataGet()
    //{
    //    //if (eyeTracking.targetName != null)
    //    //{
    //        datas = DBCommunication.instance.GetLastData(databaseTableName[eyeTracking.targetName]);
    //        //Debug.Log(databaseTableName["보조급수펌프_진동"]);
    //        value = datas.data1;
    //        value2 = datas.data2;

    //        string text1 = "<color=yellow>온도</color> : ";
    //        string text2 = " , <color=yellow>압력</color> : ";
    //        Transform information = tagNamesChildren[0].transform.Find("AimLookingInfo");
    //        information.GetComponent<TMP_Text>().text = text1 + value + text2 + value2;
    //        tagNamesChildren[0].transform.Find("Image/Text (TMP)").GetComponent<TMP_Text>().text = eyeTracking.targetName;
    //        yield return new WaitForSeconds(0.5f);

    //        //정보가 전부 들어오면 태그 정보를 확인하는 창을 뜨게 한다.
    //        tagInfo.SetActive(true);

    //        //포인트 부분도 콜라이더를 킨다.
    //        for (int j = 0; j < tagName.transform.childCount; j++)
    //        {
    //            tagName.transform.GetChild(j).GetComponent<BoxCollider>().enabled = true;
    //        }
    //    //}
    //    #region 전체 타겟 정보가 나오게 하는 코드
    //    //for (int i = 0; i < tagNamesChildren.Length; i++) //5번
    //    //{
    //    //    string targetName = tagNamesChildren[i].ToString().Split(' ')[0];

    //    //    if (DBCommunication.instance.GetLength() > 0) // Datas의 값이 존재할 때로
    //    //    {
    //    //        DBCommunication.Datas datas = DBCommunication.instance.GetLastData(databaseTableName[targetName]);

    //    //        value = datas.data1;
    //    //        value2 = datas.data2;

    //    //        //int dataLength = DBCommunication.instance.GetLength();

    //    //        string text1 = "<color=yellow>온도</color> : ";
    //    //        string text2 = " , <color=yellow>압력</color> : ";
    //    //        Transform Information = tagNamesChildren[i].transform.Find("AimLookingInfo");
    //    //        Information.GetComponent<TMP_Text>().text = text1 + value + text2 + value2;
    //    //    }
    //    //    //DBCommunication.Datas datas = DBCommunication.instance.GetLastData(databaseTableName[targetName]);
    //    //    yield return new WaitForSeconds(0.5f);


    //    //    if ( i == tagNamesChildren.Length -1) //마지막까지 입력이 모두 완료가 되었으면
    //    //    {
    //    //        //정보가 전부 들어오면 태그 정보를 확인하는 창을 뜨게 한다.
    //    //        tagInfo.SetActive(true);

    //    //        //포인트 부분도 콜라이더를 킨다.
    //    //        for (int j = 0; j < tagName.transform.childCount; j++)
    //    //        {
    //    //            tagName.transform.GetChild(j).GetComponent<BoxCollider>().enabled = true;
    //    //        }
    //    //    }
    //    //}
    //    #endregion
    //}
    #endregion

    //private void OnDestroy()
    //{
    //    sqldisConnect();
    //}
    #region DB 관련 코드 홀로렌즈에서는 안먹어 사용x
    //private void sqlConnect()
    //{
    //    //DB정보 입력
    //    string sqlDatabase = "Server=" + sqlDBip + ";Port=" + port + ";Database=" + sqlDBname + ";UserId=" + userName + ";Password=" + password + "";

    //    //접속 확인하기
    //    try
    //    {
    //        sqlconn = new MySqlConnection(sqlDatabase);
    //        sqlconn.Open();
    //    }
    //    catch (Exception msg) //이것이 위에 2개의 예외처리값 상위에 존재하지만 그냥 선언함
    //    {
    //        Debug.Log(msg);
    //        //text.GetComponent<Text>().text = msg.Message;
    //    }
    //}
    //public DataTable TableList(string query)
    //{
    //    DataTable ResDT = new DataTable();
    //    MySqlDataAdapter Sda = new MySqlDataAdapter();
    //    Sda.SelectCommand = new MySqlCommand(query, sqlconn);
    //    Sda.Fill(ResDT);//데이터 테이블에  채워넣기를함
    //    return ResDT; // 데이터 테이블을 리턴함
    //}

    //public void SqlRead() //ip입력후 확인 버튼 누를 시 
    //{
    //    //DB 연결
    //    //sqlConnect();
    //    //테이블 이름들을 가져와 넣는다.
    //    //string tableNameQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'realtimeinform'";
    //    //DataTable tableNameList = TableList(tableNameQuery);
    //    //for (int i = 0; i < tableNameList.Rows.Count; i++)
    //    //{
    //    //    databaseTableName.Add(tableNameList.Rows[i][0].ToString(),tableNameListCount);
    //    //    tableNameListCount++;
    //    //}
    //    //eyeTracking.SetActive(true);
    //}
    //public void sqlcmd(string query)
    //{
    //    MySqlCommand dbcmd = new MySqlCommand(query, sqlconn); //명령어를 커맨드에 입력
    //    dbcmd.ExecuteNonQuery(); //명령어를 SQL에 보냄
    //}
    //private void sqldisConnect()
    //{
    //    sqlconn.Close();
    //    //Debug.Log("SQL의 접속 상태 : " + sqlconn.State); //접속이 끊기면 Close가 나타남 
    //}
    #endregion
}