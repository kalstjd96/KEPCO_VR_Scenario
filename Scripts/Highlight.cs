using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Highlight : MonoBehaviour
{
    public static Highlight instance { get; private set; }
    [SerializeField] Material highlightMaterial;
    [SerializeField] Material[] highlightMaterials;

    #region 복수개의 게임오브젝트에 사용되는 파라미터
    MeshRenderer[] targets;
    List<Color[]> colorList;
    #endregion

    #region 단수개의 게임오브젝트에 사용되는 파라미터
    MeshRenderer target;
    Color[] colorPack;
    #endregion

    bool isHighLight;
    bool isOneHighLight;

    void Awake()
    {
        if (instance == null)
            instance = this;

        isHighLight = false;
        isOneHighLight = false;
    }

    void OnDestroy()
    {
        if (instance != null)
            instance = null;
    }

    #region Rendering Mode를 바꾸기 위해 사용한 코드
    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }

    }
    #endregion

    public IEnumerator HighlightOn(float time = 1f)
    {
        if (ConnManager.instance.PlayMode == RoomData.Mode.Training) yield break;

        while (true)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Material targeMat = highlightMaterial;
                Color targetColor = targeMat.color;
                targetColor.a = 0.5f;

                for (int j = 0; j < targets[i].materials.Length; j++)
                {
                    int index = j;
                    Color myColor = colorList[i][index];
                    //myColor.a = 1f;

                    //ChangeRenderMode(targets[i].materials[j], BlendMode.Transparent);
                    targets[i].materials[j] = targeMat;
                    targets[i].materials[j].SetColor("_Color",
                        Color.Lerp(myColor, targetColor, Mathf.PingPong(Time.time, time)));
                }
            }
            yield return null;
        }
    }

    public IEnumerator OneHighlightOn(float time = 1f)
    {
        if (ConnManager.instance.PlayMode == RoomData.Mode.Training) yield break;

        while (true)
        {
            Material targeMat = highlightMaterial;
            Color targetColor = targeMat.color;
            targetColor.a = 0.5f;

            for (int j = 0; j < target.materials.Length; j++)
            {
                int index = j;
                Color myColor = colorPack[index];
                //myColor.a = 1f;

                //ChangeRenderMode(target.materials[j], BlendMode.Transparent);
                target.materials[j] = targeMat;
                target.materials[j].SetColor("_Color",
                    Color.Lerp(myColor, targetColor, Mathf.PingPong(Time.time, time)));
            }
            yield return null;
        }
    }

    //해당 모델의 색상을 저장하고 하이라이트 동작을 호출
    public void On(GameObject[] models)
    {
        if (ConnManager.instance.PlayMode == RoomData.Mode.Training) return;

        if (isHighLight) 
            Off();
        
        //하이라이트가 시작이 되었다.
        isHighLight = true;

        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        for (int i = 0; i < models.Length; i++)
        {
            foreach (var renderer in models[i].GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderers.Add(renderer);
            }
        }

        targets = new MeshRenderer[meshRenderers.Count];
        colorList = new List<Color[]>();

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = meshRenderers[i];
            Color[] oriColor = new Color[targets[i].materials.Length];
            for (int j = 0; j < oriColor.Length; j++)
            {
                if (targets[i].materials[j].HasProperty("_Color"))
                {
                    Color color = targets[i].materials[j].color;
                    //color.a = 1;
                    oriColor.SetValue(color, j);
                }
            }
            colorList.Add(oriColor);
        }
        StartCoroutine(HighlightOn());
        
    }

    public void On(GameObject model)
    {
        isHighLight = true;
        isOneHighLight = true;

        if (ConnManager.instance.PlayMode == RoomData.Mode.Training) return;

        target = new MeshRenderer();
        target = model.GetComponent<MeshRenderer>();

        Color[] oriColor = new Color[target.materials.Length];

        for (int j = 0; j < oriColor.Length; j++)
        {
            if (target.materials[j].HasProperty("_Color"))
            {
                Color color = target.materials[j].color;
                oriColor.SetValue(color, j);
            }
        }
        colorPack = oriColor;
        StartCoroutine(OneHighlightOn());
    }

    public void Off()
    {
        if (ConnManager.instance.PlayMode == RoomData.Mode.Training) return;
        
        isHighLight = false;

        StopAllCoroutines();

        if (targets != null && targets.Length > 0)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Material[] materials = targets[i].materials;
                
                for (int j = 0; j < materials.Length; j++)
                {
                    Color color = colorList[i][j];
                    //color.a = 1;
                    targets[i].materials[j].color = color;
                    //ChangeRenderMode(targets[i].materials[j], BlendMode.Opaque);
                }
            }

            targets = null;
            colorList = null;
        }

        #region 단일 하이라이트를 위해서 추가
        if (isOneHighLight && target != null)
        {
            isOneHighLight = false;
            Material[] material = target.materials;

            for (int i = 0; i < material.Length; i++)
            {
                Color color = colorPack[i];
                //color.a = 1;
                target.materials[i].color = color;
                //ChangeRenderMode(target.materials[i], BlendMode.Opaque);
            }

            target = null;
            colorPack = null;
        }
        #endregion

    }

    

}