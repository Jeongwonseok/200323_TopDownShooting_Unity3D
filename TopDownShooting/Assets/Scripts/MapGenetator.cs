using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenetator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;

    [Range(0,1)]
    public float outlinePercent; // 타일 구분

	List<Coord> allTileCoords; // 모든 타일 좌표 저장 리스트
	Queue<Coord> shuffleTileCoords; // 셔플된 좌표 저장 큐

    // 셔플을 위한 이유없는 시작 값
    public int seed = 10;

    void Start()
    {
        GenerateMap();
    }

    // 맵 그리기
    public void GenerateMap()
    {
		allTileCoords = new List<Coord>();

        // 모든 타일 거쳐서 추가
		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				allTileCoords.Add(new Coord(x, y));
			}
		}
        // 셔플한거 큐에 저장
        shuffleTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));

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
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;
            }
        }

        // 생성할 장애물 수
		int obstacleCount = 10;
        // 장애물 수만큼 루프 돌면서 생성
        for(int i=0; i<obstacleCount; i++)
		{
			Coord randomCoord = GetRandomCoord();
            Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform;
            newObstacle.parent = mapHolder;
		}
    }

    // 맵의 가로 길이의 절반 만큼 왼쪽으로 이동한 점에서부터 타일 생성 시작 메서드
    Vector3 CoordToPosition(int x, int y)
	{
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
    }

    // 큐로부터 다음 아이템 얻어서 랜덤 좌표 반환 메서드
    public Coord GetRandomCoord()
	{
		Coord randomCoord = shuffleTileCoords.Dequeue();
		shuffleTileCoords.Enqueue(randomCoord);
		return randomCoord;
	}

    // 모든 타일에 대한 좌표 구조체 생성
    public struct Coord
	{
		public int x;
		public int y;

        public Coord(int _x, int _y)
		{
			x = _x;
			y = _y;

		}
	}
}
