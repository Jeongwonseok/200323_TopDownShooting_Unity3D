using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;
    protected float health; // 상속받아야 사용 가능
    protected bool dead;

    // 추상 메서드로 선언하는 이유 : 해당 클래스를 상속받는 스크립트의 Start() 메서드가 덮어쓰지 못하도록 하기 위해!! 
    protected virtual void Start()
    {
        health = startingHealth;
    }

    // 인터페이스는 상속해서 쓸때 강제 구현해야함 >> 안하면 에러
    public void TakeHit(float damage, RaycastHit hit)
    {
        health -= damage;

        // 사망
        if(health <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        dead = true;
        Destroy(gameObject);
    }
}
