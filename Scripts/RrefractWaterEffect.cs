using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RrefractWaterEffect : MonoBehaviour
{
    public Camera mainCam;
    private const float V = 20.0f;
    [Range(0, 1)]
    public float waveScale = 0.5f;
    public Vector4 waveSpeed = new Vector4(8f, 5f, -5f, -1f);
    Renderer render;
    public Material mat;
    private Camera reflectCamera;
    private Camera refractCamera;
    private RenderTexture refleRT;
    private RenderTexture refraRT;
    private int texSize = 256;

    void Awake()
    {
        render = GetComponent<Renderer>();
        render.sharedMaterial = mat;
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        if (refleRT)
        {
            refleRT = null;
        }
        if (refraRT)
        {
            refraRT = null;
        }
    }

    private void OnWillRenderObject()
    {
        if (reflectCamera == null)
        {
            reflectCamera = createCamera(true);
            UpdateCameraModes(mainCam, reflectCamera);
        }
        reflectCamera.cullingMask = ~(1 << 4); // never render water layer
        if (refleRT == null)
        {
            refleRT = new RenderTexture(texSize, texSize, 16);
            refleRT.hideFlags = HideFlags.DontSave;
            reflectCamera.targetTexture = refleRT;
        }

        //求反射矩阵
        GameObject reflectPanel = gameObject;
        Vector3 pos = reflectPanel.transform.position;
        Vector3 normal = reflectPanel.transform.up;
        float d = -Vector3.Dot(normal,pos);
        Vector4 panelVec = new Vector4(normal.x, normal.y, normal.z, d);
        Matrix4x4 reflectMat = Matrix4x4.zero;
        culReflectMatrix(ref reflectMat, panelVec);

        reflectCamera.worldToCameraMatrix = mainCam.worldToCameraMatrix* reflectMat;
        Vector4 reflectPanelVec = new Vector4();
        reflectPanelVec = calCameraSpacePanel(reflectCamera,pos,normal,1);
        reflectCamera.projectionMatrix = mainCam.CalculateObliqueMatrix(reflectPanelVec);
        //选择是否翻转背面裁剪
        GL.invertCulling = true;
        reflectCamera.Render();
        GL.invertCulling = false;
        render.sharedMaterial.SetTexture(Shader.PropertyToID("_ReflectTex"), refleRT);

        if (refractCamera == null)
        {
            refractCamera = createCamera(false);
            UpdateCameraModes(mainCam, refractCamera);

        }
        refractCamera.cullingMask = ~(1 << 4); // never render water layer
        if (refraRT == null)
        {
            refraRT = new RenderTexture(texSize, texSize, 16);
            refraRT.hideFlags = HideFlags.DontSave;
            refractCamera.targetTexture = refraRT;
        }
        refractCamera.Render();

        render.sharedMaterial.SetTexture(Shader.PropertyToID("_RefractTex"), refraRT);
    }

    void Update()
    {
        float t = Time.timeSinceLevelLoad / V;
        Vector4 waveScale4 = new Vector4(waveScale, waveScale, waveScale * 0.4f, waveScale * 0.45f);
        //Math.IEEERemainder(a,b)返回值是 a - (b*(round(a/b)),下面基本是a - round(a)，基本是a-1
        Vector4 offsetClamped = new Vector4(
            (float)Math.IEEERemainder(waveSpeed.x * waveScale4.x * t, 1.0),
            (float)Math.IEEERemainder(waveSpeed.y * waveScale4.y * t, 1.0),
            (float)Math.IEEERemainder(waveSpeed.z * waveScale4.z * t, 1.0),
            (float)Math.IEEERemainder(waveSpeed.w * waveScale4.w * t, 1.0)
            );
        render.sharedMaterial.SetVector(Shader.PropertyToID("_WaveScale"), waveScale4);
        render.sharedMaterial.SetVector(Shader.PropertyToID("_WaveOffset"), offsetClamped);
    }

    Camera createCamera(bool isRefrect)
    {
        GameObject obj = new GameObject();
        Camera cam = null;
        if (isRefrect)
        {
            obj.name = "reflectCamera";
            cam = obj.AddComponent<Camera>();
            obj.transform.position = mainCam.transform.position;
            obj.transform.eulerAngles = mainCam.transform.eulerAngles;
        }
        else
        {
            obj.name = "refractCamera";
            cam = obj.AddComponent<Camera>();
            obj.transform.position = mainCam.transform.position;
            obj.transform.eulerAngles = mainCam.transform.eulerAngles;
        }
        return cam;
    }

    //panelVec前三个元素是法向量的值，最后一个元素是d。
    void culReflectMatrix(ref Matrix4x4 refleMat,Vector4 panelVec)
    {
        refleMat.m00 = 1 - 2 * panelVec[0] * panelVec[0];
        refleMat.m01 = - 2 * panelVec[0] * panelVec[1];
        refleMat.m02 = -2 * panelVec[0] * panelVec[2];
        refleMat.m03 = -2 * panelVec[3] * panelVec[0];

        refleMat.m10 = - 2 * panelVec[0] * panelVec[1];
        refleMat.m11 = 1-2 * panelVec[1] * panelVec[1];
        refleMat.m12 = -2 * panelVec[1] * panelVec[2];
        refleMat.m13 = -2 * panelVec[3] * panelVec[1];


        refleMat.m20 = -2 * panelVec[0] * panelVec[2];
        refleMat.m21 = -2 * panelVec[1] * panelVec[2];
        refleMat.m22 = 1-2 * panelVec[2] * panelVec[2];
        refleMat.m23 = -2 * panelVec[3] * panelVec[2];

        refleMat.m30 = 0;
        refleMat.m31 = 0;
        refleMat.m32 = 0;
        refleMat.m33 = 1;

    }
    //计算摄像机空间下的平面
    Vector4 calCameraSpacePanel(Camera cam,Vector3 pos,Vector3 normal,int sign)
    {
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cnormal = m.MultiplyVector(normal).normalized*sign;
        Vector3 cpos = m.MultiplyPoint(pos);
        float d = -Vector3.Dot(cnormal, cpos);
        return new Vector4(cnormal.x, cnormal.y, cnormal.z,d);
    }


    void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
        {
            return;
        }
        // set water camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;

        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }
}
