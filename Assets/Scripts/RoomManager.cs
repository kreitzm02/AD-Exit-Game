using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("ROOMS")]
    [SerializeField] private List<RoomData> rooms = new List<RoomData>();

    [Header("REFERENCES")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("FADE UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float fadeTransitionTime = 0.25f;

    private RoomData currentRoom;
    private bool isTransitioning;

    private Tween<float> fadeTween;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        foreach (var room in rooms)
            room.roomParent.SetActive(false);

        if (rooms.Count > 0)
            LoadInitialRoom(rooms[0], 0);

        SetFadeAlpha(1.0f);
        StartCoroutine(BlackHoldThenFadeOut());
    }

    private void LoadInitialRoom(RoomData room, int entryIndex)
    {
        room.roomParent.SetActive(true);
        currentRoom = room;

        player.position = room.entryPoints[entryIndex].position;
        playerCamera.SetNewBounds(room.cameraBounds);
    }

    public void ChangeRoom(string targetRoomId, int targetEntryPointIndex)
    {
        if (isTransitioning) return;

        RoomData targetRoom = rooms.Find(r => r.roomId == targetRoomId);

        if (targetRoom == null)
        {
            Debug.LogError("[RoomManager] Room does not exist: " + targetRoomId);
            return;
        }

        isTransitioning = true;

        StartFade(0f, 1f, () =>
        {
            currentRoom.roomParent.SetActive(false);
            targetRoom.roomParent.SetActive(true);

            player.position = targetRoom.entryPoints[targetEntryPointIndex].position;
            playerCamera.SetNewBounds(targetRoom.cameraBounds);
            playerCamera.SnapToTarget();

            currentRoom = targetRoom;

            StartCoroutine(BlackHoldThenFadeOut());
        });
    }

    private void StartFade(float from, float to, System.Action onComplete)
    {
        fadeImage.gameObject.SetActive(true);

        fadeTween?.Stop(TweenStopBehavior.DoNotModify);

        fadeTween = gameObject.Tween(
            "RoomFade",
            from,
            to,
            fadeDuration,
            TweenScaleFunctions.Linear,
            (t) =>
            {
                SetFadeAlpha(t.CurrentValue);
            },
            (t) =>
            {
                SetFadeAlpha(to);

                if (to == 0f)
                    fadeImage.gameObject.SetActive(false);

                onComplete?.Invoke();
            }
        );
    }

    private void SetFadeAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }

    private System.Collections.IEnumerator BlackHoldThenFadeOut()
    {
        if (fadeDuration > 0f)
            yield return new WaitForSeconds(fadeDuration);

        StartFade(1f, 0f, () =>
        {
            isTransitioning = false;
        });
    }
}

[System.Serializable]
public class RoomData
{
    public string roomId;
    public GameObject roomParent;
    public BoxCollider2D cameraBounds;
    public Transform[] entryPoints;
}
