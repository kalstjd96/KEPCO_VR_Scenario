using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void Function();

public class LookCheck : MonoBehaviour
{
    Function onComplete;

    Vector2 length;
    Vector2 direction;
    Vector3 targetVector;
    [Header("������")]
    public float speed = 10f;
    public float checkRange = 0.2f;
    public float checkTime;
    List<GameObject> targets;
    List<GameObject> highLightTarget;
    float zDirection;
    float targetRange;
    private Transform target;
    float currentValue;
    public Image loadingBar;
    [NonReorderable] public bool isCheck;
    bool isAnimation;

    [Header("���̵�ȭ��ǥ")]
    public bool showGuideArrow = true;
    public Transform arrow;
    public Text guideText;
    Transform oriTransform;
    public static LookCheck instance { get; set; }

    void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }


    private void Awake()
    {
        if (instance == null)
            instance = this;

        highLightTarget = new List<GameObject>();
        loadingBar.fillAmount = 0f;
        isCheck = true;
        target = null;
        currentValue = 0f;
        isAnimation = false;
    }


    private void Start()
    {
        oriTransform = lookSymbolHighlight.parent;
        //Settings(targets);

        if (ConnManager.instance.PlayMode == RoomData.Mode.Training)
        {
            guideText.GetComponent<Text>().enabled = false;
            arrow.GetComponent<Image>().enabled = false;
            if (lookSymbolHighlight.transform.Find("HotspotEffect"))
                lookSymbolHighlight.transform.Find("HotspotEffect").gameObject.SetActive(false);
            foreach (var item in lookSymbolHighlight.GetComponentsInChildren<Image>(true))
            {
                item.enabled = false;
            }
        }
    }
    public void Init()
    {
        StopAllCoroutines();

        highLightTarget = new List<GameObject>();
        loadingBar.fillAmount = 0f;
        isCheck = true;
        target = null;
        targets = new List<GameObject>();
        currentValue = 0f;
        if (arrow)
            arrow.gameObject.SetActive(false);
        if (guideText)
            guideText.gameObject.SetActive(false);
        if (lookSymbolHighlight)
            lookSymbolHighlight.gameObject.SetActive(false);

        Highlight.instance.Off();
    }

    public void Settings(List<GameObject> targets, Function onComplete = null)
    {
        this.onComplete = onComplete;

        Init();
        this.targets = targets;
        StartCoroutine(LookProcess());
    }

    bool objHighLight = false;
    public Transform lookSymbolHighlight;

    public IEnumerator LookProcess()
    {
        WaitUntil waitUntil = new WaitUntil(() => isCheck);
        WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);
        for (int i = 0; i < targets.Count; i++) // 9-000- ��¼�� TagNo Obj����
        {
            if (guideText)
            {
                if (!SeqManager.instance.dataRow["isFade"].ToString().Contains("MCR"))
                {
                    guideText.gameObject.SetActive(true);
                    guideText.text = "<color=#FFCC00>" + targets[i].name + "</color>��(��) Ȯ���ϼ���.";
                }
            }

            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targets[i].transform));


            if (targets[i].layer == 5) //Target�� UI�϶�
            {
                //1. ���̶���Ʈ UI �ѱ�.
                lookSymbolHighlight.gameObject.SetActive(true);
                if (lookSymbolHighlight.GetComponent<Canvas>().enabled == false)
                    lookSymbolHighlight.GetComponent<Canvas>().enabled = true;

                //2. ���̶���Ʈ UI �ش� �±׷� �̵���Ű��
                lookSymbolHighlight.SetParent(targets[i].transform);
                lookSymbolHighlight.localPosition = Vector3.zero;
                lookSymbolHighlight.localEulerAngles = Vector3.zero;
                //3. ���̶���Ʈ UI ������ ���߱�
                RectTransform highlgihtRect = lookSymbolHighlight.GetComponent<RectTransform>();
                highlgihtRect.anchorMin = Vector2.zero;
                highlgihtRect.anchorMax = Vector2.one;
                highlgihtRect.offsetMin = Vector2.zero;
                highlgihtRect.offsetMax = Vector2.zero;
                //highlgihtRect.sizeDelta = Vector2.one ;
                //highlgihtRect.sizeDelta =
                //new Vector3(targets[i].GetComponent<RectTransform>().rect.width + 50f, targets[i].GetComponent<RectTransform>().rect.height + 50f, -0.1f);
                highlgihtRect.localScale = Vector3.one;
            }
            else //obj�� ���
            {
                highLightTarget = new List<GameObject>();

                foreach (MeshFilter item in targets[i].GetComponentsInChildren<MeshFilter>())
                    highLightTarget.Add(item.gameObject);

                Highlight.instance.On(highLightTarget.ToArray());
                objHighLight = true;
            }

            target = targets[i].transform;

            // ���� ������ �����Ҷ� - 0802
            if (!SeqManager.instance.dataRow["isFade"].ToString().Contains("MCR"))
                MinimapManager.instance.Set(target.name, "Look");

            isCheck = false;

            //1. �ִϸ��̼��� �ִٸ� �������� �ѹ� ���� �� �ٷ� ���� - MS
            if (target.GetComponentInChildren<Animation>() != null)
            {
                yield return new WaitUntil(() => loadingBar.gameObject.activeSelf);
                Animation targetAni = target.GetComponentInChildren<Animation>();

                targetAni[targetAni.clip.name].time = 0;
                targetAni[targetAni.clip.name].speed = 1;
                targetAni.Play();
            }

            yield return waitUntil;

            // ���� ������ ��������
            MinimapManager.instance.PointState(target.name, "End");
            target = null;

            if (objHighLight)
            {
                Highlight.instance.Off(); objHighLight = false;
            }
            else
                lookSymbolHighlight.gameObject.SetActive(false);

            AudioManager.instance.PlayMultiAudio("Sound/complete");

            yield return waitForSeconds;

            if (i == (targets.Count - 1))
            {
                lookSymbolHighlight.SetParent(oriTransform);

                // ��� ���������� ������ ���������� �Ѿ��
                MinimapManager.instance.Initialize();
                Init();
                if (onComplete == null)
                    SeqManager.instance.IsOn_true();
                else
                {
                    onComplete.Invoke();
                    onComplete = null;
                }
            }
        }
    }
    public void TagetLookCheck()
    {
        Vector3 targetVector = Vector3.zero;
        if (target.gameObject.layer == 5) //Target�� UI�϶�
            targetVector = target.position;
        else targetVector = target.GetComponentInChildren<Renderer>() ? RendererCenter(target) : target.position;

        zDirection = Camera.main.transform.InverseTransformPoint(targetVector).z - Camera.main.transform.localPosition.z;
        Vector3 targetPosition = Camera.main.transform.InverseTransformPoint(targetVector);
        targetPosition.z = 0f;
        targetRange = targetPosition.magnitude;

        //Ÿ�� �Ÿ��� Player�� �Ÿ� ���
        float distance = Vector3.Distance(targetVector, Camera.main.transform.position);

        if (zDirection > 0 && targetRange < checkRange)
        {
            if (!loadingBar.gameObject.activeSelf)
                loadingBar.gameObject.SetActive(true);

            if (currentValue < 100 && loadingBar.gameObject.activeSelf)
            {
                loadingBar.fillAmount = currentValue / 100;
                currentValue += speed * Time.deltaTime;
            }
            else
            {
                loadingBar.transform.gameObject.SetActive(false);
                Highlight.instance.Off();
                currentValue = 0f;
                isCheck = true;
                //target = null;
            }
        }
        else
        {
            currentValue = 0f;
            loadingBar.fillAmount = 0f;
            loadingBar.transform.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCheck && target)
        {
            MinimapManager.instance.PointState(target.name, "Start");
            TagetLookCheck();   //�ٶ󺸾� ������ ä���

            if (!SeqManager.instance.dataRow["isFade"].ToString().Equals("MCR"))
                GuideArrow(target); //local���� ȭ��ǥ�� �ؽ�Ʈ�� ��� ���̵� ���ֱ�
        }
        if (isTracking && isCheck && lookTarget)
        {
            GuideArrow(lookTarget);
        }
    }

    //������ ����
    public Vector3 RendererCenter(Transform target)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer render in renderers)
        {
            if (!render.GetComponent<SpriteRenderer>())
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(render.bounds);
                }
                else
                {
                    bounds = render.bounds;
                    hasBounds = true;
                }
            }
        }
        return bounds.center;
    }

    #region ���̵�ȭ��ǥ
    public void GuideArrow(Transform target)
    {
        if (target == null ||
            (SeqManager.instance && SeqManager.instance.dataRow["isFade"].ToString().Contains("MCR")))
        {
            guideText.gameObject.SetActive(false);
            arrow.gameObject.SetActive(false);
            return;
        }

        Vector3 targetVector = Vector3.zero;
        if (target.gameObject.layer == 5) //Target�� UI�϶�
            targetVector = target.position;
        else targetVector = target.GetComponentInChildren<Renderer>() ? RendererCenter(target) : target.position;

        arrow.localScale = new Vector3(arrow.localScale.x, Mathf.Abs(arrow.localScale.y), arrow.localScale.z);
        Vector2 guideLength = Camera.main.transform.InverseTransformPoint(arrow.position) - Camera.main.transform.InverseTransformPoint(targetVector);
        Vector2 guideDirection = guideLength.normalized;
        float angle = Mathf.Atan2(guideDirection.x, guideDirection.y) * Mathf.Rad2Deg - 180;
        arrow.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle));


        float zDirection = Camera.main.transform.InverseTransformPoint(targetVector).z - Camera.main.transform.localPosition.z;
        Vector3 targetPosition = Camera.main.transform.InverseTransformPoint(targetVector);
        targetPosition.z = 0f;

        targetRange = targetPosition.magnitude;
        float distance = Vector3.Distance(targetVector, Camera.main.transform.position);

        if (zDirection > 0 && targetRange < checkRange)
        {
            guideText.gameObject.SetActive(false);
            arrow.gameObject.SetActive(false);
        }
        else
        {
            guideText.gameObject.SetActive(true);
            arrow.gameObject.SetActive(true);
        }
    }


    #region ���ϴ� ��, ���ϴ� ����, ���ϴ� �ؽ�Ʈ�� ���̵� ȭ��ǥ ����
    Transform lookTarget;
    bool isTracking;

    /// <summary>
    /// taget�� �ٶ󺸰� �� ��� ��������� ȭ��ǥ�� ���̵� ����.
    /// ********�ʱ�ȭ�� �� �ݵ�� Tracker(false) ȣ������� ȭ��ǥ �����
    /// </summary>
    /// <param name="isOn"></param>
    /// <param name="target"></param>
    /// ���� : ���̵� ȭ��ǥ ����ֱ� ���ϴ� ������ Tracker() �Լ� ����,
    ///          ������ ���� Tracker(false) �� ȣ�����ָ� ��
    public void Tracker(bool isOn, string guideText = "", Transform target = null)
    {
        lookTarget = target;
        isTracking = isOn;

        if (isOn)
        {
            GuideArrow(lookTarget);

            if (instance.guideText && !string.IsNullOrEmpty(guideText))
            {
                instance.guideText.gameObject.SetActive(true);
                instance.guideText.text = "<color=#FFCC00>" + guideText + "</color>";
            }
        }
        else
        {
            instance.guideText.gameObject.SetActive(false);
            GuideArrow(null);
        }
    }
    #endregion
    #endregion
}