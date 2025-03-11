using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	[Header("Component References")]
	[SerializeField] Transform camera;
	public Animator animator;
	[SerializeField] Slider slider;

	[Header("Parameters")]
	[SerializeField] float speed = 10f;
	[SerializeField] float jumpHeight = 10f;
	public bool isGrounded = true;
	[SerializeField] float maxHealth = 20f;
	[SerializeField] float currentHealth;

	private float currentVelocity;
	private Rigidbody rigidBody;
	public bool isAttacking = false;
	private bool isCrouching = false;
	private bool isRunning = false;

	private void Start()
	{
		rigidBody = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		currentHealth = maxHealth;
	}

	private void Update()
	{
		slider.value = currentHealth / maxHealth;

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
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
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

		//Jumping
		if (Input.GetButtonDown("Jump") && isGrounded)
		{
			rigidBody.linearVelocity = Vector3.up * jumpHeight;
			isGrounded = false;
			animator.SetBool("Jumping", true);
		}

		//Running
		if (Input.GetKeyDown(KeyCode.LeftShift) && !isCrouching)
		{
			isRunning = true;
			animator.SetBool("Running", true);
			speed += 2;
		}
		else if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			isRunning = true;
			animator.SetBool("Running", false);
			speed -= 2;
		}

		//Crouching
		if (Input.GetKeyDown(KeyCode.LeftControl) && !isRunning)
		{
			isCrouching = true;
			speed -= 2;
		}
		else if (Input.GetKeyUp(KeyCode.LeftControl))
		{
			isCrouching = false;
			speed += 2;
		}
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