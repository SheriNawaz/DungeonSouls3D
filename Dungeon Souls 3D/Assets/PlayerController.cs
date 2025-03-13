using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
	[Header("Component References")]
	[SerializeField] Transform camera;
	public Animator animator;
	[SerializeField] Slider healthSlider;
	[SerializeField] Slider staminaSlider;
	[SerializeField] CapsuleCollider normalCollider;
	[SerializeField] CapsuleCollider crouchCollider;
	[SerializeField] TMP_Text healthText;

	[Header("Parameters")]
	[SerializeField] float walkSpeed = 5f;
	[SerializeField] float runSpeed = 7f;
	[SerializeField] float crouchSpeed = 3.5f;
	[SerializeField] float jumpHeight = 10f;
	public bool isGrounded = true;
	[SerializeField] float maxHealth = 20f;
	[SerializeField] float currentHealth;
	[SerializeField] float maxStamina = 100f;
	[SerializeField] float staminaReduction = 0.1f;
	[SerializeField] float staminaRegen = 0.1f;

	private float currentVelocity;
	private Rigidbody rigidBody;
	public bool isAttacking = false;
	private bool isCrouching = false;
	private bool isRunning = false;
	private float speed;
	private float currentStamina;

	private void Start()
	{
		speed = walkSpeed;
		rigidBody = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		currentHealth = maxHealth;
		currentStamina = maxStamina;
	}

	private void Update()
	{
		healthSlider.value = currentHealth / maxHealth;
		healthText.text = currentHealth + " / " + maxHealth;

		staminaSlider.value = currentStamina / maxStamina;

		if(isGrounded && !isRunning && currentStamina <= 100 && currentStamina >= 0)
		{
			currentStamina += staminaRegen;
		}
		else if (currentStamina <= 0)
		{
			animator.SetBool("Running", false);
			isRunning = false;
			StartCoroutine(RegenStam());
		}

		if (isRunning)
		{
			currentStamina -= staminaReduction;
		}



		HandleMovement();
		HandleCombat();
		HandleDeath();
	}

	private void HandleDeath()
	{
		if(currentHealth <= 0)
		{
			Debug.Log("DEAD");
		}
	}

	private void HandleCombat()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0) && currentStamina >= 10)
		{
			currentStamina -= 20f;
			isAttacking = true;
			animator.SetTrigger("Attacking");
			StartCoroutine("Attacking");
		}
	}

	IEnumerator Attacking()
	{
		yield return new WaitForSeconds(1.4f);
		isAttacking = false;
	}

	private void HandleMovement()
	{
		//Moving
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");

		float movementMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
		animator.SetFloat("Speed", movementMagnitude);

		Vector3 dir = new Vector3(horizontalInput, 0f, verticalInput).normalized;

		if (dir.magnitude >= 0.1f)
		{
			float target = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
			if (movementMagnitude > 0.1f)
			{
				float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target, ref currentVelocity, 0.1f);
				transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
			}


			Vector3 moveDir = Quaternion.Euler(0f, target, 0f) * Vector3.forward;
			transform.position += moveDir.normalized * Time.deltaTime * speed;
		}

		if (currentStamina > 0)
		{
			//Jumping
			if (Input.GetButtonDown("Jump") && isGrounded)
			{
				rigidBody.linearVelocity = Vector3.up * jumpHeight;
				isGrounded = false;
				animator.SetBool("Jumping", true);
				currentStamina -= 20f;
			}

			//Running
			if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && currentStamina >= 5)
			{
				StopCoroutine(RegenStam());
				isRunning = true;
				animator.SetBool("Running", true);
				speed = runSpeed;
			}
			else if (Input.GetKeyUp(KeyCode.LeftShift))
			{
				isRunning = false;
				animator.SetBool("Running", false);
				speed = walkSpeed;
			}
		}

		//Crouching
		if (Input.GetKeyDown(KeyCode.LeftControl) && !isRunning && isGrounded)
		{
			crouchCollider.enabled = true;
			normalCollider.enabled = false;
			isCrouching = true;
			animator.SetBool("Crouching", true);
			speed = crouchSpeed;
		}
		else if (Input.GetKeyUp(KeyCode.LeftControl) || isAttacking)
		{
			crouchCollider.enabled = false;
			normalCollider.enabled = true;
			isCrouching = false;
			animator.SetBool("Crouching", false);
			speed = walkSpeed;
		}
	}

	public IEnumerator RegenStam()
	{
		yield return new WaitForSeconds(5f);
		currentStamina += staminaRegen;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Enemy")
		{
			EnemyController enemy = other.gameObject.GetComponentInParent<EnemyController>();
			if (enemy.isAttacking)
			{
				currentHealth -= enemy.damage;
				Debug.Log("Hit");
				enemy.isAttacking = false;
			}
		}
	}
}