using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    // Impact 관련 Dictionary 선언
    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        foreach(SoundGroup soundGroup in soundGroups)
        {
            groupDictionary.Add(soundGroup.groupID, soundGroup.group);
        }
    }

    // 그룹 Dictionary string 찾기 메서드
    public AudioClip GetClipFromName(string name)
    {
        if(groupDictionary.ContainsKey(name))
        {
            AudioClip[] sounds = groupDictionary[name];
            return sounds[Random.Range(0, sounds.Length)];
        }
        return null;
    }

    // Sound Group화 해서 관리
    [System.Serializable]
    public class SoundGroup
    {
        public string groupID;
        public AudioClip[] group;
    }
}
