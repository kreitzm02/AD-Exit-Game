using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TraceGuideLine : MonoBehaviour
{
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;
    [SerializeField] private float tilingPerUnit = 4f;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.textureMode = LineTextureMode.Tile;
    }

    private void LateUpdate()
    {
        if (!a || !b) return;

        lr.SetPosition(0, a.position);
        lr.SetPosition(1, b.position);

        float len = Vector3.Distance(a.position, b.position);
        Material mat = lr.material;
        if (mat && mat.mainTexture != null)
            mat.mainTextureScale = new Vector2(len * tilingPerUnit, 1f);
    }
}
