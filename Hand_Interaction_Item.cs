using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Hand_Interaction_Item : MonoBehaviour
{
    [SerializeField] GameObject hand;
    //VR_ScrollView scrollView;
    //ScrollRect scrollRect;
    //Vector3 scrollVec3;

    //MS
    bool isScrollStart;
    float scrollTime = 0f;
    Transform content;
    Vector3 previous;
    public float speed;

    #region 원본
    //private void OnTriggerStay(Collider other)
    //{
    //    if (SteamVR_Actions.default_GrabGrip.GetState(SteamVR_Input_Sources.RightHand) && other.transform.GetComponentInParent<HandCollider>() != null)
    //    {
    //        if (scrollVec3 == Vector3.zero)
    //            scrollVec3 = other.transform.GetComponentInParent<HandCollider>().transform.localPosition;
    //        //scrollVec3 = other.transform.root.GetComponent<YoulSystem.VR_DragWorld>().GetControllerPosition();
    //        Vector3 currentPos = other.transform.GetComponentInParent<HandCollider>().transform.localPosition;

    //        if (scrollView != null && scrollRect != null)
    //        {
    //            scrollView.isClicked = true;

    //            if (scrollView.isClicked)
    //            {
    //                if (scrollView.isVertical)
    //                {
    //                    if (Mathf.Abs(scrollVec3.y - currentPos.y) > 0.005f && scrollVec3.y > currentPos.y)
    //                    {
    //                        scrollRect.verticalScrollbar.value = (scrollRect.verticalScrollbar.value + scrollView.sensitivity);
    //                        scrollVec3 = currentPos;
    //                    }
    //                    else if (Mathf.Abs(scrollVec3.y - currentPos.y) > 0.005f && scrollVec3.y < currentPos.y)
    //                    {
    //                        scrollRect.verticalScrollbar.value = (scrollRect.verticalScrollbar.value - scrollView.sensitivity);
    //                        scrollVec3 = currentPos;
    //                    }
    //                }

    //                if (scrollView.isHorizontal)
    //                {
    //                    if (Mathf.Abs(scrollVec3.x - currentPos.x) > 0.5f && scrollVec3.x > currentPos.x)
    //                    {
    //                        scrollRect.verticalScrollbar.value = (scrollRect.verticalScrollbar.value - scrollView.sensitivity);
    //                        scrollVec3 = currentPos;
    //                    }
    //                    else if (Mathf.Abs(scrollVec3.x - currentPos.x) > 0.5f && scrollVec3.x < currentPos.x)
    //                    {
    //                        scrollRect.horizontalScrollbar.value = (scrollRect.horizontalScrollbar.value + scrollView.sensitivity);
    //                        scrollVec3 = currentPos;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        scrollVec3 = Vector3.zero;

    //        if (scrollView != null)
    //            scrollView.isClicked = false;
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (transform.GetComponent<VR_ScrollView>() && other.gameObject.layer == LayerMask.NameToLayer("Interactive"))
    //    {
    //        scrollView = transform.GetComponent<VR_ScrollView>();
    //        scrollRect = transform.GetComponent<ScrollRect>();
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    //scrollView = null;
    //    //scrollRect = null;
    //}
    #endregion

    #region MS 텍스트 길이가 길어 다 보이지 않을 때 일정 시간이 되면 조금씩 올라간다.
    private void OnEnable()
    {
        scrollTime = 0f;
        content = transform.Find("Viewport/Content");
        isScrollStart = true;
    }

    private void OnDisable()
    {
        isScrollStart = false;
    }

    private void Update()
    {
        if (isScrollStart)
        {
            scrollTime += Time.deltaTime;
            if (scrollTime >= 2f)
                PopUpScroll();
        }
    }


    public void PopUpScroll()
    {
        Vector3 currentPos = content.GetComponent<RectTransform>().position;

        previous = content.GetComponent<RectTransform>().position;
        currentPos.y += speed;
        content.GetComponent<RectTransform>().position = currentPos;

        if (previous.y == content.GetComponent<RectTransform>().position.y)
            isScrollStart = false;
        
    }
    #endregion
}
