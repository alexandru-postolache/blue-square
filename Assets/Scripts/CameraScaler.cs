using UnityEngine;
using System.Collections;

public class CameraScaler : MonoBehaviour {

	GameObject leftBound, rightBound;
	float goodValueForCamera = -0.029f;

	void Awake() {
		leftBound = GameObject.FindWithTag ("LeftBound").gameObject;
		rightBound = GameObject.FindWithTag ("RightBound").gameObject;
	}

	// Use this for initialization
	void Start () {
		Camera camera = gameObject.GetComponent<Camera> ();
		Vector3 leftBoundPosition = camera.WorldToViewportPoint (leftBound.transform.position);
	
		bool positive = true;
		float value = Mathf.Round(leftBoundPosition.x * 1000f) / 1000f;
	
		if (value < goodValueForCamera) {
			positive = false;
		} else if (value > goodValueForCamera) {
			positive = true;
		} else {
			return;
		}

		while (value != goodValueForCamera) {
			if (positive) {
				camera.orthographicSize -= 0.015f;
			} else {
				camera.orthographicSize += 0.015f;
			}
			leftBoundPosition = camera.WorldToViewportPoint (leftBound.transform.position);
			value = Mathf.Round(leftBoundPosition.x * 1000f) / 1000f;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
