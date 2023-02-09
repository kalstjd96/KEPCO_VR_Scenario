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
    [Header("게이지")]
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

    [Header("가이드화살표")]
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
        for (int i = 0; i < targets.Count; i++) // 9-000- 어쩌고 TagNo Obj들임
        {
            if (guideText)
            {
                if (!SeqManager.instance.dataRow["isFade"].ToString().Contains("MCR"))
                {
                    guideText.gameObject.SetActive(true);
                    guideText.text = "<color=#FFCC00>" + targets[i].name + "</color>을(를) 확인하세요.";
                }
            }

            if (SeqManager.instance.playMoveType.Equals(SeqManager.PlayMoveType.Teleport) && i != 0)
                yield return StartCoroutine(SeqManager.instance.PlayerTeleportCor(targets[i].transform));


            if (targets[i].layer == 5) //Target이 UI일때
            {
                //1. 하이라이트 UI 켜기.
                lookSymbolHighlight.gameObject.SetActive(true);
                if (lookSymbolHighlight.GetComponent<Canvas>().enabled == false)
                    lookSymbolHighlight.GetComponent<Canvas>().enabled = true;

                //2. 하이라이트 UI 해당 태그로 이동시키기
                lookSymbolHighlight.SetParent(targets[i].transform);
                lookSymbolHighlight.localPosition = Vector3.zero;
                lookSymbolHighlight.localEulerAngles = Vector3.zero;
                //3. 하이라이트 UI 사이즈 맞추기
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
            else //obj일 경우
            {
                highLightTarget = new List<GameObject>();

                foreach (MeshFilter item in targets[i].GetComponentsInChildren<MeshFilter>())
                    highLightTarget.Add(item.gameObject);

                Highlight.instance.On(highLightTarget.ToArray());
                objHighLight = true;
            }

            target = targets[i].transform;

            // 설비 조작이 시작할때 - 0802
            if (!SeqManager.instance.dataRow["isFade"].ToString().Contains("MCR"))
                MinimapManager.instance.Set(target.name, "Look");

            isCheck = false;

            //1. 애니메이션이 있다면 게이지가 한번 켜질 때 바로 실행 - MS
            if (target.GetComponentInChildren<Animation>() != null)
            {
                yield return new WaitUntil(() => loadingBar.gameObject.activeSelf);
                Animation targetAni = target.GetComponentInChildren<Animation>();

                targetAni[targetAni.clip.name].time = 0;
                targetAni[targetAni.clip.name].speed = 1;
                targetAni.Play();
            }

            yield return waitUntil;

            // 설비 조작이 끝났을때
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

                // 모든 설비조작이 끝나고 다음절차로 넘어갈때
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
        if (target.gameObject.layer == 5) //Target이 UI일때
            targetVector = target.position;
        else targetVector = target.GetComponentInChildren<Renderer>() ? RendererCenter(target) : target.position;

        zDirection = Camera.main.transform.InverseTransformPoint(targetVector).z - Camera.main.transform.localPosition.z;
        Vector3 targetPosition = Camera.main.transform.InverseTransformPoint(targetVector);
        targetPosition.z = 0f;
        targetRange = targetPosition.magnitude;

        //타겟 거리와 Player의 거리 계산
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
            TagetLookCheck();   //바라보아 게이지 채우기

            if (!SeqManager.instance.dataRow["isFade"].ToString().Equals("MCR"))
                GuideArrow(target); //local에서 화살표와 텍스트로 대상 가이드 해주기
        }
        if (isTracking && isCheck && lookTarget)
        {
            GuideArrow(lookTarget);
        }
    }

    //렌더러 센터
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

    #region 가이드화살표
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
        if (target.gameObject.layer == 5) //Target이 UI일때
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


    #region 원하는 때, 원하는 설비에, 원하는 텍스트로 가이드 화살표 띄우기
    Transform lookTarget;
    bool isTracking;

    /// <summary>
    /// taget에 바라보게 할 대상 집어넣으면 화살표가 가이드 해줌.
    /// ********초기화할 때 반드시 Tracker(false) 호출해줘야 화살표 사라짐
    /// </summary>
    /// <param name="isOn"></param>
    /// <param name="target"></param>
    /// 사용법 : 가이드 화살표 띄워주기 원하는 시점에 Tracker() 함수 실행,
    ///          꺼야할 때는 Tracker(false) 만 호출해주면 됨
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