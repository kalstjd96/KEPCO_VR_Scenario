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

    #region ȸ�� Ÿ��
    public Transform rotationTarget;
    private ValveSet[] InfoStruct;

    [NonSerialized] public bool isHandle;
    //������Ʈ �̸����� ã�´�.
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
    public GameObject[] RotationHand_center = new GameObject[2];       //�� �������, ��긦 ���� ��ġ�� ȸ����Ű�� �ϱ� ���ؼ�(���׶� ��길 �ش�!)
    #endregion

    #region
    [Header("�����ų� ����")]
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
    private bool isFirst; //��ư�� ���� �� �� ó������ �Ҹ��� ������ �ֱ� ����
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

    #region ��� ��� ȸ�� �ϴ� ���
    public void Rotation(DataRow dataRow)
    {
        //1. SeqManager���� ���� TagNo���� ȸ�����Ѿ��� Target�� ã�� ��´�.
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
                    Debug.Log("�̹��ִ� ���Դϴ�.");
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
            //��� �������϶�
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

                #region V_IsLock value �޾ƿ���
                if (!string.IsNullOrEmpty(dataRow["V_IsLock"].ToString()))
                {
                    string[] isLockArray = dataRow["V_IsLock"].ToString().Split('/');
                    for (int i = 0; i < isLockArray.Length; i++)
                    {
                        //2. ȸ�� ���� , ȸ�� ���� ����
                        if (isLockArray[i].Trim().Equals("positive")) //(1) ȸ�� ����
                            positive[i] = true;    //����(���,����)
                        else
                            positive[i] = false;   //����(����)
                    }
                }
                #endregion

                #region V_Count value �޾ƿ���
                if (!string.IsNullOrEmpty(dataRow["V_Count"].ToString()))
                {
                    string[] Instead = dataRow["V_Count"].ToString().Split('/');
                    for (int i = 0; i < Instead.Length; i++)
                    {
                        switch (Instead[i].Split(',').Length)
                        {
                            case 3: //Ư�� �� ������ �������ϴ� ���
                                endAngle[i] = float.Parse(Instead[i].Split(',')[0].Trim());     //������ �� ��
                                count[i] = float.Parse(Instead[i].Split(',')[1].Trim());        //������ �Ǿ�� �� ��
                                startAngle[i] = float.Parse(Instead[i].Split(',')[2].Trim());   //�����ؾ� �� ��
                                break;
                            case 2:
                                endAngle[i] = float.Parse(Instead[i].Split(',')[0].Trim());     //������ �� ��
                                count[i] = float.Parse(Instead[i].Split(',')[1].Trim());        //������ �Ǿ�� �� ��
                                startAngle[i] = 0f;                                             //�����ؾ� �� ��
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

                #region Standard �޾ƿ���
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

                #region trouble ����(���峵���� �ƴ���) value �� check �ؾ��� �������� �ƴ��� 
                if (!string.IsNullOrEmpty(dataRow["TakenModel"].ToString()))
                {
                    string[] array = dataRow["TakenModel"].ToString().Split('#');
                    Debug.Log($"TakenModel Į���� #���� �ɰ� �� : {array.Length}");
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

                //ValveSet ��Ʈ���� ��Ƽ� ��������
                for (int i = 0; i < InfoStruct.Length; i++)
                    InfoStruct[i] = new ValveSet(positive[i], count[i], Standard[i], Unit[i], endAngle[i], trouble[i], check[i], startAngle[i]);

                StartCoroutine(RotationProcess(targets));
            }
            //���ܱ� �������϶�
            else
            {
                MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

                /*
                 * ���ܱ�� �������� �� 3�� �ۿ� ����.
                 * �׷��� �׳� �� �ϵ�� ���ϰ� �����ص� �����غ���
                 * ON -> OFF : �� �ð� �������� 90�� �����ش�. ���� ���� ���� ���´�. [�ʱⰪ : ����ġ�� 'ON' ����, û����]
                 * OFF -> ON : �ð� �������� 90�� �����ش�. ���� û�� ���� ���´�. [**But, �ش� �������� �������� ����.]
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

                //DB���� ���� �� ����
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
        //1. SeqManager���� ���� TagNo���� ȸ�����Ѿ��� Target�� ã�� ��´�.
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
                    Debug.Log("�̹��ִ� ���Դϴ�.");
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
            //��� �������϶�
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

                #region V_IsLock value �޾ƿ���
                if (!string.IsNullOrEmpty(dataRow["V_IsLock"].ToString()))
                {
                    string[] isLockArray = dataRow["V_IsLock"].ToString().Split('/');
                    for (int i = 0; i < isLockArray.Length; i++)
                    {
                        //2. ȸ�� ���� , ȸ�� ���� ����
                        if (isLockArray[i].Trim().Equals("positive")) //(1) ȸ�� ����
                            positive[i] = true;    //����(���,����)
                        else
                            positive[i] = false;   //����(����)
                    }
                }
                #endregion

                #region V_Count value �޾ƿ���
                if (!string.IsNullOrEmpty(dataRow["V_Count"].ToString()))
                {
                    string[] Instead = dataRow["V_Count"].ToString().Split('/');
                    for (int i = 0; i < Instead.Length; i++)
                    {
                        //�����ؾ� �� ���� ������, 
                        if (Instead[i].Split(',').Length == 2)
                        {
                            endAngle[i] = float.Parse(Instead[i].Split(',')[0].Trim());        //������ �� ��
                            count[i] = float.Parse(Instead[i].Split(',')[1].Trim());        //������ �Ǿ�� �� ��
                        }
                        else
                        {
                            endAngle[i] = 0f;
                            count[i] = float.Parse(Instead[i].Split(',')[0].Trim());
                        }
                    }
                }
                #endregion

                #region Standard �޾ƿ���
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

                //ValveSet ��Ʈ���� ��Ƽ� ��������
                for (int i = 0; i < InfoStruct.Length; i++)
                {
                    InfoStruct[i] = new ValveSet(positive[i], count[i], Standard[i], Unit[i], endAngle[i]);
                }

                StartCoroutine(ChainRotation(targets));
            }

        }
    }

    #region ���ܱ� ���� �� �з� �� ����
    public void Circuit_n_Rotation1(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

        //1. SeqManager���� ���� TagNo���� ȸ�����Ѿ��� Target�� ã�� ��´�.
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

            #region ���ܱ� ������ �޾ƿ���
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
                        Debug.Log("�����ϴ�.");
                        break;
                }
            }
            else
                Debug.Log("���� �����ϴ�.");
            #endregion

            targetmodel.GetComponent<CircuitnRotation>().SetData(MY_State, GOAL_State);

        }

        StartCoroutine(CircuitnRotation1(targets));

    }

    public void Circuit_n_Rotation2(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

        //1. SeqManager���� ���� TagNo���� ȸ�����Ѿ��� Target�� ã�� ��´�.
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

            #region � �ִϸ��̼��� ���������� ������ �޾ƿ��� �κ�_2023-01-02
            if (!string.IsNullOrEmpty(dataRow["Standard"].ToString()))
            {
                Data_state = dataRow["Standard"].ToString();
                AnimName = Data_state;
            }
            #endregion

            #region ������ Ÿ�� ������ �޾ƿ���
            if (!string.IsNullOrEmpty(dataRow["V_IsLock"].ToString()))
            {
                Data_state = dataRow["V_IsLock"].ToString();
                if (Data_state == "nagative")
                    direction = false;
                else
                    direction = true;
            }
            else
                Debug.Log("���� �����ϴ�.");

            if (!string.IsNullOrEmpty(dataRow["V_Count"].ToString()))
            {
                Data_state = dataRow["V_Count"].ToString().Trim();
                value = float.Parse(Data_state);
            }
            else
                Debug.Log("���� �����ϴ�.");
            #endregion
            targetmodel.GetComponent<CircuitnRotation>().SetData_Rotation(direction, value, AnimName);
        }
        StartCoroutine(CircuitnRotation2(targets));

    }

    public void Pump_Operation(DataRow dataRow)
    {
        MinimapManager.instance.Set(dataRow["TagNo"].ToString(), "Control");

        //1. SeqManager���� ���� TagNo���� ȸ�����Ѿ��� Target�� ã�� ��´�.
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

            #region ������ Ÿ�� ������ �޾ƿ���
            if (!string.IsNullOrEmpty(dataRow["Standard"].ToString()))
            {
                Data_state = dataRow["Standard"].ToString().Trim().Split(',');

                //�����·� ������ �ȵ����� ��� ����
                if (Data_state[0] == "Start")
                    goalState = State.START;
                else
                    goalState = State.STOP;

                //�����·� ������ �ȵ����� ��� ����
                if (Data_state.Length > 1)
                {
                    Is_ReTurn = true;
                }

                targetmodel.GetComponentInChildren<HandCircuit>().SetData_Rotation(goalState, Is_ReTurn);
            }
            else
                Debug.Log("���� �����ϴ�.");
            #endregion

        }
        StartCoroutine(HandSwitch_Operation(targets));
    }

    #endregion

    //����
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
        /// ������ ����
        /// </summary>
        public bool positive { get; set; }
        /// <summary>
        /// ������ �ϴ� Ƚ��
        /// </summary>
        public float count { get; set; }
        /// <summary>
        /// ���� UI���� Title�� �� ���� ����
        /// </summary>
        public string Standard { get; set; }
        /// <summary>
        /// UI���� ���� ��Ÿ�� �ҽ��� �� ����
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// �� ������ ������ ���̴� ����
        /// </summary>
        public float EndRo { get; set; }
        /// <summary>
        /// ���� ������ üŷ�ϴ� ����
        /// </summary>
        public bool trouble { get; set; }
        /// <summary>
        ///  ������ Ȯ���ϴ� Ÿ������ üŷ�ϴ� ����
        /// </summary>
        public bool check { get; set; }
        /// <summary>
        /// �������� Ư�� ������ �������ֱ� ���� ����
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
    /// ��� ������ �Լ�
    /// </summary>
    /// <param name="targets">�����ߵǴ� ��� List</param>
    /// <returns></returns>
    public IEnumerator RotationProcess(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        InfoPanel.SetActive(true);
        InfoPanel.GetComponent<VR_Popup>().enabled = true;

        //3. TagNo�� ��ϵ� ��� ������ ���������� Handle�� ���� => Handle �ڵ鿡 ��ũ��Ʈ �ֱ�
        for (int i = 0; i < targets.Count; i++)
        {
            isHandle = false;
            List<GameObject> highLightList = new List<GameObject>();
            /*AudioControlManager acm = targets[i].GetComponentInChildren<AudioControlManager>();
            if (acm != null) acm.Check_IsAudioSoundAndON(targets[i].name);  //���尡 �����ϴ� ������Ʈ�� ������ Ʋ���ֱ�.*/

            foreach (var handle in targets[i].GetComponentsInChildren<Transform>())
            {
                if (handle.tag == "Handle") //��� ������ Ÿ�� �� �����ϰ� Handle ������Ʈ�� ������ ����
                {
                    // ���� ������ �����Ҷ�
                    //(3) ã�� �ڵ� ���̶���Ʈ ����
                    foreach (var item in handle.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    //(5) ���� target ����
                    rotationTarget = handle.transform;
                    rotationTarget.GetComponent<MeshCollider>().enabled = true;
                    //(7) �ڵ��� ã������ foreach ������
                    break;
                }
            }

            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targets[i].transform));

            LookCheck.instance.Tracker(true, rotationTarget.name + "��(��) �����ϼ���.", rotationTarget);

            //��� ���� ������ UI ��� ���� ��ġ�ǵ��� �ϱ�

            ShowTag(rotationTarget);  
            SteeringWheelControll TargetScript = rotationTarget.GetComponent<SteeringWheelControll>();
            TargetScript.Index_Handcenter_number = RotationHand_center;
            TargetScript.centerObj_SetParent(0);
            TargetScript.centerObj_SetParent(1);

            #region ���峭 Ÿ������ �Ǻ��� �� �ֵ��� �� �����ϴ� �κ�
            //���峭 Ÿ������ �ƴ���
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

            #region ��긦 ��¦ ������ üũ�ϴ� Ÿ������ �ƴ��� �Ǻ��ϵ��� �� �����ϴ� �κ�
            //üũ�ϴ� Ÿ������ �ƴ���
            if (InfoStruct[i].check == true)
                TargetScript.check = true;
            else
                TargetScript.check = false;
            #endregion

            TargetScript.enabled = true;

            #region ���� ��� ������ ���� ����� ���� ���� ���ֱ� => ���Ŀ� �ٽ� ���ֱ�
            //���� ������ ���� ���ֱ�
            if (TargetScript.FootHold != null)
            {
                TargetScript.FootHold.SetActive(true);
                TargetScript.FootHold.GetComponent<FootHold>().Init();
            }
            #endregion

            #region �±׿� ��ġ�ų�, ��� ��ü�� UI�� ������ �������� ��ġ�� �����ϴ� ���� ������ �� �� ��� ��������ϰ�, �ƴϸ� �ڵ����� �ٿ ������ ��ŭ ���� �����ǰ� �ϱ�
            InfoPanel.transform.position = new Vector3(rotationTarget.position.x, rotationTarget.GetComponent<MeshRenderer>().bounds.size.y / 2 + TargetScript.InfoPanel_up_Position_Value + rotationTarget.position.y, rotationTarget.position.z);

            //if (OK.activeSelf) OK.SetActive(false);     //�Ϸ�Ǿ��ٴ� üũ UI�̹��� ��Ȱ��ȭ ��Ű��
            MainPanel.transform.Find("Title").GetComponent<Text>().text = InfoStruct[i].Standard;
            MainPanel.transform.Find("Unit").GetComponent<Text>().text = InfoStruct[i].Unit;

            #endregion

            #region (InfoStruct[i].count) �����ߵǴ� Ƚ���� int�϶�
            if ((InfoStruct[i].count % 1) / 0.1 == 0)
            {
                TargetScript.SetData(InfoStruct[i].positive, InfoStruct[i].count, InfoStruct[i].SettingStartAngle, 1f, InfoStruct[i].EndRo);

                //���� ��ġ ����
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

                            //��ݹ����� 100% -> 0%�� ������.
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

                            //��ݹ����� 100% -> 0%�� ������.
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
            #region �����ߵǴ� Ƚ���� �Ҽ����϶���, �������ϴ� Ƚ�����ƴ�, ��������ϴ� float�� ���̴�.
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

                        if (!InfoStruct[i].trouble && !InfoStruct[i].check) //���� �� ������
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
                        else  //���� ������
                        {
                            float Set_N = 0;

                            //��ݹ����� 100% -> 0%�� ������.
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

            //count��ŭ ������ true => isHandle
            yield return waitUntil;

            //HideTag(rortationTarget);   //�±� ���ֱ�

            /*if (acm != null) acm.Check_IsAudioSoundAndOFF();   //���尡 Ʋ���� �ִٸ� ���ֱ�*/
            LookCheck.instance.Tracker(false);
            TargetScript.enabled = false;
            yield return waitForSeconds;
            #region ���� ����� �־��ٸ� ���� ���ֱ�
            if (TargetScript.FootHold != null)
            {
                TargetScript.FootHold.GetComponent<FootHold>().ReturnHigh();
                TargetScript.FootHold.GetComponent<FootHold>().Init();
                TargetScript.FootHold.SetActive(false);
            }
            #endregion
            #region ��Ȱ��ȭ �ؾ��� ������Ʈ�� �ִٸ� ���ֱ�
            if (TargetScript.eventActiveTarget != null && TargetScript.eventActiveTarget.activeSelf)
                TargetScript.eventActiveTarget.SetActive(false);
            #endregion         
            MinimapManager.instance.PointState(targets[i].name, "End");
            OK.SetActive(false);
            rotationTarget.GetComponent<MeshCollider>().enabled = false;
            // �ùķ����ͷ� �� �۽�
            //targets[i].GetComponent<Simulator>().Send();

            //������ ������� 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                InfoPanel.GetComponent<VR_Popup>().enabled = false;
                InfoPanel.SetActive(false);
                // ��� ���������� ������ ���������� �Ѿ��
                MinimapManager.instance.Initialize();
                SeqManager.instance.IsOn_true();
                InfoStruct = null;
            }

        }

    }

    /// <summary>
    /// ü�� ��� ���� Ÿ��
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    public IEnumerator ChainRotation(List<Transform> targets)
    {
        WaitUntil waitUntil = new WaitUntil(() => isHandle);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);

        InfoPanel.SetActive(true);
        InfoPanel.GetComponent<VR_Popup>().enabled = true;

        //3. TagNo�� ��ϵ� ��� ������ ���������� Handle�� ���� => Handle �ڵ鿡 ��ũ��Ʈ �ֱ�
        for (int i = 0; i < targets.Count; i++)
        {
            isHandle = false;
            List<GameObject> highLightList = new List<GameObject>();
            //AudioControlManager acm = targets[i].GetComponentInChildren<AudioControlManager>();
            //if (acm != null) acm.Check_IsAudioSoundAndON(targets[i].name);  //���尡 �����ϴ� ������Ʈ�� ������ Ʋ���ֱ�.

            foreach (var Chain in targets[i].GetComponentsInChildren<Transform>())
            {
                if (Chain.tag == "Chain") //��� ������ Ÿ�� �� �����ϰ� Chain ������Ʈ�� ������ ����
                {
                    // ���� ������ �����Ҷ�

                    //(3) ã�� �ڵ� ���̶���Ʈ ����
                    foreach (var item in Chain.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    //(5) ���� target ����
                    rotationTarget = Chain.transform;
                    rotationTarget.GetComponent<MeshCollider>().enabled = true;
                    Debug.Log("rortationTarget : " + rotationTarget.name, rotationTarget);
                    //(7) �ڵ��� ã������ foreach ������
                    break;
                }
            }

            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targets[i].transform));

            LookCheck.instance.Tracker(true, rotationTarget.name + "��(��) �����ϼ���.", rotationTarget);

            //��� ���� ������ UI ��� ���� ��ġ�ǵ��� �ϱ�

            ShowTag(rotationTarget);
            ChainRotation TargetScript = rotationTarget.GetComponent<ChainRotation>();
            InfoPanel.transform.position = new Vector3(rotationTarget.position.x, rotationTarget.GetComponent<MeshRenderer>().bounds.size.y / 2 + TargetScript.InfoPanel_up_Position_Value + rotationTarget.position.y, rotationTarget.position.z);
            TargetScript.enabled = true;

            //Debug.Log(InfoStruct[i].count % 1);
            MainPanel.transform.Find("Title").GetComponent<Text>().text = InfoStruct[i].Standard;
            MainPanel.transform.Find("Unit").GetComponent<Text>().text = InfoStruct[i].Unit;

            #region (InfoStruct[i].count) �����ߵǴ� Ƚ���� int�϶�
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

                        //��ݹ����� 100% -> 0%�� ������.
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

            //count��ŭ ������ true => isHandle
            yield return waitUntil;

            //if (acm != null) acm.Check_IsAudioSoundAndOFF();   //���尡 Ʋ���� �ִٸ� ���ֱ�
            //Destroy(rortationTarget.GetComponent<RotationCheck>()); //�� �������� ���� ����� ��ũ��Ʈ ����
            //RotationInit(); //�ʱ�ȭ
            LookCheck.instance.Tracker(false);
            TargetScript.enabled = false;
            yield return waitForSeconds;
            MinimapManager.instance.PointState(targets[i].name, "End");
            OK.SetActive(false);
            rotationTarget.GetComponent<MeshCollider>().enabled = false;
            // �ùķ����ͷ� �� �۽�
            //targets[i].GetComponent<Simulator>().Send();

            //������ ������� 
            if (i == (targets.Count - 1))
            {
                yield return waitForSeconds;
                InfoPanel.GetComponent<VR_Popup>().enabled = false;
                InfoPanel.SetActive(false);
                // ��� ���������� ������ ���������� �Ѿ��
                MinimapManager.instance.Initialize();
                SeqManager.instance.IsOn_true();
                InfoStruct = null;
            }

        }
    }


    /// <summary>
    /// ������ ���ܱ� ���
    /// </summary>
    /// <param name="targets">���ܱ� �±׳ѹ� List</param>
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
                if (Circuit.tag == "Circuit") //���� Ÿ�� �� �����ϰ� Handle ������Ʈ�� ������ ����
                {
                    foreach (var item in Circuit.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    rotationTarget = Circuit.transform;
                    break;
                }
            }

            LookCheck.instance.Tracker(true, rotationTarget.name + "��(��) �����ϼ���.", rotationTarget);
            yield return waitUntil;
            LookCheck.instance.Tracker(false);
            // �ùķ����ͷ� �� �۽�
            //targets[i].GetComponentInParent<Simulator>().Send();

            //������ ������� 
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
                if (Circuit.tag == "Circuit") //���� Ÿ�� �� �����ϰ� Handle ������Ʈ�� ������ ����
                {
                    foreach (var item in Circuit.GetComponentsInChildren<MeshFilter>())
                        highLightList.Add(item.gameObject);
                    Highlight.instance.On(highLightList.ToArray());
                    rotationTarget = Circuit.transform;
                    break;
                }
            }

            LookCheck.instance.Tracker(true, rotationTarget.name + "��(��) �����ϼ���.", rotationTarget);

            yield return waitUntil;

            MinimapManager.instance.PointState(targets[i].name, "End");
            LookCheck.instance.Tracker(false);

            // �ùķ����ͷ� �� �۽�
            //targets[i].GetComponentInParent<Simulator>().Send();

            //������ ������� 
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

            LookCheck.instance.Tracker(true, rotationTarget.name + "��(��) �����ϼ���.", rotationTarget);

            yield return waitUntil;
            MinimapManager.instance.PointState(targets[i].name, "End");
            LookCheck.instance.Tracker(false);

            // �ùķ����ͷ� �� �۽�
            //targets[i].GetComponentInParent<Simulator>().Send();

            //������ ������� 
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

            //AudioSource�� ������ �־��ֱ�
            AudioControlManager acm = targets[i].GetComponentInChildren<AudioControlManager>();

            LookCheck.instance.Tracker(true, rotationTarget.name + "��(��) �����ϼ���.", rotationTarget);

            yield return waitUntil;

            //���尡 �����ϴ� ������Ʈ�� ������ Ʋ���ֱ�.
            if (acm != null) acm.Check_IsAudioSoundAndON(targets[i].name);

            MinimapManager.instance.PointState(targets[i].name, "End");
            LookCheck.instance.Tracker(false);

            // �ùķ����ͷ� �� �۽�
            //targets[i].GetComponentInParent<Simulator>().Send();

            //������ ������� 
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

    #region Postitioner Ÿ��
    public void PostitionerValve(DataRow dataRow)
    {
        postitionerState = PostitionerMode.RUNPV;

        //�̰Ŵ� �ٸ� ��� ������ �����ؼ� �ϳ��� ������...
        for (int j = 0; j < SeqManager.tagNoList.Count; j++)
        {
            if (SeqManager.tagNoList[j].name.Equals(dataRow["TagNo"].ToString().Trim()))
            {
                //ex. targets�� ���� Obj 5-511-V-0071
                loadingBar = SeqManager.tagNoList[j].Find("Postitioner/Canvas/LoadingBar").GetComponent<Image>();
                StartCoroutine(PostitionerProcess(SeqManager.tagNoList[j]));
                //2023-01-10 �ֿ�ȣ �߰�
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

        #region 1. �ش� �� �ʱ� ����  
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

        LookCheck.instance.Tracker(true, target.name + "�� Postitioner��(��) �����ϼ���.", postitionerTarget);



        // ���� ������ �����Ҷ�
        MinimapManager.instance.PointState(postitionerTarget.name, "Start");
        #endregion

        #region 2. ���� ��ư�� Function ����
        for (int j = 0; j < postitioner.Length; j++)
        {
            //1. ��ũ��Ʈ Ű��
            postitioner[j].enabled = true;
            if (postitioner[j].GetComponent<PostitionerLoading>())
                postitioner[j].GetComponent<PostitionerLoading>().enabled = true;

            //2. ��ư�� ��� ����
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

        // ���� ������ ��������
        MinimapManager.instance.PointState(postitionerTarget.name, "End");

        LookCheck.instance.Tracker(false);
        // ��� ���������� ������ ���������� �Ѿ��
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
        //1. �Ҹ� ����
        AudioManager.instance.PlayMultiAudio("Sound/MP_Button");

        //2. ���� �ֱ�
        SteamVR_Action_Vibration hapticAntion = SteamVR_Actions._default.Haptic;
        hapticAntion.Execute(0, 0.3f, 50, 100, SteamVR_Input_Sources.RightHand, true);

        isFirst = false;
    }
    #endregion

    #region ��� ���� ��� TagNo�� ������ �Ѱ��϶��� ��
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

        LookCheck.instance.Tracker(true, target.name + "�� ��극���� �����ϼ���.", target.transform);

        // ���� ������ �����Ҷ�
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

        // target�� �ڵ��̶� �θ�� �������ش�.
        target = target.parent;
        GameObject lc;
        GameObject tagPanel;

        #region �±׸� �ߺ������� ������ �����Ƿ�, ����ó�� - 2022-12-06 YH
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
        /////Tag�� ���� Description �߰� [������ Description�κ��� ������ ��]/////
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
