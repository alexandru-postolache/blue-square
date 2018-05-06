using UnityEngine;
using System.Collections;

public class RepeatLateralBounds : MonoBehaviour {

	private Transform player;
	private GameObject leftBound;
	private GameObject rightBound;
	private GameObject background;
	private float repeatBoundsEvery = 10f;
	private float distanceBetweenBoundSpawn = 17f;
	private int numberOfBoundsSpawns = 1;

	private float repeatBackgroundEvery = 20f;
	private float distanceBetweenBackgroundSpawn = 47f;
	private int numberOfBackgroundSpawns = 1;

	void Awake() {
		player = GameObject.FindWithTag ("Player").transform;
		leftBound = GameObject.FindWithTag ("LeftBound");
		rightBound = GameObject.FindWithTag ("RightBound");
		background = GameObject.FindWithTag ("Background");
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (player.position.y > (repeatBoundsEvery * numberOfBoundsSpawns)) {
			Instantiate (leftBound, new Vector3(leftBound.transform.position.x, leftBound.transform.position.y + (distanceBetweenBoundSpawn * numberOfBoundsSpawns), 0f), leftBound.transform.rotation);
			Instantiate (rightBound, new Vector3(rightBound.transform.position.x, rightBound.transform.position.y + (distanceBetweenBoundSpawn * numberOfBoundsSpawns), 0f), rightBound.transform.rotation);
			numberOfBoundsSpawns++;
		}
		if (player.position.y > (repeatBackgroundEvery * numberOfBackgroundSpawns)) {
			Instantiate (background, new Vector3 (background.transform.position.x, background.transform.position.y + (distanceBetweenBackgroundSpawn * numberOfBackgroundSpawns), 0f), background.transform.rotation);
			numberOfBackgroundSpawns++;
		}
	}
}
