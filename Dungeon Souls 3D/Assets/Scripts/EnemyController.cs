using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class EnemyController : MonoBehaviour
{
	[SerializeField] Slider healthSlider;
	public float health = 20f;
	public float damage = 10f;
	public bool isAttacking = false;
	public EnemyPathfinding me;
	public bool hitPlayer = false;
	public bool invulnerable = false;
	public int xpDropped = 250;
	public bool isDead =false;
	private float maxHealth;
	
	private PlayerController player;

	public void Start()
	{
		player = FindAnyObjectByType<PlayerController>();
		me = GetComponent<EnemyPathfinding>();
		maxHealth = health;
	}

	private void Update()
	{
		HandleAttacking();
		//Constantly ensure that the players healthbar is correct
		healthSlider.value = health / maxHealth;
		HandleDeath();
	}

	private void HandleDeath()
	{
		if(health <= 0 && !isDead)
		{
			//Grant player xp upon death
			isDead = true;
			player.currentXP += xpDropped;
			player.totalXP += xpDropped;
			StartCoroutine(Die());
		}
	}

	private void HandleAttacking()
	{
		if (me.canAttack && !isAttacking)
		{
			//Attack if able to
			StartCoroutine(Attack());
		}
	}

	private IEnumerator Die()
	{
		//Play death animation if enemy dies and destroy enemy gameobject after animation plays
		me.enabled = false;
		isAttacking = false;
		hitPlayer = false;
		me.animator.SetTrigger("Death");
		GetComponent<Rigidbody>().useGravity = false;
		AnimatorStateInfo stateInfo = me.animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length;
		yield return new WaitForSeconds(animLength);
		Destroy(gameObject);
	}

	public IEnumerator Attack()
	{
		//Play attack animation, wait before attacking again. Hit player used to make sure player doesnt repeatedly take damage from the same attack
		me.animator.SetBool("isAttacking", true);
		yield return new WaitForSeconds(0.5f);
		isAttacking = true;
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
			//Take damage and grant temporary invulnerability if hit by player attack
			player.currentEnemy = this;
			print("Ouch " + player.damage + "Invulnerable");
			health -= player.damage;
			invulnerable = true;

		}
	}
}