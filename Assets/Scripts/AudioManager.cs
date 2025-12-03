using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("START MUSIC")]
    [SerializeField] private EventReference startMusic;

    [Header("MUSIC CROSSFADE")]
    [SerializeField] private float musicFadeDuration = 1.5f;

    private EventInstance currentMusic;
    private EventInstance nextMusic;

    private bool isFading;

    private EventInstance activeSnapshot;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!startMusic.IsNull)
            PlayMusic(startMusic);
    }

    public void PlayMusic(EventReference musicEvent)
    {
        StopMusicImmediate();

        currentMusic = RuntimeManager.CreateInstance(musicEvent);
        currentMusic.setVolume(1f);
        currentMusic.start();
    }

    public void CrossfadeMusic(EventReference newMusic)
    {
        if (isFading)
            return;

        if (currentMusic.isValid() == false)
        {
            PlayMusic(newMusic);
            return;
        }

        nextMusic = RuntimeManager.CreateInstance(newMusic);
        nextMusic.setVolume(0f);
        nextMusic.start();

        StartCoroutine(FadeMusicRoutine());
    }

    private IEnumerator FadeMusicRoutine()
    {
        isFading = true;

        float t = 0f;

        while (t < musicFadeDuration)
        {
            t += Time.deltaTime;
            float k = t / musicFadeDuration;

            currentMusic.setVolume(1f - k);
            nextMusic.setVolume(k);

            yield return null;
        }

        currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentMusic.release();

        currentMusic = nextMusic;
        nextMusic = default;

        isFading = false;
    }

    public void StopMusicImmediate()
    {
        if (!currentMusic.isValid())
            return;

        currentMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        currentMusic.release();
    }

    public void PlaySFX(EventReference sound)
    {
        if (sound.IsNull)
            return;

        RuntimeManager.PlayOneShot(sound, Camera.main.transform.position);
    }

    public void StartSnapshot(EventReference snapshot)
    {
        if (snapshot.IsNull)
            return;

        StopSnapshot();

        activeSnapshot = RuntimeManager.CreateInstance(snapshot);
        activeSnapshot.start();
    }

    public void StopSnapshot()
    {
        if (!activeSnapshot.isValid())
            return;

        activeSnapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        activeSnapshot.release();
    }
}
