using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed = 10f;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        // 총알 Shoot
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }
}
