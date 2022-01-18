using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

	Rigidbody2D rb;
	Vector2 movimiento;

	public float velocidad;

	public ParticleSystem orbeContacto;
	public ParticleSystem badOrbeContact;
	public ParticleSystem velocidadActivada;
	public ParticleSystem orbeMalo;
	public ParticleSystem particleGameOver;
	public ParticleSystem perderOrbes;
	public ParticleSystem SlowMoAct;


	public Transform luzPlayer;

	public AudioSource audioS;

	public float pitchValue = 1f;

	bool juegoATerminado = false;

	//Texto UI
	[SerializeField]
	Text coinCounter;
	//HiScore Ui
	[SerializeField]
	Text orbesMaximos;
	[SerializeField]
	Text orbesSumadosText;

	[SerializeField]
	GameObject coinMagnet;

	//Almacenar los coins
	int coinsNumber;
	//Almacenar los orbes obtenidos
	int orbesObtenidos;
	int orbesSumados;

	//Detener el player cuando toca un BadOrbe
	bool _aturdido;

	//Detener el player cuando muere
	bool _gameOver;
	
	private Animator anim;

	public AudioSource orbeSound;
	
    
    // Variable para demorar el reinicio del nivel
    public float restartDelay = 5f;

	private int valorAleatorio;

	private float? _tiempoInicioContadorMonedas;

	private float _tiempoMaxMonedas = 1f;

	public Image imagenOrbe;

	//Sensibilidad en las flechas del Joystick
	public float sensitivity = 3f;
	
	float smoothx;
	float smoothy;

	public bool extraDesbloqueado_1, extraDesbloqueado_2, extraDesbloqueado_3, extraDesbloqueado_4, extraDesbloqueado_5, extraDesbloqueado_6, extraDesbloqueado_7, extraDesbloqueado_8;



	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		orbesObtenidos = PlayerPrefs.GetInt("orbesObtenidos");

		// Create a temporary reference to the current scene.
		Scene currentScene = SceneManager.GetActiveScene();

		// Retrieve the name of this scene.
		string sceneName = currentScene.name;

		if (sceneName == "Nivel_1")
		{
			orbesObtenidos = 0;
		}
		
	}

	float SmoothX()
	{
		float target = Input.GetAxis("Horizontal");
		smoothx = Mathf.MoveTowards(smoothx,
				   target, sensitivity * Time.deltaTime);		
		return smoothx;
	}
	float SmoothY()
	{
		float target = Input.GetAxis("Vertical");
		smoothy = Mathf.MoveTowards(smoothy,
				   target, sensitivity * Time.deltaTime);		
		return smoothy;
	}

	// Update is called once per frame
	void Update () 
	{	
		
		movimiento.x = SmoothX();
		movimiento.y = SmoothY();

		//Animacion del movimiento
		anim.SetFloat("Horizontal", movimiento.x);
		anim.SetFloat("Vertical", movimiento.y);

		//Transformar la variable numerica a texto
		coinCounter.text = coinsNumber.ToString();
		orbesMaximos.text = orbesObtenidos.ToString();
		orbesSumadosText.text = orbesSumados.ToString();

        if (coinsNumber == 0)
        {
			coinCounter.color = Color.grey;
			imagenOrbe.color = Color.gray;
		}
		
			

		coinMagnet.transform.position = new Vector2 (transform.position.x, transform.position.y);		
		
	}
    private void FixedUpdate()
    {
		rb.velocity = movimiento * velocidad;

		//Frenar al player
		if (_aturdido)
        {
			rb.velocity = Vector2.zero;

            _aturdido = true;

        }

		//Frenar al player
        if (_gameOver)
        {
			rb.velocity = Vector2.zero;

			_gameOver = true;
		}
        

	}
    void LateUpdate()
    {
		//Frenar el movimiento del player el tiempo que la animacion dure
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Aturdido"))
        {
            _aturdido = true;
        }
        else
        {
            _aturdido = false;
        }

		//Frenar el movimiento del player al morir segun el tiempo que la animacion dure
		if (anim.GetCurrentAnimatorStateInfo(0).IsTag("GameOver"))
		{
			_gameOver = true;
		}
		else
		{
			_gameOver = false;
		}
	}



    void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.tag.Equals ("Coin")) 
		{
			if (_tiempoInicioContadorMonedas is null)
			{
				_tiempoInicioContadorMonedas = Time.time;
			}		

			//Instanciar sonido de Orbe
			orbeSound.Play();

			//FindObjectOfType<AudioManager>().Play("Orbe");

			//Particulas cuando tocas el Orbe
			LargarParticulas();			

			//Destruir Objeto que tocas
			Destroy (col.gameObject);
			
			//Sumar Orbes
			coinsNumber += 1;
			coinCounter.color = Color.white;
			imagenOrbe.color = Color.white;

			if (Time.time - _tiempoInicioContadorMonedas < _tiempoMaxMonedas)
            {
				//Subir el pitch
				orbeSound.pitch = (pitchValue += 0.1f);
            }
            else
            {
                //Colocar el pitch en 1
                orbeSound.pitch = (pitchValue = 1);

                //...y colocar el tiempo Inicio contador en nulo
                _tiempoInicioContadorMonedas = null;
            }
        }


        if (col.gameObject.tag.Equals("BadOrbe"))
		{
			if(coinsNumber == 0)
            {
				anim.SetTrigger("GameOver");
				GameOverPar();
				Electricidad();
				FindObjectOfType<AudioManager>().Play("BadOrbeDeath");				
			}
            //Instanciar sonido de BadOrbe
            //Instantiate(badOrbeSound);
            FindObjectOfType<AudioManager>().Play("BadOrbe");


            //Instanciar particulas de BadOrbe
            Electricidad();

			//Frenar movimiento del player
			_aturdido = true;

			//Activar animacion Aturdido
			anim.SetTrigger("Aturdido");

			//Play particulas
			badOrbeContact.Play();

			PerderOrbesBuenos();

			//StartCoroutine("VisualFeedback");

			//Destruir el objeto que colicionas
			Destroy(col.gameObject);

			//Restar orbes
			coinsNumber -= 10;

			//Condicion para que los orbes no pasen de 0
            if (coinsNumber < 0)
            {
				coinsNumber = 0;				
			}

		}


		
		//Condicion para Matar al Player
		if (col.gameObject.tag.Equals("Fluid"))
        {
			//Animacion para frenar el personaje
			anim.SetTrigger("GameOver");

			FindObjectOfType<AudioManager>().Play("PlayerDeath");

			FindObjectOfType<AudioManager>().Stop("BadOrbeDeath");

			audioS.Stop();

			//Fin del juego
			EndGame();
        }
		if (col.gameObject.tag.Equals("PowerUp"))
		{
			valorAleatorio = Random.Range(0,2);
			if(valorAleatorio == 0)
            {
				AumentoVelocidad();
				StartCoroutine(aumentarVelocidad());				
				Destroy(col.gameObject);
			}
			else if(valorAleatorio == 1)
            {
				AumentoSlowMo();
				StartCoroutine(disminuirVelocidad());
				Destroy(col.gameObject);
			}
						
		}
		if (col.gameObject.tag.Equals("SumadorOrbes"))
        {
			orbesSumados = coinsNumber + orbesObtenidos;
			PlayerPrefs.SetInt("orbesObtenidos", orbesSumados);
			Scene currentScene = SceneManager.GetActiveScene();

			// Retrieve the name of this scene.
			string sceneName = currentScene.name;

			if (sceneName == "Nivel_1")
            {
				if (coinsNumber > 20)
				{
					extraDesbloqueado_1 = true;
					PlayerPrefs.SetInt("Extralevel1", extraDesbloqueado_1 ? 1 : 0);
				}
			}
			else if (sceneName == "Nivel_2")
			{
				if (coinsNumber > 20)
				{
					extraDesbloqueado_2 = true;
					PlayerPrefs.SetInt("Extralevel2", extraDesbloqueado_2 ? 1 : 0);
				}
			}
			else if (sceneName == "Nivel_3")
			{
				if (coinsNumber > 20)
				{
					extraDesbloqueado_3 = true;
					PlayerPrefs.SetInt("Extralevel3", extraDesbloqueado_3 ? 1 : 0);
				}
			}
			else if (sceneName == "Nivel_4")
			{
				if (coinsNumber > 20)
				{
					extraDesbloqueado_4 = true;
					PlayerPrefs.SetInt("Extralevel4", extraDesbloqueado_4 ? 1 : 0);
				}
			}
			else if (sceneName == "Nivel_5")
			{
				if (coinsNumber > 20)
				{
					extraDesbloqueado_5 = true;
					PlayerPrefs.SetInt("Extralevel5", extraDesbloqueado_5 ? 1 : 0);
				}
			}
			else if (sceneName == "Nivel_6")
			{
				if (coinsNumber > 20)
				{
					extraDesbloqueado_6 = true;
					PlayerPrefs.SetInt("Extralevel6", extraDesbloqueado_6 ? 1 : 0);
				}
			}
			else if (sceneName == "Nivel_7")
			{
				if (coinsNumber > 20)
				{
					extraDesbloqueado_7 = true;
					PlayerPrefs.SetInt("Extralevel7", extraDesbloqueado_7 ? 1 : 0);

                    if (orbesObtenidos > 1000)
                    {
						extraDesbloqueado_8 = true;
						PlayerPrefs.SetInt("ExtralevelSpiriat", extraDesbloqueado_8 ? 1 : 0);
					}
				}
			}

		}


	}

	void LargarParticulas()
	{
		orbeContacto.Play();
	}
	void AumentoVelocidad()
    {
		velocidadActivada.Play();

	}
	void Electricidad()
    {
		orbeMalo.Play();
    }
	void GameOverPar()
    {
		particleGameOver.Play();
    }
	void PerderOrbesBuenos()
    {
		perderOrbes.Play();
    }
	void AumentoSlowMo()
    {
		SlowMoAct.Play();
    }

	public void EndGame()
    {
		if(juegoATerminado == false)
        {
			juegoATerminado = true;
			Debug.Log("GAME OVER");
			Invoke("Restart", restartDelay);			
        }
    }
	void Restart()
    {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

	private IEnumerator aumentarVelocidad()
    {		
		velocidad = 30f;
		audioS.pitch = (pitchValue = 1.05f);
		yield return new WaitForSeconds(9f);
		velocidad = 15f;
		audioS.pitch = (pitchValue = 1f);
	}
	
	private IEnumerator disminuirVelocidad()
    {
		audioS.pitch = (pitchValue = 0.95f);
		velocidad = 3f;
		yield return new WaitForSeconds(9f);
		velocidad = 15f;
		audioS.pitch = (pitchValue = 1f);
	}

	
	
}
