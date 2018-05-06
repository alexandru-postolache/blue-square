using UnityEngine;
using System.Collections;

public class BackgroundLoop : MonoBehaviour {

	public float speed = 0.01f;
	Renderer renderer;
	GameObject player;

	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer> ();
		renderer.sortingLayerName = "Background";
		renderer.sortingOrder = 10;
		player = GameObject.FindWithTag ("Player").gameObject;

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (player.activeSelf) {
			GetComponent<Renderer> ().material.color = Color.Lerp (Color.blue, Color.green, Mathf.PingPong (Time.time, 1));
		}
	}
}
