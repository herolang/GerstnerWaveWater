using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class WaterWaves : MonoBehaviour
{
    Mesh mesh;
    [Range(0, 10f)]
    public float height = 0.2f;
    public WaveMode waveMode = WaveMode.Gerstner;
    //尖锐
    [Range(0, 1f)]
    public float sharp = 0.5f;
    [Range(0.5f, 10f)]
    public float[] speeds;
    public Vector3[] WaveDirs;
    [Range(1, 50)]
    public int[] waveTs;

    private Vector3[] baseVertices;
    private void OnEnable()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
    }

    void Update()
    {
        if (WaveDirs.Length != speeds.Length || WaveDirs.Length != waveTs.Length)
        {
            Debug.LogWarning("设置的变量不对！");
            return;
        }
        if (waveMode == WaveMode.Sin)
        {
            Vector3[] vertices = this.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertice = this.baseVertices[i];
                float A = this.height;
                for (int k = 0; k < WaveDirs.Length; k++)
                {
                    float w = (float)(2 * Math.PI / waveTs[k]);
                    if (A >= 0.4f) A = 0.3f;//A值太大会闪烁，不知道是什么问题
                    vertice.x += A * Mathf.Sin(Time.time * speeds[k] + w * Vector2.Dot(WaveDirs[k], new Vector2(vertices[i].x, vertices[i].z)));
                    vertice.z += A * Mathf.Sin(Time.time * speeds[k] + w * Vector2.Dot(WaveDirs[k], new Vector2(vertices[i].x, vertices[i].z)));
                    vertice.y += A * Mathf.Sin(Time.time * speeds[k] + w * Vector2.Dot(WaveDirs[k], new Vector2(vertices[i].x, vertices[i].z)));
                }
                vertices[i] = vertice;
            }
            this.mesh.vertices = vertices;
            this.mesh.RecalculateNormals();
        }
        else if (waveMode == WaveMode.Gerstner)
        {
            Vector3[] vertices = this.mesh.vertices;
            Vector3[] normals = this.mesh.normals;
            Vector4[] tangents = this.mesh.tangents;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertice = this.baseVertices[i];
                Vector3 normal = this.baseVertices[i];
                float allY = 0;
                for (int j = 0; j < WaveDirs.Length; j++)
                {
                    Vector3 ver = this.calSingleWaveVertexs(j, vertice);
                    vertice.x += ver.x;
                    vertice.z += ver.z;
                    allY += ver.y;
                }
                // vertice.y = allY;
                //这样波峰会更尖锐
                vertice = vertice + normals[i] * allY;
                vertices[i] = vertice;

                float nx = 0;
                float nz = 0;
                float ny = 0;
                float tx = 0;
                float tz = 0;
                float ty = 0;
                for (int j = 0; j < WaveDirs.Length; j++)
                {
                    Vector3 tangent;
                    Vector3 ver = this.calSingleWaveNormals(j, vertice, out tangent);
                    nx += ver.x;
                    nz += ver.z;
                    ny += ver.y;
                    tx += tangent.x;
                    tz += tangent.z;
                    ty += tangent.y;
                }
                if (tangents.Length > 0)
                {
                    tangents[i].x = -(tx);
                    tangents[i].z = 1 - (tz);
                    tangents[i].y = ty;
                }
                normal.x = -(nx);
                normal.z = -(nz);
                normal.y = 1 - (ny);
                normal = Vector3.Normalize(normal);
                normals[i] = normal;
            }
            this.mesh.vertices = vertices;
            // this.mesh.tangents = tangents;
            //this.mesh.normals = normals;
            //this.mesh.RecalculateNormals();
        }
    }

    private Vector3 calSingleWaveVertexs(int index, Vector3 vertice)
    {
        Vector3 ver = Vector3.zero;
        int waveT = waveTs[index];
        Vector2 WaveDir = WaveDirs[index];
        float speed = speeds[index];
        float w = (float)(2 * Math.PI / waveT);
        float A = this.height;
        //float w = (float)(2 * Math.PI / waveT);
        float Qi = sharp / (w * A);
        float cosNum = Mathf.Cos(w * Vector2.Dot(WaveDir, new Vector2(vertice.x, vertice.z)) + Time.time * speed);
        float sinNum = Mathf.Sin(w * Vector2.Dot(WaveDir, new Vector2(vertice.x, vertice.z)) + Time.time * speed);

        ver.x = Qi * A * WaveDir.x * cosNum;
        ver.z = Qi * A * WaveDir.y * cosNum;
        ver.y = sinNum * A;
        return ver;
    }

    private Vector3 calSingleWaveNormals(int index, Vector3 vertice, out Vector3 tangent)
    {
        Vector2 WaveDir = WaveDirs[index];
        int waveT = waveTs[index];
        Vector3 ver = Vector3.zero;
        float speed = speeds[index];
        Vector3 _tangent = Vector3.zero;
        Vector3 P = new Vector3(vertice.x, vertice.z, vertice.y);
        float A = this.height;
        float w = (float)(1f / waveT);
        //float w = (float)(2 * Math.PI / waveT);
        float WA = w * A;

        float Qi = sharp / WA;

        float sFun = Mathf.Sin(w * Vector3.Dot(WaveDir, P) + speed * Time.time);
        float cFun = Mathf.Cos(w * Vector3.Dot(WaveDir, P) + speed * Time.time);

        float nx = WaveDir.x * WA * cFun;
        float nz = WaveDir.y * WA * cFun;
        float ny = Qi * WA * sFun;
        ver.x = -nx;
        ver.z = -nz;
        ver.y = ny - 1;

        _tangent.x = Qi * WaveDir.x * WaveDir.y * WA * sFun;
        _tangent.z = Qi * WaveDir.y * WaveDir.y * WA * sFun;
        _tangent.y = -WaveDir.y * WA * cFun;

        tangent = Vector3.Normalize(_tangent);
        return ver;
    }
}
