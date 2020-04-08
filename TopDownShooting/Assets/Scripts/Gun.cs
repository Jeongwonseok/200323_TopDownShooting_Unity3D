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
    public int projectilesPerMag; // 재장전 총알 수
    public float reloadTime = 0.3f; // 재장전 시간

    [Header ("Recoil")]
    public Vector2 kickMinMax = new Vector2(0.05f, 0.2f); // 반동 관련 최소, 최대 변수
    public Vector2 recoilAngleMinMax = new Vector2(3,5); // 반동 각도 관련 최소, 최대 변수
    public float recoilMoveSettleSpeed = 0.1f; // 반동 움직임 스피드 변수
    public float recoilRotationSettleSpeed = 0.1f; // 반동 회전 스피드 변수

    [Header("Effect")]
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    MuzzleFlash muzzleflash;
    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst; // 남은 버스트 총알 수
    int projectilesRemainingInMag; // 재장전 총알 수
    bool isReloading; // 재장전 가능 여부 변수

    Vector3 recoilSmoothDampVelocity; // 반동 속도 변수
    float recoilRotSmoothDampVelocity; // 반동 회전 속도 변수
    float recoilAngle; // 반동 각 변수

    void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }
   
    void LateUpdate()
    {
        // Recoil Animation
        // Vector3.SmoothDamp : 시간에 따라 원하는 위치로 부드럽게 벡터 변화
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleSpeed);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleSpeed);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if(!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }

    // 총알 생성 및 발사
    void Shoot()
    {
        if(!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
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
                if(projectilesRemainingInMag == 0)
                {
                    break;
                }
                projectilesRemainingInMag--;
                // (현재시간 + 연사력) / 1000 = 밀리초로 변환된 연사 간격
                nextShotTime = Time.time + msBetweenShots / 1000;

                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }
            Instantiate(shell, shellEjection.position, shellEjection.rotation);

            // 총알 이펙트 쏠때마다 활성화
            muzzleflash.Activate();

            // 반동 기능 정의
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    // 재장전 메서드
    public void Reload()
    {
        if(!isReloading && projectilesRemainingInMag != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    // 재장전 Animation 코루틴
    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    // 총 Aim 디테일 메서드
    public void Aim(Vector3 aimPoint)
    {
        if(!isReloading)
            transform.LookAt(aimPoint);
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
