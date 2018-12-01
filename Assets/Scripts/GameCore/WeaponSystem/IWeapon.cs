using UnityEngine;
using System.Collections;

public abstract class IWeapon : MonoBehaviour
{
    //攻击力
    public float atk = 0;
    //攻击范围
    public float atkRange = 0;

    public void Attack()
    {

    }

    public abstract void ShowEffect();

    public abstract void PlaySound();

}
