using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("MUSIC")]
    [SerializeField] private EventReference menuMusic;
    [SerializeField] private EventReference lvl1Music;
    [SerializeField] private EventReference lvl2Music;
    [SerializeField] private EventReference lvl3Music;

    [Header("MUSIC CROSSFADE")]
    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("General Sfx")]
    [SerializeField] private EventReference buttonClickSfx;

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

    public void PlayMusic(EventReference musicEvent)
    {
        StopMusicImmediate();

        Debug.Log("Play Music");

        //currentMusic = RuntimeManager.CreateInstance(musicEvent);
        //currentMusic.setVolume(0.5f);
        //currentMusic.start();

        //RuntimeManager.PlayOneShot(musicEvent, Camera.main.transform.position);

        currentMusic = RuntimeManager.CreateInstance(musicEvent);
        RuntimeManager.AttachInstanceToGameObject(currentMusic, Camera.main.transform, Camera.main.GetComponent<Rigidbody>());
        currentMusic.start();
        currentMusic.setVolume(0.1f);
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
        currentMusic.setVolume(0.1f);

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

    public void PlayButtonSound()
    {
        PlaySFX(buttonClickSfx);
    }

    public void PlayMenuMusic()
    {
        if (menuMusic.IsNull) return;
        CrossfadeMusic(menuMusic);
    }

    public void PlayLvl1Music()
    {
        if (lvl1Music.IsNull) return;
        CrossfadeMusic(lvl1Music);
    }

    public void PlayLvl2Music()
    {
        if (lvl2Music.IsNull) return;
        CrossfadeMusic(lvl2Music);
    }

    public void PlayLvl3Music()
    {
        if (lvl3Music.IsNull) return;
        CrossfadeMusic(lvl3Music);
    }
}
