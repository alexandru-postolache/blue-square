using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

	public GameObject bolt;
	public GameObject enemy;
	public GameObject coin;
	public float timeBetweenShots = 1.9f;
	public float timeBetweenCoinSpawn = 10f;
	public float coinSpeed = 3f;
	public Vector2 speedRange = new Vector2(2, 3);
	public float velocity = 5f;

	public Vector2 minXAndMaxX = new Vector2 (0, 0);
	public bool isStarted = false;

	float shotTimestamp;
	float coinTimestamp;
	float gapDistance = 4f; // 3.5f before
	float gapDistanceForWave = 4f;

	int shootsFired = 0;
	int scoreWhenWaveEnded = 0;

	public bool waveStarted = false;

	GameObject player;
	GameController gameController;
	Queue<Vector3> lastPositions = new Queue<Vector3> ();

	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag ("Player").gameObject;
		gameController = GameObject.FindWithTag ("GameController").gameObject.GetComponent<GameController> ();
		lastPositions.Enqueue (new Vector3 (0, 0, 0));
	}

	void Update(){
		if (isStarted) {
			if (isWaveReady ()) {
				SpawnWave ();
			} else {
				SpawnObstacle ();
			}
			//SpawnCoin ();
		}
	}
		
	void Shot () {
		if (Time.time >= shotTimestamp && player.activeSelf) {
			bool goodPosition = false;
			Vector3 position = Vector3.forward;
			while (!goodPosition) {
				position = new Vector3 (Random.Range (minXAndMaxX.x, minXAndMaxX.y), transform.position.y, 0f);
				goodPosition = true;
				foreach (Vector3 lastPosition in lastPositions) {
					if (Mathf.Abs (position.x - lastPosition.x) < 1) {
						goodPosition = false;
					}
				}

				if (goodPosition) {
					lastPositions.Enqueue (position);
				}

				if (lastPositions.Count > 3) {
					lastPositions.Dequeue ();
				}
			}
			GameObject clone = Instantiate (bolt, position, new Quaternion ()) as GameObject;

			Rigidbody2D rb = clone.GetComponent<Rigidbody2D> ();
			rb.velocity = new Vector2(0f, -Random.Range(speedRange.x, speedRange.y));

			shotTimestamp = Time.time + timeBetweenShots;
			shootsFired++;

			if (shootsFired % 10 == 0 && timeBetweenShots >= 0.75f) {
				timeBetweenShots -= 0.05f;
				Debug.Log (timeBetweenShots);
			}
		}
	}

	void SpawnObstacle()
	{
		if(!waveStarted) {
			if (Time.time >= shotTimestamp && player.activeSelf) {

				//float velocity = -Random.Range (speedRange.x, speedRange.y);
				if (this.velocity < 5.9f) {
					this.velocity += 0.1f;	
				}

				if (this.timeBetweenShots > 1.2) {
					this.timeBetweenShots -= 0.05f;
				}
				if (this.gapDistance > 3.6f) {
					this.gapDistance -= 0.03f;
				}
				float velocity = -this.velocity;

				Vector3 leftSegmentPosition = new Vector3 (-4f, transform.position.y, 1f);
				GameObject leftSegment = Instantiate (enemy, leftSegmentPosition, enemy.transform.localRotation) as GameObject;
				float rightMargin = Random.Range (0.5f, 4f);
				leftSegment.transform.localScale = new Vector3 (2f, rightMargin);
				leftSegment.transform.position = new Vector3 (-(4.8f - (rightMargin * 0.8f)), transform.position.y, -1f);

				Rigidbody2D leftSegmentRb = leftSegment.GetComponent<Rigidbody2D> ();
				leftSegmentRb.velocity = new Vector2 (0f, velocity);


				Vector3 rightSegmentPosition = new Vector3 (4.8f, transform.position.y, 1f);
				GameObject rightSegment = Instantiate (enemy, rightSegmentPosition, enemy.transform.localRotation) as GameObject;
				float freeSpace = rightMargin + gapDistance;
				float rightSegmentDimension = 8f - freeSpace;
				rightSegment.transform.localScale = new Vector3 (2f, rightSegmentDimension);
				rightSegment.transform.position = new Vector3 (4.8f - (rightSegmentDimension * 0.8f), transform.position.y, -1f);

				Rigidbody2D rightSegmentRb = rightSegment.GetComponent<Rigidbody2D> ();
				rightSegmentRb.velocity = new Vector2 (0f, velocity);

				shotTimestamp = Time.time + timeBetweenShots;
			}
		}
	}

	void SpawnCoin()
	{
		if (Time.time >= coinTimestamp && player.activeSelf) {
			Vector3 position = new Vector3 (Random.Range (minXAndMaxX.x, minXAndMaxX.y), transform.position.y, 0f);
			GameObject clone = Instantiate (coin, position, Quaternion.identity) as GameObject;
			Rigidbody2D rb = clone.GetComponent<Rigidbody2D> ();
			rb.velocity = new Vector2(0f, -coinSpeed);
			coinTimestamp = Time.time + timeBetweenCoinSpawn;
		}
	}

	void SpawnWave() {
		if (!waveStarted) {
			waveStarted = true;
			StartCoroutine(gameController.ShowStartWaveText ());
			StartCoroutine (PlayWave ());
		}
	}

	IEnumerator PlayWave()
	{
		yield return new WaitForSeconds(1f);
		float leftNumber, rightNumber;
		if (Random.value > 0.5f) {
			leftNumber = 1f;
			rightNumber = 2.8f;
		} else {
			leftNumber = 2f;
			rightNumber = 3.8f;
		}

		if (this.gapDistanceForWave > 3.6f) {
			this.gapDistanceForWave -= 0.05f;
		}

		for(int i = 0; i < 10; i++) {
			if (isStarted) {
				float velocity = -4f;

				Vector3 leftSegmentPosition = new Vector3 (-4f, transform.position.y, 0f);
				GameObject leftSegment = Instantiate (enemy, leftSegmentPosition, enemy.transform.localRotation) as GameObject;

				float rightMargin = Random.Range (leftNumber, rightNumber);
				leftSegment.transform.localScale = new Vector3 (2f, rightMargin); // the left segment dimension
				leftSegment.transform.position = new Vector3 (-(4.8f - (rightMargin * 0.8f)), transform.position.y);

				Rigidbody2D leftSegmentRb = leftSegment.GetComponent<Rigidbody2D> ();
				leftSegmentRb.velocity = new Vector2 (0f, velocity);


				Vector3 rightSegmentPosition = new Vector3 (4.8f, transform.position.y, 0f);
				GameObject rightSegment = Instantiate (enemy, rightSegmentPosition, enemy.transform.localRotation) as GameObject;
				float freeSpace = rightMargin + gapDistanceForWave;
				float rightSegmentDimension = 8f - freeSpace; // right segment dimension
				rightSegment.transform.localScale = new Vector3 (2f, rightSegmentDimension);
				rightSegment.transform.position = new Vector3 (4.8f - (rightSegmentDimension * 0.8f), transform.position.y);

				Rigidbody2D rightSegmentRb = rightSegment.GetComponent<Rigidbody2D> ();
				rightSegmentRb.velocity = new Vector2 (0f, velocity);

	
				yield return new WaitForSeconds (0.8f);
			}
		}
		yield return new WaitForSeconds (2f);
		scoreWhenWaveEnded = gameController.score;
		waveStarted = false;

	}

	bool isWaveReady()
	{
		if (gameController.score != 0 && gameController.score - scoreWhenWaveEnded >= 30) {
			return true;
		}
		return false;
	}
}
