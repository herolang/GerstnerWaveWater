using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum WaveMode
{
    Sin,
    Gerstner,
}
public class WaterWave : MonoBehaviour
{
    Mesh mesh;
    public float height;
    public WaveMode waveMode = WaveMode.Gerstner;
    //尖锐
    [Range(0, 1f)]
    public float sharp = 0.5f;
    [Range(0.5f, 10f)]
    public float speed = 2f;
    [Range(1, 50)]
    public int waveT;//周期
    public Vector3 WaveDir = Vector3.left;

    private Vector3[] baseVertices;
    private void OnEnable()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
    }

    void Update()
    {
        if (waveMode == WaveMode.Sin)
        {
            Vector3[] vertices = this.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertice = this.baseVertices[i];
                float A = this.height;
                float w = (float)(2 * Math.PI / waveT);
                if (A >= 0.4f) A = 0.3f;
                vertice.x += A * Mathf.Sin(Time.time * speed + w * Vector2.Dot(WaveDir, new Vector2(vertices[i].x, vertices[i].z)));
                vertice.z += A * Mathf.Sin(Time.time * speed + w * Vector2.Dot(WaveDir, new Vector2(vertices[i].x, vertices[i].z)));
                vertice.y += A * Mathf.Sin(Time.time * speed + w * Vector2.Dot(WaveDir, new Vector2(vertices[i].x, vertices[i].z)));
                vertices[i] = vertice;
            }
            this.mesh.vertices = vertices;
            this.mesh.RecalculateNormals();
        }
        else if (waveMode == WaveMode.Gerstner)
        {
            Vector3[] vertices = this.mesh.vertices;
            Vector3[] normals = this.mesh.normals;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertice = this.baseVertices[i];
                Vector3 normal = this.baseVertices[i];
                Vector3 P = Vector3.zero;
                float A = this.height;
                //float w = (float)(2 * Math.PI / waveT);
                float w = (float)(1f / waveT);
                float WA = w * A;
                float Qi = sharp / WA;
                float cosNum = Mathf.Cos(Time.time * speed + w * Vector2.Dot(WaveDir, new Vector2(vertices[i].x, vertices[i].z)));
                float sinNum = Mathf.Sin(Time.time * speed + w * Vector2.Dot(WaveDir, new Vector2(vertices[i].x, vertices[i].z)));

                vertice.x += Qi * A * WaveDir.x * cosNum;
                vertice.z += Qi * A * WaveDir.y * cosNum;
                //vertice.y = sinNum * A;
                vertice = vertice + normals[i] * sinNum * A;

                vertices[i] = vertice;
                P = new Vector3(vertice.x, vertice.z, vertice.y);
                float sFun = Mathf.Sin(w * Vector3.Dot(WaveDir, P) + speed * Time.time);
                float cFun = Mathf.Cos(w * Vector3.Dot(WaveDir, P) + speed * Time.time);

                float nx = WaveDir.x * WA * cFun;
                float nz = WaveDir.y * WA * cFun;
                float ny = Qi * WA * sFun;

                normal.x = -(nx);
                normal.z = -(nz);
                //本来是normal.y = 1 - (ny);的，后来发现不对，但是在shader里不会有问题。
                normal.y = 1 - (ny-1);
                normal = Vector3.Normalize(normal);
                normals[i] = normal;
            }
            this.mesh.vertices = vertices;
            //this.mesh.normals = normals;
            //this.mesh.RecalculateNormals();
        }
    }
}
