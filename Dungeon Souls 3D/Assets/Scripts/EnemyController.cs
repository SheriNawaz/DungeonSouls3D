using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class EnemyController : MonoBehaviour
{
	[SerializeField] float health = 20f;
	public float damage = 10f;
	public bool isAttacking = false;
	private EnemyPathfinding me;
	public bool hitPlayer = false;
	public bool invulnerable = false;
	
	private PlayerController player;

	private void Start()
	{
		player = FindAnyObjectByType<PlayerController>();
		me = GetComponent<EnemyPathfinding>();
	}

	private void Update()
	{
		HandleAttacking();
		HandleDeath();
	}

	private void HandleDeath()
	{
		if(health <= 0)
		{
			StartCoroutine(Die());
		}
	}

	private void HandleAttacking()
	{
		if (me.canAttack && !isAttacking)
		{
			StartCoroutine(Attack());
		}
	}

	private IEnumerator Die()
	{
		GetComponentInChildren<Image>().enabled = false;

		me.enabled = false;
		isAttacking = false;
		hitPlayer = false;
		me.animator.SetTrigger("Death");
		AnimatorStateInfo stateInfo = me.animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length;
		yield return new WaitForSeconds(animLength);
		Destroy(gameObject);
	}

	private IEnumerator Attack()
	{
		isAttacking = true;
		me.animator.SetBool("isAttacking", true);
		yield return null;
		AnimatorStateInfo stateInfo = me.animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length;
		yield return new WaitForSeconds(animLength);
		isAttacking = false;
		me.animator.SetBool("isAttacking", false);
		hitPlayer = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Sword" && !invulnerable && player.isAttacking)
		{
			health -= player.damage;
			invulnerable = true;
		}
	}
}