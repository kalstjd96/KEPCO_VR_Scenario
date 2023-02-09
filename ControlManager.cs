using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ControlManager : MonoBehaviour
{
    public static ControlManager instance;

    #region 회전 타입
    public Transform rotationTarget;
    private ValveSet[] InfoStruct;

    [NonSerialized] public bool isHandle;
    //오브젝트 이름으로 찾는다.
    [NonSerialized] public Transform gripRightHand;
    [NonSerialized] public Transform gripLeftHand;

    public GameObject Target;
    public bool exception;
    public GameObject TagPanel;
    public GameObject InfoPanel;
    public GameObject MainPanel;
    public GameObject Notway;
    public GameObject OK;
    public Image Gaze_fill;

    Dictionary<string, string> tagDescriptionPair;
    public GameObject[] RotationHand_center = new GameObject[2];       //손 잡았을때, 밸브를 잡은 위치로 회전시키게 하기 위해서(동그란 밸브만 해당!)
    #endregion

    #region
    [Header("포지셔너 정보")]
    public Text valveText;
    public Text stateText;
    public PostitionerMode postitionerState;
    public Image loadingBar;
    public float currentValue;
    public float speed;
    public bool isLoading = false;
    string previousState = null;
    public enum PostitionerMode
    {
        RUNPV,
        Manual,
        Control
    }

    private bool isPostitioner;
    private bool isSynchronization;
    private bool isFirst; //버튼을 누를 때 맨 처음에만 소리와 진동을 넣기 위함
    #endregion


    private void Awake()
    {
        if (instance == null)
            instance = this;

        InfoPanel = GameObject.Find("ValveState_Popup");
        MainPanel = InfoPanel.transform.Find("MainPanel").gameObject;
        Notway = InfoPanel.transform.Find("NotWay").gameObject;
        OK = InfoPanel.transform.Find("OK").gameObject;
        Gaze_fill = MainPanel.transform.Find("Gaze_fill").GetComponent<Image>();

    }
    private void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }

    #region 밸브 잡고 회전 하는 기능
    public void Rotation(DataRow dataRow)
    {
        //1. SeqManager에서 담은 TagNo에서 회전시켜야할 Target을 찾아 담는다.
        List<Transform> targets = new List<Transform>();
        tagDescriptionPair = new Dictionary<string, string>();
        string[] tagArray = dataRow["TagNo"].ToString().Split('/');
        string[] descriptionArray = dataRow["Valve_Description"].ToString().Split('$');
        for (int i = 0; i < tagArray.Length; i++)
        {
            try
            {
                if (!tagDescriptionPair.ContainsKey(tagArray[i].Trim()))
                    tagDescriptionPair.Add(tagArray[i].Trim(), descriptionArray[i]);
                else
                    Debug.Log("이미있는 모델입니다.");
            }
            catch
            {
                tagDescriptionPair.Add(tagArray[i].Trim(), null);
            }
        }
        for (int i = 0; i < tagArray.Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim()))
                {
                    targets.Add(SeqManager.tagNoList[j]);
                    break;
                }
            }
        }

        string standard_value = dataRow["Standard"].ToString();

        if (!string.IsNullOrEmpty(standard_value))
        {
            //밸브 시퀀스일때
            if (standard_value != "OFF" && standard_value != "ON")
            {
                MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Valve");

                InfoStruct = new ValveSet[targets.Count];
                bool[] positive = new bool[targets.Count];
                float[] count = new float[targets.Count];
                string[] Standard = new string[targets.Count];
                string[] Unit = new string[targets.Count];
                float[] endAngle = new float[targets.Count];
                bool[] trouble = new bool[targets.Count];
                bool[] check = new bool[targets.Count];
                float[] startAngle = new float[targets.Count];

                #region V_IsLock value 받아오기
                if (!string.IsNullOrEmpty(dataRow["V_IsLock"].ToString()))
                {
                    string[] isLockArray = dataRow["V_IsLock"].ToString().Split('/');
                    for (int i = 0; i < isLockArray.Length; i++)
                    {
                        //2. 회전 방향 , 회전 수를 저장
                        if (isLockArray[i].Trim().Equals("positive")) //(1) 회전 방향
                            positive[i] = true;    //닫힘(폐쇄,차단)
                        else
                            positive[i] = false;   //열림(개방)
                    }
                }
                #endregion

                #region V_Count value 받아오기
                if (!string.IsNullOrEmpty(dataRow["V_Count"].ToString()))
                {
                    string[] Instead = dataRow["V_Count"].ToString().Split('/');
                    for (int i = 0; i < Instead.Length; i++)
                    {
                        switch (Instead[i].Split(',').Length)
                        {
                            case 3: //특정 값 까지만 돌려야하는 경우
                                endAngle[i] = float.Parse(Instead[i].Split(',')[0].Trim());     //돌려야 할 값
                                count[i] = float.Parse(Instead[i].Split(',')[1].Trim());        //마무리 되어야 할 값
                                startAngle[i] = float.Parse(Instead[i].Split(',')[2].Trim());   //시작해야 할 값
                                break;
                            case 2:
                                endAngle[i] = float.Parse(Instead[i].Split(',')[0].Trim());     //돌려야 할 값
                                count[i] = float.Parse(Instead[i].Split(',')[1].Trim());        //마무리 되어야 할 값
                                startAngle[i] = 0f;                                             //시작해야 할 값
                                break;
                            default:
                                endAngle[i] = 0f;
                                count[i] = float.Parse(Instead[i].Split(',')[0].Trim());
                                startAngle[i] = 0f;
                                break;
                        }
                    }
                }
                #endregion

                #region Standard 받아오기
                for (int i = 0; i < standard_value.Split('&').Length; i++)
                {
                    Standard[i] = standard_value.Split('&')[i].Trim();
                }

                for (int i = 0; i < Standard.Length; i++)
                {
                    if (Standard[i].Split(',').Length > 1)
                    {
                        Unit[i] = Standard[i].Split(',')[1].Trim();
                        Standard[i] = Standard[i].Split(',')[0].Trim();
                    }

                }
                #endregion

                #region trouble 유무(고장났는지 아닌지) value 과 check 해야할 유무인지 아닌지 
                if (!string.IsNullOrEmpty(dataRow["TakenModel"].ToString()))
                {
                    string[] array = dataRow["TakenModel"].ToString().Split('#');
                    Debug.Log($"TakenModel 칼럼을 #으로 쪼갠 값 : {array.Length}");
                    if (array.Length != trouble.Length)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i].Trim().Equals("trouble"))
                                trouble[i] = true;
                            else
                                trouble[i] = false;

                            if (array[i].Trim().Equals("check"))
                                check[i] = true;
                            else
                                check[i] = false;
                        }

                        for (int i = array.Length; i < trouble.Length; i++)
                            trouble[i] = false;

                        for (int i = array.Length; i < check.Length; i++)
                            check[i] = false;
                    }
                    else
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i].Trim().Equals("trouble"))
                                trouble[i] = true;
                            else
                                trouble[i] = false;

                            if (array[i].Trim().Equals("check"))
                                check[i] = true;
                            else
                                check[i] = false;
                        }
                    }
                }

                #endregion

                //ValveSet 스트럭에 모아서 정보관리
                for (int i = 0; i < InfoStruct.Length; i++)
                    InfoStruct[i] = new ValveSet(positive[i], count[i], Standard[i], Unit[i], endAngle[i], trouble[i], check[i], startAngle[i]);

                StartCoroutine(RotationProcess(targets));
            }
            //차단기 시퀀스일때
            else
            {
                MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

                /*
                 * 차단기는 시퀀스가 딱 3개 밖에 없다.
                 * 그래서 그냥 딱 하드로 정하고 구현해도 무방해보임
                 * ON -> OFF : 반 시계 방향으로 90도 돌려준다. 위에 적색 불이 들어온다. [초기값 : 스위치는 'ON' 상태, 청색불]
                 * OFF -> ON : 시계 방향으로 90도 돌려준다. 위에 청색 불이 들어온다. [**But, 해당 시퀀스는 존재하지 않음.]
                 * */
                List<Transform> circuit_List = new List<Transform>();

                foreach (var item in targets)
                {
                    //CircuitBreaker circuit = item.GetComponent<CircuitBreaker>();
                    string[] obj = dataRow["TakenModel"].ToString().Split('/');

                    for (int i = 0; i < obj.Length; i++)
                    {
                        circuit_List.Add(item.Find(obj[i].Trim()));
                    }
                }

                //DB값에 따라서 값 세팅
                if (standard_value == "OFF")
                {
                    foreach (var com in circuit_List)
                    {
                        com.GetComponent<CircuitBreaker>().MyGoalState = State.OFF;
                        com.GetComponent<CircuitBreaker>().MyState = State.ON;
                        com.GetComponent<CircuitBreaker>().Change_State(State.ON);

                    }

                }
                else
                {
                    foreach (var com in circuit_List)
                    {
                        com.GetComponent<CircuitBreaker>().MyGoalState = State.ON;
                        com.GetComponent<CircuitBreaker>().MyState = State.OFF;
                        com.GetComponent<CircuitBreaker>().Change_State(State.OFF);

                    }
                }

                StartCoroutine(CircuitBreakerSeq(circuit_List));
            }
        }
    }

    public void ChaninValve(DataRow dataRow)
    {
        //1. SeqManager에서 담은 TagNo에서 회전시켜야할 Target을 찾아 담는다.
        List<Transform> targets = new List<Transform>();
        tagDescriptionPair = new Dictionary<string, string>();
        string[] tagArray = dataRow["TagNo"].ToString().Split('/');
        string[] descriptionArray = dataRow["Valve_Description"].ToString().Split('$');
        for (int i = 0; i < tagArray.Length; i++)
        {
            try
            {
                if (!tagDescriptionPair.ContainsKey(tagArray[i].Trim()))
                    tagDescriptionPair.Add(tagArray[i].Trim(), descriptionArray[i]);
                else
                {
                    Debug.Log("이미있는 모델입니다.");
                }
            }
            catch
            {
                tagDescriptionPair.Add(tagArray[i].Trim(), null);
            }
        }
        for (int i = 0; i < tagArray.Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim()))
                {
                    targets.Add(SeqManager.tagNoList[j]);
                    break;
                }
            }
        }

        string standard_value = dataRow["Standard"].ToString();

        if (!string.IsNullOrEmpty(standard_value))
        {
            //밸브 시퀀스일때
            if (standard_value != "OFF" && standard_value != "ON")
            {
                MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Valve");

                InfoStruct = new ValveSet[targets.Count];
                bool[] positive = new bool[targets.Count];
                float[] count = new float[targets.Count];
                string[] Standard = new string[targets.Count];
                string[] Unit = new string[targets.Count];
                float[] endAngle = new float[targets.Count];
                bool[] trouble = new bool[targets.Count];
                bool[] check = new bool[targets.Count];

                #region V_IsLock value 받아오기
                if (!string.IsNullOrEmpty(dataRow["V_IsLock"].ToString()))
                {
                    string[] isLockArray = dataRow["V_IsLock"].ToString().Split('/');
                    for (int i = 0; i < isLockArray.Length; i++)
                    {
                        //2. 회전 방향 , 회전 수를 저장
                        if (isLockArray[i].Trim().Equals("positive")) //(1) 회전 방향
                            positive[i] = true;    //닫힘(폐쇄,차단)
                        else
                            positive[i] = false;   //열림(개방)
                    }
                }
                #endregion

                #region V_Count value 받아오기
                if (!string.IsNullOrEmpty(dataRow["V_Count"].ToString()))
                {
                    string[] Instead = dataRow["V_Count"].ToString().Split('/');
                    for (int i = 0; i < Instead.Length; i++)
                    {
                        //시작해야 할 값이 있으면, 
                        if (Instead[i].Split(',').Length == 2)
                        {
                            endAngle[i] = float.Parse(Instead[i].Split(',')[0].Trim());        //돌려야 할 값
                            count[i] = float.Parse(Instead[i].Split(',')[1].Trim());        //마무리 되어야 할 값
                        }
                        else
                        {
                            endAngle[i] = 0f;
                            count[i] = float.Parse(Instead[i].Split(',')[0].Trim());
                        }
                    }
                }
                #endregion

                #region Standard 받아오기
                for (int i = 0; i < standard_value.Split('&').Length; i++)
                {
                    Standard[i] = standard_value.Split('&')[i].Trim();
                }

                for (int i = 0; i < Standard.Length; i++)
                {
                    if (Standard[i].Split(',').Length > 1)
                    {
                        Unit[i] = Standard[i].Split(',')[1].Trim();
                        Standard[i] = Standard[i].Split(',')[0].Trim();
                    }

                }
                #endregion

                //ValveSet 스트럭에 모아서 정보관리
                for (int i = 0; i < InfoStruct.Length; i++)
                {
                    InfoStruct[i] = new ValveSet(positive[i], count[i], Standard[i], Unit[i], endAngle[i]);
                }

                StartCoroutine(ChainRotation(targets));
            }

        }
    }

    #region 차단기 오픈 후 압력 값 조절
    public void Circuit_n_Rotation1(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

        //1. SeqManager에서 담은 TagNo에서 회전시켜야할 Target을 찾아 담는다.
        List<Transform> targets = new List<Transform>();
        for (int i = 0; i < dataRow["TagNo"].ToString().Split('/').Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim()))
                {
                    targets.Add(SeqManager.tagNoList[j]);
                    break;
                }
            }
        }

        foreach (Transform targetmodel in targets)
        {
            State GOAL_State = State.MAN;
            State MY_State = State.OPEN;

            string Data_state;

            #region 차단기 데이터 받아오기
            if (!string.IsNullOrEmpty(dataRow["Standard"].ToString()))
            {
                Data_state = dataRow["Standard"].ToString();

                switch (Data_state)
                {
                    case "CLOSE":
                        GOAL_State = State.CLOSE;
                        MY_State = State.OPEN;
                        break;
                    case "OPEN":
                        GOAL_State = State.OPEN;
                        MY_State = State.CLOSE;
                        break;
                    case "MAN":
                        GOAL_State = State.MAN;
                        MY_State = State.AUTO;
                        break;
                    case "AUTO":
                        GOAL_State = State.AUTO;
                        MY_State = State.MAN;
                        break;
                    case "OFF":
                        GOAL_State = State.OFF;
                        MY_State = State.ON;
                        break;
                    case "ON":
                        GOAL_State = State.ON;
                        MY_State = State.OFF;
                        break;
                    default:
                        Debug.Log("없습니다.");
                        break;
                }
            }
            else
                Debug.Log("값이 없습니다.");
            #endregion

            targetmodel.GetComponent<CircuitnRotation>().SetData(MY_State, GOAL_State);

        }

        StartCoroutine(CircuitnRotation1(targets));

    }

    public void Circuit_n_Rotation2(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

        //1. SeqManager에서 담은 TagNo에서 회전시켜야할 Target을 찾아 담는다.
        List<Transform> targets = new List<Transform>();
        for (int i = 0; i < dataRow["TagNo"].ToString().Split('/').Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim()))
                {
                    targets.Add(SeqManager.tagNoList[j]);
                    break;
                }
            }
        }

        foreach (Transform targetmodel in targets)
        {

            bool direction = false;
            float value = 0f;
            string AnimName= "";

            string Data_state;

            #region 어떤 애니메이션을 실행해줄지 데이터 받아오는 부분_2023-01-02
            if (!string.IsNullOrEmpty(dataRow["Standard"].ToString()))
            {
                Data_state = dataRow["Standard"].ToString();
                AnimName = Data_state;
            }
            #endregion

            #region 돌리는 타입 데이터 받아오기
            if (!string.IsNullOrEmpty(dataRow["V_IsLock"].ToString()))
            {
                Data_state = dataRow["V_IsLock"].ToString();
                if (Data_state == "nagative")
                    direction = false;
                else
                    direction = true;
            }
            else
                Debug.Log("값이 없습니다.");

            if (!string.IsNullOrEmpty(dataRow["V_Count"].ToString()))
            {
                Data_state = dataRow["V_Count"].ToString().Trim();
                value = float.Parse(Data_state);
            }
            else
                Debug.Log("값이 없습니다.");
            #endregion
            targetmodel.GetComponent<CircuitnRotation>().SetData_Rotation(direction, value, AnimName);
        }
        StartCoroutine(CircuitnRotation2(targets));

    }

    public void Pump_Operation(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

        //1. SeqManager에서 담은 TagNo에서 회전시켜야할 Target을 찾아 담는다.
        List<Transform> targets = new List<Transform>();
        for (int i = 0; i < dataRow["TagNo"].ToString().Split('/').Length; i++)
        {
            for (int j = 0; j < SeqManager.tagNoList.Count; j++)
            {
                if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Split('/')[i].Trim()))
                {
                    targets.Add(SeqManager.tagNoList[j]);
                    break;
                }
            }
        }

        foreach (Transform targetmodel in targets)
        {
            bool Is_ReTurn = false;
            State goalState;
            string[] Data_state;

            #region 돌리는 타입 데이터 받아오기
            if (!string.IsNullOrEmpty(dataRow["Standard"].ToString()))
            {
                Data_state = dataRow["Standard"].ToString().Trim().Split(',');

                //원상태로 돌릴지 안돌릴지 기능 변수
                if (Data_state[0] == "Start")
                    goalState = State.START;
                else
                    goalState = State.STOP;

                //원상태로 돌릴지 안돌릴지 기능 변수
                if (Data_state.Length > 1)
                {
                    Is_ReTurn = true;
                }

                targetmodel.GetComponentInChildren<HandCircuit>().SetData_Rotation(goalState, Is_ReTurn);
            }
            else
                Debug.Log("값이 없습니다.");
            #endregion

        }
        StartCoroutine(HandSwitch_Operation(targets));
    }

    #endregion

    //상태
    public enum State
    {
        OPEN,
        CLOSE,
        MAN,
        AUTO,
        OFF,
        ON,
        START,
        STOP
    }


    private struct ValveSet
    {
        /// <summary>
        /// 돌리는 방향
        /// </summary>
        public bool positive { get; set; }
        /// <summary>
        /// 돌려야 하는 횟수
        /// </summary>
        public float count { get; set; }
        /// <summary>
        /// 개도 UI에서 Title에 들어갈 내용 변수
        /// </summary>
        public string Standard { get; set; }
        /// <summary>
        /// UI에서 단위 나타낼 소스에 들어갈 변수
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 값 까지만 돌릴때 쓰이는 변수
        /// </summary>
        public float EndRo { get; set; }
        /// <summary>
        /// 고장 유무를 체킹하는 변수
        /// </summary>
        public bool trouble { get; set; }
        /// <summary>
        ///  돌려서 확인하는 타입인지 체킹하는 변수
        /// </summary>
        public bool check { get; set; }
        /// <summary>
        /// 돌리기전 특정 값으로 세팅해주기 위한 변수
        /// </summary>
        public float SettingStartAngle { get; set; }

        public ValveSet(bool positive, float count, string Standard, string Unit, float EndRo, bool trouble = false, bool check = false, float setAngle = 0f)
        {
            this.positive = positive;
            this.count = count;
            this.Standard = Standard;
            this.Unit = Unit;
            this.EndRo = EndRo;
            this.trouble = trouble;
            this.check = check;
            this.SettingStartAngle = setAngle;
        }

    }

    /// <summary>
    /// 밸브 돌리는 함수
    /// </summary>
    /// <param name="targets">돌려야되는 밸브 List</param>
    /// <returns></returns>
    public IEnumerator RotationProcess(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        InfoPanel.SetActive(true);
        InfoPanel.GetComponent<VR_Popup>().enabled = true;

        //3. TagNo에 기록된 밸브 하위에 공통적으로 Handle이 존재 => Handle 핸들에 스크립트 넣기
        for (int i = 0; i < targets.Count; i++)
        {
            isHandle = false;
            List<GameObject> highLightList = new List<GameObject>();
            /*AudioControlManager acm = targets[i].GetComponentInChildren<AudioControlManager>();
            if (acm != null) acm.Check_IsAudioSoundAndON(targets[i].name);  //사운드가 나야하는 오브젝트가 있으면 틀어주기.*/

            foreach (var handle in targets[i].GetComponentsInChildren<Transform>())
            {
                if (handle.tag == "Handle") //밸브 돌리는 타입 시 동일하게 Handle 오브젝트가 하위에 존재
                {
                    // 설비 조작이 시작할때
                    //(3) 찾은 핸들 하이라이트 적용
                    foreach (var item in handle.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    //(5) 현재 target 기입
                    rotationTarget = handle.transform;
                    rotationTarget.GetComponent<MeshCollider>().enabled = true;
                    //(7) 핸들을 찾았으면 foreach 나가기
                    break;
                }
            }

            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targets[i].transform));

            LookCheck.instance.Tracker(true, rotationTarget.name + "을(를) 조작하세요.", rotationTarget);

            //밸브 정보 띄워즈는 UI 밸브 위에 위치되도록 하기

            ShowTag(rotationTarget);  
            SteeringWheelControll TargetScript = rotationTarget.GetComponent<SteeringWheelControll>();
            TargetScript.Index_Handcenter_number = RotationHand_center;
            TargetScript.centerObj_SetParent(0);
            TargetScript.centerObj_SetParent(1);

            #region 고장난 타입인지 판별할 수 있도록 값 세팅하는 부분
            //고장난 타입인지 아닌지
            if (InfoStruct[i].trouble == true)
            {
                TargetScript.trouble = true;
                TargetScript.Popup_Trouble_BT_Event();
            }
            else
            {
                TargetScript.trouble = false;
            }
            #endregion

            #region 밸브를 살짝 돌려서 체크하는 타입인지 아닌지 판별하도록 값 세팅하는 부분
            //체크하는 타입인지 아닌지
            if (InfoStruct[i].check == true)
                TargetScript.check = true;
            else
                TargetScript.check = false;
            #endregion

            TargetScript.enabled = true;

            #region 발판 기능 있으면 발판 기능을 위해 발판 켜주기 => 이후에 다시 켜주기
            //발판 있으면 발판 켜주기
            if (TargetScript.FootHold != null)
            {
                TargetScript.FootHold.SetActive(true);
                TargetScript.FootHold.GetComponent<FootHold>().Init();
            }
            #endregion

            #region 태그와 겹치거나, 밸브 자체가 UI를 가릴때 수동으로 위치를 조정하는 값이 있으면 준 값 대로 띄워지게하고, 아니면 자동으로 바운스 사이즈 만큼 위에 생성되게 하기
            InfoPanel.transform.position = new Vector3(rotationTarget.position.x, rotationTarget.GetComponent<MeshRenderer>().bounds.size.y / 2 + TargetScript.InfoPanel_up_Position_Value + rotationTarget.position.y, rotationTarget.position.z);

            //if (OK.activeSelf) OK.SetActive(false);     //완료되었다는 체크 UI이미지 비활성화 시키자
            MainPanel.transform.Find("Title").GetComponent<Text>().text = InfoStruct[i].Standard;
            MainPanel.transform.Find("Unit").GetComponent<Text>().text = InfoStruct[i].Unit;

            #endregion

            #region (InfoStruct[i].count) 돌려야되는 횟수가 int일때
            if ((InfoStruct[i].count % 1) / 0.1 == 0)
            {
                TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, InfoStruct[i].SettingStartAngle, 1f, InfoStruct[i].EndRo);

                //스템 위치 세팅
                if (InfoStruct[i].positive)
                    TargetScript.SetStemHight_from_InputData(100);
                else
                    TargetScript.SetStemHight_from_InputData(0);

                TargetScript.AddEvent(
                    delegate {

                        if (!InfoStruct[i].trouble && !InfoStruct[i].check)
                        {
                            float A = TargetScript.LimitAngle;
                            float B = TargetScript.targetAngle + TargetScript.plusAngle;
                            float C = TargetScript.finishAndgle;

                            float Set_N = 0;

                            if (A <= B)
                                Set_N = 1f;
                            else
                            {
                                if(C == 0)
                                    Set_N = (B / A);
                                else
                                {
                                    if(C <= B)
                                        Set_N = (C / A);
                                    else
                                        Set_N = (B / A);
                                }

                            }

                            //잠금방향은 100% -> 0%로 가야함.
                            if (InfoStruct[i].positive)
                            {
                                Set_N = 1 - Set_N;
                                if (TargetScript.AudioObject != null) TargetScript.AudioObject.volume = Set_N;
                            }


                            Gaze_fill.fillAmount = Set_N;
                            string result = ((int)(Set_N * 100)).ToString().Trim();
                            TargetScript.StemUpnDown(TargetScript.stemTarget, Set_N);
                            MainPanel.transform.Find("Value").GetComponent<Text>().text = result;
                        }
                        else
                        {
                            float Set_N = 0;

                            //잠금방향은 100% -> 0%로 가야함.
                            if (InfoStruct[i].positive)
                                Set_N = 1 - Set_N;

                            Gaze_fill.fillAmount = Set_N;
                            string result = ((int)(Set_N * 100)).ToString().Trim();
                            MainPanel.transform.Find("Value").GetComponent<Text>().text = result;
                        }

                    }, delegate {

                        float LimitAngle = 0f;

                        if (TargetScript.finishAndgle != 0f)
                            LimitAngle = TargetScript.finishAndgle;
                        else
                            LimitAngle = TargetScript.LimitAngle;

                        if (LimitAngle <= TargetScript.targetAngle)
                        {
                            TargetScript.isStart = false;
                            OK.SetActive(true);
                            MainPanel.SetActive(false);
                            Notway.SetActive(false);
                            isHandle = true;

                            for (int i = 0; i < RotationHand_center.Length; i++)
                            {
                                RotationHand_center[i].transform.SetParent(SeqManager.instance.TagGroup);
                            }
                        }
                    });
            }
            #endregion
            #region 돌려야되는 횟수가 소수점일때는, 돌려야하는 횟수가아닌, 맞춰줘야하는 float형 값이다.
            else
            {

                if (InfoStruct[i].positive)
                {
                    if (InfoStruct[i].EndRo != 0f)
                        TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, 0f, -1f, InfoStruct[i].EndRo);
                    else
                        TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, InfoStruct[i].count + 2, -1);

                    TargetScript.SetStemHight_from_InputData(100);
                }
                else
                {
                    if (InfoStruct[i].EndRo != 0f)
                        TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, 0f, 1f, InfoStruct[i].EndRo);
                    else
                        TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, InfoStruct[i].count - 2);

                    TargetScript.SetStemHight_from_InputData(0);
                }
                TargetScript.AddEvent(
                    delegate {

                        if (!InfoStruct[i].trouble && !InfoStruct[i].check) //고장 안 났을때
                        {
                            float A = TargetScript.LimitAngle;
                            float B = TargetScript.targetAngle + TargetScript.plusAngle;

                            float Set_N = 0;

                            if (InfoStruct[i].positive)
                            {
                                if (A >= B)
                                    Set_N = 1f;
                                else
                                    Set_N = (A / B);

                                Set_N = 1 - Set_N;
                            }
                            else
                            {
                                if (A <= B)
                                    Set_N = 1f;
                                else
                                    Set_N = (B / A);
                            }

                            Gaze_fill.fillAmount = Set_N;
                            string result = ((int)(Set_N * 100)).ToString().Trim();
                            TargetScript.StemUpnDown(TargetScript.stemTarget, Set_N);
                            MainPanel.transform.Find("Value").GetComponent<Text>().text = result;
                        }
                        else  //고장 났을때
                        {
                            float Set_N = 0;

                            //잠금방향은 100% -> 0%로 가야함.
                            if (InfoStruct[i].positive)
                                Set_N = 1 - Set_N;

                            Gaze_fill.fillAmount = Set_N;
                            string result = ((int)(Set_N * 100)).ToString().Trim();
                            MainPanel.transform.Find("Value").GetComponent<Text>().text = result;
                        }

                    }, delegate {

                        if (InfoStruct[i].positive)
                        {
                            float LimitAngle = 0f;

                            if (TargetScript.finishAndgle != 0f)
                                LimitAngle = TargetScript.finishAndgle;
                            else
                                LimitAngle = TargetScript.LimitAngle;

                            if (LimitAngle >= TargetScript.targetAngle)
                            {
                                TargetScript.isStart = false;
                                OK.SetActive(true);
                                MainPanel.SetActive(false);
                                Notway.SetActive(false);
                                isHandle = true;

                                for (int i = 0; i < RotationHand_center.Length; i++)
                                    RotationHand_center[i].transform.SetParent(SeqManager.instance.TagGroup);
                            }
                        }
                        else
                        {
                            float LimitAngle = 0f;

                            if (TargetScript.finishAndgle != 0f)
                                LimitAngle = TargetScript.finishAndgle;
                            else
                                LimitAngle = TargetScript.LimitAngle;

                            if (LimitAngle <= TargetScript.targetAngle)
                            {
                                TargetScript.isStart = false;
                                OK.SetActive(true);
                                MainPanel.SetActive(false);
                                Notway.SetActive(false);
                                isHandle = true;
                            }
                        }
                    });
            }
            #endregion

            //count만큼 돌리면 true => isHandle
            yield return waitUntil;

            //HideTag(rortationTarget);   //태그 꺼주기

            /*if (acm != null) acm.Check_IsAudioSoundAndOFF();   //사운드가 틀어져 있다면 꺼주기*/
            LookCheck.instance.Tracker(false);
            TargetScript.enabled = false;
            yield return waitForSeconds;
            #region 발판 기능이 있었다면 발판 꺼주기
            if (TargetScript.FootHold != null)
            {
                TargetScript.FootHold.GetComponent<FootHold>().ReturnHigh();
                TargetScript.FootHold.GetComponent<FootHold>().Init();
                TargetScript.FootHold.SetActive(false);
            }
            #endregion
            #region 비활성화 해야할 오브젝트가 있다면 꺼주기
            if (TargetScript.eventActiveTarget != null && TargetScript.eventActiveTarget.activeSelf)
                TargetScript.eventActiveTarget.SetActive(false);
            #endregion         
            MinimapManager.instance.PointState(targets[i].name, "End");
            OK.SetActive(false);
            rotationTarget.GetComponent<MeshCollider>().enabled = false;
            // 시뮬레이터로 값 송신
            //targets[i].GetComponent<Simulator>().Send();

            //마지막 절차라면 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                InfoPanel.GetComponent<VR_Popup>().enabled = false;
                InfoPanel.SetActive(false);
                // 모든 설비조작이 끝나고 다음절차로 넘어갈때
                MinimapManager.instance.Initialize();
                SeqManager.instance.IsOn_true();
                InfoStruct = null;
            }

        }

    }

    /// <summary>
    /// 체인 밸브 조작 타입
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    public IEnumerator ChainRotation(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        InfoPanel.SetActive(true);
        InfoPanel.GetComponent<VR_Popup>().enabled = true;

        //3. TagNo에 기록된 밸브 하위에 공통적으로 Handle이 존재 => Handle 핸들에 스크립트 넣기
        for (int i = 0; i < targets.Count; i++)
        {
            isHandle = false;
            List<GameObject> highLightList = new List<GameObject>();
            //AudioControlManager acm = targets[i].GetComponentInChildren<AudioControlManager>();
            //if (acm != null) acm.Check_IsAudioSoundAndON(targets[i].name);  //사운드가 나야하는 오브젝트가 있으면 틀어주기.

            foreach (var Chain in targets[i].GetComponentsInChildren<Transform>())
            {
                if (Chain.tag == "Chain") //밸브 돌리는 타입 시 동일하게 Chain 오브젝트가 하위에 존재
                {
                    // 설비 조작이 시작할때

                    //(3) 찾은 핸들 하이라이트 적용
                    foreach (var item in Chain.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    //(5) 현재 target 기입
                    rotationTarget = Chain.transform;
                    rotationTarget.GetComponent<MeshCollider>().enabled = true;
                    Debug.Log("rortationTarget : " + rotationTarget.name, rotationTarget);
                    //(7) 핸들을 찾았으면 foreach 나가기
                    break;
                }
            }

            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targets[i].transform));

            LookCheck.instance.Tracker(true, rotationTarget.name + "을(를) 조작하세요.", rotationTarget);

            //밸브 정보 띄워즈는 UI 밸브 위에 위치되도록 하기

            ShowTag(rotationTarget);
            ChainRotation TargetScript = rotationTarget.GetComponent<ChainRotation>();
            InfoPanel.transform.position = new Vector3(rotationTarget.position.x, rotationTarget.GetComponent<MeshRenderer>().bounds.size.y / 2 + TargetScript.InfoPanel_up_Position_Value + rotationTarget.position.y, rotationTarget.position.z);
            TargetScript.enabled = true;

            //Debug.Log(InfoStruct[i].count % 1);
            MainPanel.transform.Find("Title").GetComponent<Text>().text = InfoStruct[i].Standard;
            MainPanel.transform.Find("Unit").GetComponent<Text>().text = InfoStruct[i].Unit;

            #region (InfoStruct[i].count) 돌려야되는 횟수가 int일때
            if ((InfoStruct[i].count % 1) / 0.1 == 0)
            {
                TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, 0f, 1f, InfoStruct[i].EndRo);

                TargetScript.AddEvent(
                    delegate {

                        float A = TargetScript.LimitAngle;
                        float B = TargetScript.targetAngle + TargetScript.plusAngle;

                        float Set_N = 0;

                        if (A <= B)
                            Set_N = 1f;
                        else
                            Set_N = (B / A);

                        //잠금방향은 100% -> 0%로 가야함.
                        if (InfoStruct[i].positive)
                            Set_N = 1 - Set_N;

                        Gaze_fill.fillAmount = Set_N;
                        string result = ((int)(Set_N * 100)).ToString().Trim();
                        //TargetScript.StemUpnDown(TargetScript.stemTarget, Set_N);
                        MainPanel.transform.Find("Value").GetComponent<Text>().text = result;

                    }, delegate {

                        float LimitAngle = 0f;

                        if (TargetScript.finishAndgle != 0f)
                            LimitAngle = TargetScript.finishAndgle;
                        else
                            LimitAngle = TargetScript.LimitAngle;

                        if (LimitAngle <= TargetScript.targetAngle)
                        {
                            TargetScript.isStart = false;
                            OK.SetActive(true);
                            MainPanel.SetActive(false);
                            Notway.SetActive(false);
                            isHandle = true;
                        }
                    });
            }
            #endregion

            //count만큼 돌리면 true => isHandle
            yield return waitUntil;

            //if (acm != null) acm.Check_IsAudioSoundAndOFF();   //사운드가 틀어져 있다면 꺼주기
            //Destroy(rortationTarget.GetComponent<RotationCheck>()); //다 돌렸으면 돌린 밸브의 스크립트 제고
            //RotationInit(); //초기화
            LookCheck.instance.Tracker(false);
            TargetScript.enabled = false;
            yield return waitForSeconds;
            MinimapManager.instance.PointState(targets[i].name, "End");
            OK.SetActive(false);
            rotationTarget.GetComponent<MeshCollider>().enabled = false;
            // 시뮬레이터로 값 송신
            //targets[i].GetComponent<Simulator>().Send();

            //마지막 절차라면 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                InfoPanel.GetComponent<VR_Popup>().enabled = false;
                InfoPanel.SetActive(false);
                // 모든 설비조작이 끝나고 다음절차로 넘어갈때
                MinimapManager.instance.Initialize();
                SeqManager.instance.IsOn_true();
                InfoStruct = null;
            }

        }
    }


    /// <summary>
    /// 돌리는 차단기 기능
    /// </summary>
    /// <param name="targets">차단기 태그넘버 List</param>
    /// <returns></returns>
    public IEnumerator CircuitBreakerSeq(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        for (int i = 0; i < targets.Count; i++)
        {
            isHandle = false;
            List<GameObject> highLightList = new List<GameObject>();
            //MinimapManager.instance.PointState(targets[i].name, "Start");
            foreach (var Circuit in targets[i].GetComponentsInChildren<Transform>())
            {
                if (Circuit.tag == "Circuit") //차단 타입 시 동일하게 Handle 오브젝트가 하위에 존재
                {
                    foreach (var item in Circuit.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    rotationTarget = Circuit.transform;
                    break;
                }
            }

            LookCheck.instance.Tracker(true, rotationTarget.name + "을(를) 조작하세요.", rotationTarget);
            yield return waitUntil;
            LookCheck.instance.Tracker(false);
            // 시뮬레이터로 값 송신
            //targets[i].GetComponentInParent<Simulator>().Send();

            //마지막 절차라면 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                SeqManager.instance.IsOn_true();
                MinimapManager.instance.Initialize();
            }
        }

        yield return null;
    }

    public IEnumerator CircuitnRotation1(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        isHandle = false;
        for (int i = 0; i < targets.Count; i++)
        {

            List<GameObject> highLightList = new List<GameObject>();
            MinimapManager.instance.PointState(targets[i].name, "Start");
            foreach (var Circuit in targets[i].GetComponentsInChildren<Transform>())
            {
                if (Circuit.tag == "Circuit") //차단 타입 시 동일하게 Handle 오브젝트가 하위에 존재
                {
                    foreach (var item in Circuit.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    rotationTarget = Circuit.transform;
                    break;
                }
            }

            LookCheck.instance.Tracker(true, rotationTarget.name + "을(를) 조작하세요.", rotationTarget);

            yield return waitUntil;

            MinimapManager.instance.PointState(targets[i].name, "End");
            LookCheck.instance.Tracker(false);

            // 시뮬레이터로 값 송신
            //targets[i].GetComponentInParent<Simulator>().Send();

            //마지막 절차라면 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                SeqManager.instance.IsOn_true();
                MinimapManager.instance.Initialize();
            }
        }
        yield return null;
    }

    public IEnumerator CircuitnRotation2(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        isHandle = false;
        for (int i = 0; i < targets.Count; i++)
        {

            List<GameObject> highLightList = new List<GameObject>();
            MinimapManager.instance.PointState(targets[i].name, "Start");
            foreach (var Circuit in targets[i].GetComponentsInChildren<Transform>())
            {
                if (Circuit.tag == "Circuit_Ro")
                {
                    foreach (var item in Circuit.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    rotationTarget = Circuit.transform;
                    break;
                }
            }

            LookCheck.instance.Tracker(true, rotationTarget.name + "을(를) 조작하세요.", rotationTarget);

            yield return waitUntil;
            MinimapManager.instance.PointState(targets[i].name, "End");
            LookCheck.instance.Tracker(false);

            // 시뮬레이터로 값 송신
            //targets[i].GetComponentInParent<Simulator>().Send();

            //마지막 절차라면 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                SeqManager.instance.IsOn_true();
                MinimapManager.instance.Initialize();
            }
        }
        yield return null;
    }

    public IEnumerator HandSwitch_Operation(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        isHandle = false;
        for (int i = 0; i < targets.Count; i++)
        {

            List<GameObject> highLightList = new List<GameObject>();
            MinimapManager.instance.PointState(targets[i].name, "Start");
            foreach (var Circuit in targets[i].GetComponentsInChildren<Transform>())
            {
                if (Circuit.tag == "Circuit_Ro")
                {
                    foreach (var item in Circuit.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    rotationTarget = Circuit.transform;
                    break;
                }
            }

            //AudioSource가 있으면 넣어주기
            AudioControlManager acm = targets[i].GetComponentInChildren<AudioControlManager>();

            LookCheck.instance.Tracker(true, rotationTarget.name + "을(를) 조작하세요.", rotationTarget);

            yield return waitUntil;

            //사운드가 나야하는 오브젝트가 있으면 틀어주기.
            if (acm != null) acm.Check_IsAudioSoundAndON(targets[i].name);

            MinimapManager.instance.PointState(targets[i].name, "End");
            LookCheck.instance.Tracker(false);

            // 시뮬레이터로 값 송신
            //targets[i].GetComponentInParent<Simulator>().Send();

            //마지막 절차라면 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                SeqManager.instance.IsOn_true();
                MinimapManager.instance.Initialize();
            }
        }
        yield return null;
    }
    #endregion

    #region Postitioner 타입
    public void PostitionerValve(DataRow dataRow)
    {
        postitionerState = PostitionerMode.RUNPV;

        //이거는 다른 모든 절차와 동일해서 하나로 통합을...
        for (int j = 0; j < SeqManager.tagNoList.Count; j++)
        {
            if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Trim()))
            {
                //ex. targets에 들어가는 Obj 5-511-V-0071
                loadingBar = SeqManager.tagNoList[j].Find("Postitioner/Canvas/LoadingBar").GetComponent<Image>();
                StartCoroutine(PostitionerProcess(SeqManager.tagNoList[j]));
                //2023-01-10 최영호 추가
                PopupManager.instance.Pop(dataRow, true);
                break;
            }
        }
    }

    private IEnumerator PostitionerProcess(Transform target)
    {
        isPostitioner = false;
        isFirst = true;
        isSynchronization = false;
        WaitUntil waitUntil = new WaitUntil(() => isPostitioner);

        #region 1. 해당 모델 초기 세팅  
        PressJointBtn2[] postitioner = target.GetComponentsInChildren<PressJointBtn2>(true);

        Transform postitionerTarget = postitioner[0].transform.parent.parent;

        stateText = postitionerTarget.Find("Display/ValveText/Percentage").GetComponent<Text>();
        valveText = postitionerTarget.Find("Display/ValveText/ControlText").GetComponent<Text>();
        valveText.text = "RUN PV";
        stateText.text = "0.0%";
        previousState = stateText.text;
        List<GameObject> highLight = new List<GameObject>();
        foreach (var highLightObj in postitionerTarget.GetComponentsInChildren<MeshFilter>())
            highLight.Add(highLightObj.gameObject);

        Highlight.instance.On(highLight.ToArray());
        bool isHighLight = true;

        LookCheck.instance.Tracker(true, target.name + "의 Postitioner을(를) 조작하세요.", postitionerTarget);



        // 설비 조작이 시작할때
        MinimapManager.instance.PointState(postitionerTarget.name, "Start");
        #endregion

        #region 2. 모델의 버튼들 Function 정의
        for (int j = 0; j < postitioner.Length; j++)
        {
            //1. 스크립트 키기
            postitioner[j].enabled = true;
            if (postitioner[j].GetComponent<PostitionerLoading>())
                postitioner[j].GetComponent<PostitionerLoading>().enabled = true;

            //2. 버튼들 기능 정의
            string btnName = postitioner[j].transform.parent.name; //Up, Down, Enter, Esc
           
            postitioner[j].GetComponent<PressJointBtn2>().AddListener(delegate
            {
                if (isFirst)
                    SoundVibration();

                if (isHighLight)
                {
                    Highlight.instance.Off();
                    isHighLight = false;
                }


                switch (btnName)
                {
                    case "Up":
                        if (postitionerState == PostitionerMode.Control)
                            ValueState(true);
                        break;
                    case "Down":
                        if (postitionerState == PostitionerMode.Control)
                            ValueState(false);
                        break;
                    case "Enter":
                        if (postitionerState == PostitionerMode.Manual)
                        {
                            postitionerState = PostitionerMode.Control;
                            stateText.text = previousState;
                            string manufacture = stateText.text.ToString().Replace(".", "").Replace("%", "");
                            valveText.text = "*MA " + manufacture;
                        }
                        else if (postitionerState == PostitionerMode.RUNPV)
                            isLoading = true;
                        break;
                    case "Esc":
                        if (postitionerState == PostitionerMode.Control)
                        {
                            postitionerState = PostitionerMode.Manual;
                            valveText.text = "MANURL";
                            stateText.text = "";
                        }
                        else if (postitionerState == PostitionerMode.Manual)
                        {
                            postitionerState = PostitionerMode.RUNPV;
                            valveText.text = "RUN PV";
                            stateText.text = previousState;
                        }
                        break;
                    default:
                        break;
                }
            }, delegate { isFirst = true;
                if (btnName.Equals("Enter"))
                {
                    if (postitionerState == PostitionerMode.RUNPV)
                        isLoading = false;
                }
            });
            
        }
        #endregion

        yield return waitUntil;

        for (int j = 0; j < postitioner.Length; j++)
        {
            postitioner[j].enabled = false;
            if (postitioner[j].GetComponent<PostitionerLoading>())
                postitioner[j].GetComponent<PostitionerLoading>().enabled = false;
        }

        isPostitioner = false;

        // 설비 조작이 끝났을때
        MinimapManager.instance.PointState(postitionerTarget.name, "End");

        LookCheck.instance.Tracker(false);
        // 모든 설비조작이 끝나고 다음절차로 넘어갈때
        MinimapManager.instance.Initialize();

        SeqManager.instance.IsOn_true();
    }

    public void ValueState(bool upBtn)
    {
        int value = int.Parse(valveText.text.ToString().Replace("*MA ", ""));
        if (value < 50)
        {
            if (upBtn)
                value += 10;
            else
                value -= 10;
        }

        valveText.text = "*MA " + value.ToString();

        if (!isSynchronization)
        {
            isSynchronization = true;
            StartCoroutine(StateTextSynchronization());
        }
    }

    private IEnumerator StateTextSynchronization()
    {
        yield return new WaitForSeconds(2f);

        string manufacture = valveText.text.Replace("*MA ", "");
        bool result = int.TryParse(manufacture, out int i);
        if (result)
        {
            string manufacture2 = manufacture.Substring(0, manufacture.Length - 1);
            string manufacture3 = manufacture.Substring(manufacture.Length - 1);

            if (string.IsNullOrEmpty(manufacture2))
                manufacture2 = "0";

            stateText.text = manufacture2 + "." + manufacture3 + "%";
            previousState = stateText.text;
            isSynchronization = false;

            if (i == 50)
                isPostitioner = true;
        }
    }

    private void SoundVibration()
    {
        //1. 소리 나기
        AudioManager.instance.PlayMultiAudio("Sound/MP_Button");

        //2. 진동 넣기
        SteamVR_Action_Vibration hapticAntion = SteamVR_Actions._default.Haptic;
        hapticAntion.Execute(0, 0.3f, 50, 100, SteamVR_Input_Sources.RightHand, true);

        isFirst = false;
    }
    #endregion

    #region 밸브 레버 기능 TagNo는 무조건 한개일때만 됨
    GameObject target;
    List<GameObject> highLightList;
    public void ValveLever(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");
        for (int j = 0; j < SeqManager.tagNoList.Count; j++)
        {
            if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Trim()))
            {
                target = SeqManager.tagNoList[j].gameObject;
                break;
            }
        }

        LookCheck.instance.Tracker(true, target.name + "의 밸브레버를 조작하세요.", target.transform);

        // 설비 조작이 시작할때
        MinimapManager.instance.PointState(target.name, "Start");

        highLightList = new List<GameObject>();

        foreach (var item in target.GetComponentsInChildren<MeshFilter>())
            highLightList.Add(item.gameObject);

        Highlight.instance.On(highLightList.ToArray());

        //EmergencyLever ELever = EmergencyLever.GetComponent<EmergencyLever>();
        EmergencyLever ELever = target.GetComponentInChildren<EmergencyLever>();

        ELever.Set(true, 1, Direction.UPWARD, delegate
        {
            Highlight.instance.Off();
            MinimapManager.instance.PointState(target.name, "End");
            LookCheck.instance.Tracker(false);
            MinimapManager.instance.Initialize();
            ELever.Set(false);
            Invoke("DisableGauge_Invoke", 3f);
            SeqManager.instance.IsOn_true();
        });
    }

    #endregion

    public void ShowTag(Transform target)
    {
        Vector3 targetVector = target.GetComponentInChildren<Renderer>() ? LookCheck.instance.RendererCenter(target) : target.position;

        // target이 핸들이라 부모로 변경해준다.
        target = target.parent;
        GameObject lc;
        GameObject tagPanel;

        #region 태그를 중복생성할 여지가 있으므로, 예외처리 - 2022-12-06 YH
        try
        {
            lc = target.Find(target.name + "_Tag").gameObject;
            tagPanel = lc.gameObject;
        }
        catch (Exception)
        {
            tagPanel = Instantiate(TagPanel);
        }
        #endregion

        tagPanel.transform.SetParent(target);
        tagPanel.transform.position = targetVector;
        tagPanel.transform.name = target.name + "_Tag";

        Text TAG = tagPanel.GetComponentsInChildren<Text>(true)[0];
        Text DESCRIPTION = tagPanel.GetComponentsInChildren<Text>(true)[1];
        string description = "";
        if (tagDescriptionPair.ContainsKey(target.name.Trim()))
            description = tagDescriptionPair[target.name.Trim()];
        TAG.text = target.name;
        /////Tag에 대한 Description 추가 [없으면 Description부분은 꺼지게 함]/////
        DESCRIPTION.text = description;
        if (string.IsNullOrEmpty(description))
            DESCRIPTION.transform.parent.gameObject.SetActive(false);
        else
        {
            DESCRIPTION.transform.parent.gameObject.SetActive(true);
            var posRT = tagPanel.transform.Find("pos").GetComponent<RectTransform>();
            tagPanel.transform.Find("Image").GetComponent<RectTransform>().position = posRT.position;
            tagPanel.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = posRT.sizeDelta;
        }//////////////////////////////////////////////////////////////////////////
        tagPanel.SetActive(true);
    }

    public void HideTag(Transform target)
    {
        target = target.parent;
        Destroy(target.Find(target.name + "_Tag").gameObject);
    }
}
