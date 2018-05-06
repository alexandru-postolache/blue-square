using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class Movement : MonoBehaviour {

	GameController gameController;
	bool toRight;
	GameObject engine;
	bool quitBool = false;
	Rigidbody2D rb;
	float turningInterpolater = 0.8f;
	public float rotateSpeed = 200f;
	public float speed = 6f;

	public bool isStarted = false;
	public SoundManager soundManager;

	void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody2D> ();
	}

	void Start()
	{
		gameController = GameObject.FindWithTag ("GameController").gameObject.GetComponent<GameController> ();
		soundManager = gameObject.GetComponent<SoundManager> ();
		toRight = RandomBoolean ();

	}

	// Update is called once per frame
	void Update () {
		if (isStarted) {
			StartGame ();
		}

		if(Input.touchCount > 1) quitBool = false;
		if (Input.GetKeyDown(KeyCode.Escape) && quitBool == true){
			Application.Quit();
		}
		if(Input.anyKey){
			if (Input.GetKey(KeyCode.Escape)) quitBool = true;
			else quitBool = false;
		}
	}

	void StartGame()
	{
//		bool wasSpacePressed = Input.GetButtonDown ("Jump");
		bool wasSpacePressed = false;
		for(int i = 0; i < Input.touchCount; ++i) {
			wasSpacePressed = Input.GetTouch(i).phase == TouchPhase.Began;
			break;
		}

		if (wasSpacePressed) {
			gameController.addScore ();
			gameController.scoreChanged ();
			turningInterpolater = 0.8f;
			soundManager.playSwipeSound ();
			if (toRight) {
				toRight = false;
				//material.SetColor ("_TopColor", Color.black);
				//material.SetColor ("_BottomColor", Color.white);
			} else {
				toRight = true;
			}
		}

		if (toRight) {
			rb.velocity = new Vector2(speed * Mathf.Lerp(0.7f, 1f, turningInterpolater), 0);
			rb.transform.Rotate (new Vector3 (0f, 0f, rotateSpeed * Time.deltaTime));
		} else {
			rb.velocity = new Vector2(-speed * Mathf.Lerp(0.7f, 1f, turningInterpolater), 0);
			rb.transform.Rotate (new Vector3 (0f, 0f, -rotateSpeed * Time.deltaTime));
		}

		turningInterpolater -= 0.1f;
	}

	void OnTriggerEnter2D(Collider2D coll) {
		switch (coll.tag) 
		{
		case "Boundary":
			break;
		case "Coin":
			gameController.addCoin ();
			coll.gameObject.GetComponent<Sound>().playSound();
			coll.gameObject.SetActive (false);
			break;
		case "Shot":
		case "LateralBound":
			gameController.Die ();
			break;
		}
	}

	bool RandomBoolean ()
	{
		if (Random.value >= 0.5)
		{
			return true;
		}
		return false;
	}

}
