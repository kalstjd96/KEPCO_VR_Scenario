using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject dbCommunication;
    [SerializeField] GameObject messageCanvas;
    [SerializeField] Text messageText;
    [SerializeField] GameObject menu;

  

    string version = "1.0v";
    byte maxPlayer = 10;
    string roomNameNum;
    string playType;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = version;
            PhotonNetwork.ConnectUsingSettings();
        }

      
    }

    public override void OnConnectedToMaster()
    {
        if (SceneManager.GetActiveScene().name.Equals("Server")) //서버로 지정할 씬이름과 동일해야한다.
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = maxPlayer;
            roomNameNum = Random.Range(0, 999).ToString("000");
            PhotonNetwork.CreateRoom("CyberPlant" + roomNameNum, roomOptions);
            //PhotonNetwork.CreateRoom("CyberPlant" + Random.Range(0, 999).ToString("000"), roomOptions);
        }
        else if (SceneManager.GetActiveScene().name.Equals("HoloLens_Scene_Hardware")) //마찬가지 씬이름과 동일해야한다.
        //else if (SceneManager.GetActiveScene().name.Equals("HoloLens_Scene_MainBackUp")) //마찬가지 씬이름과 동일해야한다.
        {
            PhotonNetwork.JoinRandomRoom();
            //PhotonNetwork.JoinRoom("CyberPlant RealTime-Check");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //try
        //{
        //    Debug.Log(" 111 : " + message);
        //}
        //catch (System.Exception)
        //{
        //    Debug.Log("접속실패 : " + message + " : " + returnCode);
        //    throw;
        //}
        //if (messageCanvas) messageCanvas.SetActive(true);
        //if (messageText) messageText.text = "서버와 연결에 실패했습니다.";
        
        //연결에 실패할 때 메뉴들을 끄는 것인데 해제
        //if (menu)
        //{
        //    menu.SetActive(false);
        //}
        //여기서는 연결이 실패했을 경우 재실행 버튼을 만들어 클릭 시 ReJoinRandomRoom()를 호출해야한다.
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("접속 성공");
        dbCommunication.SetActive(true);
        MariaDB_Manager mariaDB_Manager = FindObjectOfType<MariaDB_Manager>();
        if (mariaDB_Manager)
            //mariaDB_Manager.SqlRead();
            mariaDB_Manager.FirstSeq();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("누가 들어옴");
    }

    public void ReJoinRandomRoom()
    {
        //PhotonNetwork.Reconnect();
        PhotonNetwork.JoinRandomRoom();
        //menu.SetActive(true);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        PhotonNetwork.LeaveRoom();

        if (messageCanvas) messageCanvas.SetActive(true);
        //if (messageText) messageText.text = "서버와 연결에 실패했습니다.";
    }
}
