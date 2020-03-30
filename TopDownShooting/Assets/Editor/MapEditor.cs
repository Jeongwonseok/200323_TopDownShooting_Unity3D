using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 맵 custom 하는 방법
[CustomEditor (typeof (MapGenetator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // CustomEditor 키워드로 이 에디터 스크립트가 다룰것이라고 선언한 오브젝트가 자동으로 target 변수에 설정된다.
        MapGenetator map = target as MapGenetator;

        if (DrawDefaultInspector())
        { 
            map.GenerateMap();
        }

        if(GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
