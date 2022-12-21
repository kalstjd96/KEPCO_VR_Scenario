using System.Collections;
using UnityEngine;
using System;
using MySql.Data.MySqlClient; //클라이언트 기능을사용하기 위해서 사용
using MySql.Data;//MYSQL함수들을 불러오기 위해서 사용
using System.Data;
using UnityEngine.UI;

using UnityEngine.Events;
using System.Collections.Generic;


public class MariaDB_Manager : MonoBehaviour
{
    MySqlConnection sqlconn = null;


    //private string sqlDBip = "192.168.0.71"; //DB IP 주소 
    private string sqlDBip = "192.168.0.8"; //DB IP 주소 
    //private string sqlDBip = "192.168.0.71"; //DB IP 주소 
    //private string sqlDBip = ""; //DB IP 주소 
    //private string sqlDBname = "mariadb"; //데이터 베이스 이름 
    private string sqlDBname = "realtimeinform"; //데이터 베이스 이름 
    private string sqlDBid = "root"; //계정 ID
    private string sqlDBpw = "youlsys"; //계정 비밀번호
    private string port = "3307";
    string query;
    string textMessage = null;
    public GameObject text;
    int equip_aValue = 0;
    int equip_bValue = 0;
    int count = 0;
    bool isIPEnter = false;
    bool isStopBtn = false;
    List<string> texts = new List<string>(100);

    public InputField input;
    public InputField port_input2;
    public Button startBtn;
    public Button stopBtn;
    public GameObject ipEnterPage;
    Dictionary<int, string> databaseTableName = new Dictionary<int, string>() //이는 데이터 베이스 이름
        {
            { 0 , "hs_7471a"},
            { 1,  "zk_5014h"},
            { 2 , "tx_9933p"},
            { 3 , "lw_4451t"},
            { 4 , "yf_5503e"},
            { 5 , "tag_5111k"}
        };

    private void Awake()
    {
        sqlDBip = null;
        port = null;
    }
    public void FirstSeq()
    {
        startBtn.interactable = false;
        stopBtn.interactable = false;
        ipEnterPage.SetActive(true);
    }

    public void IpCheck() //ip입력 후 확인 버튼 누를 시
    {
        sqlDBip = input.text;
        port = port_input2.text;
        if (sqlDBip == "127.0.0.1" && port != null)
        {
            ipEnterPage.SetActive(false);
            startBtn.interactable = true;
            stopBtn.interactable = true;
            SqlRead();
        }
        else
        {
            input.text = null;
            port_input2.text = null;
        }
        
    }
    public void SqlRead() //ip입력후 확인 버튼 누를 시 
    {
        sqlConnect();
        StartCoroutine(Read());
    }

    private void OnDestroy()
    {
        sqldisConnect();
    }

    public void StartBtn()
    {
        isStopBtn = false;
    }

    public void StopBtn()
    {
        isStopBtn = true;
    }

    IEnumerator Read()
    {
        //while (!string.IsNullOrEmpty(sqlDBip))
        while (true)
        {
            for (int i = 0; i < databaseTableName.Count; i++)
            {
                DataTable dataTables = new DataTable();
                string querys = "SELECT COUNT(dates) AS dates FROM " + databaseTableName[i] + ";";
                dataTables = CountSelect(querys);

                int countResult = 0;
                countResult = int.Parse(dataTables.Rows[0]["dates"].ToString());

                if (countResult < 100)
                {
                    //임시 값임 
                    int equip_aValue;
                    equip_aValue = UnityEngine.Random.Range(50, 60);

                    int equip_bValue;
                    equip_bValue = UnityEngine.Random.Range(70, 80);

                    if (!isStopBtn)
                    {
                        string message = null;
                            message = databaseTableName[i] + " : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Temp : " + equip_aValue + " / Pressur : " + equip_bValue + " ," + "\n";
                            if (databaseTableName[i].Equals("tag_5111k"))
                            {
                                message += "<color=#ff0000>" + "==============================================" + "</color>" + "\n";
                            }
                            textMessage += message;
                            text.GetComponent<Text>().text = textMessage;
                            count++;
                        
                        if(count >= 100) //서버에 줄을 지우는 거
                        {
                            //string[] aa = new string[1];  //1을 한 이유는 2개의 배열만 만들기 위해서 0, 1
                            //aa = textMessage.Split(','); // 처음 부분만 걸러지겠지??
                            string str = textMessage;
                            string result = str.Substring(str.IndexOf(",")+1);
                            string aaa = str.Substring(0, str.LastIndexOf(","));
                           
                            textMessage = result;
                            count--;
                            text.GetComponent<Text>().text = textMessage;
                        }

                    }
                    
                    query = "INSERT INTO  " + databaseTableName[i] + " VALUES(" + equip_aValue + "," + equip_bValue + ", NOW());";

                    if (DBCommunication.instance)
                    {
                        string[] datas = new string[2];
                        DBCommunication.instance.SetData(equip_aValue.ToString(), equip_bValue.ToString(), i);
                    }
                }

                if (countResult >= 100)
                {
                    query = "DELETE FROM " + databaseTableName[i] + " ORDER BY dates asc LIMIT 1;";
                    if (i == 0)// && DBCommunication.instance.GetLength() > 0)
                    {
                        if (DBCommunication.instance)
                        {
                           
                            if (DBCommunication.instance.GetLength() >= 11)
                            {
                                DBCommunication.instance.RemoveAtData(0);
                            }
                        }
                    }
                    //if (!isStopBtn)
                    //{
                    //    if (count > 100)
                    //    {
                    //        Debug.Log("이제 짜를꺼임");
                    //        string[] aa = new string[1];  //1을 한 이유는 2개의 배열만 만들기 위해서 0, 1
                    //        aa = textMessage.Split(','); // 처음 부분만 걸러지겠지??
                    //        Debug.Log("짜를 부분" +aa[0] + " 출력할 부분" + aa[1]);
                    //        textMessage = aa[1].ToString();
                    //        count--;
                    //        text.GetComponent<Text>().text = textMessage;
                    //    }
                    //}
                    
                }
                sqlcmd(query);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public DataTable EndSelect()
    {
        DataTable datat = new DataTable();
        sqlConnect();
        MySqlDataAdapter Sda = new MySqlDataAdapter();
        string resQuery = "SELECT temperature FROM equip_a WHERE dates = (SELECT MAX(dates) FROM equip_a);";
        Sda.SelectCommand = new MySqlCommand(resQuery, sqlconn);
        Sda.Fill(datat);
        sqldisConnect();
        return datat;
    }

    public DataTable CountSelect(string query)
    {
        DataTable ResDT = new DataTable();
        MySqlDataAdapter Sda = new MySqlDataAdapter();
        Sda.SelectCommand = new MySqlCommand(query, sqlconn);
        Sda.Fill(ResDT);//데이터 테이블에  채워넣기를함
        return ResDT; // 데이터 테이블을 리턴함
    }

    private void sqlConnect()
    {
        //DB정보 입력
        string sqlDatabase = "Server=" + sqlDBip + ";Port=" + port + ";Database=" + sqlDBname + ";UserId=" + sqlDBid + ";Password=" + sqlDBpw + "";

        //접속 확인하기
        try
        {
            sqlconn = new MySqlConnection(sqlDatabase);
            sqlconn.Open();
        }
        catch (Exception msg)
        {
            Debug.Log(msg);
            text.GetComponent<Text>().text = msg.Message;
        }
    }

    public void sqlcmd(string query)
    {
        MySqlCommand dbcmd = new MySqlCommand(query, sqlconn); //명령어를 커맨드에 입력
        dbcmd.ExecuteNonQuery(); //명령어를 SQL에 보냄
    }
    private void sqldisConnect()
    {
        sqlconn.Close();
        //Debug.Log("SQL의 접속 상태 : " + sqlconn.State); //접속이 끊기면 Close가 나타남 
    }
}