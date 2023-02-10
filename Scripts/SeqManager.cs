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
    MAIN_RCS_Charging_Pump,                               //���������� �̿��� RCS ���
    MAIN_RCS_Dynamic_Vent,                                //������� �� �������
    MAIN_RCS_Initial_Preparation,                         //RCS ������ ���� �غ�
    MAIN_RCS_Shutdown_Cooling_Pump,                       //�����ð����� 01A�� �̿��� ���
    MAIN_CONDENSER_Turbine_Seal_Steam,                    //��������������-�ͺ�к����� ����з� ����
    MAIN_CONDENSER_CWP_Abnormal_Stop,                     //��������������-�������� ��ȯ�� ���� ������ ����
    MAIN_CONDENSER_Vacuum_Destruction_Valve,              //��������������-������ ���� �ı���� ������ ����
    MAIN_CONDENSER_Exhaust_Valve_Abnormal_Open,           //��������������-����� ������ ����
    MAIN_CONDENSER_Insufficient_Supply_Of_ShaftSealWater, //��������������-�������� ����� �� �к��� ���޺���
    MAIN_CONDENSER_Other_Causes,                          //��������������-��Ÿ����
    MAIN_FIRE_DGControlRoom,                              //DG Control Panel Room ȭ��
    MAIN_FIRE_KitchenDiningRoom,                          //Kitchen & Dining Room ȭ��
    MAIN_CRITICAL_ACCIDENT,                               //�ߴ���
    IMAGE_LOSS_Of_RCP_CCW_Sealed_Injection_Water,         //RCP ���� CCW �� �к����Լ� ���û��
    IMAGE_ABNORMAL_CLOSE_RCP_IsolationValve,              //RCP ��������� �ݸ���� ������ ����
    IMAGE_ABNORMAL_OPENED_SBCV,                           //SBCV ������ ����
    IMAGE_ABNORMAL_OPENED_Pressurizer_SprayValve,         //���б� ������ ���� ����
    IMAGE_STICKING_Of_WaterLevel_ControlValve,            //�������� �к��� ������ũ ���������� �������
    IMAGE_LEAKAGE_CondenserPipeLine,                      //������� ����
    IMAGE_LOSS_Of_SealedWater_Of_VacuumPump,              //�������� ������ �������� �к��� ���
    IMAGE_ABNORMAL_OPENED_SpilledWater_ControlValve,      //����� ���������(CV-201PQ) ������ ����
    IMAGE_CCW_TemperatureControl,                         //����� ����ȯ�� �µ������ ����� CCW �µ�����
    IMAGE_ABNORMAL_VALVE_Of_Low_Pressure_Turbine,         //�����ͺ� ����ĵ� ������ ������
    IMAGE_ABNORMAL_SG_WaterLevel_Control_System,          //����߻��� ����������� ������
    IMAGE_Control_Tank_Level_Transmitter_FailLo_226,      //ü��������ũ �������۱� CV-LT-226 Fail Lo
    IMAGE_Control_Tank_Level_Transmitter_FailLo_227,      //ü��������ũ �������۱� CV-LT-227 Fail Lo (��ȭ)
    IMAGE_FAILURE_Control_Tank_Inlet_Valve_PHIX_VCT,      //ü��������ũ �Ա���� ���� (PHIX-VCT)
    IMAGE_FAILURE_Control_Tank_Inlet_Valve_VCT_PHIX,      //ü��������ũ �Ա���� ���� (VCT-PHIX)
    IMAGE_ABNORMAL_OPENED_Charging_Flow_Control_Valve,    //�������� ������ CV-V212PQ ������ ����
    IMAGE_MANUAL_OPERATION_Of_Hydraulic_Drive_System,     //���б�����ġ ��������
    IMAGE_TURBINE_TRIP                                    //�ͺ�Ʈ��
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
    public bool isOn;                   //���� �������� �Ѿ�� ����ϴ� ����
    public bool isCall;                 //���� �������� �Ѿ�� ����ϴ� ����
    public bool IsNextStep;             //UI�� ������ ���� ������ �Ѿ�� �ϱ� ���� bool ����
    public bool isStart;        //ó�� ���ۿ��� Ȯ�� ��ư�� ������ ���� �������� �Ѿ�� �ϱ� ���� ����
    public bool HighlightOn; //����,�ǽ� ��带 �������� ���� ������ ���̶���Ʈ�� ǥ�������� ������ ���� ���� ����
    public bool fadeState;
    public int TimeAttack = 3000; //30���϶�
    private string isFadeValue = ""; //Player isFade�� ��� ���� ��ġ�� ���� ��ġ�� ���ϱ� ����
    public Vector3 currentPhonePos;
    [System.NonSerialized] public DataRow dataRow;
    public DataTable dataTable { get; set; }
    public string sequenceTableName { get; set; }
    public string sequenceType { get; set; }
    public int rowIndex { get; set; }                    //�ó����� ����
    public int sequenceCount { get; set; }

    public Transform TagGroup;
    public Transform pagingphoneGroup;

    public GameObject PlayTime;
    public GameObject SequenceTime;
    //MS
    public static List<Transform> tagNoList; //3�� Ÿ�� PC�� �ش��ϴ� TagNo�� ������ ��� TagNo�� �̸��� ��ġ�ϴ� obj �ֱ�����
    public static List<Transform> phoneList; //3�� Ÿ�� PC�� �ش��ϴ� TagNo�� ������ ��� TagNo�� �̸��� ��ġ�ϴ� obj �ֱ�����
    Dictionary<string, GameObject> buildDic;
    Dictionary<string, GameObject> pagingPhoneDic;
    public Transform malFunction;
    GameObject valveLoc;

    [Header("Resource Prefabs�� ���� ��ġ")]
    [SerializeField] Transform local;
    [SerializeField] Transform pagingPhone_Location;
    [SerializeField] Transform valveLocation;

    public GameObject player { get; set; } //Play ������Ʈ ��Ƶα�
    public GameObject LeftHand; //Play ������Ʈ ��Ƶα�
    public GameObject RightHand; //Play ������Ʈ ��Ƶα�
    public Transform mcrPositon; //MCR ��ġ�� Fade�� �ش� Transform���� �̵���
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

    #region ��Ƽ �÷��̸� ���� Fields
    [Header("Player Info List")]
    public Dictionary<Role, List<GameObject>> playerKey_Actor = new Dictionary<Role, List<GameObject>>();
    public List<GameObject> PlayerList = new List<GameObject>();        //���ӳ��� ���� ����ڵ� List�� ��Ƶδ� ����
    public GameObject ItsMe;                                            //��Ƽ�� ���� ������ ���� Player
    bool isJoin = false;                                                //��� �����ߴ��� ���ߴ��� ��Ÿ���ֱ����� ����
    public bool IsGetSimulData = false;
    bool isWaitInit = false;
    public PhotonView PV;
    bool isNext = false;
    bool isReady = false;
    public bool isJump { get; set; }
    #endregion ��Ƽ �÷��̸� ���� Fields

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
        //����Ʈ �̵� ����� �ڷ���Ʈ
        SetPlayMoveType(PlayMoveType.Teleport);
        PV = transform.GetComponent<PhotonView>();
        foreach (var ackbtn in acknowledgeBtn.GetComponentsInChildren<PressTriggerEvent>())
            ackbtn.AddHanddPressListner(delegate { OnPressAcknowledge(); });

        #region �� ���̶�Ű ����

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
        #region �� ���̶�Ű ����

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            //1. Build ����
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

            //2. PagingPhone ����
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

        //3. ValveLocation ����
        if (ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_KitchenDiningRoom 
            || ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_DGControlRoom)
        {
            //**ȭ��� RPC ����ϱ� ������ ��Ʈ��ũ �ν��Ͻ� �������� ó��
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

        dataRow = dataTable.Rows[rowIndex];        //ó�� �ó����� �� ������
        UIManager.instance.ActorSet(rowIndex);   //UIPanel�� Actor �ؽ�Ʈ �������ֱ�
        Is_AlarmSeq();
        TagNoListSetting();
        #endregion

        if (!ConnManager.instance.UseNetwork) //��Ƽ�÷��̰� �ƴҶ�
        {
            UIManager.instance.SequencePanel.SetActive(true);
            StartCoroutine(Process());
        }
        else //��Ƽ�÷��� �϶�
        {
            Move_to_StartPoint(); //ó���� �ڱ� ���� �Ǻ��ؼ� ���� ��ġ ��������.
            RolePlay.instance.MyActorSet();
            UIManager.instance.SequencePanel.SetActive(false);            //ó���� �Ž����� ������.
            UIManager.instance.NetinfoPanel.SetActive(true);
        }
    }

    public void TagNoListSetting()
    {
        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            //PC ���� ����� �ƴϸ鼭 TagNo�� �ִ� ��
            if (!string.IsNullOrEmpty(dataTable.Rows[i]["TagNo"].ToString()))
            {
                string[] splitTagNo = dataTable.Rows[i]["TagNo"].ToString().Split('/');

                for (int j = 0; j < splitTagNo.Length; j++)
                {
                    // LOCAL ������ ValveLocation �׷쿡�� ���� Ʈ������ ã��
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

            #region Phone���� �־�α� ���� �ڵ� Transform���� ���� �ʿ�

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

    #region �÷��̾� �ڷ���Ʈ

    //Player�� �̵��ϴ� ���
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
            //YH - �߰��� �κ� 2022-08-29
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

            //�ٴ� �����Ͽ� �÷��̾� ���� ����
            playerObjDistance.SetPlayerHeight(targetBound);
            player.GetComponent<JoystickMove>().standardY = player.transform.position.y;
            player.GetComponent<JoystickMove>().enabled = true;
            StartCoroutine(FadeEffect.instance.IN());
            yield return new WaitForSeconds(FadeEffect.instance.fadeDuration);
            yield return fadeState;
        }
    }
    // VR Player�� Ű, ��ũ�� � ������� VR ī�޶� ������ ����� �ٶ󺸵��� ����
    public void VRCameraMoveTo(Vector3 destination, Vector3 lookTarget)
    {
        player.transform.position = destination - Camera.main.transform.localPosition;
        player.transform.LookAt(lookTarget);
        float cameraRoataionY = Camera.main.transform.localEulerAngles.y;
        player.transform.localEulerAngles = new Vector3(0f, player.transform.localEulerAngles.y - cameraRoataionY, 0f);

        player.transform.position = destination-Camera.main.transform.localPosition;
    }

    #endregion �÷��̾� �ڷ���Ʈ

    #region ������

    #region ������ ���� ���μ���

    public IEnumerator Process()
    {
        //�ٴ� �����Ͽ� �÷��̾� ��������
        //playerObjDistance.SetPlayerHeight();

        #region �����ϱ� ��ư ������������ �ȳѾ�� �ϴ� �κ� + [YH] Photon�� ����(2022-08-07)
        if (!isStart)
        {
            WaitUntil WaitStart = new WaitUntil(() => isStart);

            PopupManager.instance.PopipStart();
            yield return WaitStart;

            //�����ϱ��� �ùķ����� �� �������ֱ�
            if (SimulatorManager.instance.isSimulator)
                SimulatorManager.instance.VR_TO_SIMULATOR(dataRow["VR_To_Simulator"].ToString());

        }
        #endregion �����ϱ� ��ư ������������ �ȳѾ�� �ϴ� �κ�

        WaitUntil waitUntil = new WaitUntil(() => isOn);
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);
        //������ ���� ��ŭ i�� �ӽ� ���
        for (int i = rowIndex; i < dataTable.Rows.Count; i++)
        {
            i = rowIndex;
            Debug.Log("RowIndex : " + rowIndex);

            InitState();        //���µ� �ʱ�ȭ���ִ� �Լ�
            LookCheck.instance.Init();
            SeqListSetting.instance.Already_TimeCheck();

            #region 0728-MS �ڷ���Ʈ�ȿ� �ִ� IsBuilding Ȯ�� �� Local �� Ű�� ��� ������ ��

            string fadeStart = dataRow["isFade"].ToString().Trim();
            if (!fadeStart.Equals("MCR"))
                BuildingOn(dataRow["IsBuilding"].ToString());
            //else BuildingOff();

            #endregion

            #region 0706MS-������ �����ϱ� �� ��ġ�� �̵��ؾ��ϴ� �� Ȯ�� �� �̵�
            if (playMoveType.Equals(PlayMoveType.Teleport))
            {
                string location = dataRow["isFade"].ToString().Trim();
                if (location.Equals("MCR")) //�̵��ؾ� �� ��ġ�� MCR�� ��
                {
                    if (!isMCR) //����¡������ �̵����� ���� ���
                    {
                        if (!string.IsNullOrEmpty(isFadeValue) && !isFadeValue.Equals("MCR")) //���� ��ġ�� MCR�� �ƴ϶��,
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
                        //YH - �߰��� �κ� 2022-08-29
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

            #endregion 0706MS-������ �����ϱ� �� ��ġ�� �̵��ؾ��ϴ� �� Ȯ�� �� �̵�

            TimerManager.instance.StartTimer(SequenceTime);
            CheckSequence();
            yield return waitUntil;

            LookCheck.instance.Tracker(false);

            //Simulator�� MalFunction Send
            if (SimulatorManager.instance.isSimulator)
            {
                SimulatorManager.instance.simulatorData = dataRow["VR_To_Simulator"].ToString();
                bool isRandom = dataRow["RandomFunction"].ToString().ToLower().Contains("true");
                if (!string.IsNullOrEmpty(SimulatorManager.instance.simulatorData))
                    SimulatorManager.instance.StartCoroutine(SimulatorManager.instance.VR_TO_SIMULATOR(dataRow["VR_To_Simulator"].ToString()));
                //SimulatorManager.instance.VR_TO_SIMULATOR(SimulatorManager.instance.simulatorData, isRandom);
            }

            //////���� ���� �� �б� (JumpSeq) Į���� ���� ������ YES NO�˾� ����//////
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
                yield return waitUntil; //����ڰ� YES or NO ���� ������ ���
                // YES or NO ���ÿ� ���� �ó����� ���ᰡ �Ǿ�� �ϸ�,
                if (rowIndex == dataTable.Rows.Count - 1)
                {
                    yield return new WaitForSeconds(1f);
                    PopupManager.instance.PopupEnd();
                    yield break;
                }
            }

            yield return StartCoroutine(Actor_Check2());    //�ش� �ó��������� ���� ���ؼ� ����¡�� ����� �ϰ��ϱ����� �Լ�
            yield return StartCoroutine(WaitStepFinish());  //�������� ������ ���ܹ�ư�� ������ ������ ����ϱ����� �ڷ�ƾ

            if (rowIndex == dataTable.Rows.Count - 1)
                PopupManager.instance.PopupEnd();
            else
                PlayNext();

            yield return waitForSeconds;
        }
        yield return null;
    }
    #endregion ������ ���� ���μ���

    #region ��Ƽ ������ ���� ���μ���
    //��Ƽ �÷��� ���μ���
    public IEnumerator MultiProcess()
    {
        WaitUntil WaitPeople = new WaitUntil(() => isJoin);
        //Masterclient�� �����ϱ� Yes��ư�� ������ �Ѿ.
        yield return WaitPeople;

        #region �����ϱ� ��ư�� �ùķ����� ������ �޾ƿ��� ������ ����ϴ� �κ� + [YH] Photon�� ����(2022-08-07)
        if (!isStart)
        {
            WaitUntil WaitStart = new WaitUntil(() => isStart);
            //�����Ͱ� ��ŸƮ ��ư�� ������ �ٰ��� �����.
            PopupManager.instance.PopipStart_Multi();
            yield return WaitStart;

            //Mater�� Simulater�� ���� �� ����ڿ��� �ѷ��ֱ� ������ ���.
            WaitStart = new WaitUntil(() => IsGetSimulData);

            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("RPC_IsGetSimulData", RpcTarget.MasterClient);    //������
            }
            else IsGetSimulData = true;
            yield return WaitStart;
        }
        #endregion �����ϱ� ��ư ������������ �ȳѾ�� �ϴ� �κ�

        #region ��ũ ���߱� ���ؼ� WaitUntil�� wait Point�� ���.
        WaitUntil waitUntil = new WaitUntil(() => isOn);    //"�� �������� ������ ������ ��"�� �˷��ִ� ����
        WaitUntil iscall = new WaitUntil(() => isCall);     //"��� ������� ��ȭ������ ����������"�� �˷��ִ� ����
        WaitUntil R_U_Ready = new WaitUntil(() => isReady); //"�� ������� clone ������Ʈ ���ο� NetClonePlayer.cs �� �ִµ�, �ڱⰡ �ؾ��� ��� ���� �������Ǿ� �����ܰ�� �Ѿ �غ񰡵�. "IsReady ������ true�� �ٲ�"�� �˷��ִ� ����
        WaitUntil WaitNext = new WaitUntil(() => isNext);   //"��� ����ڰ� ������ wait point�� ����" �� �˷��ִ� ����
        WaitUntil WaitJump = new WaitUntil(() => isJump);   //��� ����ڰ� ������������ �Դ��� �ƴ��� �������ؼ�
        #endregion

        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        //������ ���� ��ŭ i�� �ӽ� ���
        for (int i = rowIndex; i < dataTable.Rows.Count; i++)
        {
            i = rowIndex;

            Debug.Log("RowIndex : " + rowIndex);

            TimerManager.instance.StartTimer(SequenceTime);

            #region ������ �ʱ�ȭ���ٶ����� ���
            WaitUntil isInit = new WaitUntil(() => isWaitInit);

            InitState();
            yield return isInit;
            RPC_waitInitState_F();  //�ʱ�ȭ �����Ƿ�, �ٽ� �ʱ�ȭ ���ֱ�
            #endregion

            //�˶��� �ִ� ���������� Runtime �ð� ��ŸƮ ���ֱ� ���� �Լ�
            StartTimePoint(dataRow);
            //�ڱ� ������ �ƴ��� üũ�� �ڱ� ���ʸ� ����. �ƴϸ� ���
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
                //        if (activeBuildingArray.Contains(localBuildingList[t].name)) //DB�� �����ִ� Building �Ѱ�,
                //            localBuildingList[t].gameObject.SetActive(true);
                //        else localBuildingList[t].gameObject.SetActive(false); //������ ����
                //    }
                //}
                //else
                //{
                //    for (int t = 0; t < localBuildingList.Count; t++) //IsBuilding�� ���� ������ MCR�̶� �Ǵ�, ���� ��� ����
                //        localBuildingList[t].gameObject.SetActive(false);
                //}
                #endregion

                #region 0820MS-������ �����ϱ� �� ��ġ�� �̵��ؾ��ϴ� �� Ȯ�� �� �̵�
                //MCR �ٹ��ڴ� �̵��� ���� ���� -MS
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

                #endregion 0820MS-������ �����ϱ� �� ��ġ�� �̵��ؾ��ϴ� �� Ȯ�� �� �̵�

                CheckSequence();
                yield return waitUntil;
                LookCheck.instance.Tracker(false);
                Debug.Log(i + " ������ ���� �Ϸ��.");

                #region �ùķ����� ������ �޾ƿ��� �κ�
                if (PhotonNetwork.IsMasterClient)
                {
                    PV.RPC("RPC_IsGetSimulData", RpcTarget.MasterClient);    //������
                }
                //if (SimulatorManager.instance.isSimulator)
                //{
                //    SimulatorManager.instance.simulatorData = dataRow["VR_To_Simulator"].ToString();
                //    bool isRandom = dataRow["RandomFunction"].ToString().ToLower().Contains("true");
                //    if (!string.IsNullOrEmpty(SimulatorManager.instance.simulatorData))
                //        SimulatorManager.instance.VR_TO_SIMULATOR(SimulatorManager.instance.simulatorData, isRandom);
                //}
                #endregion

                //��Ʈ��ŷ �����ؼ� �����Ǿ����.
                //////���� ���� �� �б� (JumpSeq) Į���� ���� ������ YES NO�˾� ����//////
                #region Jump ��� üũ�ؼ� �����ϴ� �κ�
                if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()))
                {
                    #region Jump�� �������� Exit �϶� + �ܼ��� ������ �Ҷ�
                    if (!dataRow["JumpSeq"].ToString().Contains("/") && dataRow["JumpSeq"].ToString().ToLower().Contains("exit"))
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }

                    if (dataRow["JumpSeq"].ToString().Split('/').Length > 1)
                    {
                        ChooseManager.instance.JumpSeq(dataRow);    //������ �� ���ʰ� �ƴϸ� Yes, NO ��ư Ŭ�� ����.
                        yield return WaitJump;                      //����ڰ� YES or NO ���� ������ ���
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

                    #region Jump������, ������ ���������
                    if (rowIndex == dataTable.Rows.Count - 1)   // YES or NO ���ÿ� ���� �ó����� ���ᰡ �Ǿ�� �ϸ�,
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }
                    #endregion

                }
                #endregion

                yield return StartCoroutine(Multi_Actor_Check());    //�ش� �ó��������� ���� ���ؼ� ����¡�� ����� �ϰ��ϱ����� �Լ�
            }
            else
            {
                yield return waitUntil;

                #region  Jump ��� üũ�ؼ� �����ϴ� �κ�
                if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()) )
                {
                    #region Jump�� �������� Exit �϶� + �ܼ��� ������ �Ҷ�
                    if (!dataRow["JumpSeq"].ToString().Contains("/") && dataRow["JumpSeq"].ToString().ToLower().Contains("exit"))
                    {
                        yield return new WaitForSeconds(1f);
                        PopupManager.instance.PopupEnd();
                        yield break;
                    }

                    if (dataRow["JumpSeq"].ToString().Split('/').Length > 1)
                    {
                        ChooseManager.instance.JumpSeq(dataRow);    //������ �� ���ʰ� �ƴϸ� Yes, NO ��ư Ŭ�� ����.
                        yield return WaitJump;                      //����ڰ� YES or NO ���� ������ ���
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

                    #region Jump������, ������ ���������
                    if (rowIndex == dataTable.Rows.Count - 1)   // YES or NO ���ÿ� ���� �ó����� ���ᰡ �Ǿ�� �ϸ�,
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
            yield return StartCoroutine(WaitStepFinish());                          //�������� ������ ���ܹ�ư�� ������ ������ ����ϱ����� �ڷ�ƾ
            ItsMe.GetComponent<PhotonView>().RPC("RPC_imReady_T", RpcTarget.All);   //"���� �Ϸ� �Ǿ���"�� ��ο��� �˷��ֱ�
            yield return R_U_Ready;                     //��� ����ڰ� �Ϸ� �Ǿ� ���⼭ ����ϰ� �ִ��� �ƴ��� üũ�ϱ� ���� �κ�.

            isReady_reset();

            //���� �������� �Ѿ�°� �����Ͱ� ����ó�����ٶ����� ��ٸ��� �ϱ�
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


    #region �ʱ�ȭ �� ���������� ����

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

        UIManager.instance.ActorSet(rowIndex);   //UIPanel�� Actor �ؽ�Ʈ �������ֱ�
        SeqListSetting.instance.ON(rowIndex);    //���� ��ư �ʱ�ȭ ���ְ�

        // ���� ������ ���� �� ����Ǿ�� �� �� ���°� ������ ����  --- JM.
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

    #endregion �ʱ�ȭ �� ���������� ����

    #region ������ Ÿ�Ժ� ����

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
        //PopupManager.instance.PopMessage(dataRow);      //Attention Į���� �޼���ó���Ұ� ������ ����ֱ�! => 2022-08-17 YH ����
        //Ȯ���Ѵ�, �����Ѵ�, �� ��� Ÿ�� ����
        Debug.Log("SequenceType : " + sequenceType);
        switch (sequenceType)
        {
            case "0":
                PopupManager.instance.PopNoticeSeq(dataRow);     //���� �������� �˾� ��Ű��

                break;

            case "1": //Ȯ���ϴ�
                PopupManager.instance.Pop(dataRow);     //�븮���� ¥�Ű�
                break;

            case "2": //�����Ѵ�.
                ControlManager.instance.Rotation(dataRow);
                break;

            case "3": //MCR PC ����� ���� Ȯ�� �۾� ��
                CheckManager.instance.ControlMMI(dataRow);
                break;

            case "4":
                //PagingPhone
                CheckManager.instance.ControlMMI(dataRow);
                //Calum���� "MCR_Phone","Local_Phone"Į���� ����� ����¡���� �̸��� �����.
                //PopupManager.instance.Pop("����¡���� ���� Local���� ������ �����ϼ���! ����¡���� ��ȭ�⸦ �弼��.");     //�븮���� ¥�Ű�
                //���� => YH : ����¡������ �����̼Ǹ� �������ϱ�. 2022-08-17
                //if (!string.IsNullOrEmpty(dataRow["Narration"].ToString()))
                //    AudioManager.instance.PlayEffAudio("Narration/" + dataRow["Narration"].ToString().Trim());
                //CallManager.instance.PhoneON(dataRow);
                break;

            case "5": // CIM ���� ����
                //PopupManager.instance.PopMessage(dataRow);
                MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");
                CIMManager.instance.CIM_ON(dataRow);
                break;

            case "6": //��� �ٶ󺸾� Ȯ���ϱ�
                CheckManager.instance.LookTarget(dataRow);
                break;

            case "8": // ���̳� �µ���
                RotationBar.instance.GaugeSetting(dataRow);
                break;

            case "9": // ��ü �� Ȯ��
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

            case "99": // ���� �ǳʶٱ� (�׽�Ʈ��)
                IsOn_true();
                break;

            default: //ȭ��
                if (ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_DGControlRoom || ConnManager.instance.ScenarioList == ScenarioTable.MAIN_FIRE_KitchenDiningRoom)
                    FireScenarioManager.instance.operatorFunctionPairs[dataRow["OperatorType"].ToString()]();
                else if (ConnManager.instance.ScenarioList == ScenarioTable.MAIN_CRITICAL_ACCIDENT)
                    CriticalManager.instance.operatorFunctionPairs[dataRow["OperatorType"].ToString()]();
                break;
        }
    }

    #endregion ������ Ÿ�Ժ� ����

    #endregion ������

    #region PagingPhone Part Check �Լ�

    /// <summary>
    /// ���������(recent)�� ����������(next)�� Actor�� ���ؼ� MCR -> Local ��ȯ�Ǵ� ��Ȳ�϶�, PagingPhone ������ �����ϱ�.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Actor_Check2()
    {
        string recent = dataRow["Actor"].ToString().Trim(); //��������� ��������
        string attention = dataRow["Attention"].ToString().Trim(); //��������� ���ǻ��� ����
        string narration = dataRow["Narration"].ToString().Trim(); //��������� �����̼� ����
        if (rowIndex == dataTable.Rows.Count - 1)
            yield return null;
        else
        {
            int nextIndex = 0;
            if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()))
                nextIndex = ChooseManager.instance.targetIndex;
            else
                nextIndex = rowIndex + 1;

            string next = dataTable.Rows[nextIndex]["Actor"].ToString().Trim(); //���������� ��������
                                                                                //�ش� �������� Actor�� MCR�̰�,
            #region MCR => LOCAL �϶� 
            if (recent.Equals("RO") || recent.Equals("TO") || recent.Equals("EO") || recent.Equals("STA"))
            {
                //���� �������� Actor�� Local�̸�
                if (!next.Equals("RO") && !next.Equals("TO") && !next.Equals("EO") && !next.Equals("STA"))
                {
                    WaitUntil iscall = new WaitUntil(() => isCall);
                    //Paging�� �������� �����϶�.
                    CallManager.instance.PhoneON(dataRow);
                    yield return iscall;
                }
            }
            #endregion
            #region LOCAL => MCR �϶�
            else //�ش� �������� Actor�� Local�̰�
            {
                //���� �������� Actor�� MCR�̸�
                if (next.Equals("RO") || next.Equals("TO") || next.Equals("EO") || next.Equals("STA"))
                {
                    WaitUntil iscall = new WaitUntil(() => isCall);

                    if (currentPhonePos != player.transform.position)
                    {
                        currentPhonePos = Vector3.zero;

                        yield return StartCoroutine(FadeEffect.instance.OUT());
                        //Player ��ġ �̵�

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
                            #region door �ִ� ������Ʈ��, Door ������ �̵�
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
                            //YH - �߰��� �κ� 2022-08-29
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
                   
                    //Paging�� �������� �����϶�.ew
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
     * [ȥ��] �϶� [�������]�϶��� �����ؾ��Ѵ�.
     * ȥ���϶� ������ �Ȱ��� �����ϰ�
     * ȥ�ڰ� �ƴҶ� ��ȭ�� �� ����� ��� ������ ������ ��ȭ ����� ����Ǿ���Ѵ�.
     */
    private IEnumerator Multi_Actor_Check()
    {
        string recent = dataRow["Actor"].ToString().Trim(); //��������� ��������
        string attention = dataRow["Attention"].ToString().Trim(); //��������� ���ǻ��� ����
        string narration = dataRow["Narration"].ToString().Trim(); //��������� �����̼� ����

        if (rowIndex == dataTable.Rows.Count - 1)
            yield return null;
        else
        {
            WaitUntil iscall = new WaitUntil(() => isCall);
            //��Ƽ ����϶�
            //if (PlayerList.Count > 1)
            //{
                if (!string.IsNullOrEmpty(dataRow["MCR_Phone"].ToString()))
                {
                    #region �����̼� ���� 
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
        TimerManager.instance.NoteTime(SequenceTime);  //������ ����ð� �� ����
        TimerManager.instance.ResetTimer(SequenceTime);  //������ ����ð� �� ����
        AudioManager.instance.PlayMultiAudio("Sound/", "MP_Pling"); //������ �����ٴ� ȿ����
    }

    #endregion PagingPhone Part Check �Լ�


    #region �� ������ �������Ǳ�����, ���ܹ�ư�� ������ ���� �������� �Ѿ�����ϴ� �Լ�

    /// <summary>
    /// �� ������ �������Ǳ�����, ���ܹ�ư�� ������ ���� �������� �Ѿ���� �Ѵ�.
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitStepFinish()
    {
        //���� ������ stage�� oper(����) ��
        int operLength = SeqListSetting.instance.Stage_Tasklist_Dic[SeqListSetting.instance.RecentStage].Count;
        int nowSeqNo = SeqListSetting.instance.RowNo_Dic[SeqListSetting.instance.RecentStage];

        int dataIndex = int.Parse(DataManager.instance.GetRowData(dataTable, rowIndex)["RowNo"].ToString());

        if (!string.IsNullOrEmpty(dataRow["JumpSeq"].ToString()) && operLength == 1)
            IsNextStep = true;

        //���� ���� �߰� : ���ܴ����� �������� ����Ǳ� ��, Operration�� �����ϱ� ���� �������� �� �޾ƿ������ϴ� �κ� => �ϴ� ����¡���ʿ��� �ð�üũ�ϰ� ����ϵ��� �س���
        if (dataIndex == (nowSeqNo + operLength - 1))
        {
            WaitUntil waitTime = new WaitUntil(() => IsNextStep);
            UIManager.instance.SequencePanel.SetActive(true);

            //���� �������� Operator�� ������ 1���̰�, OperatorType�� 1 �̸�,
            if ((dataRow["OperatorType"].ToString().Trim().Equals("1") && operLength == 1))
                IsNextStep = true;

            yield return waitTime;      //���� ���⼭ ����ڿ� ������ ��� ����ؾ���.
            AudioManager.instance.PlayMultiAudio("Sound/Select");
            yield return new WaitForSeconds(0.4f);
            AudioManager.instance.PlayMultiAudio("Sound/", "NextStep");

            #region ������ ����ð� �缳�� - UIManager���ε�, �������� ���⿡ �� �� �ۿ� ����..��

            TimerManager.instance.NoteStateTime(SeqListSetting.instance.RecentStage);

            #endregion ������ ����ð� �缳�� - UIManager���ε�, �������� ���⿡ �� �� �ۿ� ����..��

            yield return new WaitForSeconds(1f);
        }

        yield return null;
    }
    #endregion �� ������ �������Ǳ�����, ���ܹ�ư�� ������ ���� �������� �Ѿ�����ϴ� �Լ�


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
    #region ��ŵ

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
    /// �ٰ��� Main Process�� �����Ű�� ���ؼ�
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
        //���� �������� Datarow�� �����ͼ�
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

        //�� ���ʰ� �´��� �ƴ��� �Ǻ��Ѵ�.
        for (int i = 0; i < myActor.Length; i++)
        {
            foreach (GameObject Player in playerKey_Actor[myActor[i]])
            {
                if(Player.GetComponent<PhotonView>().IsMine)
                    return true;    //������ turue
            }    
        }

        if (playerKey_Actor.ContainsKey(Role.All))
        {
            //��� ������ ������ �ִ� ���(Role.All)�� ����� ��� True
            foreach (GameObject Player in playerKey_Actor[Role.All])
            {
                if (Player.GetComponent<PhotonView>().IsMine)
                    return true;    //������ turue
            }
        }


        return false;     //�ƴϸ� false
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

        //�� reday ���¸� true;
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
    /// Local ����� �������� �������ִ� �Լ�
    /// </summary>
    public void Move_to_StartPoint()
    {
        //�� ������ LOCAL �̸�(* MCR �̸� �̵� x)
        //���忡�� �¾��.
        Role[] tmp = ConnManager.instance.myActor;

        //1. Local �����̸�
        if (!tmp.Contains(Role.All) && !tmp.Contains(Role.RO) && !tmp.Contains(Role.TO) && !tmp.Contains(Role.EO) && !tmp.Contains(Role.STA)) 
        {
            Role firstRole;
            //2. ���� ���� ù��° ������ ã�Ƽ�
            for (int i = 0; i < tmp.Length; i++)
            {
                firstRole = tmp[i];

                //3. ������ ���̺��� ù��° LOCAL Pagingphone�� ã�Ƽ� ���� ������ �̵�
                for (int j = 0; j < dataTable.Rows.Count; j++)
                {
                    //4. �� ������ ���� �������̰�, �̵��� PagingPhone ���� ������ ����¡�� ã�Ƽ� �̵� �Լ� �ֱ�
                    if (/*(firstRole.ToString() == dataTable.Rows[j]["Actor"].ToString().Trim()) && */!string.IsNullOrEmpty(dataTable.Rows[j]["Local_Phone"].ToString()))
                    {
                        //����¡�� ã��!
                        foreach (Transform phone in phoneList)
                        {
                            //ã������ �ڷ���Ʈ + Room�� ������!
                            if (phone.gameObject.name == dataTable.Rows[j]["Local_Phone"].ToString())
                            {

                                #region Local �� ���ֱ�
                                BuildingOn(dataTable.Rows[j]["IsBuilding"].ToString());
                                #endregion

                                Transform target = phone;
                                Vector3 targetBound = target.GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(target) : target.position;
                                Vector3 destination = targetBound + Vector3.forward * 1.5f;

                                Transform direction_obj = null;
                                //YH - �߰��� �κ� 2022-08-29
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

                                //�ٴ� �����Ͽ� �÷��̾� ���� ����
                                playerObjDistance.SetPlayerHeight(targetBound);

                                return;
                            }
                        }

                        Debug.LogError("Local�� ��ã���Ŵϱ�, TagName �ѹ� üũ�غ�!");
                        return;
                    }
                }
                break;
            }
        }
        else
        {
            //MCR ������ ������, �̵� X
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

    #region ���� �� ���� �Լ�

    public void BuildingOn(string building)
    {
        if (!string.IsNullOrEmpty(building))
        {
            string[] activeBuildingArray = building.Trim().Split('/');
            for (int t = 0; t < localBuildingList.Count; t++)
            {
                if (activeBuildingArray.Contains(localBuildingList[t].name)) //DB�� �����ִ� Building �Ѱ�,
                    localBuildingList[t].gameObject.SetActive(true);
                else localBuildingList[t].gameObject.SetActive(false); //������ ����
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
        //�˶� �溸�� ����
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
                Debug.Log("���� DB���� AlarmStartPoint Į���� �߰����� �ʾҽ��ϴ�.");
            }
        }
    }
    bool IsAlarmSeq = false;

    /// <summary>
    /// �˶��� �ִ� �ó��������� Ư���� �ð��� ��ŸƮ ���ֱ� ���ؼ�, �ִ�|���� �� �Ǻ��ϴ� IsAlarmSeq bool�� ������ �α����� ���� �Լ�
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
                Debug.Log("���� DB���� AlarmStartPoint Į���� �߰����� �ʾҽ��ϴ�.");
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