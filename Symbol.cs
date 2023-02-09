using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Symbol : MonoBehaviour
{
    [Header("게이지")]
    public Image loadingBar;
    public float speed = 20f;

    [System.NonSerialized] public bool isCheck;
    [System.NonSerialized] public bool isHover;

    float currentValue = 0f;

    // Start is called before the first frame update
    void Start()
    {
        isHover = false;
        isCheck = false;

        loadingBar = transform.Find("LoadingBar").GetComponent<Image>();
        loadingBar.fillAmount = 0f;

        if (ConnManager.instance.PlayMode == RoomData.Mode.Training)
        {
            foreach (var item in GetComponentsInChildren<Image>(true))
            {
                item.enabled = false;
            }
            transform.Find("HotspotEffect").gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SeqManager.instance.sequenceType.Equals("4"))
        {
            if (!isCheck && isHover)
                LoadingOn();
            else if (!isCheck && !isHover)
                LoadingOff();
        }
    }

    #region 하이라이트 로딩 게이지 이벤트

    public void LoadingOn()
    {
        // (1). 게이지 켜주기
        if (!loadingBar.gameObject.activeSelf)
            loadingBar.gameObject.SetActive(true);

        // (2). 게이지 증가
        if (currentValue < 100 && loadingBar.gameObject.activeSelf)
        {
            currentValue += speed * Time.deltaTime;
            loadingBar.fillAmount = currentValue / 100;
        }
        else
        {
            LoadingOff();
            isCheck = true;
            SeqManager.instance.symbolHighlight.SymbolDestroy(transform);
        }
    }

    public void LoadingOff()
    {
        currentValue = 0f;
        loadingBar.fillAmount = 0f;
        loadingBar.gameObject.SetActive(false);
    }

    #endregion
}
