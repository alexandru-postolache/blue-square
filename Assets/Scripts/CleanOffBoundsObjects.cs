using UnityEngine;
using System.Collections;

public class CleanOffBoundsObjects : MonoBehaviour {

	void OnTriggerExit2D(Collider2D collider) {
		if (!collider.CompareTag("Player")) {
			Destroy (collider.gameObject);
		}
	}
}
