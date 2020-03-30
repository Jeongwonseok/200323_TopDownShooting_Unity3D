using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenetator : MonoBehaviour
{
    // Map 관련 선언
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab; // 타일
    public Transform obstaclePrefab; // 장애물

    public Transform navmeshFloor; // 안보이는 맵
    public Transform navmeshMaskPrefab; // 전체 맵 사방에 마스킹 할 프리펩

    public Vector2 maxMapSize; // 최대 맵 사이즈 변수

    [Range(0,1)]
    public float outlinePercent; // 타일 구분

    public float tileSize; // 타일 사이즈 변수

	List<Coord> allTileCoords; // 모든 타일 좌표 저장 리스트
    Queue<Coord> shuffleTileCoords; // 셔플된 좌표 저장 큐
    Queue<Coord> shuffleOpenTileCoords; // 셔플된 적 스폰 좌표 저장 큐
    Transform[,] tileMap; // tile 정보 가져오기

    Map currentMap;

    void Start()
    {
        GenerateMap();
    }

    // 맵 그리기
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        /* 좌표 생성 */
        allTileCoords = new List<Coord>();

        // 모든 타일 거쳐서 추가
		for (int x = 0; x < currentMap.mapSize.x; x++)
		{
			for (int y = 0; y < currentMap.mapSize.y; y++)
			{
				allTileCoords.Add(new Coord(x, y));
			}
		}
        // 셔플한거 큐에 저장
        shuffleTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        /* 맵 홀더 오브젝트 생성 */
        string holderName = "Generated Map";

        // 에디터에서 호출하려면 DestroyImmediate() 사용
        if(transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        /* 타일들을 스폰 */
        for (int x=0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                // 맵의 가로 길이의 절반 만큼 왼쪽으로 이동한 점에서부터 타일 생성 시작
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile; // 타일 저장
            }
        }

        /* 장애물들을 스폰 */
        // 알고리즘 통해서 탐색 참고에 사용될 map bool 배열
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        // 생성할 장애물 수
		int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        // 현재 생성된 장애물 수
        int currentObstacleCount = 0;
        // 오픈 타일 : 모든 타일의 좌표를 복사해 가져와 초기화
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        // 장애물 수만큼 루프 돌면서 생성
        for (int i=0; i<obstacleCount; i++)
		{
			Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            // Flood Fill 알고리즘 사용해서 생성 가능 여부 확인
            // 이미 살펴보았던 타일들을 표시하고, 같은 타일을 계속 또 보지 않도록 하는것이 중요함!!
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) // 정중앙이 아니고, 접근 가능하면
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1-outlinePercent) * tileSize, obstacleHeight, (1-outlinePercent) * tileSize);

                // 장애물 색 지정
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                // 장애물 선택 타일 제거해주기 >> 오픈 타일 세팅
                allOpenCoords.Remove(randomCoord);

            }
            else // 생성 못하면
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
		}
        // 적 스폰 위치 셔플하기
        shuffleOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        /* NavMesh 마스크 생성 */
        // 플레이어 및 적들 맵 바깥으로 나가지 못하게 마스킹
        // 왼쪽
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        // 오른쪽
        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        // 위쪽
        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        // 아래쪽
        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        // 네비게이션 베이킹 할 안보이는 전체 맵 크기 지정
        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }

    // 장애물 생성 가능 여부 확인 메서드 (장애물 여부 배열, 여태 얼마나 생성되었는지 확인 변수)
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        // 알고리즘 내부에서 증가시키면서 마지막에 목표 타일수와 비교해서 탐색 종료하기 위한 변수
        int accessibleTileCount = 1;

        // 큐안에 좌표가 있다면 >> Flood Fill 알고리즘 시작
        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            // 이웃한 모든 타일 탐색
            for(int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;
                    // 대각선 제외하고 탐색
                    if (x==0 || y==0)
                    {
                        // 좌표가 obstacleMap 내부에 있는지 확인
                        if(neighborX >= 0 && neighborX < obstacleMap.GetLength(0) && neighborY >= 0 && neighborY < obstacleMap.GetLength(1))
                        {
                            // 이 타일을 이전에 체크하지 않았고, 이것이 장애물이 아니라면
                            if(!mapFlags[neighborX,neighborY] && !obstacleMap[neighborX,neighborY])
                            {
                                mapFlags[neighborX, neighborY] = true;
                                queue.Enqueue(new Coord(neighborX, neighborY));
                                accessibleTileCount++;
                            }
                        }
                    }

                }
            }
        }
        // 목표 타일 수 = 전체 타일 수 - 현재 장애물 타일 수
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        // 값이 같으면 true 반환
        return targetAccessibleTileCount == accessibleTileCount;
    }

    // 맵의 가로 길이의 절반 만큼 왼쪽으로 이동한 점에서부터 타일 생성 시작 메서드
    Vector3 CoordToPosition(int x, int y)
	{
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    // 현재 플레이어 위치 반환
    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        // 맵 밖에 있는 값 제외시키는 부분
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    // 큐로부터 다음 아이템 얻어서 랜덤 좌표 반환 메서드
    public Coord GetRandomCoord()
	{
		Coord randomCoord = shuffleTileCoords.Dequeue();
		shuffleTileCoords.Enqueue(randomCoord);
		return randomCoord;
	}

    // 큐로부터 다음 아이템 얻어서 적 스폰할 랜덤 좌표 반환 메서드
    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffleOpenTileCoords.Dequeue();
        shuffleOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    // 모든 타일에 대한 좌표 구조체 생성
    [System.Serializable]
    public struct Coord
	{
		public int x;
		public int y;

        public Coord(int _x, int _y)
		{
			x = _x;
			y = _y;

		}

        // 오퍼레이터 정의 해줘야함 >> 구조체로 새로 만들었기 때문에
        public static bool operator == (Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator != (Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize; // 원하는 맵 사이즈 변수
        [Range(0,1)]
        public float obstaclePercent; // 장애물 퍼센트
        public int seed; // 셔플을 위한 이유없는 시작 값
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        // 맵의 정중앙 지정
        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
