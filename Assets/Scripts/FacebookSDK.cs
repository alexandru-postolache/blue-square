using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Include Facebook namespace
using Facebook.Unity;
using System;

public class FacebookSDK : MonoBehaviour {

	public GameController gameController;

	// Awake function from Unity's MonoBehavior
	void Awake ()
	{
		if (!FB.IsInitialized) {
			// Initialize the Facebook SDK
			FB.Init(InitCallback, OnHideUnity);
		} else {
			// Already initialized, signal an app activation App Event
			FB.ActivateApp();
		}
	}

	private void InitCallback ()
	{
		if (FB.IsInitialized) {
			// Signal an app activation App Event
			FB.ActivateApp();
			// Continue with Facebook SDK
			// ...
		} else {
			Debug.Log("Failed to Initialize the Facebook SDK");
		}
	}

	public void FacebookLogin()
	{
		var perms = new List<string>(){"public_profile", "email", "user_friends"};
		FB.LogInWithReadPermissions(perms, AuthCallback);
	}

	private void AuthCallback (ILoginResult result) 
	{
		if (FB.IsLoggedIn) {
			UnityAndroidExtras.instance.makeToast("You have succesfully logged in!", 1);
		} else {
			UnityAndroidExtras.instance.makeToast("Sorry! Login failed!", 1);
		}
	}

	private void OnHideUnity (bool isGameShown)
	{
		if (!isGameShown) {
			// Pause the game - we will need to hide
			Time.timeScale = 0;
		} else {
			// Resume the game - we're getting focus again
			Time.timeScale = 1;
		}
	}
		
	public void InviteFriend() 
	{
		FB.Mobile.AppInvite(
			new Uri("https://fb.me/810530068992919"),
			new Uri("https://scontent.fotp3-2.fna.fbcdn.net/v/t1.0-9/18194826_300970133672017_173493546457593982_n.jpg?oh=445714aa4f9bc0bb86ace15c52f55ae0&oe=5982F1CE"),
			AppInviteCallback
		);
	}

	private void AppInviteCallback(IResult result)
	{
		if (result.Error == null && result.Cancelled == false) {
			UnityAndroidExtras.instance.makeToast ("Thank you! Your lives are now replenished!", 1);
			gameController.FillMaxLives ();
		} else if(result.Error != null) {
			UnityAndroidExtras.instance.makeToast ("Sorry! An error has occurred!", 1);
		}
	}

	public void ShareScore() 
	{
		if (gameController.hasInternetConnection) {
			var highscore = gameController.GetHighscore ();
			FB.ShareLink(
				contentURL: new Uri ("https://www.facebook.com/blue.square.game/"),
				contentTitle: "My highscore",
				contentDescription: "I just made " + highscore + " points in Blue Square. Can you beat my highscore ?",
				photoURL: new Uri ("https://scontent.fotp3-2.fna.fbcdn.net/v/t1.0-9/18194826_300970133672017_173493546457593982_n.jpg?oh=445714aa4f9bc0bb86ace15c52f55ae0&oe=5982F1CE"),
				callback: FeedCallback
			);
		} else {
			gameController.activateInternetToSharePrompt.SetActive (true);
		}
	}

	private void FeedCallback(IShareResult result)
	{
		if (result.Error == null && result.Cancelled == false) {
			UnityAndroidExtras.instance.makeToast ("Thank you! Your lives are now replenished!", 1);
			gameController.FillMaxLives ();
		} else if(result.Error != null) {
			UnityAndroidExtras.instance.makeToast ("Sorry! An error has occurred!", 1);
		}
	}
		
}
