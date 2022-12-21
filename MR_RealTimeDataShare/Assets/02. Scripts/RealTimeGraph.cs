using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealTimeGraph : MonoBehaviour
{
    public Transform pointPrefab;
    [Range(10, 100)] public int resolution = 50; //cube 개수
    Transform[] points;

    void Awake()
    {
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        Vector3 position;
        position.y = position.z = 0f;
        points = new Transform[resolution];
        for (int i = 0; i < resolution; i++)
        {
            Transform point = Instantiate(pointPrefab); //큐브 생성

            //point.localPosition = Vector3.right * ((i+0.5f) / 5 - 1f);
            position.x = (i + 0.5f) * step - 1f;
            //position.y = position.x * position.x * position.x;
            point.localPosition = position;
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
        }
    }
    void Update()
    {
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;            
            position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
            if (position.y >0)
                position.y -= 0.3f;
            else
                position.y += 0.3f;
            //position.y = Mathf.Sin(Mathf.PI * ());
            //position.y = position.x * position.x * position.x;
            point.localPosition = position;
        }
    }


}