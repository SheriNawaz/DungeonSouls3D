using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class PlayerController : MonoBehaviour
{
	[Header("Component References")]
	[SerializeField] Transform camera;
	[SerializeField] GameObject lockOnCam;
	[SerializeField] Camera mainCam;
	[SerializeField] Transform groundCheck;
	[SerializeField] Animator animator;
	[SerializeField] Slider staminaSlider;
	[SerializeField] Slider healthSlider;
	[SerializeField] TMP_Text healthText;
	[SerializeField] GameObject weaponSlot;
	[SerializeField] GameObject inventory;
	[SerializeField] TMP_Text healthPotions;
	[SerializeField] GameObject healingEffect;
	[SerializeField] TMP_Text statsGold;
	[SerializeField] TMP_Text invGold;
	[SerializeField] TMP_Text shopGold;
	public SpawnPoint currentSpawnPoint;
	public GameObject bossHealthBar;

	[Header("Movement")]
	[SerializeField] float walkSpeed = 6f;
	[SerializeField] float runSpeed = 10f;
	[SerializeField] float jumpSpeed = 3f;
	[SerializeField] float jumpForce = 20f;
	[SerializeField] float turnSmoothTime = 0.1f;
	[SerializeField] float animationBlend = 4f;
	[SerializeField] float rollKeyTime = 0.2f;

	[Header("Stamina")]
	public float maxStamina = 200f;
	[SerializeField] float stamRegen = 10f;
	[SerializeField] float stamDelay = 1f;
	[SerializeField] float runStam = 15f;
	[SerializeField] float jumpStam = 20f;
	[SerializeField] float rollStam = 25f;

	[Header("Combat")]
	[SerializeField] float attackingSpeed = 3f;
	[SerializeField] float attackStam = 15f;
	public float maxHealth = 100f;
	public float damage;
	public float currentHealth;
	[SerializeField] float damageModifier = 0f;

	[Header("Player Loot")]
	public int maxPotions = 3;
	public int healthPots;
	public int currentGold = 0;

	[Header("Experience")]
	[SerializeField] int currentLevel = 0;
	public int currentXP = 0;
	[SerializeField] int nextLevelXP = 50;
	[SerializeField] int defenceLevel = 0;
	[SerializeField] int strengthLevel = 0;
	[SerializeField] int healthLevel = 0;
	[SerializeField] int staminaLevel = 0;
	[SerializeField] TMP_Text levelText;
	[SerializeField] TMP_Text xpText;
	[SerializeField] TMP_Text defenceText;
	[SerializeField] TMP_Text strengthText;
	[SerializeField] TMP_Text healthLvlText;
	[SerializeField] TMP_Text staminaText;
	public int totalXP = 0;

	public bool hasBossKey = false;
	public bool hasMapFragment = false;
	private bool resetting = false;

	private Rigidbody rigidBody;
	private CapsuleCollider hitbox;
	private WeaponManager weapon;
	public EnemyController currentEnemy = null;

	public float currentStamina;
	private float lastStaminaUseTime;
	private float turnSmoothVelocity;
	private float currentSpeed;
	private float shiftTime = 0f;
	private float attackSpeed = 0f;
	private float damageReduction = 0f;
	private float startMaxHP;
	private float startmaxStamina;
	private float startstamRegen;
	private float startrunStam;
	private float startrollStam;
	private float startjumpStam;
	private int startNextLVLXP;


	[HideInInspector] public bool isGrounded = true;
	public bool isAttacking = false;
	private bool canRun = true; 
	private bool isRunning;
	private bool animInProgress = false;
	private bool isRolling;
	private bool sprinting = false;
	private bool lockedIn = false;

	private Vector3 blendedInput = Vector3.zero;
	private Transform currentTarget = null;

	private void Start()
	{
		hitbox = GetComponent<CapsuleCollider>();
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.freezeRotation = true;
		currentSpeed = walkSpeed;
		currentStamina = maxStamina;
		lastStaminaUseTime = -stamDelay;
		currentHealth = maxHealth;
		weapon = GetComponentInChildren<WeaponManager>();
		healthPots = maxPotions;
		healthPotions.text = maxPotions.ToString();

		startMaxHP = maxHealth;
		startmaxStamina = maxStamina;
		startstamRegen = stamRegen;
		startrunStam = runStam;
		startrollStam = rollStam;
		startjumpStam = jumpStam;
		startNextLVLXP = nextLevelXP;
	}

	private void FixedUpdate()
	{
		HandleMovement();
	}

	private void Update()
	{
		HandleStamina();
		HandleJumping();
		HandleRunAndRoll();
		HandleCombat();
		HandleCrouching();
		HandleUI();
		HandleLockOn();
		UpdatePlayerRotationToFaceTarget();
		HandleDeath();
		HandleHealing();
		if (Input.GetKeyDown(KeyCode.M) && hasMapFragment)
		{
			FindFirstObjectByType<PauseMenu>().ViewMap();
		}
		if(currentXP <= nextLevelXP)
		{
			resetting = false;
		}
	}

	public void UseCharmStats(string previous, string current)
	{
		//Increase stats when a charm is used and decrease stats when its removed
		switch (previous)
		{
			case "Defence":
				defenceLevel -= 5;
				damageReduction -= 5;
				break;
			case "Strength":
				strengthLevel -= 5;
				damageModifier -= 10;
				break;
			case "Health":
				healthLevel -= 5;
				currentHealth -= 25;
				maxHealth -= 5;
				break;
			case "Stamina":
				staminaLevel -= 5;
				maxStamina -= 50;
				stamRegen -= 2.5f;
				runStam += 0.5f;
				rollStam += 0.5f;
				jumpStam += 0.5f;
				break;
			default:
				break;
		}
		switch (current)
		{
			case "Defence":
				defenceLevel += 5;
				damageReduction += 5;
				break;
			case "Strength":
				strengthLevel += 5;
				damageModifier += 10;
				break;
			case "Health":
				healthLevel += 5;
				currentHealth += 25;
				maxHealth += 5;
				break;
			case "Stamina":
				staminaLevel += 5;
				maxStamina += 50;
				stamRegen += 2.5f;
				runStam -= 0.5f;
				rollStam -= 0.5f;
				jumpStam -= 0.5f;
				break;
			default:
				break;
		}
	}

	private void HandleHealing()
	{
		//Allow usage of heal potions when states are correct and play an effect and animation for it
		if (Input.GetKeyDown(KeyCode.R) && healthPots > 0 && isGrounded && !isAttacking && !isRolling)
		{
			if(currentHealth >= maxHealth - 35f && currentHealth < maxHealth)
			{
				Instantiate(healingEffect, transform);
				StartCoroutine(PerformAnimation("isHealing"));
				currentHealth = maxHealth;
				healthPots--;
			}
			else if(currentHealth >= maxHealth)
			{
				return;
			}
			else
			{
				Instantiate(healingEffect, transform);

				StartCoroutine(PerformAnimation("isHealing"));
				healthPots--;
				currentHealth += 35f;
			}
		}
	}

	private void HandleDeath()
	{
		if(currentHealth <= 0)
		{
			//Reset position and heal bosses
			bossHealthBar.SetActive(false);
			LevelChanger level = FindFirstObjectByType<LevelChanger>();
			if (!level.nextLevel)
			{
				BossController levelBoss = FindFirstObjectByType<BossController>();
				levelBoss.health = levelBoss.maxHealth;
				levelBoss.transform.position = levelBoss.startPos;
			}
			else
			{
				DragonController levelBoss = FindFirstObjectByType<DragonController>();
				levelBoss.health = levelBoss.maxHealth;
				levelBoss.transform.position = levelBoss.startPos;
			}
			if (currentSpawnPoint == null)
			{
				currentSpawnPoint = FindFirstObjectByType<SpawnPoint>();
			}

			transform.position = currentSpawnPoint.transform.position;
			currentHealth = maxHealth;
			currentStamina = maxStamina;
			healthPots = maxPotions;
			
		}
	}

	private void UpdatePlayerRotationToFaceTarget()
	{
		if (lockedIn && currentTarget != null)
		{
			//Face target when locked in
			Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
			directionToTarget.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50f * Time.deltaTime);
		}
	}

	private void HandleLockOn()
	{
		if (Input.GetKeyDown(KeyCode.Mouse2))
		{
			if (lockedIn)
			{
				lockedIn = false;
				currentTarget = null;
				lockOnCam.SetActive(false);
				camera.gameObject.SetActive(true);
			}
			else
			{
				//Find the closest enemy and lock onto them if middle mouse is pressed
				EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
				float closestDistance = Mathf.Infinity;
				EnemyController closestEnemy = null;

				foreach (EnemyController enemy in enemies)
				{
					Vector3 directionToEnemy = enemy.transform.position - transform.position;
					float angleToEnemy = Vector3.Angle(camera.forward, directionToEnemy);

					if (angleToEnemy < 30f)
					{
						float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
						if (distanceToEnemy < closestDistance)
						{
							closestEnemy = enemy;
							closestDistance = distanceToEnemy;
						}
					}
				}

				if (closestEnemy != null)
				{
					lockOnCam.SetActive(true);
					camera.gameObject.SetActive(false);

					lockedIn = true;
					currentTarget = closestEnemy.transform;
					UpdatePlayerRotationToFaceTarget();
				}
			}
		}
	}

	private void HandleUI()
	{
		staminaSlider.value = currentStamina / maxStamina;
		healthSlider.value = currentHealth / maxHealth;
		healthPotions.text = healthPots.ToString();
		healthText.text = currentHealth + "/" + maxHealth;
		statsGold.text = currentGold.ToString();
		invGold.text = currentGold.ToString();
		shopGold.text = currentGold.ToString();
		levelText.text = "Level " + currentLevel.ToString();
		xpText.text = "XP: " + currentXP.ToString() + "/" + nextLevelXP.ToString();
		defenceText.text = "Defence " + defenceLevel.ToString();
		strengthText.text = "Strength " + strengthLevel.ToString();
		healthLvlText.text = "Health " + healthLevel.ToString();
		staminaText.text = "Stamina " + staminaLevel.ToString();
	}

	private void HandleCrouching()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			hitbox.center = new Vector3(0f, 0.75f, 0f);
			hitbox.height = 1.5f;
			animator.SetBool("isCrouching", true);
		}
		if (Input.GetKeyUp(KeyCode.LeftControl))
		{
			hitbox.center = new Vector3(0f, 1f, 0f);
			hitbox.height = 2f;
			animator.SetBool("isCrouching", false);
		} 
	}

	private void HandleStamina()
	{
		if (Time.time > lastStaminaUseTime + stamDelay)
		{
			currentStamina += stamRegen * Time.deltaTime;
			currentStamina = Mathf.Min(currentStamina, maxStamina);
		}

		if (isRunning && isGrounded && rigidBody.linearVelocity.magnitude > 0.1f)
		{
			UseStamina(runStam * Time.deltaTime);
			if (currentStamina <= 0)
			{
				canRun = false;
				isRunning = false;
			}
		}
		else if (currentStamina > runStam * 0.5f) 
		{
			canRun = true;
		}
	}

	private void UseStamina(float amount)
	{
		currentStamina -= amount;
		currentStamina = Mathf.Max(currentStamina, 0); 
		lastStaminaUseTime = Time.time; 
	}

	private void HandleCombat()
	{
		damage = weaponSlot.GetComponentInChildren<WeaponManager>().damage + damageModifier;
		attackSpeed = weaponSlot.GetComponentInChildren<WeaponManager>().attackSpeedModifier;

		if (Input.GetKeyDown(KeyCode.Mouse0) && !animInProgress && currentStamina >= attackStam)
		{
			UseStamina(attackStam);
			StartCoroutine(PerformAnimation("isAttacking"));
		}
	}

	private IEnumerator PerformAnimation(string anim)
	{
		animInProgress = true;
		currentSpeed = jumpSpeed;
		animator.SetBool(anim, true);

		if (anim == "isAttacking")
		{
			isAttacking = true;
			currentSpeed = 1f;
			animator.speed = attackSpeed;
		}

		if (anim == "isRolling")
		{
			isRolling = true;
			hitbox.enabled = false;
			rigidBody.useGravity = false;

			Vector3 rollVelocity = rigidBody.linearVelocity;
			rollVelocity.y = 0;
			rigidBody.linearVelocity = rollVelocity;
		}

		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		float animLength = stateInfo.length / animator.speed;
		yield return new WaitForSeconds(animLength);
		if (anim == "isAttacking")
		{
			isAttacking = false;
			if (currentEnemy != null)
			{
				currentEnemy.invulnerable = false;
			}
			animator.speed = 1f;
		}

		if (anim == "isRolling")
		{
			isRolling = false;
			hitbox.enabled = true;
			rigidBody.useGravity = true;
		}
		animator.SetBool(anim, false);
		animInProgress = false;
		ResetSpeed();
	}

	private void HandleRunAndRoll()
	{
		if (Input.GetKeyDown(KeyCode.LeftShift)) //Sprint when sprint key held down
		{
			shiftTime = Time.time;
			sprinting = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift)) //Roll if sprint key was tapped and not held
		{
			float keyHeldDuration = Time.time - shiftTime;
			sprinting = false;

			if (keyHeldDuration < rollKeyTime && !animInProgress && isGrounded && currentStamina >= rollStam)
			{
				UseStamina(rollStam);
				StartCoroutine(PerformAnimation("isRolling"));
			}
		}

		isRunning = sprinting && canRun;
	}

	private void HandleJumping()
	{
		if (isRolling) return;

		if (isGrounded && Input.GetButtonDown("Jump") && currentStamina >= jumpStam)
		{
			UseStamina(jumpStam);
			rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			isGrounded = false;
			currentSpeed = jumpSpeed;
			animator.SetBool("isJumping", true);
		}
		if (isGrounded)
		{
			animator.SetBool("isJumping", false);
		}
		animator.SetBool("isGrounded", isGrounded);
	}

	private void HandleMovement()
	{
		//Set animations, rotations and movements of player
		ResetSpeed();
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");
		Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized;
		blendedInput = Vector3.Lerp(blendedInput, new Vector3(horizontal, 0f, vertical), animationBlend * Time.deltaTime);
		animator.SetFloat("Horizontal", blendedInput.x);
		animator.SetFloat("Vertical", blendedInput.z);

		animator.SetFloat("speed", Mathf.Abs(blendedInput.x + blendedInput.z));
		if (dir.magnitude >= 0.01f)
		{
			float targetAngle;
			if (lockedIn) //Set rotation when lock on camera enabled
				targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + lockOnCam.transform.eulerAngles.y;
			else
				targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
			float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
				
			if(!lockedIn)
				transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
				
			Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

			if (isRolling)
			{
				rigidBody.linearVelocity = new Vector3(moveDirection.x * currentSpeed, 0, moveDirection.z * currentSpeed);
			}
			else
			{
				rigidBody.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rigidBody.linearVelocity.y, moveDirection.z * currentSpeed);
			}
		}
		else
		{
			if (isRolling)
			{
				rigidBody.linearVelocity = new Vector3(0f, 0f, 0f);
			}
			else
			{
				rigidBody.linearVelocity = new Vector3(0f, rigidBody.linearVelocity.y, 0f);
			}
		}
	}

	private void ResetSpeed()
	{
		if (isAttacking)
		{
			currentSpeed = attackingSpeed;
		}
		if (isGrounded && !isRunning && !isAttacking)
		{
			currentSpeed = walkSpeed;
			animator.speed = 1f;
		}
		else if (isGrounded && isRunning && !isAttacking)
		{
			currentSpeed = runSpeed;
			animator.speed = 1.2f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "EnemySword")
		{
			//Take damage from enemy
			EnemyController enemy = other.gameObject.GetComponentInParent<EnemyController>();
			if (enemy.isAttacking && !enemy.hitPlayer && !isRolling)
			{
				enemy.hitPlayer = true;
				if (enemy.damage - damageReduction > 1)
					currentHealth -= (enemy.damage - damageReduction);
				else
					currentHealth -= 1; //Make enemy deal less damage when defence is higher. But enemies can never deal 0 damage
				currentEnemy = enemy;
				print(currentEnemy.name);
			}
		}
		else if (other.tag == "BossSword")
		{
			//Take damage from boss
			BossController enemy = other.gameObject.GetComponentInParent<BossController>();
			if (enemy.isAttacking && !enemy.hitPlayer && !isRolling)
			{
				enemy.hitPlayer = true;
				if (enemy.damage - damageReduction > 1)
					currentHealth -= (enemy.damage - damageReduction);
				else
					currentHealth -= 1;
			}
		}
		else if (other.tag == "Dragon")
		{
			//Take damage from dragon
			DragonController enemy = FindFirstObjectByType<DragonController>();
			if (enemy.isAttacking && !enemy.hitPlayer && !isRolling)
			{
				print("Hit");
				enemy.hitPlayer = true;
				if (enemy.damage - damageReduction > 1)
					currentHealth -= (enemy.damage - damageReduction);
				else
					currentHealth -= 1;
			}
		}
	}

	private void OnParticleCollision()
	{
		//Take damage from dragon breath
		DragonController enemy = FindFirstObjectByType<DragonController>();
		if (enemy.isAttacking && !enemy.hitPlayer && !isRolling)
		{
			enemy.hitPlayer = true;
			if (enemy.flameDamage - damageReduction > 1)
				currentHealth -= (enemy.flameDamage - damageReduction);
			else
				currentHealth -= 1;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if(collision.gameObject.tag == "Bossdoor" && hasBossKey)
		{
			//Enter bossroom if colliding with the bossdoor and if the player has the key
			transform.position = new Vector3(0f, 250.5f, 0f);
			bossHealthBar.SetActive(true);
		}

	}

	public void DefenceUp() //Level up defence
	{
		if(currentXP >= nextLevelXP)
		{
			currentXP = currentXP - nextLevelXP;
			nextLevelXP += 50;
			currentLevel++;
			defenceLevel++;
			damageReduction += 1;
		}
	}

	public void HealthUp() //Level up health
	{
		if (currentXP >= nextLevelXP)
		{
			currentXP = currentXP - nextLevelXP;
			nextLevelXP += 50;
			currentLevel++;
			healthLevel++;
			maxHealth += 5;
			currentHealth += 5;
		}
	}

	public void StaminaUp() //Level up stamina
	{
		if (currentXP >= nextLevelXP)
		{
			currentXP = currentXP - nextLevelXP;
			nextLevelXP += 50;
			currentLevel++;
			staminaLevel++;

			maxStamina += 15;
			stamRegen += 0.5f;
			runStam -= 0.5f;
			rollStam -= 0.5f;
			jumpStam -= 0.5f;
		}
	}

	public void StrengthUp() //Level up strength
	{
		if (currentXP >= nextLevelXP)
		{
			currentXP = currentXP - nextLevelXP;
			nextLevelXP += 50;
			currentLevel++;
			strengthLevel++;

			damageModifier += 2;
		}
	}

	public void ResetStats()
	{
		//For resetting stats to 0 for respeccing character
		resetting = false;
		if(currentGold >= 5000 && !resetting)
		{
			resetting = true;
			nextLevelXP = startNextLVLXP;
			currentXP = totalXP;
			currentGold -= 5000;
			currentLevel = 0;
			strengthLevel = 0;
			defenceLevel = 0;
			healthLevel = 0;
			staminaLevel = 0;
			damageModifier = 0;
			damageReduction = 0;
			maxHealth = startMaxHP;
			maxStamina = startmaxStamina;
			stamRegen = startstamRegen;
			runStam = startrunStam;
			rollStam = startrollStam;
			jumpStam = startjumpStam;
		}
		
	}
}