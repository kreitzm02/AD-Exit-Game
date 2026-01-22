using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonHighlight : MonoBehaviour
{
    [Header("Authoring (optional)")]
    [SerializeField] private PolygonCollider2D sourcePolygon;

    [Header("Visuals")]
    [SerializeField] private Color fillColor = new Color(1f, 1f, 1f, 0.20f);
    [SerializeField] private Color outlineColor = new Color(1f, 1f, 1f, 0.90f);
    [SerializeField] private float outlineWidth = 0.02f;
    [SerializeField] private float zOffset = 0.01f;

    [Header("Orientation")]
    [SerializeField] private bool billboardToCamera = false;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;
    private Mesh mesh;

    private void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        lineRenderer = GetComponent<LineRenderer>();
        if (!lineRenderer) lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = outlineWidth;

        ApplyColors();

        RebuildFromSource();
        SetVisible(false);
    }

    private void LateUpdate()
    {
        if (!billboardToCamera) return;
        if (!Camera.main) return;

        transform.forward = Camera.main.transform.forward;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        ApplyColors();
        RebuildFromSource();
    }

    public void SetVisible(bool visible)
    {
        if (meshRenderer) meshRenderer.enabled = visible;
        if (lineRenderer) lineRenderer.enabled = visible;
    }

    public void RebuildFromSource()
    {
        if (!sourcePolygon) return;

        Vector2[] pts2 = sourcePolygon.GetPath(0);

        List<Vector3> pts = new List<Vector3>(pts2.Length);
        for (int i = 0; i < pts2.Length; i++)
            pts.Add(new Vector3(pts2[i].x, pts2[i].y, zOffset));

        BuildMesh(pts);
        BuildOutline(pts);
    }

    private void ApplyColors()
    {
        if (!meshRenderer) return;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", fillColor);
        block.SetColor("_Color", fillColor);
        meshRenderer.SetPropertyBlock(block);

        if (lineRenderer)
        {
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
            lineRenderer.widthMultiplier = outlineWidth;
        }
    }

    private void BuildOutline(List<Vector3> pts)
    {
        if (lineRenderer == null) return;
        lineRenderer.positionCount = pts.Count;
        for (int i = 0; i < pts.Count; i++)
            lineRenderer.SetPosition(i, pts[i]);
    }

    private void BuildMesh(List<Vector3> pts)
    {
        if (!meshFilter) return;

        if (pts == null || pts.Count < 3) return;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "PolygonHighlightMesh";
        }
        else
        {
            mesh.Clear();
        }

        var vertices = pts.ToArray();
        var triangles = TriangulateEarClipping(vertices);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    private int[] TriangulateEarClipping(Vector3[] verts)
    {
        int n = verts.Length;
        var indices = new List<int>();
        for (int i = 0; i < n; i++) indices.Add(i);

        var tris = new List<int>(Mathf.Max(0, (n - 2) * 3));

        bool isCCW = SignedArea(verts) > 0f;

        int guard = 0;
        while (indices.Count > 3 && guard++ < 5000)
        {
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int i0 = indices[(i - 1 + indices.Count) % indices.Count];
                int i1 = indices[i];
                int i2 = indices[(i + 1) % indices.Count];

                Vector2 a = new Vector2(verts[i0].x, verts[i0].y);
                Vector2 b = new Vector2(verts[i1].x, verts[i1].y);
                Vector2 c = new Vector2(verts[i2].x, verts[i2].y);

                if (!IsConvex(a, b, c, isCCW)) continue;

                bool anyInside = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    int iv = indices[j];
                    if (iv == i0 || iv == i1 || iv == i2) continue;

                    Vector2 p = new Vector2(verts[iv].x, verts[iv].y);
                    if (PointInTriangle(p, a, b, c))
                    {
                        anyInside = true;
                        break;
                    }
                }

                if (anyInside) continue;

                tris.Add(i0);
                tris.Add(i1);
                tris.Add(i2);

                indices.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound) break;
        }

        if (indices.Count == 3)
        {
            tris.Add(indices[0]);
            tris.Add(indices[1]);
            tris.Add(indices[2]);
        }

        return tris.ToArray();
    }

    private float SignedArea(Vector3[] v)
    {
        float area = 0f;
        for (int i = 0; i < v.Length; i++)
        {
            int j = (i + 1) % v.Length;
            area += v[i].x * v[j].y - v[j].x * v[i].y;
        }
        return area * 0.5f;
    }

    private bool IsConvex(Vector2 a, Vector2 b, Vector2 c, bool isCCW)
    {
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        return isCCW ? cross > 0f : cross < 0f;
    }

    private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // barycentric technique
        float s1 = c.y - a.y;
        float s2 = c.x - a.x;
        float s3 = b.y - a.y;
        float s4 = p.y - a.y;

        float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
        float w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0f && w2 >= 0f && (w1 + w2) <= 1f;
    }
}
