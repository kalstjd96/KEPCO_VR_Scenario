using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostitionerLoading : MonoBehaviour
{
    public Image loadingBar;
    public float currentValue;
    public float speed = 25f;

    public void Awake()
    {
        currentValue = 0f;
        loadingBar.fillAmount = 0f;
        loadingBar.transform.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (ControlManager.instance.isLoading && loadingBar != null)
            Loading();
        else if (!ControlManager.instance.isLoading)
            LoadBarInit();
    }

    public void Loading()
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
            ControlManager.instance.postitionerState = ControlManager.PostitionerMode.Manual;
            ControlManager.instance.valveText.text = "MANURL";
            ControlManager.instance.stateText.text = "";

            ControlManager.instance.isLoading = false;
            loadingBar.transform.gameObject.SetActive(false);
            currentValue = 0f;
        }
    }

    public void LoadBarInit()
    {
        currentValue = 0f;
        loadingBar.fillAmount = 0f;
        loadingBar.transform.gameObject.SetActive(false);
    }
}
