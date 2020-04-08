using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode; // 발전 모드 여부 변수

    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerT;

    Wave currentWave;      // 현재 웨이브의 레퍼런 가져올 변수
    int currentWaveNumber; // 현재 웨이브 횟수

    int enemiesRemainingToSpawn; // 남아있는 스폰할 적의 수
    int enemiesRemainingAlive;  // 살아있는 적의 수
    float nextSpawnTime;        // 다음번 스폰 시간

    MapGenetator map;

    // 캠핑 방지 요소 관련 변수
    float timeBeetweenCampingChecks = 2; // 캠핑 검사 간격
    float campThresholdDistance = 1.5f; // 캠핑 체크 사이에 움직여야할 최소 한계 거리 >> 2초안에 최소한 1.5단위는 움직여야 한다는 의미
    float nextCampCheckTime; // 다음 검사 예정 시간
    Vector3 campPositionOld; // 가장 최근 캠핑 체크할때 플레이어가 있었던 위치
    bool isCamping; // 캠핑 여부

    bool isDisabled; // 적 스폰 여부

    // Wave 관련 델리게이트 생성
    public event System.Action<int> OnNewWave;

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBeetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;

        // 델리게이트에 플레이어 죽으면 발생될 메서드 추가
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenetator>();
        NextWave();
    }

    void Update()
    {
        if(!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBeetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }
            // 스폰해야 할 적이 0보다 크거나 infinite가 true이고, 현재 시간이 다음번 스폰 시간보다 크면
            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                // 남아있는 스폰할 적 줄이기
                enemiesRemainingToSpawn--;
                // 다음 스폰 시간 = 현재 시간 + 스폰 시간 간격
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
            }
        }

        // 개발자 모드 추가
        if(devMode)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                StopCoroutine("SpawnEnemy");

                // 적이 존재하면 적 생성하는 Spawner 전부 파괴하고 다음 Wave로 전환
                foreach(Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }
    }

    // Enemy 생성 Animation 코루틴
    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1; // 대기시간
        float tileFlashSpeed = 4; // 타일 반짝이는 속도

        Transform spawnTile = map.GetRandomOpenTile();
        // 만약 플레이어가 캠핑중이면 랜덤하지 않은 플레이어 위치에 적 스폰
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0; // 경과시간

        // 대기시간 지나면 적 프리펩 생성하도록 지연
        while(spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        // 적 프리펩 생성
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        // 상속을 통해 LivingEntity 스크립트의 델리게이트 변수에 함수가 추가된다.
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    // 플레이어 죽으면 적 스폰 중지 메서드
    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    // 적의 수 관리 및 다음 웨이브 실행 메서드
    void OnEnemyDeath()
    {
        // 살아있는 적의 수 죽을때마다 1씩 감소
        enemiesRemainingAlive--;

        // 적이 다 죽으면 다음 웨이브 실행
        if(enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    // 다음번 웨이브 일으키는 메서드
    void NextWave()
    {
        if(currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");
        }

        // 현재 웨이브
        currentWaveNumber++;

        // 배열 예외 처리해주기
        if(currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            // 남아있는 스폰할 적의 수에 현재 적의 수 대입
            enemiesRemainingToSpawn = currentWave.enemyCount;
            // 살아있는 적의 수에 남아있는 스폰할 적의 수 대입
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if(OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
        }
    }

    // 플레이어 위치 초기화 메서드
    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3; 
    }

    // 웨이브 정보 저장할 클래스
    [System.Serializable]
    public class Wave
    {
        public bool infinite; // 무한대 생성 관련 변수

        public int enemyCount; // 적의 수
        public float timeBetweenSpawns; // 스폰 간격

        public float moveSpeed; // 적의 속도
        public int hitsToKillPlayer; // 플레이어 공격 횟수
        public float enemyHealth; // 적 hp
        public Color skinColor; // 적 색상
    }
}
