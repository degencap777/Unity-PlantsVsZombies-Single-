using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHealthy : MonoBehaviour
{
    public AudioClip damageSound;
    public int hp;
    protected Animator animator;
    protected bool hasHead=true;
    public Vector3 headOffset;
    public GameObject head;
    public int lostHeadHp = 40;
    protected Blink blink;

    protected void Awake()
    {
        animator = transform.Find("Zombie").GetComponent<Animator>();
        blink = transform.Find("Zombie").GetComponent<Blink>();
    }

    public virtual void Damage(int value)
    {
        if (hp <= 0)
        {
            return;
        }
        AudioManager.GetInstance().PlaySound(damageSound);
        hp -= value;
        animator.SetInteger("hp",hp);
        blink.Begin(0.15f);
        if (hp <= lostHeadHp && hasHead)
        {
            LostHead();
        }
        if (hp <= 0)
        {
            Die();
        }
    }

    protected virtual void LostHead()
    {
        GameObject newHead = Instantiate(head);
        newHead.transform.position = transform.position + headOffset;
        Destroy(newHead,3);
        hasHead = false;
    }
    protected void Die()
    {
        ZombieMove move = GetComponent<ZombieMove>();
        GameModel.GetInstance().zombieList[move.row].Remove(gameObject);
        move.enabled = false;
        GetComponent<ZombieAttack>().StopAttack();
        Destroy(gameObject,3);
    }

    public void BoomDie()
    {
        if (hp <= 0)
        {
            return;
        }
        animator.SetTrigger("boomDie");
        Die();
    }
}
