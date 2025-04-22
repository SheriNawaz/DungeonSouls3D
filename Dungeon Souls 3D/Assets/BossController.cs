using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] TMP_Text bossNameText;
	[SerializeField] string bossName;
	[SerializeField] Slider healthSlider;
	[Header("Stats")]
	public float health = 20f;
	public float damage = 10f;

	[SerializeField] float attack1Dmg = 10f;
	[SerializeField] float attack2Dmg = 50f;
	[SerializeField] float attack3Dmg = 100f;
	public Vector3 startPos = new Vector3(20f, 251f, 20f);
	[Header("Movement")]
	public float moveSpeed = 3f;
	public float attackRange = 2f;
	public float retreatRange = 1f;
	public float stoppingDistance = 0.5f;
	[Header("Retreat Behavior")]
	[SerializeField][Range(0f, 1f)] float retreatChance = 0.3f; 
	[SerializeField] float retreatDuration = 5f; 
	[SerializeField] float retreatSpeed = 4f; 
	[SerializeField] float waitAfterRetreatDuration = 1f;
	[Header("Combat")]
	public bool isAttacking = false;
	public bool hitPlayer = false;
	public int xpDropped = 1000;

	public GameObject fireBreath;

	private bool isRetreating = false;
	private bool isWaitingAfterRetreat = false;
	private float retreatTimer = 0f;
	private float waitAfterRetreatTimer = 0f;
	public float maxHealth;
	private bool isDead = false;

	[Header("Player")]
	public Transform player;
	private PlayerController playerController;
	private Animator animator;
	private float startY;

	private void Start()
	{
		bossNameText.text = bossName;
		health = maxHealth;
		animator = GetComponent<Animator>();
		player = FindFirstObjectByType<PlayerController>().transform;
		playerController = player.GetComponent<PlayerController>();
		startY = transform.position.y;
	}

	private void Update()
	{
		transform.position = new Vector3(transform.position.x, startY, transform.position.z);
		healthSlider.value = health / maxHealth;

		if (isDead) return;

		float distance = Vector3.Distance(transform.position, player.position);

		if (isRetreating)
		{
			Retreat();
			return;
		}

		if (isWaitingAfterRetreat)
		{
			WaitAfterRetreat();
			return;
		}
		//Retreat or attack if within range
		if (distance <= retreatRange)
		{
			if (Random.value < retreatChance * Time.deltaTime * 10) 
			{
				StartRetreat();
			}
			else if (!isAttacking)
			{
				StartCoroutine(Attack());
			}
		}
		else if (distance <= attackRange && distance > retreatRange && !isAttacking)
		{
			StartCoroutine(Attack());
		}
		else if (distance > attackRange)
		{
			MoveTowardsPlayer();
		}

		HandleDeath();
	}

	void MoveTowardsPlayer()
	{
		Vector3 direction = (player.position - transform.position).normalized;
		transform.position += direction * moveSpeed * Time.deltaTime;
		FacePlayer();
		animator.SetBool("isAttacking", false);
		animator.SetFloat("speed", 1);
	}

	void StartRetreat()
	{
		isRetreating = true;
		retreatTimer = retreatDuration;
		animator.SetBool("isAttacking", false);
		animator.SetFloat("speed", 1.5f); // Can use a faster animation if available
	}

	void Retreat()
	{
		// Move away from player
		Vector3 retreatDirection = (transform.position - player.position).normalized;
		transform.position += retreatDirection * retreatSpeed * Time.deltaTime;
		FacePlayer(); // Still look at player while backing away
		retreatTimer -= Time.deltaTime;
		if (retreatTimer <= 0)
		{
			isRetreating = false;
			StartWaitAfterRetreat(); // Start waiting period after retreat ends
		}
	}

	void StartWaitAfterRetreat()
	{
		isWaitingAfterRetreat = true;
		waitAfterRetreatTimer = waitAfterRetreatDuration;
		animator.SetFloat("speed", 0); // Set animation speed to 0 during waiting
	}

	void WaitAfterRetreat()
	{
		// Just wait and count down the timer
		waitAfterRetreatTimer -= Time.deltaTime;
		if (waitAfterRetreatTimer <= 0)
		{
			isWaitingAfterRetreat = false;
			animator.SetFloat("speed", 1); 
		}
	}

	IEnumerator Attack()
	{
		isAttacking = true;

		//Select random attack
		int attackNum = Random.Range(1, 4);
		switch (attackNum)
		{
			case 1:
				damage = attack1Dmg;
				break;
			case 2:
				damage = attack2Dmg;
				break;
			case 3:
				damage = attack3Dmg;
				break;
			default:
				break;
		}
		animator.SetInteger("Attack", attackNum);
		animator.SetBool("isAttacking", true);
		yield return new WaitForSeconds(0.5f);
		//Wait between attacks
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length;
		yield return new WaitForSeconds(animLength);
		animator.SetBool("isAttacking", false);
		animator.SetInteger("Attack", 0);
		hitPlayer = false;
		isAttacking = false;

	}

	void FacePlayer()
	{
		Vector3 lookDirection = player.position - transform.position;
		lookDirection.y = 0f;
		if (lookDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 10f * Time.deltaTime);
		}
	}

	void HandleDeath()
	{
		if (health <= 0 && !isDead)
		{
			//Drop player xp
			isDead = true;
			playerController.currentXP += xpDropped;
			playerController.totalXP += xpDropped;
			StartCoroutine(Die());
		}
	}

	IEnumerator Die()
	{
		isAttacking = false;
		hitPlayer = false;
		animator.SetTrigger("Death");

		isRetreating = false;
		isWaitingAfterRetreat = false;

		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length;
		yield return new WaitForSeconds(animLength);
		FindFirstObjectByType<LevelChanger>().ChangeLevel();
		Destroy(gameObject, 2f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Sword") && playerController.isAttacking)
		{
			//Take damage
			playerController.currentEnemy = null; 
			health -= playerController.damage;
			Debug.Log("Boss hit for " + playerController.damage + " damage");
		}
	}
}