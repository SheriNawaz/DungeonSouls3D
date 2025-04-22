using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DragonController : MonoBehaviour
{
	public float maxHealth = 1000f;
	public GameObject healthBarObject;
	public Slider healthBarSlider;
	public TMP_Text bossText;
	public string bossName;
	public float damage = 50f;
	public float flameDamage = 75f;
	public ParticleSystem flame1;
	public ParticleSystem flame2;
	private Animator animator;
	public float health = 1000f;
	public Transform player;
	public float detectionRange = 20f;
	public float attackRange = 5f;
	public float flyingHeight = 3f; 

	public float groundAttackDelay = 3f;
	public float flyingAttackDelay = 5f;
	public float flyingDuration = 30f;
	public float flyingSpeed = 10f;
	public float walkSpeed = 3.5f;
	public float circlingRadius = 15f;
	public float circlingSpeed = 2f;

	private enum DragonState { Idle, Walking, Attacking, TakingOff, Flying, Landing }
	private DragonState currentState = DragonState.Idle;

	private Coroutine attackRoutine;
	private Coroutine flyingRoutine;
	public bool isAttacking = false;
	public bool hitPlayer = false;
	private float distanceToPlayer;
	private Vector3 directionToPlayer;

	private bool isDead = false;

	private float initialGroundLevel;
	private PlayerController playerController;
	public Vector3 startPos = new Vector3(12, 251, 16);

	private void Start()
	{
		playerController = FindFirstObjectByType<PlayerController>();
		health = maxHealth;
		animator = GetComponent<Animator>();
		initialGroundLevel = transform.position.y;

		if (player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player")?.transform;
		}
	}

	private void Update()
	{
		//Update health bar
		if (FindFirstObjectByType<LevelChanger>().nextLevel)
		{
			healthBarSlider.value = health / maxHealth;
			bossText.text = bossName;
		}

		directionToPlayer = player.position - transform.position;
		directionToPlayer.y = 0; 
		distanceToPlayer = directionToPlayer.magnitude;

		//State machine for dragon states
		switch (currentState)
		{
			case DragonState.Idle:
				HandleIdleState();
				break;

			case DragonState.Walking:
				HandleWalkingState();
				break;

			case DragonState.Attacking:
				break;

			case DragonState.TakingOff:
				break;

			case DragonState.Flying:
				break;

			case DragonState.Landing:
				break;
		}

		HandleDeath();
	}

	void HandleDeath()
	{
		if (health <= 0 && !isDead)
		{
			isDead = true;
			StartCoroutine(Die());
		}
	}

	IEnumerator Die()
	{
		animator.SetTrigger("Death");

		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length;
		yield return new WaitForSeconds(animLength);
		Destroy(gameObject, 2f);
		SceneManager.LoadScene(2);
	}

	private void HandleIdleState()
	{
		// Check if player is in detection range
		if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
		{
			// Start walking toward player
			currentState = DragonState.Walking;
			animator.SetTrigger("Walk");
		}
		else if (distanceToPlayer <= attackRange)
		{
			// Player is in attack range, start attack sequence
			currentState = DragonState.Attacking;
			animator.SetTrigger("StopWalking");
			attackRoutine = StartCoroutine(GroundAttackSequence());
		}
	}

	private void HandleWalkingState()
	{
		// Calculate direction to player
		Vector3 walkDirection = directionToPlayer.normalized;

		// Rotate towards player
		Quaternion targetRotation = Quaternion.LookRotation(walkDirection);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

		// Move towards player
		transform.position += transform.forward * walkSpeed * Time.deltaTime;

		// Attack if close enough
		if (distanceToPlayer <= attackRange)
		{
			currentState = DragonState.Attacking;
			animator.SetTrigger("StopWalking");
			attackRoutine = StartCoroutine(GroundAttackSequence());
		}
	}

private IEnumerator GroundAttackSequence()
{
    yield return new WaitForSeconds(0.5f);
    
    // Keep attacking for a while before possibly taking off
    float attackSequenceTimer = 0f;
    float timeUntilTakeoff = Random.Range(15f, 25f);

    while (attackSequenceTimer < timeUntilTakeoff)
    {
        // Always face the player before each attack
        Vector3 currentDirectionToPlayer = player.position - transform.position;
        currentDirectionToPlayer.y = 0;
        
        if (currentDirectionToPlayer.magnitude > 0.1f)
        {
            // Fully rotate to face player over a short duration
            Quaternion targetRotation = Quaternion.LookRotation(currentDirectionToPlayer.normalized);
            
            float rotationDuration = 0.2f;
            float elapsedTime = 0f;
            Quaternion startRotation = transform.rotation;
            
            while (elapsedTime < rotationDuration)
            {
                transform.rotation = Quaternion.Slerp(
                    startRotation, 
                    targetRotation, 
                    elapsedTime / rotationDuration
                );
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.rotation = targetRotation;
        }

        int attackType = Random.Range(1, 4); //Select random attack

        switch (attackType)
        {
            case 1:
                animator.SetTrigger("Attack1");
                break;
            case 2:
                animator.SetTrigger("Attack2");
                break;
            case 3:
                animator.SetTrigger("Attack3");
					yield return new WaitForSeconds(1f);
					flame1.Play(); //Attack3 shoots fire out
					break;
        }
		isAttacking = true;
        // Wait for attack animation and add delay between attacks
        float attackAnimDuration = 2.0f; // Approximate attack animation length
        yield return new WaitForSeconds(attackAnimDuration + groundAttackDelay);
		hitPlayer = false;
		isAttacking = false;
		flame1.Stop();
        // Check if player is still in range
        distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z), 
            new Vector3(player.position.x, 0, player.position.z)
        );
        
        if (distanceToPlayer > attackRange * 1.5f)
        {
            // Player moved away, go back to idle which will trigger walking
            currentState = DragonState.Idle;
            yield break;
        }

        attackSequenceTimer += attackAnimDuration + groundAttackDelay;
    }


	currentState = DragonState.TakingOff;
    animator.SetTrigger("TakeOff");

    yield return new WaitForSeconds(3.0f); 

    currentState = DragonState.Flying;
    flyingRoutine = StartCoroutine(FlyingSequence());
}

	private IEnumerator FlyingSequence()
	{
		animator.applyRootMotion = false;
		//Fly the dragon up and fly around in a circle occasionally breathing fire down at player

		float flyHeight = initialGroundLevel + flyingHeight;
		float flyingTimer = 0f;
		float nextAttackTime = Random.Range(5f, 10f);
		Vector3 circlingCenter = player.position;
		float currentAngle = Random.Range(0f, 2f * Mathf.PI);

		Vector3 currentPosition = transform.position;
		float targetHeight = Mathf.Min(currentPosition.y + flyingHeight, flyHeight);
		float adjustmentDuration = 0.5f;
		float adjustmentTimer = 0f;
		float startHeight = currentPosition.y;

		while (adjustmentTimer < adjustmentDuration)
		{
			float t = adjustmentTimer / adjustmentDuration;
			Vector3 pos = transform.position;
			pos.y = Mathf.Lerp(startHeight, targetHeight, t);
			transform.position = pos;
			adjustmentTimer += Time.deltaTime;
			yield return null;
		}

		while (flyingTimer < flyingDuration)
		{
			//Calculate path to fly around enemy
			circlingCenter = new Vector3(player.position.x, targetHeight, player.position.z);

			currentAngle += circlingSpeed * Time.deltaTime;
			float x = circlingCenter.x + Mathf.Cos(currentAngle) * circlingRadius;
			float z = circlingCenter.z + Mathf.Sin(currentAngle) * circlingRadius;

			Vector3 targetPosition = new Vector3(x, targetHeight, z);

			Vector3 moveDirection = targetPosition - transform.position;
			if (moveDirection.magnitude > 0.1f)
			{
				Vector3 flatDirection = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
				Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
			}

			Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, flyingSpeed * Time.deltaTime);
			newPosition.y = targetHeight; //Keep height consistent
			transform.position = newPosition;

			if (flyingTimer >= nextAttackTime)
			{
				// Trigger attack animation
				animator.SetTrigger("FlyAttack");
				flame2.Play();
			
				Vector3 attackTarget = new Vector3(player.position.x, targetHeight, player.position.z);
				Vector3 originalPosition = transform.position;

				// Brief dive toward player
				float attackDuration = 1.5f;
				float elapsed = 0f;

				float originalDistance = Vector3.Distance(originalPosition, attackTarget);

				while (elapsed < attackDuration)
				{
					float t = elapsed / attackDuration;

					// Move toward player
					Vector3 direction = (attackTarget - originalPosition).normalized;
					float distance = Mathf.Lerp(originalDistance, 5f, t); // Get closer
					Vector3 newPos = attackTarget - direction * distance;
					newPos.y = targetHeight; // Maintain height

					transform.position = newPos;

					// Look at player
					Vector3 lookDirection = new Vector3(attackTarget.x, transform.position.y, attackTarget.z) - transform.position;
					if (lookDirection.magnitude > 0.1f)
					{
						Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
						transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
					}

					elapsed += Time.deltaTime;
					yield return null;
				}

				// Brief pause at attack 
				yield return new WaitForSeconds(0.5f);
				flame2.Stop();
				// Set next attack time
				nextAttackTime = flyingTimer + Random.Range(flyingAttackDelay, flyingAttackDelay * 1.5f);
			}

			yield return null;
			flyingTimer += Time.deltaTime;
		}

		currentState = DragonState.Landing;
		animator.SetTrigger("Land");

		// Move to position near player for landing
		Vector3 landingPosition = player.position - player.forward * attackRange;
		landingPosition.y = transform.position.y; 

		Vector3 landingDirectionFlat = new Vector3(landingPosition.x, transform.position.y, landingPosition.z) - transform.position;
		if (landingDirectionFlat.magnitude > 0.1f)
		{
			Quaternion landingRotation = Quaternion.LookRotation(landingDirectionFlat);
			transform.rotation = landingRotation;
		}

		yield return new WaitForSeconds(1.0f);

		float groundLevel = initialGroundLevel; 
		currentPosition = transform.position;

		// Only descend above ground level
		if (currentPosition.y > groundLevel + 0.1f)
		{
			float descentDuration = 1.0f;
			float descentTimer = 0f;
			float startingHeight = currentPosition.y;

			while (descentTimer < descentDuration)
			{
				float t = descentTimer / descentDuration;
				Vector3 pos = transform.position;
				pos.y = Mathf.Lerp(startingHeight, groundLevel, t);
				transform.position = pos;
				descentTimer += Time.deltaTime;
				yield return null;
			}
		}

		yield return new WaitForSeconds(2.0f);

		animator.applyRootMotion = true;

		currentState = DragonState.Idle;
	}

	public void OnTakeOffComplete()
	{
		if (currentState == DragonState.TakingOff && flyingRoutine == null)
		{
			currentState = DragonState.Flying;
			flyingRoutine = StartCoroutine(FlyingSequence());
		}
	}

	public void OnLandComplete()
	{
		currentState = DragonState.Idle;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);

		if (player != null && currentState == DragonState.Flying)
		{
			float displayHeight = initialGroundLevel + flyingHeight;
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(new Vector3(player.position.x, displayHeight, player.position.z), circlingRadius);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Sword") && playerController.isAttacking)
		{
			playerController.currentEnemy = null;
			health -= playerController.damage;
			Debug.Log("Boss hit for " + playerController.damage + " damage");
		}
	}
}