using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareModelOnOff : MonoBehaviour
{

    public GameObject originalModel;
    public GameObject compareModel;
    public Material orignalModelColor;
    public Material compareModelColor;
    public MeshRenderer[] originalModelMesh;
    public MeshRenderer[] originalModelMeshBackUP;
    public MeshRenderer[] compareModelMesh;

    bool IsCompare = false;

    #region 렌더러 모드 변경 후 모델을 투명하게 하는 코드
    public void CompareModeOn()
    {
        if (!IsCompare)
        {
            IsCompare = true;
            compareModel.SetActive(true);
        
            originalModelMeshBackUP = originalModel.transform.GetComponentsInChildren<MeshRenderer>();

            originalModelMesh = originalModel.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer item in originalModelMesh)
            {
                for (int i = 0; i < item.materials.Length; i++)
                {
                    ChangeRenderMode(item.materials[i], BlendMode.Transparent);
                    Color color = item.materials[i].color;
                    color.a = 0.5f;
                    item.materials[i].color = color;
                }
            
            }

            compareModelMesh = compareModel.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer item in compareModelMesh)
            {
                item.material = compareModelColor;
            }
        }
        else
        {
            IsCompare = false;
            compareModel.SetActive(false);

            foreach (MeshRenderer item in originalModelMeshBackUP)
            {
                for (int i = 0; i < item.materials.Length; i++)
                {
                    ChangeRenderMode(item.materials[i], BlendMode.Opaque);
                    Color color = item.materials[i].color;
                    color.a = 1f;
                    item.materials[i].color = color;
                }
                
            }
        }
    }
    #endregion 

    #region 렌더러 모드로 인해 투명하게 바뀐 것을 원래 색깔로 되돌리는 코드
    public void CompareModeOff()
    {
        compareModel.SetActive(false);

        foreach (MeshRenderer item in originalModelMeshBackUP)
        {
            item.material = orignalModelColor;
        }
    }
    #endregion

    #region 렌더러 모드 변경 코드
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
}
