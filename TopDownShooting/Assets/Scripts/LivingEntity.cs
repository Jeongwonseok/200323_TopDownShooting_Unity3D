using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;
    public float health { get; protected set; }
    protected bool dead; // 상속받아야 사용 가능

    // 적 죽음 감지 이벤트 생성
    // System.Action (델리게이트 메소드) : 다를 메소드의 위치를 가르키고 불러올 수 있는 타입 (포인터와 비슷하다.)
    public event System.Action OnDeath;

    // 추상 메서드로 선언하는 이유 : 해당 클래스를 상속받는 스크립트의 Start() 메서드가 덮어쓰지 못하도록 하기 위해!!
    protected virtual void Start()
    {
        health = startingHealth;
    }

    // 인터페이스는 상속해서 쓸때 강제 구현해야함 >> 안하면 에러
    // 추상클래스로 만들어서 자식 클래스에서 오버라이딩 할수 있도록!!
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        // 나중에 hit 관련된 행동 정의 예정

        TakeDamage(damage);
    }

    // 추상클래스로 만들어서 자식 클래스에서 오버라이딩 할수 있도록!!
    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        // 사망
        if (health <= 0)
        {
            Die();
        }
    }

    // 플레이어 사망
    // Player 스크립트에서 Override 하기 위해 추상 메서드로 선언
    [ContextMenu("Self Destruct")]
    public virtual void Die()
    {
        dead = true;

        // 적이 죽을 때마다 OnDeath 호출
        // 델리게이트 이벤트 메소드를 여기서 호출하는 것은 >> 즉, Spawner 스크립트의 OnEnemyDeath()를 링크해서 실행하는 것을 의미한다.
        if (OnDeath != null)
        {
            OnDeath();
        }
        Destroy(gameObject);
    }
}
