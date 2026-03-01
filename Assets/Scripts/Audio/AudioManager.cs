using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public enum MusicType { MENU = 0, LVL1 = 1, LVL2 = 2, LVL3 = 3, NONE = 4 };

    [Header("MUSIC")]
    [SerializeField] private EventReference menuMusic;
    [SerializeField] private EventReference lvl1Music;
    [SerializeField] private EventReference lvl2Music;
    [SerializeField] private EventReference lvl3Music;

    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.1f;

    [Header("MUSIC CROSSFADE")]
    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("General Sfx")]
    [SerializeField] private EventReference buttonClickSfx;

    private EventInstance currentMusic;
    private EventInstance nextMusic;

    private bool isFading;

    private EventInstance activeSnapshot;

    private Coroutine musicLoopRoutine;

    private EventReference currentMusicRef;

    private MusicType currentMusicType = MusicType.NONE;

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

    public void PlayMusic(EventReference musicEvent)
    {
        StopMusicImmediate(false);

        currentMusicRef = musicEvent;

        currentMusic = RuntimeManager.CreateInstance(musicEvent);
        RuntimeManager.AttachInstanceToGameObject(currentMusic, Camera.main.transform, Camera.main.GetComponent<Rigidbody>());
        currentMusic.start();
        currentMusic.setVolume(musicVolume);

        if (musicLoopRoutine != null) StopCoroutine(musicLoopRoutine);
        musicLoopRoutine = StartCoroutine(MonitorAndLoopCurrentMusic());
    }

    public void PlayNewMusic(MusicType type)
    {
        if (currentMusicType == type) return;
        currentMusicType = type;

        switch (type)
        {
            case MusicType.MENU:
                PlayMusic(menuMusic);
                break;
            case MusicType.LVL1:
                PlayMusic(lvl1Music);
                break;
            case MusicType.LVL2:
                PlayMusic(lvl2Music);
                break;
            case MusicType.LVL3:
                PlayMusic(lvl3Music);
                break;
            case MusicType.NONE:
                StopMusicImmediate(false);
                break;
        }
    }

    public void CrossfadeMusic(EventReference newMusic)
    {
        if (isFading) return;

        if (!currentMusic.isValid())
        {
            PlayMusic(newMusic);
            return;
        }

        currentMusic.getVolume(out float currentVol, out _);

        nextMusic = RuntimeManager.CreateInstance(newMusic);
        nextMusic.setVolume(0f);
        nextMusic.start();

        StartCoroutine(FadeMusicRoutine(currentVol));
    }

    public void CrossfadeNewMusic(MusicType type)
    {
        if (currentMusicType == type) return;
        currentMusicType = type;

        switch (type)
        {
            case MusicType.MENU:
                CrossfadeMusic(menuMusic);
                break;
            case MusicType.LVL1:
                CrossfadeMusic(lvl1Music);
                break;
            case MusicType.LVL2:
                CrossfadeMusic(lvl2Music);
                break;
            case MusicType.LVL3:
                CrossfadeMusic(lvl3Music);
                break;
            case MusicType.NONE:
                StopMusicImmediate(false);
                break;
        }
    }

    private IEnumerator FadeMusicRoutine(float baseVol)
    {
        isFading = true;

        float t = 0f;

        while (t < musicFadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / musicFadeDuration);

            currentMusic.setVolume(Mathf.Lerp(baseVol, 0f, k));

            nextMusic.setVolume(Mathf.Lerp(0f, baseVol, k));

            yield return null;
        }

        currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentMusic.release();

        currentMusic = nextMusic;
        nextMusic = default;

        isFading = false;
    }

    private IEnumerator MonitorAndLoopCurrentMusic()
    {
        while (true)
        {
            if (!currentMusic.isValid())
            {
                yield return null;
                continue;
            }

            currentMusic.getPlaybackState(out PLAYBACK_STATE state);

            if (state == PLAYBACK_STATE.STOPPED)
            {
                currentMusic.getVolume(out float vol, out _);

                currentMusic.release();

                currentMusic = RuntimeManager.CreateInstance(currentMusicRef);
                currentMusic.setVolume(vol);
                currentMusic.start();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StopMusicImmediate(bool immediate = true)
    {
        if (musicLoopRoutine != null)
        {
            StopCoroutine(musicLoopRoutine);
            musicLoopRoutine = null;
        }

        if (!currentMusic.isValid())
            return;

        if (immediate) currentMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        else currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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

    public void PlayButtonSound()
    {
        PlaySFX(buttonClickSfx);
    }

    public void PlayMenuMusic()
    {
        if (menuMusic.IsNull) return;
        PlayMusic(menuMusic);
    }

    public void PlayLvl1Music()
    {
        if (lvl1Music.IsNull) return;
        PlayMusic(lvl1Music);
    }

    public void PlayLvl2Music()
    {
        if (lvl2Music.IsNull) return;
        PlayMusic(lvl2Music);
    }

    public void PlayLvl3Music()
    {
        if (lvl3Music.IsNull) return;
        PlayMusic(lvl3Music);
    }
}
