using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenetator : MonoBehaviour
{
    public Transform tilePrefab;
    public Vector2 mapSize;

    [Range(0,1)]
    public float outlinePercent; // 타일 구분

    void Start()
    {
        GenerateMap();
    }

    // 맵 그리기
    public void GenerateMap()
    {
        string holderName = "Generated Map";

        // 에디터에서 호출하려면 DestroyImmediate() 사용
        if(transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for(int x=0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // 맵의 가로 길이의 절반 만큼 왼쪽으로 이동한 점에서부터 타일 생성 시작
                Vector3 tilePosition = new Vector3(-mapSize.x/2 + 0.5f + x, 0, -mapSize.y/2 + 0.5f + y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;
            }
        }
    }
}
