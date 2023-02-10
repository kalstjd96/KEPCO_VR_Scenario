#define Multi
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Valve.VR.InteractionSystem;
using NA;
using UnityEditor;
using Valve.VR;

public enum ScenarioTable
{
    NONE,
    MAIN_RCS_Charging_Pump,                               //충전펌프를 이용한 RCS 충수
    MAIN_RCS_Dynamic_Vent,                                //정적배기 및 동적배기
    MAIN_RCS_Initial_Preparation,                         //RCS 층수를 위한 준비
    MAIN_RCS_Shutdown_Cooling_Pump,                       //정지냉각펌프 01A를 이용한 충수
    MAIN_CONDENSER_Turbine_Seal_Steam,                    //복수기진공저하-터빈밀봉증기 모관압력 감소
    MAIN_CONDENSER_CWP_Abnormal_Stop,                     //복수기진공저하-운전중인 순환수 펌프 비정상 정지
    MAIN_CONDENSER_Vacuum_Destruction_Valve,              //복수기진공저하-복수기 진공 파괴밸브 비정상 개방
    MAIN_CONDENSER_Exhaust_Valve_Abnormal_Open,           //복수기진공저하-배기밸브 비정상 개방
    MAIN_CONDENSER_Insufficient_Supply_Of_ShaftSealWater, //복수기진공저하-진공관련 밸브의 축 밀봉수 공급부족
    MAIN_CONDENSER_Other_Causes,                          //복수기진공저하-기타원인
    MAIN_FIRE_DGControlRoom,                              //DG Control Panel Room 화재
    MAIN_FIRE_KitchenDiningRoom,                          //Kitchen & Dining Room 화재
    MAIN_CRITICAL_ACCIDENT,                               //중대사고
    IMAGE_LOSS_Of_RCP_CCW_Sealed_Injection_Water,         //RCP 공급 CCW 및 밀봉주입수 동시상실
    IMAGE_ABNORMAL_CLOSE_RCP_IsolationValve,              //RCP 제어유출수 격리밸브 비정상 닫힘
    IMAGE_ABNORMAL_OPENED_SBCV,                           //SBCV 비정상 개방
    IMAGE_ABNORMAL_OPENED_Pressurizer_SprayValve,         //가압기 살수밸브 고장 열림
    IMAGE_STICKING_Of_WaterLevel_ControlValve,            //복수계통 밀봉수 주입탱크 수위제어밸브 개방고착
    IMAGE_LEAKAGE_CondenserPipeLine,                      //복수기관 누설
    IMAGE_LOSS_Of_SealedWater_Of_VacuumPump,              //운전중인 복수기 진공펌프 밀봉수 상실
    IMAGE_ABNORMAL_OPENED_SpilledWater_ControlValve,      //유출수 배압제어밸브(CV-201PQ) 비정상 닫힘
    IMAGE_CCW_TemperatureControl,                         //유출수 열교환기 온도제어기 고장시 CCW 온도제어
    IMAGE_ABNORMAL_VALVE_Of_Low_Pressure_Turbine,         //저압터빈 배기후드 살수밸브 비정상
    IMAGE_ABNORMAL_SG_WaterLevel_Control_System,          //증기발생기 수위제어계통 비정상
    IMAGE_Control_Tank_Level_Transmitter_FailLo_226,      //체적제어탱크 수위전송기 CV-LT-226 Fail Lo
    IMAGE_Control_Tank_Level_Transmitter_FailLo_227,      //체적제어탱크 수위전송기 CV-LT-227 Fail Lo (고도화)
    IMAGE_FAILURE_Control_Tank_Inlet_Valve_PHIX_VCT,      //체적제어탱크 입구밸브 고장 (PHIX-VCT)
    IMAGE_FAILURE_Control_Tank_Inlet_Valve_VCT_PHIX,      //체적제어탱크 입구밸브 고장 (VCT-PHIX)
    IMAGE_ABNORMAL_OPENED_Charging_Flow_Control_Valve,    //충전유량 제어밸브 CV-V212PQ 비정상 열림
    IMAGE_MANUAL_OPERATION_Of_Hydraulic_Drive_System,     //유압구동장치 수동조작
    IMAGE_TURBINE_TRIP                                    //터빈트립
}

public enum Role
{
    All,
    RO,
    RO1,
    RO2,
    TO,
    TO1,
    TO2,
    EO,
    EO1,
    EO2,
    STA,
    LO,
    LO1,
    LO2,
    NONE
}

public class SeqManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Fields

    public static SeqManager instance;
    public bool isOn;                   //다음 시퀀스로 넘어갈때 사용하는 변수
    public bool isCall;                 //다음 시퀀스로 넘어갈때 사용하는 변수
    public bool IsNextStep;             //UI를 눌러서 다음 절차로 넘어가게 하기 위한 bool 변수
    public bool isStart;        //처음 시작에서 확인 버튼을 눌러야 다음 시퀀스로 넘어가게 하기 위한 변수
    public bool HighlightOn; //연습,실습 모드를 구분짓기 위한 것으로 하이라이트를 표시해줄지 말지를 결정 짓는 변수
    public bool fadeState;
    public int TimeAttack = 3000; //30분일때
    private string isFadeValue = ""; //Player isFade에 사용 이전 위치와 현재 위치를 비교하기 위함
    public Vector3 currentPhonePos;
    [System.NonSerialized] public DataRow dataRow;
    public DataTable dataTable { get; set; }
    public string sequenceTableName { get; set; }
    public string sequenceType { get; set; }
    public int rowIndex { get; set; }                    //시나리오 순서
    public int sequenceCount { get; set; }

    public Transform TagGroup;
    public Transform pagingphoneGroup;

    public GameObject PlayTime;
    public GameObject SequenceTime;
    //MS
    public static List<Transform> tagNoList; //3번 타입 PC에 해당하는 TagNo를 제외한 모든 TagNo와 이름이 일치하는 obj 넣기위함
    public static List<Transform> phoneList; //3번 타입 PC에 해당하는 TagNo를 제외한 모든 TagNo와 이름이 일치하는 obj 넣기위함
    Dictionary<string, GameObject> buildDic;
    Dictionary<string, GameObject> pagingPhoneDic;
    public Transform malFunction;
    GameObject valveLoc;

    [Header("Resource Prefabs을 넣을 위치")]
    [SerializeField] Transform local;
    [SerializeField] Transform pagingPhone_Location;
    [SerializeField] Transform valveLocation;

    public GameObject player { get; set; } //Play 오브젝트 담아두기
    public GameObject LeftHand; //Play 오브젝트 담아두기
    public GameObject RightHand; //Play 오브젝트 담아두기
    public Transform mcrPositon; //MCR 위치로 Fade시 해당 Transform으로 이동함
    public SymbolHighlight symbolHighlight;
    public PlayerObjDistance playerObjDistance;

    public List<Transform> localBuildingList;
    [HideInInspector] public bool isMCR;

    public enum PlayMoveType
    {
        Teleport,
        TouchPadMove
    }

    public PlayMoveType playMoveType;
    #endregion Fields

    #region 멀티 플레이를 위한 Fields
    [Header("Player Info List")]
    public Dictionary<Role, List<GameObject>> playerKey_Actor = new Dictionary<Role, List<GameObject>>();
    public List<GameObject> PlayerList = new List<GameObject>();        //게임내에 들어온 사용자들 List를 담아두는 변수
    public GameObject ItsMe;                                            //멀티를 위해 생성된 나의 Player
    bool isJoin = false;                                                //모두 참여했는지 안했는지 나타내주기위한 변수
    public bool IsGetSimulData = false;
    bool isWaitInit = false;
    public PhotonView PV;
    bool isNext = false;
    bool isReady = false;
    public bool isJump { get; set; }
    #endregion 멀티 플레이를 위한 Fields

    private void Awake()
    {
        if (instance == null)
            instance = this;
        isOn = true;
        isStart = false;
        isWaitInit = false;
        isNext = false;
        isJump = false;
        currentPhonePos = Vector3.zero;
        sequenceType = null;
        isFadeValue = null;
        rowIndex = 0;
        player = GameObject.Find("Player");
        tagNoList = new List<Transform>();
        phoneList = new List<Transform>();
        //디폴트 이동 방식은 텔레포트
        SetPlayMoveType(PlayMoveType.Teleport);
        PV = transform.GetComponent<PhotonView>();
        foreach (var ackbtn in acknowledgeBtn.GetComponentsInChildren<PressTriggerEvent>())
            ackbtn.AddHanddPressListner(delegate { OnPressAcknowledge(); });

        #region 씬 하이라키 세팅

        buildDic = new Dictionary<string, GameObject>();
        pagingPhoneDic = new Dictionary<string, GameObject>();
        localBuildingList = new List<Transform>();
        malFunction.GetComponent<MalFunction>().onOffGameObject = new List<GameObject>();
        valveLoc = null;

        #endregion
    }

    IEnumerator Start()
    {
        dataTable = ConnManager.instance.sequenceTable;
        #region 씬 하이라키 세팅

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            //1. Build 세팅
            if (!string.IsNullOrEmpty(dataTable.Rows[i]["IsBuilding"].ToString()))
            {
                string[] splitBuild = dataTable.Rows[i]["IsBuilding"].ToString().Split('/');
                for (int j = 0; j < splitBuild.Length; j++)
                {
                    string buildName = splitBuild[j].Trim();
                    if (!buildDic.ContainsKey(buildName))
                    {
                        GameObject buildObj = Resources.Load<GameObject>("Local/Building/" + buildName);
                        
                        GameObject newObj = Instantiate(buildObj, local);
                        localBuildingList.Add(newObj.transform);
                        newObj.name = buildObj.name;
                        //newObj.SetActive(true);
                        buildDic.Add(buildName, newObj);
                    }
                }
            }

            //2. PagingPhone 세팅
            if (!string.IsNullOrEmpty(dataTable.Rows[i]["Local_Phone"].ToString()))
            {
                string phoneName = dataTable.Rows[i]["Local_Phone"].ToString().Trim();
                if (!pagingPhoneDic.ContainsKey(phoneName))
                {
                    GameObject phoneObj = Resources.Load<GameObject>("Local/PagingPhone/" + phoneName);
                    if (phoneObj != null)
                    {
                        GameObject newObj = Instantiate(phoneObj,pagingPhone_Location);
                        newObj.name = phoneObj.name;
                        newObj.SetActive(true);
                        pagingPhoneDic.Add(phoneName, newObj);
                    }
                        
                }
            }

        }

        //3. ValveLocation 세팅
        if (ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_KitchenDiningRoom 
            || ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_DGControlRoom)
        {
            //**화재는 RPC 사용하기 때문에 네트워크 인스턴스 생성으로 처리
            yield return new WaitUntil(() => PhotonNetwork.InRoom);

            if (PhotonNetwork.IsMasterClient)
                valveLoc = PhotonNetwork.Instantiate(
                    "Local/ValveLocation/" + ConnManager.instance.ScenarioList.ToString(), 
                    Vector3.zero, 
                    Quaternion.identity, 
                    0);

            yield return new WaitUntil(() => FindObjectOfType<FireScenarioManager>());
        }
        else
        {
            if (Resources.Load<GameObject>("Local/ValveLocation/" + ConnManager.instance.ScenarioList.ToString().Trim()) != null)
            {
                valveLoc = Instantiate(Resources.Load<GameObject>("Local/ValveLocation/" + ConnManager.instance.ScenarioList.ToString().Trim()), valveLocation);
                malFunction.GetComponent<MalFunction>().onOffGameObject.Add(valveLoc);
            }
        }

        #endregion
        //

        fadeState = false;
        HighlightOn = true;
        isReady = false;

        #region Setting Data
        //dataTable = ConnManager.instance.sequenceTable;

        SeqListSetting.instance.Set();
        SeqListSetting.instance.ON(rowIndex);

        dataRow = dataTable.Rows[rowIndex];        //처음 시나리오 값 들고오기
        UIManager.instance.ActorSet(rowIndex);   //UIPanel의 Actor 텍스트 변경해주기
        Is_AlarmSeq();
        TagNoListSetting();
        #endregion

        if (!ConnManager.instance.UseNetwork) //멀티플레이가 아닐때
        {
            UIManager.instance.SequencePanel.SetActive(true);
            StartCoroutine(Process());
        }
        else //멀티플레이 일때
        {
            Move_to_StartPoint(); //처음에 자기 직무 판별해서 스폰 위치 지정해줌.
            RolePlay.instance.MyActorSet();
            UIManager.instance.SequencePanel.SetActive(false);            //처음에 거슬려서 꺼뒀음.
            UIManager.instance.NetinfoPanel.SetActive(true);
        }
    }

    public void TagNoListSetting()
    {
        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            //PC 동작 기능이 아니면서 TagNo가 있는 것
            if (!string.IsNullOrEmpty(dataTable.Rows[i]["TagNo"].ToString()))
            {
                string[] splitTagNo = dataTable.Rows[i]["TagNo"].ToString().Split('/');

                for (int j = 0; j < splitTagNo.Length; j++)
                {
                    // LOCAL 하위에 ValveLocation 그룹에서 하위 트랜스폼 찾기
                    foreach (Transform tag in TagGroup.GetComponentsInChildren<Transform>(true))
                    {
                        if (tag.name.ToString().Trim().Equals(splitTagNo[j].ToString().Trim()))
                        {
                            tagNoList.Add(tag);
                            break;
                        }
                    }
                }
            }

            #region Phone들을 넣어두기 위한 코드 Transform에서 수정 필요

            if (!string.IsNullOrEmpty(dataTable.Rows[i]["MCR_Phone"].ToString()) && !string.IsNullOrEmpty(dataTable.Rows[i]["Local_Phone"].ToString()))
            {
                bool mcrState = false;
                bool localState = false;

                foreach (var pagingPhone in pagingphoneGroup.GetComponentsInChildren<PagingPhoneManager>())
                {
                    Transform transform = pagingPhone.transform.parent;

                    if (!mcrState && transform.name.ToString().Trim().Equals(dataTable.Rows[i]["MCR_Phone"].ToString().Trim()))
                    {
                        phoneList.Add(transform);
                        mcrState = true;
                    }

                    if (!localState && transform.name.ToString().Trim().Equals(dataTable.Rows[i]["Local_Phone"].ToString().Trim()))
                    {
                        phoneList.Add(transform);
                        localState = true;
                    }

                    if (mcrState && localState)
                        break;
                }

            }
            #endregion

        }

    }

    #region 플레이어 텔레포트

    //Player가 이동하는 방법
    public void SetPlayMoveType(PlayMoveType playMoveType)
    {
        this.playMoveType = playMoveType;
    }

    public IEnumerator PlayerTeleportCor(Transform target = null)
    {
        if (target == null)
            target = mcrPositon;

        Vector3 targetBound = target.GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(target) : target.position;
        bool isTelePort = playerObjDistance.DistanceComparison(target, targetBound);

        if (isTelePort)
        {
            StartCoroutine(FadeEffect.instance.OUT());
            yield return new WaitForSeconds(FadeEffect.instance.fadeDuration);
            player.GetComponent<JoystickMove>().enabled = false;
            //player.transform.position = targetBound + target.transform.forward * -1f;

            Transform direction_obj = null;
            Transform customDestination = null;
            //YH - 추가한 부분 2022-08-29
            for (int j = 0; j < target.childCount; j++)
            {
                if (target.GetChild(j).tag == "Direction")
                {
                    direction_obj = target.GetChild(j);
                    break;
                }
                if (target.GetChild(j).name == "CustomDestination")
                {
                    customDestination = target.GetChild(j);
                }
            }

            Vector3 destination = targetBound + Vector3.forward * 1.5f;

            if (direction_obj != null)
                destination = targetBound + direction_obj.forward * 1.5f;
            if (customDestination)
                destination = customDestination.position;

            if (target.Equals(mcrPositon))
            {
                player.transform.eulerAngles = Vector3.zero;
                player.transform.position = targetBound;
            }
            else VRCameraMoveTo(destination, targetBound);

            //바닥 감지하여 플레이어 높이 보정
            playerObjDistance.SetPlayerHeight(targetBound);
            player.GetComponent<JoystickMove>().standardY = player.transform.position.y;
            player.GetComponent<JoystickMove>().enabled = true;
            StartCoroutine(FadeEffect.instance.IN());
            yield return new WaitForSeconds(FadeEffect.instance.fadeDuration);
            yield return fadeState;
        }
    }
    // VR Player의 키, 방크기 등에 상관없이 VR 카메라가 무조건 대상을 바라보도록 보정
    public void VRCameraMoveTo(Vector3 destination, Vector3 lookTarget)
    {
        player.transform.position = destination - Camera.main.transform.localPosition;
        player.transform.LookAt(lookTarget);
        float cameraRoataionY = Camera.main.transform.localEulerAngles.y;
        player.transform.localEulerAngles = new Vector3(0f, player.transform.localEulerAngles.y - cameraRoataionY, 0f);

        player.transform.position = destination-Camera.main.transform.localPosition;
    }

    #endregion 플레이어 텔레포트

    #region 시퀀스

    #region 시퀀스 메인 프로세스

    public IEnumerator Process()
    {
        //바닥 감지하여 플레이어 높이조정
        //playerObjDistance.SetPlayerHeight();

        #region 시작하기 버튼 누르기전까지 안넘어가게 하는 부분 + [YH] Photon을 위해(2022-08-07)
        if (!isStart)
        {
            WaitUntil WaitStart = new WaitUntil(() => isStart);

            PopupManager.instance.PopipStart();
            yield return WaitStart;

            //시작하기전 시뮬레이터 값 세팅해주기
            if (SimulatorManager.instance.isSimulator)
                SimulatorManager.instance.VR_TO_SIMULATOR(dataRow["VR_To_Simulator"].ToString());

        }
        #endregion 시작하기 버튼 누르기전까지 안넘어가게 하는 부분

        WaitUntil waitUntil = new WaitUntil(() => isOn);
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);
        //데이터 개수 만큼 i는 임시 기록
        for (int i = rowIndex; i < dataTable.Rows.Count; i++)
        {
            i = rowIndex;
            Debug.Log("RowIndex : " + rowIndex);

            InitState();        //상태들 초기화해주는 함수
            LookCheck.instance.Init();
            SeqListSetting.instance.Already_TimeCheck();

            #region 0728-MS 텔레포트안에 있던 IsBuilding 확인 후 Local 모델 키는 기능 밖으로 뺌

            string fadeStart = dataRow["isFade"].ToString().Trim();
            if (!fadeStart.Equals("MCR"))
                BuildingOn(dataRow["IsBuilding"].ToString());
            //else BuildingOff();

            #endregion

            #region 0706MS-직무를 수행하기 전 위치를 이동해야하는 지 확인 후 이동
            if (playMoveType.Equals(PlayMoveType.Teleport))
            {
                string location = dataRow["isFade"].ToString().Trim();
                if (location.Equals("MCR")) //이동해야 할 위치가 MCR일 때
                {
                    if (!isMCR) //페이징폰으로 이동하지 않은 경우
                    {
                        if (!string.IsNullOrEmpty(isFadeValue) && !isFadeValue.Equals("MCR")) //직전 위치가 MCR이 아니라면,
                            yield return StartCoroutine(PlayerTeleportCor(mcrPositon));
                    }
                }
                if (!location.Equals("MCR"))
                {
                    string destinationStr = dataRow["TagNo"].ToString().Split('/')[0].Trim();
                    if (!string.IsNullOrEmpty(destinationStr))
                    {
                        Transform destination = tagNoList.Find(x => x.name.Equals(destinationStr));
                        Transform direction_obj = null;
                        //YH - 추가한 부분 2022-08-29
                        for (int j = 0; j < destination.childCount; j++)
                        {
                            if(destination.GetChild(j).tag == "Direction")
                            {
                                direction_obj = destination.GetChild(j);
                                break;
                            }
                        }
                        /////////////////////////////
                        if (destination != null)
                            yield return StartCoroutine(PlayerTeleportCor(destination));
                    }
                }

                yield return fadeState;
                isFadeValue = dataRow["isFade"].ToString().Trim();
            }

            #endregion 0706MS-직무를 수행하기 전 위치를 이동해야하는 지 확인 후 이동

            TimerManager.instance.StartTimer(SequenceTime);
            CheckSequence();
            yield return waitUntil;

            LookCheck.instance.Tracker(false);

            //Simulator에 MalFunction Send
            if (SimulatorManager.instance.isSimulator)
            {
                SimulatorManager.instance.simulatorData = dataRow["VR_To_Simulator"].ToString();
                bool isRandom = dataRow["RandomFunction"].ToString().ToLower().Contains("true");
                if (!string.IsNullOrEmpty(SimulatorManager.instance.simulatorData))
                    SimulatorManager.instance.StartCoroutine(SimulatorManager.instance.VR_TO_SIMULATOR(dataRow["VR_To_Simulator"].ToString()));
                //SimulatorManager.instance.VR_TO_SIMULATOR(SimulatorManager.instance.simulatorData, isRandom);
            }

            //////업무 수행 후 분기 (JumpSeq) 칼럼에 값이 있으면 YES NO팝업 띄우기//////
            if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()) && sequenceType != "99")
            {
                if (dataRow["JumpSeq"].ToString().ToLower().Contains("exit"))
                {
                    yield return new WaitForSeconds(1f);
                    PopupManager.instance.PopupEnd();
                    yield break;
                }

                isOn = false;
                ChooseManager.instance.JumpSeq(dataRow);
                yield return waitUntil; //사용자가 YES or NO 누를 때까지 대기
                // YES or NO 선택에 의해 시나리오 종료가 되어야 하면,
                if (rowIndex == dataTable.Rows.Count - 1)
                {
                    yield return new WaitForSeconds(1f);
                    PopupManager.instance.PopupEnd();
                    yield break;
                }
            }

            yield return StartCoroutine(Actor_Check2());    //해당 시나리오에서 액터 비교해서 페이징폰 기능을 하게하기위한 함수
            yield return StartCoroutine(WaitStepFinish());  //시퀀스가 끝나고 스텝버튼을 누르기 전까지 대기하기위한 코루틴

            if (rowIndex == dataTable.Rows.Count - 1)
                PopupManager.instance.PopupEnd();
            else
                PlayNext();

            yield return waitForSeconds;
        }
        yield return null;
    }
    #endregion 시퀀스 메인 프로세스

    #region 멀티 시퀀스 메인 프로세스
    //멀티 플레이 프로세스
    public IEnumerator MultiProcess()
    {
        WaitUntil WaitPeople = new WaitUntil(() => isJoin);
        //Masterclient가 시작하기 Yes버튼을 눌러야 넘어감.
        yield return WaitPeople;

        #region 시작하기 버튼과 시뮬레이터 데이터 받아오기 전까지 대기하는 부분 + [YH] Photon을 위해(2022-08-07)
        if (!isStart)
        {
            WaitUntil WaitStart = new WaitUntil(() => isStart);
            //마스터가 스타트 버튼을 눌러야 다같이 실행됨.
            PopupManager.instance.PopipStart_Multi();
            yield return WaitStart;

            //Mater가 Simulater가 값을 각 사용자에게 뿌려주기 전까지 대기.
            WaitStart = new WaitUntil(() => IsGetSimulData);

            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("RPC_IsGetSimulData", RpcTarget.MasterClient);    //데이터
            }
            else IsGetSimulData = true;
            yield return WaitStart;
        }
        #endregion 시작하기 버튼 누르기전까지 안넘어가게 하는 부분

        #region 싱크 맞추기 위해서 WaitUntil로 wait Point를 줬다.
        WaitUntil waitUntil = new WaitUntil(() => isOn);    //"한 시퀀스의 직무가 마무리 됨"을 알려주는 변수
        WaitUntil iscall = new WaitUntil(() => isCall);     //"모든 사용자의 전화절차가 마무리됐음"을 알려주는 변수
        WaitUntil R_U_Ready = new WaitUntil(() => isReady); //"각 사용자의 clone 오브젝트 내부에 NetClonePlayer.cs 가 있는데, 자기가 해야할 모든 일이 마무리되어 다음단계로 넘어갈 준비가됨. "IsReady 변수가 true로 바뀜"을 알려주는 변수
        WaitUntil WaitNext = new WaitUntil(() => isNext);   //"모든 사용자가 마지막 wait point로 왔음" 을 알려주는 변수
        WaitUntil WaitJump = new WaitUntil(() => isJump);   //모든 사용자가 점프시퀀스로 왔는지 아닌지 보기위해서
        #endregion

        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        //데이터 개수 만큼 i는 임시 기록
        for (int i = rowIndex; i < dataTable.Rows.Count; i++)
        {
            i = rowIndex;

            Debug.Log("RowIndex : " + rowIndex);

            TimerManager.instance.StartTimer(SequenceTime);

            #region 방장이 초기화해줄때까지 대기
            WaitUntil isInit = new WaitUntil(() => isWaitInit);

            InitState();
            yield return isInit;
            RPC_waitInitState_F();  //초기화 됐으므로, 다시 초기화 해주기
            #endregion

            //알람이 있는 시퀀스에서 Runtime 시간 스타트 해주기 위한 함수
            StartTimePoint(dataRow);
            //자기 턴인지 아닌지 체크한 자기 차례면 진행. 아니면 대기
            if (CheckMyTurn(dataRow))
            {
                #region isBuilding 

                string fadeStart = dataRow["isFade"].ToString().Trim();
                if (!fadeStart.Equals("MCR"))
                    BuildingOn(dataRow["IsBuilding"].ToString());

                //if (!string.IsNullOrEmpty(dataRow["IsBuilding"].ToString()))
                //{
                //    string[] activeBuildingArray = dataRow["IsBuilding"].ToString().Trim().Split('/');
                //    for (int t = 0; t < localBuildingList.Count; t++)
                //    {
                //        if (activeBuildingArray.Contains(localBuildingList[t].name)) //DB에 적혀있는 Building 켜고,
                //            localBuildingList[t].gameObject.SetActive(true);
                //        else localBuildingList[t].gameObject.SetActive(false); //나머지 끄기
                //    }
                //}
                //else
                //{
                //    for (int t = 0; t < localBuildingList.Count; t++) //IsBuilding에 값이 없으면 MCR이라 판단, 빌딩 모두 끄기
                //        localBuildingList[t].gameObject.SetActive(false);
                //}
                #endregion

                #region 0820MS-직무를 수행하기 전 위치를 이동해야하는 지 확인 후 이동
                //MCR 근무자는 이동을 막기 위해 -MS
                if (playMoveType.Equals(PlayMoveType.Teleport) && !dataRow["isFade"].ToString().Trim().Equals("MCR"))
                {
                    if (!string.IsNullOrEmpty(dataRow["TagNo"].ToString()))
                    {
                        string destinationStr = dataRow["TagNo"].ToString().Split('/')[0].Trim();
                        Transform destination = tagNoList.Find(x => x.name.Equals(destinationStr));
                        if (destination != null)
                            yield return StartCoroutine(PlayerTeleportCor(destination));
                    }

                    yield return fadeState;
                }

                #endregion 0820MS-직무를 수행하기 전 위치를 이동해야하는 지 확인 후 이동

                CheckSequence();
                yield return waitUntil;
                LookCheck.instance.Tracker(false);
                Debug.Log(i + " 직무가 진행 완료됨.");

                #region 시뮬레이터 데이터 받아오는 부분
                if (PhotonNetwork.IsMasterClient)
                {
                    PV.RPC("RPC_IsGetSimulData", RpcTarget.MasterClient);    //데이터
                }
                //if (SimulatorManager.instance.isSimulator)
                //{
                //    SimulatorManager.instance.simulatorData = dataRow["VR_To_Simulator"].ToString();
                //    bool isRandom = dataRow["RandomFunction"].ToString().ToLower().Contains("true");
                //    if (!string.IsNullOrEmpty(SimulatorManager.instance.simulatorData))
                //        SimulatorManager.instance.VR_TO_SIMULATOR(SimulatorManager.instance.simulatorData, isRandom);
                //}
                #endregion

                //네트워킹 관련해서 수정되어야함.
                //////업무 수행 후 분기 (JumpSeq) 칼럼에 값이 있으면 YES NO팝업 띄우기//////
                #region Jump 기능 체크해서 실행하는 부분
                if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()))
                {
                    #region Jump할 시퀀스가 Exit 일때 + 단순히 점프만 할때
                    if (!dataRow["JumpSeq"].ToString().Contains("/") && dataRow["JumpSeq"].ToString().ToLower().Contains("exit"))
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }

                    if (dataRow["JumpSeq"].ToString().Split('/').Length > 1)
                    {
                        ChooseManager.instance.JumpSeq(dataRow);    //어차피 내 차례가 아니면 Yes, NO 버튼 클릭 못함.
                        yield return WaitJump;                      //사용자가 YES or NO 누를 때까지 대기
                        if (ChooseManager.instance.isExit)
                        {
                            yield return new WaitForSeconds(1f);
                            PopupManager.instance.PopupEnd();
                            yield break;
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(1f);
                        string jumpnumber = dataRow["JumpSeq"].ToString().Trim();
                        ChooseManager.instance.targetIndex = int.Parse(jumpnumber) - 1;
                        PV.RPC("RPC_IsJump_T", RpcTarget.All);
                    }
                    #endregion

                    #region Jump했을때, 마지막 시퀀스라면
                    if (rowIndex == dataTable.Rows.Count - 1)   // YES or NO 선택에 의해 시나리오 종료가 되어야 하면,
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }
                    #endregion

                }
                #endregion

                yield return StartCoroutine(Multi_Actor_Check());    //해당 시나리오에서 액터 비교해서 페이징폰 기능을 하게하기위한 함수
            }
            else
            {
                yield return waitUntil;

                #region  Jump 기능 체크해서 실행하는 부분
                if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()) )
                {
                    #region Jump할 시퀀스가 Exit 일때 + 단순히 점프만 할때
                    if (!dataRow["JumpSeq"].ToString().Contains("/") && dataRow["JumpSeq"].ToString().ToLower().Contains("exit"))
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }

                    if (dataRow["JumpSeq"].ToString().Split('/').Length > 1)
                    {
                        ChooseManager.instance.JumpSeq(dataRow);    //어차피 내 차례가 아니면 Yes, NO 버튼 클릭 못함.
                        yield return WaitJump;                      //사용자가 YES or NO 누를 때까지 대기
                        if (ChooseManager.instance.isExit)
                        {
                            yield return new WaitForSeconds(1f);
                            PopupManager.instance.PopupEnd();
                            yield break;
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(1f);
                        string jumpnumber = dataRow["JumpSeq"].ToString().Trim();
                        ChooseManager.instance.targetIndex = int.Parse(jumpnumber) - 1;
                        PV.RPC("RPC_IsJump_T", RpcTarget.All);
                    }
                    #endregion

                    #region Jump했을때, 마지막 시퀀스라면
                    if (rowIndex == dataTable.Rows.Count - 1)   // YES or NO 선택에 의해 시나리오 종료가 되어야 하면,
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }
                    #endregion

                }
                #endregion

                yield return iscall;
            }
            yield return StartCoroutine(WaitStepFinish());                          //시퀀스가 끝나고 스텝버튼을 누르기 전까지 대기하기위한 코루틴
            ItsMe.GetComponent<PhotonView>().RPC("RPC_imReady_T", RpcTarget.All);   //"나는 완료 되었음"을 모두에게 알려주기
            yield return R_U_Ready;                     //모든 사용자가 완료 되어 여기서 대기하고 있는지 아닌지 체크하기 위한 부분.

            isReady_reset();

            //다음 시퀀스로 넘어가는거 마스터가 연산처리해줄때까지 기다리게 하기
            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("RPC_NextSeq", RpcTarget.All);
            }

            yield return WaitNext;

            yield return waitForSeconds;

        }

        yield return null;
    }
    #endregion


    #region 초기화 및 다음시퀀스 진행

    [PunRPC]
    public void PlayNext()
    {
        if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()))
        {
            ChooseManager.instance.CheckJumpSeq_Timer(ChooseManager.instance.targetIndex);
            rowIndex = ChooseManager.instance.targetIndex;
            dataRow = dataTable.Rows[rowIndex];
        }
        else dataRow = dataTable.Rows[++rowIndex];

        UIManager.instance.ActorSet(rowIndex);   //UIPanel의 Actor 텍스트 변경해주기
        SeqListSetting.instance.ON(rowIndex);    //지금 버튼 초기화 해주고

        // 다음 시퀀스 시작 전 변경되어야 할 모델 상태가 있으면 변경  --- JM.
        if (!string.IsNullOrEmpty(dataRow["ChangeState_TagNo"].ToString()))
        {
            string[] array = dataRow["ChangeState_TagNo"].ToString().Split('/');
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < tagNoList.Count; j++)
                {
                    if (array[i].Trim().Equals(tagNoList[j].name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Transform tag = tagNoList[j];
                        tag.GetComponent<InitializeModelState>().Set(tag.name);
                        break;
                    }
                }
            }
        }
    }

    #endregion 초기화 및 다음시퀀스 진행

    #region 시퀀스 타입별 진행

    [PunRPC]
    public void InitState()
    {
        if (!ConnManager.instance.UseNetwork)
        {
            IsNextStep = false;
            isOn = false;
            isCall = false;
        }
        else
        {

            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("RPC_IsOn_F", RpcTarget.All);            //isOn = false;
                PV.RPC("RPC_IsCall_F", RpcTarget.All);          //isCall = false;
                PV.RPC("RPC_IsNextStep_F", RpcTarget.All);      //IsNextStep = false;
                PV.RPC("RPC_IsNext_F", RpcTarget.All);          //IsNextStep = false;
                PV.RPC("RPC_isReady_F", RpcTarget.All);         //isReady = false;
                PV.RPC("RPC_IsJump_F", RpcTarget.All);         //isJump = false;
                PV.RPC("RPC_waitInitState", RpcTarget.All);     //
            }
        }
    }

    //[PunRPC]
    private void CheckSequence()
    {
        //IsNextStep = false;
        sequenceType = dataRow["OperatorType"].ToString();
        //PopupManager.instance.PopMessage(dataRow);      //Attention 칼럼에 메세지처리할게 있으면 띄워주기! => 2022-08-17 YH 수정
        //확인한다, 조절한다, 등 모든 타입 적용
        Debug.Log("SequenceType : " + sequenceType);
        switch (sequenceType)
        {
            case "0":
                PopupManager.instance.PopNoticeSeq(dataRow);     //주의 사항으로 팝업 시키기

                break;

            case "1": //확인하다
                PopupManager.instance.Pop(dataRow);     //대리님이 짜신것
                break;

            case "2": //조작한다.
                ControlManager.instance.Rotation(dataRow);
                break;

            case "3": //MCR PC 모니터 관련 확인 작업 시
                CheckManager.instance.ControlMMI(dataRow);
                break;

            case "4":
                //PagingPhone
                CheckManager.instance.ControlMMI(dataRow);
                //Calum으로 "MCR_Phone","Local_Phone"칼럼에 사용할 페이징폰의 이름을 써야함.
                //PopupManager.instance.Pop("페이징폰을 통해 Local에게 직무를 지시하세요! 페이징폰의 수화기를 드세요.");     //대리님이 짜신것
                //수정 => YH : 페이징폰에서 나레이션만 나오게하기. 2022-08-17
                //if (!string.IsNullOrEmpty(dataRow["Narration"].ToString()))
                //    AudioManager.instance.PlayEffAudio("Narration/" + dataRow["Narration"].ToString().Trim());
                //CallManager.instance.PhoneON(dataRow);
                break;

            case "5": // CIM 동작 절차
                //PopupManager.instance.PopMessage(dataRow);
                MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");
                CIMManager.instance.CIM_ON(dataRow);
                break;

            case "6": //대상 바라보아 확인하기
                CheckManager.instance.LookTarget(dataRow);
                break;

            case "8": // 도미노 온도계
                RotationBar.instance.GaugeSetting(dataRow);
                break;

            case "9": // 교체 후 확인
                CheckManager.instance.Replacing(dataRow);
                break;

            case "10":
                ControlManager.instance.Circuit_n_Rotation1(dataRow);
                break;

            case "11":
                ControlManager.instance.Circuit_n_Rotation2(dataRow);
                break;

            case "12":
                ControlManager.instance.Pump_Operation(dataRow);
                break;

            case "13":
                ControlManager.instance.PostitionerValve(dataRow);
                break;

            case "14":
                ControlManager.instance.ValveLever(dataRow);
                break;

            case "15":
                ControlManager.instance.ChaninValve(dataRow);
                break;

            case "16":
                CheckManager.instance.SetObjectEffect(dataRow);
                break;

            case "99": // 절차 건너뛰기 (테스트용)
                IsOn_true();
                break;

            default: //화재
                if (ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_DGControlRoom || ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_KitchenDiningRoom)
                    FireScenarioManager.instance.operatorFunctionPairs[dataRow["OperatorType"].ToString()]();
                else if (ConnManager.instance.ScenarioList == ScenarioTable.MAIN_CRITICAL_ACCIDENT)
                    CriticalManager.instance.operatorFunctionPairs[dataRow["OperatorType"].ToString()]();
                break;
        }
    }

    #endregion 시퀀스 타입별 진행

    #endregion 시퀀스

    #region PagingPhone Part Check 함수

    /// <summary>
    /// 현재시퀀스(recent)와 다음시퀀스(next)의 Actor를 비교해서 MCR -> Local 전환되는 상황일때, PagingPhone 시퀀스 실행하기.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Actor_Check2()
    {
        string recent = dataRow["Actor"].ToString().Trim(); //현재시퀀스 액터정보
        string attention = dataRow["Attention"].ToString().Trim(); //현재시퀀스 주의사항 정보
        string narration = dataRow["Narration"].ToString().Trim(); //현재시퀀스 나레이션 정보
        if (rowIndex == dataTable.Rows.Count - 1)
            yield return null;
        else
        {
            int nextIndex = 0;
            if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()))
                nextIndex = ChooseManager.instance.targetIndex;
            else
                nextIndex = rowIndex + 1;

            string next = dataTable.Rows[nextIndex]["Actor"].ToString().Trim(); //다음시퀀스 액터정보
                                                                                //해당 시퀀스의 Actor가 MCR이고,
            #region MCR => LOCAL 일때 
            if (recent.Equals("RO") || recent.Equals("TO") || recent.Equals("EO") || recent.Equals("STA"))
            {
                //다음 시퀀스의 Actor가 Local이면
                if (!next.Equals("RO") && !next.Equals("TO") && !next.Equals("EO") && !next.Equals("STA"))
                {
                    WaitUntil iscall = new WaitUntil(() => isCall);
                    //Paging폰 시퀀스를 실행하라.
                    CallManager.instance.PhoneON(dataRow);
                    yield return iscall;
                }
            }
            #endregion
            #region LOCAL => MCR 일때
            else //해당 시퀀스의 Actor가 Local이고
            {
                //다음 시퀀스의 Actor가 MCR이면
                if (next.Equals("RO") || next.Equals("TO") || next.Equals("EO") || next.Equals("STA"))
                {
                    WaitUntil iscall = new WaitUntil(() => isCall);

                    if (currentPhonePos != player.transform.position)
                    {
                        currentPhonePos = Vector3.zero;

                        yield return StartCoroutine(FadeEffect.instance.OUT());
                        //Player 위치 이동

                        Transform target = null;
                        if (dataRow["Local_Phone"].ToString().Trim() != null)
                        {
                            for (int i = 0; i < CallManager.instance.LocalPagingPhone.Count; i++)
                            {
                                if (CallManager.instance.LocalPagingPhone[i].name.Equals(dataRow["Local_Phone"].ToString().Trim()))
                                {
                                    target = CallManager.instance.LocalPagingPhone[i].transform;
                                    break;
                                }
                            }

                            Vector3 targetBound;
                            #region door 있는 오브젝트는, Door 쪽으로 이동
                            if (target.transform.parent.name.Contains("Booth_Box"))
                            {
                                try
                                {
                                    GameObject boothDoor = target.transform.parent.Find("Door/Booth_Door/HotSpot_Area").gameObject;
                                    //target = boothDoor.transform;
                                    CallManager.instance.currentDoor = boothDoor.transform.GetComponent<BoothDoor>();
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }
                            #endregion
                            targetBound = target.transform.GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(target.transform) : target.transform.position;

                            Transform direction_obj = null;
                            //YH - 추가한 부분 2022-08-29
                            for (int j = 0; j < target.childCount; j++)
                            {
                                if (target.GetChild(j).tag == "Direction")
                                {
                                    direction_obj = target.GetChild(j);
                                    break;
                                }
                            }

                            Vector3 destination = targetBound + Vector3.forward * 1.5f;

                            if (direction_obj != null)
                                destination = targetBound + direction_obj.forward * 2f;

                            VRCameraMoveTo(destination, targetBound);

                            isMCR = false;
                            playerObjDistance.SetPlayerHeight(targetBound);
                        }

                        yield return StartCoroutine(FadeEffect.instance.IN());
                    }
                   
                    //Paging폰 시퀀스를 실행하라.ew
                    CallManager.instance.PhoneON(dataRow);
                    yield return iscall;
                }

            }
            #endregion
        }

        if (!isCall)
        {
            RPC_FinishPaging();
            yield return new WaitForSeconds(0.5f);
            IsCall_true();
        }
    }

    /**
     * [혼자] 일때 [여러사람]일때를 구분해야한다.
     * 혼자일땐 기존과 똑같이 가야하고
     * 혼자가 아닐땐 통화할 두 사람이 모두 절차를 끝내야 전화 기능은 종료되어야한다.
     */
    private IEnumerator Multi_Actor_Check()
    {
        string recent = dataRow["Actor"].ToString().Trim(); //현재시퀀스 액터정보
        string attention = dataRow["Attention"].ToString().Trim(); //현재시퀀스 주의사항 정보
        string narration = dataRow["Narration"].ToString().Trim(); //현재시퀀스 나레이션 정보

        if (rowIndex == dataTable.Rows.Count - 1)
            yield return null;
        else
        {
            WaitUntil iscall = new WaitUntil(() => isCall);
            //멀티 모드일때
            //if (PlayerList.Count > 1)
            //{
                if (!string.IsNullOrEmpty(dataRow["MCR_Phone"].ToString()))
                {
                    #region 나레이션 시작 
                    if (!string.IsNullOrEmpty(dataRow["Narration"].ToString().Trim()))
                        AudioManager.instance.PlayEffAudio("Narration/" + narration);
                    #endregion
                    PV.RPC("MultiPhoneStart", RpcTarget.All);
                    yield return iscall;
                }
                else
                {
                    RPC_FinishPaging();
                    yield return new WaitForSeconds(0.5f);
                    IsCall_true();
                }
            //}
        }

        if (!isCall)
        {
            RPC_FinishPaging();
            yield return new WaitForSeconds(0.5f);
            IsCall_true();
        }

        yield return null; 
    }

    public void RPC_FinishPaging()
    {
        if (!ConnManager.instance.UseNetwork)
        {
            Finish_Paging();
        }
        else
        {
            PV.RPC("Finish_Paging", RpcTarget.All);
        }
    }

    [PunRPC]
    public void Finish_Paging()
    {
        TimerManager.instance.NoteTime(SequenceTime);  //시퀀스 실행시간 재 설정
        TimerManager.instance.ResetTimer(SequenceTime);  //시퀀스 실행시간 재 설정
        AudioManager.instance.PlayMultiAudio("Sound/", "MP_Pling"); //시퀀스 끝났다는 효과음
    }

    #endregion PagingPhone Part Check 함수


    #region 한 스텝이 마무리되기전에, 스텝버튼을 눌러야 다음 시퀀스로 넘어가도록하는 함수

    /// <summary>
    /// 한 스텝이 마무리되기전에, 스텝버튼을 눌러야 다음 시퀀스로 넘어가도록 한다.
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitStepFinish()
    {
        //현재 시퀀스 stage의 oper(절차) 수
        int operLength = SeqListSetting.instance.Stage_Tasklist_Dic[SeqListSetting.instance.RecentStage].Count;
        int nowSeqNo = SeqListSetting.instance.RowNo_Dic[SeqListSetting.instance.RecentStage];

        int dataIndex = int.Parse(DataManager.instance.GetRowData(dataTable, rowIndex)["RowNo"].ToString());

        if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()) && operLength == 1)
            IsNextStep = true;

        //현재 버그 발견 : 스텝눌러서 시퀀스가 진행되기 전, Operration을 생성하기 전에 눌러져서 잘 받아오지못하는 부분 => 일단 페이징폰쪽에서 시간체크하고 대기하도록 해놨음
        if (dataIndex == (nowSeqNo + operLength - 1))
        {
            WaitUntil waitTime = new WaitUntil(() => IsNextStep);
            UIManager.instance.SequencePanel.SetActive(true);

            //현재 시퀀스의 Operator의 개수가 1개이고, OperatorType이 1 이면,
            if ((dataRow["OperatorType"].ToString().Trim().Equals("1") && operLength == 1))
                IsNextStep = true;

            yield return waitTime;      //지금 여기서 사용자와 마스터 모두 대기해야함.
            AudioManager.instance.PlayMultiAudio("Sound/Select");
            yield return new WaitForSeconds(0.4f);
            AudioManager.instance.PlayMultiAudio("Sound/", "NextStep");

            #region 시퀀스 실행시간 재설정 - UIManager꺼인데, 시퀀스상 여기에 들어갈 수 밖에 없음..ㅠ

            TimerManager.instance.NoteStateTime(SeqListSetting.instance.RecentStage);

            #endregion 시퀀스 실행시간 재설정 - UIManager꺼인데, 시퀀스상 여기에 들어갈 수 밖에 없음..ㅠ

            yield return new WaitForSeconds(1f);
        }

        yield return null;
    }
    #endregion 한 스텝이 마무리되기전에, 스텝버튼을 눌러야 다음 시퀀스로 넘어가도록하는 함수


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (ConnManager.instance.UseNetwork)
            {
                PV.RPC("SkipTo", RpcTarget.All, rowIndex + 1);
            }
            else
            {
                SkipTo(rowIndex + 1);
            }
        }

        if (SteamVR_Actions.default_AButton.GetStateDown(SteamVR_Input_Sources.RightHand) || SteamVR_Actions.default_MenuBtn.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            PopupManager.instance.ExitPopupRequest();
        }


    }
    #region 스킵

    [PunRPC]
    public void SkipTo(int rowIndex)
    {
        StopAllCoroutines();
        isOn = true;
        isMCR = false;
        isStart = true;

        LookCheck.instance.Tracker(false);

        if (FireScenarioManager.instance)
            FireScenarioManager.instance.InitializeState();

        PopupManager.instance.Initialize();
        //ControlManager.instance.RotationInit();
        CheckManager.instance.Initialize();
        ChooseManager.instance.Initialize();
        CallManager.instance.Init();
        AudioManager.instance.StopEffAudio();
        AudioManager.instance.StopMultiAudio();
        FadeEffect.instance.Initialize();
        symbolHighlight.InitializeState();
        Highlight.instance.Off();
        BuildingOff();
        //ChooseManager.instance.CheckJumpSeq_Timer(rowIndex);
        TimerManager.instance.NoteTime(SeqListSetting.instance.Oper_Dic[this.rowIndex + 1], "00 : 00 : 00");
        //TimerManager.instance.NoteStateTime(SeqListSetting.instance.Stage_Dic[this.rowIndex + 1]);

        this.rowIndex = rowIndex;

        dataRow = dataTable.Rows[rowIndex];
        UIManager.instance.ActorSet(rowIndex);
        UIManager.instance.ShowPane();

        SeqListSetting.instance.ON(rowIndex);
        AudioManager.instance.PlayMultiAudio("Sound/", "NextStep");

        if (ConnManager.instance.UseNetwork)
        {
            isJoin = true;
            isStart = true;
            StartCoroutine(MultiProcess());
        }
        else
            StartCoroutine(Process());
    }

    #endregion


    #region RPC Fuction

    [PunRPC]
    public void MultiPhoneStart()
    {
        MultiPagingPhonManager.instance.MultiPhoneSetting();
    }
    [PunRPC]
    public void IsOn_true()
    {
        if (ConnManager.instance.UseNetwork)
            PV.RPC("RPC_IsOn_T", RpcTarget.All);
        else
            this.isOn = true;
    }
    [PunRPC]
    public void IsCall_true()
    {
        if (ConnManager.instance.UseNetwork)
            PV.RPC("RPC_IsCall_T", RpcTarget.All);
        else
            this.isCall = true;
    }

    [PunRPC]
    public void SuccessJoin()
    {
        isJoin = true;
        UIManager.instance.NetinfoPanel.SetActive(false);
    }

    /// <summary>
    /// IsON = true
    /// </summary>
    [PunRPC]
    public void RPC_IsOn_T()
    {
        isOn = true;
    }

    /// <summary>
    /// IsON = false
    /// </summary>
    [PunRPC]
    public void RPC_IsOn_F()
    {
        isOn = false;
    }

    [PunRPC]
    public void RPC_IsCall_T()
    {
        isCall = true;
    }

    [PunRPC]
    public void RPC_IsCall_F()
    {
        isCall = false;
    }

    [PunRPC]
    public void RPC_IsNextStep_T()
    {
        IsNextStep = true;
    }

    [PunRPC]
    public void RPC_IsNextStep_F()
    {
        IsNextStep = false;
    }

    [PunRPC]
    public void RPC_IsNext_T()
    {
        isNext = true;
    }

    [PunRPC]
    public void RPC_IsNext_F()
    {
        isNext = false;
    }

    [PunRPC]
    public void RPC_IsJump_T()
    {
        isJump = true;
    }

    [PunRPC]
    public void RPC_IsJump_F()
    {
        isJump = false;
    }


    [PunRPC]
    public void RPC_NextSeq()
    {

        if (rowIndex == dataTable.Rows.Count - 1)
            PopupManager.instance.PopupEnd();
        else
            PlayNext();

        if (PhotonNetwork.IsMasterClient)
            PV.RPC("RPC_IsNext_T", RpcTarget.All);
    }

    /// <summary>
    /// 다같이 Main Process를 실행시키기 위해서
    /// </summary>
    [PunRPC]
    public void RPC_StartMainProcess()
    {
        StartCoroutine(MultiProcess());
    }


    [PunRPC]
    public void RPC_StartButton()
    {
        isStart = true;
        PopupManager.instance.startPanel.SetActive(false);
    }

    [PunRPC]
    public void RPC_waitInitState()
    {
        isWaitInit = true;
    }

    [PunRPC]
    public void RPC_waitInitState_F()
    {
        isWaitInit = false;
    }

    [PunRPC]
    public void RPC_IsGetSimulData()
    {
        if (SimulatorManager.instance.isSimulator)
        {
            //SimulatorManager.instance.VR_TO_SIMULATOR(dataRow["VR_To_Simulator"].ToString());
            SimulatorManager.instance.StartCoroutine(SimulatorManager.instance.VR_TO_SIMULATOR(dataRow["VR_To_Simulator"].ToString()));
            IsGetSimulData = true;
        }
        else
        {
            IsGetSimulData = true;
        }
    }

    //[PunRPC]
    public bool CheckMyTurn(DataRow datarow)
    {
        //현재 시퀀스의 Datarow를 가져와서
        string[] nowActor = datarow["Actor"].ToString().Trim().Split('/');

        Role[] myActor = new Role[nowActor.Length];

        for (int j = 0; j < nowActor.Length; j++)
        {
            foreach (Role _role in Enum.GetValues(typeof(Role)))
            {
                if (_role.ToString().Equals(nowActor[j].Trim()))
                {
                    myActor[j] = _role;
                    break;
                }
            }
        }

        //내 차례가 맞는지 아닌지 판별한다.
        for (int i = 0; i < myActor.Length; i++)
        {
            foreach (GameObject Player in playerKey_Actor[myActor[i]])
            {
                if(Player.GetComponent<PhotonView>().IsMine)
                    return true;    //맞으면 turue
            }    
        }

        if (playerKey_Actor.ContainsKey(Role.All))
        {
            //모든 직업을 가지고 있는 사람(Role.All)인 사람들 모두 True
            foreach (GameObject Player in playerKey_Actor[Role.All])
            {
                if (Player.GetComponent<PhotonView>().IsMine)
                    return true;    //맞으면 turue
            }
        }


        return false;     //아니면 false
    }

    public void Is_All_Ready()
    {

        foreach (GameObject player in PlayerList)
        {
            if (!player.GetComponent<NetClonePlayer>().imReady)
            {
                PV.RPC("RPC_isReady_F", RpcTarget.All);
                return;
            }
        }

        //다 reday 상태면 true;
        PV.RPC("RPC_isReady_T", RpcTarget.All);

    }

    [PunRPC]
    public void RPC_isReady_T()
    {
        isReady = true;
    }

    [PunRPC]
    public void RPC_isReady_F()
    {
        isReady = false;
    }

    //[PunRPC]
    public void isReady_reset()
    {
        ItsMe.GetComponent<NetClonePlayer>().RPC_imReady_F();
    }

    /// <summary>
    /// Local 사용자 스폰지역 설정해주는 함수
    /// </summary>
    public void Move_to_StartPoint()
    {
        //내 직업이 LOCAL 이면(* MCR 이면 이동 x)
        //현장에서 태어난다.
        Role[] tmp = ConnManager.instance.myActor;

        //1. Local 직무이면
        if (!tmp.Contains(Role.All) && !tmp.Contains(Role.RO) && !tmp.Contains(Role.TO) && !tmp.Contains(Role.EO) && !tmp.Contains(Role.STA)) 
        {
            Role firstRole;
            //2. 나의 현장 첫번째 직업을 찾아서
            for (int i = 0; i < tmp.Length; i++)
            {
                firstRole = tmp[i];

                //3. 데이터 테이블에서 첫번째 LOCAL Pagingphone을 찾아서 그쪽 앞으로 이동
                for (int j = 0; j < dataTable.Rows.Count; j++)
                {
                    //4. 내 직무와 같은 시퀀스이고, 이동할 PagingPhone 값이 있으면 페이징폰 찾아서 이동 함수 넣기
                    if (/*(firstRole.ToString() == dataTable.Rows[j]["Actor"].ToString().Trim()) && */!string.IsNullOrEmpty(dataTable.Rows[j]["Local_Phone"].ToString()))
                    {
                        //페이징폰 찾아!
                        foreach (Transform phone in phoneList)
                        {
                            //찾았으면 텔레포트 + Room도 켜주자!
                            if (phone.gameObject.name == dataTable.Rows[j]["Local_Phone"].ToString())
                            {

                                #region Local 룸 켜주기
                                BuildingOn(dataTable.Rows[j]["IsBuilding"].ToString());
                                #endregion

                                Transform target = phone;
                                Vector3 targetBound = target.GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(target) : target.position;
                                Vector3 destination = targetBound + Vector3.forward * 1.5f;

                                Transform direction_obj = null;
                                //YH - 추가한 부분 2022-08-29
                                for (int k = 0; j < target.childCount; k++)
                                {
                                    if (target.GetChild(k).tag == "Direction")
                                    {
                                        direction_obj = target.GetChild(k);
                                        break;
                                    }
                                }

                                if (direction_obj != null)
                                    destination = targetBound + direction_obj.forward * 1.5f;

                                VRCameraMoveTo(destination, targetBound);

                                //바닥 감지하여 플레이어 높이 보정
                                playerObjDistance.SetPlayerHeight(targetBound);

                                return;
                            }
                        }

                        Debug.LogError("Local을 못찾은거니깐, TagName 한번 체크해봐!");
                        return;
                    }
                }
                break;
            }
        }
        else
        {
            //MCR 직무가 있으면, 이동 X
            VRCameraMoveTo(mcrPositon.transform.position, TagGroup.Find("LDP_LookPoint").transform.position);
            //player.transform.LookAt(TagGroup.Find("LDP_LookPoint").transform);
            //Vector3 vec3 = player.transform.localEulerAngles;
            //player.transform.localEulerAngles = new Vector3(vec3.x, 0f, vec3.z);

            return;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(rowIndex);
        else rowIndex = (int)stream.ReceiveNext();
    }

    #endregion

    #region 현장 모델 관련 함수

    public void BuildingOn(string building)
    {
        if (!string.IsNullOrEmpty(building))
        {
            string[] activeBuildingArray = building.Trim().Split('/');
            for (int t = 0; t < localBuildingList.Count; t++)
            {
                if (activeBuildingArray.Contains(localBuildingList[t].name)) //DB에 적혀있는 Building 켜고,
                    localBuildingList[t].gameObject.SetActive(true);
                else localBuildingList[t].gameObject.SetActive(false); //나머지 끄기
            }
            foreach(var mmi in RolePlay.instance.mmi)
            {
                mmi.SetActive(false);
            }
        }
    }

    public void BuildingOff()
    {
        for (int t = 0; t < localBuildingList.Count; t++)
            localBuildingList[t].gameObject.SetActive(false);
        foreach (var mmi in RolePlay.instance.mmi)
        {
            mmi.SetActive(true);
        }
    }

    public GameObject acknowledgeBtn;
    public AudioSource warningSource;
    public void OnPressAcknowledge()
    {
        //알람 경보음 끄기
        //acknowledgeBtn.SetActive(false);
        foreach (var ackImage in acknowledgeBtn.GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            ackImage.enabled = false;
        }
        warningSource.Stop();
        //warningSource.gameObject.SetActive(false);
        var audioAlarms = NA.Alarm.Instances.Instance.ActiveAlarmList;
        for (int i = audioAlarms.Count-1;i>=0;i--)
        {
            audioAlarms[i].OnClickAck();
        }
    }
    public void ActivateAcknowledge()
    {
        foreach(var ackImage in acknowledgeBtn.GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            ackImage.enabled = true;
        }
        //acknowledgeBtn.SetActive(true);
    }
    #endregion

    //2022-12-05 YH
    public void StartTimePoint(DataRow row)
    {
        if (!TimerManager.instance.Done_RunningTime)
        {
            try
            {
                string isalarm = row["AlarmStartPoint"].ToString().Trim();
                if (!string.IsNullOrEmpty(isalarm))
                {
                    TimerManager.instance.StartTimer(PlayTime);
                    TimerManager.instance.Done_RunningTime = true;
                }
            }
            catch (Exception)
            {
                Debug.Log("현재 DB에는 AlarmStartPoint 칼럼을 추가하지 않았습니다.");
            }
        }
    }
    bool IsAlarmSeq = false;

    /// <summary>
    /// 알람이 있는 시나리오에서 특별히 시간을 스타트 해주기 위해서, 있다|없다 를 판별하는 IsAlarmSeq bool형 변수를 두기위해 만든 함수
    /// </summary>
    void Is_AlarmSeq()
    {
        int end = 0;
        for (end = 0; end < dataTable.Rows.Count; end++)
        {
            try
            {
                string IsAlarm = dataTable.Rows[end]["AlarmStartPoint"].ToString().Trim();
                if (!string.IsNullOrEmpty(IsAlarm))
                {
                    IsAlarmSeq = true;
                    break;
                }
            }
            catch (Exception)
            {
                Debug.Log("현재 DB에는 AlarmStartPoint 칼럼을 추가하지 않았습니다.");
            }

        }
        
        if(end == dataTable.Rows.Count)
        {
            IsAlarmSeq = true;
            TimerManager.instance.StartTimer(PlayTime);
            TimerManager.instance.Done_RunningTime = true;
        }
    }

}