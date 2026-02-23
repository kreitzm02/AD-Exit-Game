using System.Collections.Generic;
using UnityEngine;

public class JekkoFloat : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed = 1.2f;

    [Header("Idle Animation")]
    [SerializeField, Min(1f)] private float idleFps = 3f;
    [SerializeField] private int startLevel = 1;

    [SerializeField] private List<Sprite> jekkoIdleSpritesLvl1 = new();
    [SerializeField] private List<Sprite> jekkoIdleSpritesLvl2 = new();
    [SerializeField] private List<Sprite> jekkoIdleSpritesLvl3 = new();

    private SpriteRenderer spriteRenderer;

    private Vector3 startPos;
    private float t;

    private int currentLevel = 1;
    private List<Sprite> activeIdleSprites;

    private int idleFrameIndex;
    private float idleFrameTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        startPos = transform.position;
        t = Random.value * 10f;
    }

    private void Update()
    {
        t += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(t) * floatAmplitude;
        transform.position += new Vector3(0f, yOffset * Time.deltaTime, 0.0f);

        AnimateIdle();
    }

    private void AnimateIdle()
    {
        if (!spriteRenderer) return;
        if (idleFps <= 0f) return;
        if (activeIdleSprites == null || activeIdleSprites.Count == 0) return;

        idleFrameTimer += Time.deltaTime;
        float frameDuration = 1f / idleFps;

        while (idleFrameTimer >= frameDuration)
        {
            idleFrameTimer -= frameDuration;
            idleFrameIndex = (idleFrameIndex + 1) % activeIdleSprites.Count;
        }

        Sprite s = activeIdleSprites[idleFrameIndex];
        if (s) spriteRenderer.sprite = s;
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 3);

        switch (currentLevel)
        {
            case 1: activeIdleSprites = jekkoIdleSpritesLvl1; break;
            case 2: activeIdleSprites = jekkoIdleSpritesLvl2; break;
            case 3: activeIdleSprites = jekkoIdleSpritesLvl3; break;
            default: activeIdleSprites = jekkoIdleSpritesLvl1; break;
        }

        idleFrameIndex = 0;
        idleFrameTimer = 0f;

        if (spriteRenderer && activeIdleSprites != null && activeIdleSprites.Count > 0)
        {
            Sprite first = activeIdleSprites[0];
            if (first) spriteRenderer.sprite = first;
        }
    }
}

