using UnityEngine;

public class JekkoFloat : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed = 1.2f;

    private Vector3 startPos;
    private float t;

    private void OnEnable()
    {
        startPos = transform.position;
        t = Random.value * 10f;
    }

    private void Update()
    {
        t += Time.deltaTime * floatSpeed;

        float yOffset = Mathf.Sin(t) * floatAmplitude;
        transform.position = startPos + new Vector3(0f, yOffset, 0f);
    }
}
