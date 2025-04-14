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

	[Header("Movement")]
	[SerializeField] float walkSpeed = 6f;
	[SerializeField] float runSpeed = 10f;
	[SerializeField] float jumpSpeed = 3f;
	[SerializeField] float jumpForce = 20f;
	[SerializeField] float turnSmoothTime = 0.1f;
	[SerializeField] float animationBlend = 4f;
	[SerializeField] float rollKeyTime = 0.2f;

	[Header("Stamina")]
	[SerializeField] float maxStamina = 100f;
	[SerializeField] float stamRegen = 10f;
	[SerializeField] float stamDelay = 1f;
	[SerializeField] float runStam = 15f;
	[SerializeField] float jumpStam = 20f;
	[SerializeField] float rollStam = 25f;

	[Header("Combat")]
	[SerializeField] float attackingSpeed = 3f;
	[SerializeField] float attackStam = 15f;
	[SerializeField] float maxHealth = 100f;
	public float damage;
	[SerializeField] float currentHealth;
	[SerializeField] float damageModifier = 0f;

	[Header("Player Loot")]
	[SerializeField] int startingPotions = 3;
	public int maxPotions = 3;
	public int healthPots;
	public int currentGold = 0;

	public bool hasBossKey = false;
	public bool hasMapFragment = false;

	private Rigidbody rigidBody;
	private CapsuleCollider hitbox;
	private WeaponManager weapon;
	private EnemyController currentEnemy = null;

	private float currentStamina;
	private float lastStaminaUseTime;
	private float turnSmoothVelocity;
	private float currentSpeed;
	private float shiftTime = 0f;
	private float attackSpeed = 0f;

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
	}

	private void HandleHealing()
	{
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
			Debug.Log("Dead");
		}
	}

	private void UpdatePlayerRotationToFaceTarget()
	{
		if (lockedIn && currentTarget != null)
		{
			Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
			directionToTarget.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50f * Time.deltaTime);
		}
	}

	private void HandleLockOn()
	{
		Image lockOnImage = null;
		if (currentTarget != null)
			lockOnImage = currentTarget.GetComponentInChildren<Image>();

		if (Input.GetKeyDown(KeyCode.Mouse2))
		{
			if (lockedIn)
			{
				if (currentTarget != null && lockOnImage != null)
					lockOnImage.enabled = false;

				lockedIn = false;
				currentTarget = null;
				lockOnCam.SetActive(false);
				camera.gameObject.SetActive(true);
			}
			else
			{
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

					lockOnImage = currentTarget.GetComponentInChildren<Image>();
					if (lockOnImage != null)
						lockOnImage.enabled = true;
				}
				else
				{
					Debug.Log("No enemy in view to lock onto");
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
		float animLength = stateInfo.length * animator.speed;
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
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			shiftTime = Time.time;
			sprinting = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift))
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
			if (lockedIn)
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
			EnemyController enemy = other.gameObject.GetComponentInParent<EnemyController>();
			if (enemy.isAttacking && !enemy.hitPlayer)
			{
				enemy.hitPlayer = true;
				currentHealth -= enemy.damage;
				currentEnemy = enemy;
			}
		}
	}
}