using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; private set; }
    float lastEnemyKillTime;
    public static int streakCount;
    float streakExpiryTime = 1;
    //int comboCount = 0;

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKiiled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    // 적 죽으면 점수 증가 >> 2점씩 >> But, 1초안에 다음 적 죽이면 콤보 가능
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

        score += 1 + (int)Mathf.Pow(2, streakCount);
        //comboCount = comboCount + streakCount;
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKiiled;
        score = 0;
        //comboCount = 0;
    }

}
