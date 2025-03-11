using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
	[SerializeField] float health = 20f;
	[SerializeField] float currentHealth;

	private Animator animator;

	public float damage = 5f;
	private PlayerController player;
	public bool isAttacking = false;

	private void Start()
	{
		currentHealth = health;
		animator = GetComponent<Animator>();
		player = FindFirstObjectByType<PlayerController>();
	}

	private void Update()
	{
		if(currentHealth > 0)
		{
			Attack();
		}
		else
		{
			StartCoroutine(Dying());
		}
	}

	private IEnumerator Dying()
	{
		animator.SetTrigger("Death");
		print("waiting");
		yield return new WaitForSeconds(2f);
		Destroy(gameObject);
	}

	private void Attack()
	{
		if (!isAttacking) 
		{
			isAttacking = true;
			animator.SetTrigger("Attacking");
			StartCoroutine(Wait());
		}
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(5f);
		isAttacking = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Sword")
		{
			if (player.isAttacking)
			{
				print("Hit");
				player.isAttacking = false;
				currentHealth -= 5f;
			}
		}
	}
}
