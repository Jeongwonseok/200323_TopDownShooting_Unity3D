using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // BGM 관련 변수
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    void Start()
    {
        AudioManager.instance.PlayMusic(menuTheme, 2);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            AudioManager.instance.PlayMusic(mainTheme, 3);
        }
    }
}
