using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public AudioClip destroySound;
	public AudioClip swipeSound;
	public AudioClip highscoreSound;


	void Awake()
	{

	}

	public void playDestroySound()
	{
		AudioSource.PlayClipAtPoint(destroySound, new Vector3(0, 0, 0));
	}

	public void playSwipeSound()
	{
		AudioSource.PlayClipAtPoint(swipeSound, new Vector3(0, 0, 0), 0.4f);
	}

	public void playHighscoreSound()
	{
		AudioSource.PlayClipAtPoint(highscoreSound, new Vector3(0, 0, 0), 0.4f);
	}
}
