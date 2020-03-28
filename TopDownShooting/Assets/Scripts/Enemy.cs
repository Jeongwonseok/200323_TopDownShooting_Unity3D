using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle, // 아무동작 x
        Chasing, // 플레이어 추격
        Attacking // 공격하는 도중
    };

    State currentState;

    NavMeshAgent pathFinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float attackDistanceThreshold = 0.5f; // 공격할 수 있는 한계 거리 변수
    float timeBetweenAttacks = 1; // 공격 사이의 지연 시간
    float damage = 1; // 적의 공격 데미지

    float nextAttackTime; // 다음 공격 가능 시간
    float myCollisionRadius; // 적 반지름
    float targetCollisionRadius; // 플레이어 반지름

    bool hasTarget;

    // 부모 클래스를 덮어쓰고, 부모 클래스의 Start 메서드 실행
    protected override void Start()
    {
        base.Start();
        pathFinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        // 플레이어가 존재하면 실행
        if(GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            // 반지름 할당
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }
        
    }

    // 플레이어 죽으면
    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        if(hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    // 공격 코루틴
    IEnumerator Attack()
    {
        // 공격하는 동안 플레이어 추적 Off
        currentState = State.Attacking;
        pathFinder.enabled = false;

        Vector3 originalPosition = transform.position; // 적 현재 위치
        Vector3 dirToTarget = (target.position - transform.position).normalized; // 목표까지의 방향벡터

        // 타겟 위치 = 실제 타겟 위치 - 목표까지의 방향 * 적 반지름
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3; // 공격 스피드
        float percent = 0; // 반복 조건 변수

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false; // 데미지를 적용하는 도중인지

        while(percent <= 1)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            // 시간 * 공격 스피드
            percent += Time.deltaTime * attackSpeed;

            // 대칭함수 이용 (0 >> 1 >> 0) >> 현재 -> 타겟 -> 현재 로 돌아와야 하기 때문에
            // interpolation(보간) : 알려진 점들의 위치를 참조하여, 집합의 일정 범위의 점들(선)을 새롭게 그리는 방법
            float interpolation = ( -Mathf.Pow(percent, 2) + percent ) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        // 공격 끝나면 플레이어 추적 On
        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathFinder.enabled = true;
    }

    // 적 네비게이션 타겟 지정 및 이동
    // 코루틴으로 정의하는 이유 >> Update에 넣으면 매 프레임마다 추적하므로 게임 과부하
    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while(hasTarget)
        {
            if(currentState == State.Chasing) // 추적 상태일 때만 적 추적
            {
                // 목표까지의 방향벡터
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                // 타겟 위치 = 실제 타겟 위치 - 목표까지의 방향 * (적 반지름 + 타겟 반지름 + 공격 한계 거리/2)
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead)
                {
                    pathFinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
