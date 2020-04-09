using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5;

    public Crosshairs crosshairs;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;

    // 부모 클래스를 덮어쓰고, 부모 클래스의 Start 메서드 실행
    protected override void Start()
    {
        base.Start();
    }

    // Start() 전에 호출
    void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    // Wave 변경 시 HP 초기화 및 새로운 Gun 장착
    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    void Update()
    {
        // 플레이어 움직임 정의
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // 바라 보는 방향 정의
        // ScreenPointToRay() : 화면 상 위치 반환
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;

        // ray와 바닥 plane이 교차한다면
        if(groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);

            // Gun to point Aiming
            if((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.Aim(point);
            }
        }

        // 무기 조작 정의
        // 마우스 누를때,
        if(Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        // 마우스 뗄때,
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
        if(transform.position.y < -10)
        {
            TakeDamage(health);
        }
    }

    // 플레이어 죽으면 사운드 Override
    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
