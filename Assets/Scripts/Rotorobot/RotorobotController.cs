using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotorobotController : MonoBehaviour
{
    public AlertRangeCheck alertRange;
    public AlertRangeCheck attackRange;
    public DCSpriteAnimatorHost anim;
    private Coroutine coroutine;
    private GameObject hero;
    private float nextChaseCooldown;
    public float alertTime;
    public GameObject death;
    public HealthManager hm;
    public bool IsAlert => (alertTime > 0) || alertRange.AnyGameObject;
    public bool IsUnalertIdle => anim.curClipName == "Unalert Idle";
    public bool IsAttackIdle => anim.curClipName == "Attack Idle";
    public bool FacingRight => transform.GetScaleX() < 0;
    public bool InAttackRange => attackRange.AnyGameObject;
    void Die()
    {
        death.transform.parent = null;
        death.transform.localScale = transform.localScale;
        death.transform.position = transform.position;
        death.SetActive(true);
        Destroy(gameObject);
    }
    IEnumerator AtkFlip()
    {
        anim.curClipName = "Attack Turn";
        while (!IsAttackIdle) yield return null;
    }
    IEnumerator AtkFaceHero()
    {
        if (hero == null) yield break;
        var hpos = hero.transform.position;
        var spos = transform.position;
        if((hpos.x > spos.x) != FacingRight)
        {
            yield return AtkFlip();
        }
    }
    IEnumerator AtkRunFaceHero()
    {
        if (hero == null) yield break;
        var hpos = hero.transform.position;
        var spos = transform.position;
        if ((hpos.x > spos.x) != FacingRight)
        {
            anim.curClipName = Random.value < 0.5f ? "Attack Run Turn L" : "Attack Run Turn R";
            while (anim.CurrentClipName != "Attack Run") yield return null;
        }
    }
    IEnumerator AtkChaseHero()
    {
        if (hero == null || nextChaseCooldown > 0) yield break;
        
        while(IsAlert && !InAttackRange)
        {
            if(anim.CurrentClipName != "Attack Run")
            {
                anim.curClipName = "Attack Run";
            }
            if (InAttackRange)
            {
                yield return new WaitForSeconds(0.15f * Random.value);
                anim.curClipName = "Attack Idle";
                nextChaseCooldown = 3;
                yield break;
            }
            yield return AtkRunFaceHero();
        }
    }
    IEnumerator UnalertIdle()
    {
        anim.curClipName = "Unalert Idle";
        while(!IsAlert)
        {
            yield return null;
        }
        anim.curClipName = "Alert Hero";
        while(!IsAttackIdle) yield return null;
    }
    IEnumerator AlertIdle()
    {
        anim.curClipName = "Attack Idle";
        while(IsAlert)
        {
            yield return null;
            //yield return AtkFaceHero();
            yield return AtkChaseHero();
            yield return AtkFaceHero();
            var atkClip = RandomUtils.RandomEvent(2,3,1,1) switch
            {
                0 => "Attack 1",
                1 => "Attack 2",
                2 => "Attack 3",
                3 => "Attack 4",
                _ => "Attack Idle"
            };
            anim.curClipName = atkClip;
            while(!IsAttackIdle) yield return null;
            if (atkClip == "Attack 4") yield return new WaitForSeconds(0.5f * Random.value);
        }
        anim.curClipName = "Unalert Hero";
        while(!IsUnalertIdle) yield return null;
    }
    IEnumerator LifeLoop()
    {
        anim.curClipName = "Unalert Idle";
        while(true)
        {
            yield return null;
            if(IsAlert)
            {
                yield return AlertIdle();
            }
            else
            {
                yield return UnalertIdle();
            }
        }
    }
    private void Update()
    {
        nextChaseCooldown -= Time.deltaTime;
        if (alertRange.AnyGameObject)
        {
            alertTime = 8;
            hero = alertRange.gameObjects[0];
        }
        else
        {
            alertTime -= Time.deltaTime;
        }
        if(hm.hp <= 0)
        {
            Die();
        }
    }
    private void OnEnable()
    {
        anim.curClipName = "Unalert Idle";
        alertTime = 0;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(LifeLoop());
    }
    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
