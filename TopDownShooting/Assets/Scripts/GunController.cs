using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun startingGun;
    Gun equippedGun; // 장착중인 총 저장 변수

    void Start()
    {
        // 시작하자마자 플레이어에 Gun 부착
        if(startingGun != null)
        {
            EquipGun(startingGun);
        }
    }

    // Gun 장착
    public void EquipGun(Gun gunToEquip)
    {
        if(equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;

        // 총이 플레이어와 같이 회전할 수 있도록 weaponHold의 자식으로 넣기 
        equippedGun.transform.parent = weaponHold;
    }

    // 총 쏘기
    public void OnTriggerHold()
    {
        if(equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    // 총 쏘기 이후
    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }
}
