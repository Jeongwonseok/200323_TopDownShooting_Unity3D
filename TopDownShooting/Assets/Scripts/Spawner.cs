using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;      // 현재 웨이브의 레퍼런 가져올 변수
    int currentWaveNumber; // 현재 웨이브 횟수

    int enemiesRemainingToSpawn; // 남아있는 스폰할 적의 수
    int enemiesRemainingAlive;  // 살아있는 적의 수
    float nextSpawnTime;        // 다음번 스폰 시간

    void Start()
    {
        NextWave();
    }

    void Update()
    {
        // 스폰해야 할 적이 0보다 크고, 현재 시간이 다음번 스폰 시간보다 크면
        if(enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
        {
            // 남아있는 스폰할 적 줄이기
            enemiesRemainingToSpawn--;
            // 다음 스폰 시간 = 현재 시간 + 스폰 시간 간격
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            // 적 프리펩 생성
            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
            // 상속을 통해 LivingEntity 스크립트의 델리게이트 변수에 함수가 추가된다.
            spawnedEnemy.OnDeath += OnEnemyDeath;
        }
    }

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
        // 현재 웨이브
        currentWaveNumber++;
        print("Wave : " + currentWaveNumber);

        // 배열 예외 처리해주기
        if(currentWaveNumber-1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            // 남아있는 스폰할 적의 수에 현재 적의 수 대입
            enemiesRemainingToSpawn = currentWave.enemyCount;
            // 살아있는 적의 수에 남아있는 스폰할 적의 수 대입
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }
    }

    [System.Serializable]
    // 웨이브 정보 저장할 클래스
    public class Wave
    {
        public int enemyCount; // 적의 수
        public float timeBetweenSpawns; // 스폰 간격

    }
}
