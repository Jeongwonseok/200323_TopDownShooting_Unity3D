using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    Rigidbody myRigidbody;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    // 플레이어 회전
    // heightCorrectPoint : y는 고정하고 바라보기 >> 기울지 않도록!!
    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectPoint);
    }

    // FixedUpdate() : 이 부분은 정기적이고 짧게 반복적으로 실행되야 하기 때문에 Update() 대신 사용
    public void FixedUpdate()
    {
        // Time.fixedDeltaTime : 두 FixedUpdate 메소드가 호출된 시간 간격
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }
}
