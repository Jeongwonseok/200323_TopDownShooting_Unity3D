﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;            // 발사구 위치
    public Projectile projectile;       // 발사체
    public float msBetweenShots = 100;  // 연사력 (밀리초)
    public float muzzleVelocity = 35;   // 총알 속력

    float nextShotTime;

    // 총알 생성
    public void Shoot()
    {
        if(Time.time > nextShotTime)
        {
            // (현재시간 + 연사력) / 1000 = 밀리초로 변환된 연사 간격
            nextShotTime = Time.time + msBetweenShots / 1000;

            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(muzzleVelocity);
        }
    }
}
