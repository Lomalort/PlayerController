using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float longIdleTime = 5f;
	public float speed = 2.5f;
	public float jumpForce = 2.5f;

	public Transform groundCheck;
	public LayerMask groundLayer;
	public float groundCheckRadius;
	public GameObject dustEffect;
	private bool _salidaPolvo;
	public ParticleSystem polvo;

	// References
	private Rigidbody2D _rigidbody;
	public Animator _animator;
	public static PlayerController instance;

	// Long Idle
	private float _longIdleTimer;

	// Movement
	private Vector2 movement;
	private bool _facingRight = true;
	private bool _isGrounded;
	private bool _crouch;
	private bool _run;
	

	// Attack
	private bool _isAttacking;
	private bool _jump;
	private bool _dobleJump;
	private bool _jumpAttack;
	//private SpriteRenderer _renderer;
	public GameObject espadaBrillo;

	//Coyote Time
	float tiempoEnElAire;

	//Input Buffer
	//Queue<KeyCode> inputBuffer;


	

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator>();
		instance = this;
		//_renderer = GetComponent<SpriteRenderer>();
		
	}

	void Start()
    {
		polvo = GetComponentInChildren<ParticleSystem>();
		_animator = GetComponent<Animator>();
		espadaBrillo = GetComponentInChildren<GameObject>();	
	}

    void Update()
    {		
		if (_isAttacking == false) 
		{
            // Movement
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            movement = new Vector2(horizontalInput, 0f);			



			// Flip character
			if (horizontalInput < 0f && _facingRight == true) 
			{				
				Flip();
			} 
			else if (horizontalInput > 0f && _facingRight == false) 
			{
				Flip();
			}
		}



        // Is Grounded?
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        //RaycastHit2D raycastSuelo = Physics2D.Raycast(transform.position, Vector2.down, 0.25f, groundLayer);

        //Largando Polvo

        if (_isGrounded == true)
        {
			tiempoEnElAire = 0;

			if (_salidaPolvo == true)
            {
				polvo.Play();
				_salidaPolvo = false;
            }

        }
        else
        {
			tiempoEnElAire += Time.deltaTime;
			_salidaPolvo = true;
        }


        // Is Jumping?
        if (Input.GetButtonDown("Jump") && _isAttacking == false) 
		{
            if (_isGrounded || tiempoEnElAire < 0.25f)
            {
				
				CrearPolvo();

				_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

				_isAttacking = false;
				_dobleJump = true;
				_jumpAttack = true;
				
			}
           

			else if (_dobleJump)
            {
				_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

				_dobleJump = false;
				_jumpAttack = true;

				StartCoroutine("VisualFeedback");			
                				
			}			

		}

		// Wanna Run?
		if (Input.GetButtonDown("Fire2") && _isGrounded == true && _isAttacking == false && _run == false)
		{
			movement = Vector2.zero;
			_rigidbody.velocity = Vector2.zero;
			_animator.SetTrigger("ToRun");
			speed = 7f;
			_run = true;
			polvo.Play();
		}
		if (Input.GetButtonUp("Fire2") && _isGrounded == true && _isAttacking == false && _run == true)
		{
			movement = Vector2.zero;
			_rigidbody.velocity = Vector2.zero;
			_animator.SetTrigger("ToWalk");
			speed = 4f;
			_run = false;
		}
		// Wanna Attack?
		if (Input.GetButtonDown("Fire1") && _isGrounded == true && _isAttacking == false) 
		{
			
			movement = Vector2.zero;
			_rigidbody.velocity = Vector2.zero;
			_animator.SetTrigger("0");
            _isAttacking = true;

        }

		if (Input.GetButtonDown("Fire1") && _isGrounded == false && _jumpAttack == true && _isAttacking == false)
		{
			
			movement = Vector2.zero;
			_rigidbody.velocity = Vector2.zero;
			_animator.SetTrigger("JumpAttack");
			_jumpAttack = false;
			_isAttacking = true;

		}

		// Agacharce
		if (Input.GetButtonDown("Crouch") && _isGrounded == true)
        {
			_crouch = true;			
			_animator.SetBool("Crouch", true);
			
		}
			
		else if (Input.GetButtonUp("Crouch"))
        {
			_crouch = false;			
			_animator.SetBool("Crouch", false);
						
		}

		// Atacar Agachado
		if (_crouch == true && Input.GetButtonDown("Fire1"))
        {
			
			_isAttacking = true;			
			_animator.SetTrigger("CrouchAttack");

        }
			
		
	}

	void FixedUpdate()
	{
		if (_isAttacking == false) 
		{
            float horizontalVelocity = movement.normalized.x * speed;
            _rigidbody.velocity = new Vector2(horizontalVelocity, _rigidbody.velocity.y);
			
		}
		if (_isAttacking == true)
		{
			movement = Vector2.zero;
			_rigidbody.velocity = Vector2.zero;
			
		}
		if (_crouch == true)
        {
			movement = Vector2.zero;
			_rigidbody.velocity = Vector2.zero;
		}
	}
	

	void LateUpdate()
	{
		_animator.SetBool("Idle", movement == Vector2.zero);
		_animator.SetBool("IsGrounded", _isGrounded);
		_animator.SetFloat("VerticalVelocity", _rigidbody.velocity.y);

		// Animator
		
		if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
			_isAttacking = true;
		} else {
			_isAttacking = false;
		}
		

		// Long Idle
		if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle")) {
			_longIdleTimer += Time.deltaTime;

			if (_longIdleTimer >= longIdleTime) {
				_animator.SetTrigger("LongIdle");
			}
		} else {
			_longIdleTimer = 0f;
		}
	}

	private void Flip()
	{
		if (_isGrounded == true)
		{
			CrearPolvo();
		}
		_facingRight = !_facingRight;
		float localScaleX = transform.localScale.x;
		localScaleX = localScaleX * -1f;
		transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

	}
	private IEnumerator VisualFeedback()
	{
		espadaBrillo.SetActive(true);
		
		//_renderer.color = Color.blue;

		yield return new WaitForSeconds(0.5f);

		espadaBrillo.SetActive(false);

		//_renderer.color = Color.white;
	}
	private void CrearPolvo()
    {
        
		polvo.Play();
		
	}

}

