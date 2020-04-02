using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;

    float speed = 10;
    float damage = 1;

    float lifetime = 3;
    float skinWidth = 0.1f;

    void Start()
    {
        Destroy(gameObject, lifetime);

        // 발사체(총알)와 겹쳐있는 모든 충돌체들의 배열
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);

        // 총알이 생성되었을 때 어떤 충돌체 오브젝트와 이미 겹친(충돌한) 상태일 때
        if(initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0], transform.position);
        }
    }

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
        // skinWidth 더해줘서 프레임간의 값 보정 역할 해줌
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        // 충돌한 오브젝트 가져오기
        IDamageable damageableObject = c.GetComponent<IDamageable>();

        // 데미지 주기
        // 모든 게임 오브젝트에 IDamageable이 붙어있는것은 아니므로 예외 처리 해준다.
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }

        Destroy(gameObject);
    }
}
