using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel{Master, Sfx, Music};

    float masterVolumePercent = 0.2f;
    float sfxVolumePercent = 5f;
    float musicVolumePercent = 1f;

    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject newSfx2DSource = new GameObject("2D sfx source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            playerT = FindObjectOfType<Player>().transform;

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", masterVolumePercent);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", sfxVolumePercent);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", musicVolumePercent);
        }
    }

    void Update()
    {
        if(playerT != null)
        {
            audioListener.position = playerT.position;
        }
    }

    // 사운드 볼륨 설정 메서드
    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch(channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
    }

    // BGM Play 메서드 (메인, 메뉴)
    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    // Effect Play 메서드 (총 쏘기, 재장전)
    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if(clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
        }
    }

    // Effect Play 메서드 (적 사망, 적 공격, 플레이어 사망) 
    public void PlaySound(string soundName, Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    // 2D Effect Play 메서드 (Next Wave 효과음)
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }

    // BGM 전환 시 Sound Animation 발동 코루틴
    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }
}
