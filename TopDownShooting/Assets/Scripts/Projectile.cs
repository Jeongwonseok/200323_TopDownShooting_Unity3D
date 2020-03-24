using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;

    float speed = 10;
    float damage = 1;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;

        CheckCollisions(moveDistance);

        // 총알 Shoot
        transform.Translate(Vector3.forward * moveDistance);
    }

    // ray가 이동할거리, 충돌에 관한 결과 가져오기
    void CheckCollisions(float moveDistance)
    {
        // 발사체 앞 방향으로 ray 발사
        Ray ray = new Ray(transform.position, transform.forward);
        // 발사체와 충돌한 오브젝트에 대한 반환 정보
        RaycastHit hit;

        // QueryTriggerInteraction.Collide >> 트리거 콜라이더들과 충돌할지 안할지 정해준다.
        if (Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit);
        }
    }

    // 데미지 입히기
    // 충돌 시 발사체 파괴
    void OnHitObject(RaycastHit hit)
    {
        // 충돌한 오브젝트 가져오기
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();

        // 데미지 주기
        // 모든 게임 오브젝트에 IDamageable이 붙어있는것은 아니므로 예외 처리 해준다.
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hit);
        }

        Destroy(gameObject);
    }
}
