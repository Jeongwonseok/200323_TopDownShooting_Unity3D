using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single};
    public FireMode fireMode;

    public Transform[] projectileSpawn;            // 발사구 위치
    public Projectile projectile;       // 발사체
    public float msBetweenShots = 100;  // 연사력 (밀리초)
    public float muzzleVelocity = 35;   // 총알 속력
    public int burstCount; // 총 버스트 가능한 총알 수

    public Transform shell;
    public Transform shellEjection;

    MuzzleFlash muzzleflash;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst; // 남은 버스트 총알 수

    void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
    }

    // 총알 생성 및 발사
    void Shoot()
    {
        if(Time.time > nextShotTime)
        {
            // Burst 모드 - n점사
            if(fireMode == FireMode.Burst)
            {
                if(shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            // Single 모드 - 단발
            else if(fireMode == FireMode.Single)
            {
                if(!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }
            // Auto 모드 - 앞에 없이 그냥 실행 - 연사
            for (int i=0; i< projectileSpawn.Length; i++)
            {
                // (현재시간 + 연사력) / 1000 = 밀리초로 변환된 연사 간격
                nextShotTime = Time.time + msBetweenShots / 1000;

                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            // 총알 이펙트 쏠때마다 활성화
            muzzleflash.Activate();
        }
    }

    // 총 쏘기
    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    // 총 쏘기 이후
    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount; // 마우스 버튼 떼면 초기화
    }

}
