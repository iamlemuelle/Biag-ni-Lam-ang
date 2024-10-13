using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    private Animator myAnimator;
    readonly int FIRE_HASH = Animator.StringToHash("Fire"); // performance enhancing

    private void Awake() {
        myAnimator = GetComponent<Animator>();
    }
    public void Attack() {
        // ActiveWeapon.Instance.ToggleIsAttacking(false);
        myAnimator.SetTrigger(FIRE_HASH);
        GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, ActiveWeapon.Instance.transform.rotation);
        newArrow.GetComponent<Projectile>().UpdateProjectileRange(weaponInfo.weaponRange);
    }

    public WeaponInfo GetWeaponInfo() {
        return weaponInfo;
    }
}
