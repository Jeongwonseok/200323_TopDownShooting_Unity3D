using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; private set; }
    float lastEnemyKillTime;
    int streakCount;
    float streakExpiryTime = 1;

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKiiled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    // 적 죽으면 점수 증가 >> 5 + (2의 죽은 적의 수 제곱)
    void OnEnemyKiiled()
    {
        if(Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }
        lastEnemyKillTime = Time.time;

        score += 5 + (int)Mathf.Pow(2, streakCount);
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKiiled;
    }
}
