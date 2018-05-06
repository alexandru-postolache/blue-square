using UnityEngine;
using System.Collections;

public class Sound : MonoBehaviour {

	public AudioClip audio;

	public void playSound()
	{
		AudioSource.PlayClipAtPoint(audio, new Vector3(0, 0, 0), 0.6f);
	}
}
