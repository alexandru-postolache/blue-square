using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using UnityEngine.iOS;
using UnityEngine.SceneManagement;
using Assets.SimpleAndroidNotifications;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	const int MAX_CHARACTERS_IN_LEADERBOARD_ROW = 40;
	const int MINUTES_UNTIL_NEXT_LIFE = 30;

	GameObject player;
	GameObject spawner;
	GameObject playButton;
	static bool isRestart = false;
	bool isStarted = false;
	bool wasSaved = false;
	bool newHighscore = false;
	bool obtainLifeDuringGame = false;
	int coins = 0;
	int highscore = 0;
	static int lives = 10;
	static bool wasPromptedForInternetConnection = false;
	dreamloLeaderBoard leaderboard;
	RectTransform leaderboardWrapper;
	private static DateTime? energyRefillTime = null;
	static DateTime? timeWhenLeavedTheApp = null;
	public DateTime? firstTimeLeftTheApp = null;

	[HideInInspector]
	public bool hasInternetConnection = false;

	public bool soundPref = true;
	public bool isLeaderboardLoaded = false;
	public bool wasScoreAdded = false;
	public int score = 0;
	public bool wasRestarted = false;
	public Text coinsText;
	public Text countdownText;
	public Text scoreText;
	public Text highscoreText;
	public Text timeUntilNextLife;
	public Text newHighscoreText;
	public Text startWaveText;
	public Text livesText;
	public Text leaderboardResults;
	public GameObject rewardAdPrompt;
	public GameObject gameStartPanel;
	public GameObject gameOverPanel;
	public GameObject leaderboardPanel;
	public GameObject enableInternetPrompt;
	public ParticleSystem PlayerDestruction;
	public GameObject watchAddForLivesPrompt;
	public GameObject fullLivesPrompt;
	public GameObject adsCantPlayPromptAndGameOver;
	public GameObject adsCantPlayPromptAndRestartGame;
	public GameObject activateInternetToUseLeaderboardPrompt;
	public GameObject activateInternetToSharePrompt;
	public GameObject tutorialPrompt;

	public static bool veryFirstCallInApp =  true;
	public bool firstEntry;


	void Awake() {
		playButton = GameObject.FindWithTag ("Play").gameObject;
		leaderboard = GameObject.Find("dreamloPrefab").GetComponent<dreamloLeaderBoard>();
		leaderboardWrapper = leaderboardPanel.transform.FindChild ("Leaderboard/Viewport/Content").GetComponent<RectTransform> ();
		Load ();
		LoadSoundPref ();
		LoadFirstTimeLeftTheAppDatetime ();
		highscoreText.text = "Highscore: " + highscore;
		if (!soundPref) {
			gameStartPanel.transform.FindChild ("Sound/SoundOn").gameObject.SetActive (false);
			toogleSound (false);
		} else {
			gameStartPanel.transform.FindChild ("Sound/SoundOn").gameObject.SetActive (true);
			toogleSound (true);
		}
		if (veryFirstCallInApp) {
			CheckIfEnergyWaitTimeOverAfterApplicationQuit ();
		}
		veryFirstCallInApp = false;
		LoadFirstEntry ();
		if (firstEntry) {
			PlayTutorial ();
		}
		FirstEntryToFalse ();

		livesText.text = lives.ToString ();
		if (lives == 0) {
			playButton.GetComponent<Button> ().interactable = false;
			playButton.transform.position = new Vector3 (playButton.transform.position.x - 100, playButton.transform.position.y, playButton.transform.position.z);
			gameStartPanel.transform.Find ("AddLivesButton").gameObject.SetActive (true);
		}
	}

	void Start() {
		if (isRestart) {
			gameStartPanel.SetActive (false);
			score = 0;
			StartGame ();
		}
	}

	public void StartGame () {
		scoreText.gameObject.SetActive (true);
		player = GameObject.FindWithTag ("Player").gameObject;

		player.GetComponent<Movement> ().isStarted = true;

		spawner = GameObject.FindWithTag ("Spawner").gameObject;
		spawner.GetComponent<Spawner> ().isStarted = true;

		// check the internet connection 
		StartCoroutine (checkInternetConnectionWithWWW());

		leaderboard.LoadScores ();
		wasSaved = false;
		isStarted = true;
	}

	void Update () {
		if (isStarted) {
			if (player && player.activeSelf == false) {
				if (!wasSaved) {
					Save ();
					wasSaved = true;
				}
				if (!wasRestarted && hasInternetConnection && lives > 0 && obtainLifeDuringGame != true) {
					if (IsCloseToHighscore () == true) {
						if (highscore - score == 0) {
							rewardAdPrompt.transform.Find ("Title").GetComponent<Text>().text = "New highscore!";
						} else {
							rewardAdPrompt.transform.Find ("Title").GetComponent<Text>().text = "Very close to highscore!";
						}
						ShowAdQuestion ();
						wasRestarted = true;
					} else {
						GameOver ();
						wasRestarted = true;
					}
				} else if (!hasInternetConnection && !wasPromptedForInternetConnection && lives > 0 && obtainLifeDuringGame != true) {
					enableInternetPrompt.SetActive (true);
					wasPromptedForInternetConnection = true;
				} else if (!hasInternetConnection && wasPromptedForInternetConnection && lives > 0 && obtainLifeDuringGame != true) {
					GameOver ();
				}
			}
		} else {
			if (lives == 0) {
				playButton.GetComponent<Button> ().interactable = false;
				gameStartPanel.transform.Find ("AddLivesButton").gameObject.SetActive (true);
			} else {
				playButton.GetComponent<Button> ().interactable = true;
				gameStartPanel.transform.Find ("AddLivesButton").gameObject.SetActive (false);
			}
		}
		if (lives < 10) {
			if (!energyRefillTime.HasValue) {
				SetEnergyRefillTimer ();
			} else {
				if (DateTime.Now > energyRefillTime.Value) {
					if (player && player.activeSelf == false) {
						obtainLifeDuringGame = true;
					}
					AddALive();
				} else {
					ShowRemainingTimeUntilNextLife ();
				}
			}
		}
	}

	public void RestartGame() {
		isRestart = true;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void GoToHomepage() {
		isRestart = false;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void ContinueGame()
	{
		player.transform.position = new Vector3 (0f, -4f, -1f);
		player.SetActive (true);

		lives++;
		livesText.text = lives.ToString ();
		player.GetComponent<Movement> ().isStarted = false;
		spawner.GetComponent<Spawner> ().isStarted = false;
		StartCoroutine (StartCountdownAndPlay());
	}

	void Save()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

		PlayerData data = new PlayerData ();
		data.coins = coins;
		data.lives = lives;

		if (highscore < score) {
			data.highscore = score;
		}else {
			data.highscore = highscore;
		}
		if (timeWhenLeavedTheApp.HasValue) {
			data.timeWhenLeavedTheApp = timeWhenLeavedTheApp;
		}
		data.firstEntry = false;

		bf.Serialize (file, data);
		file.Close ();
	}

	void Load()
	{
		if (File.Exists (Application.persistentDataPath + "/playerInfo.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
			PlayerData data = (PlayerData)bf.Deserialize (file);
			file.Close ();

			coins = data.coins;
			highscore = data.highscore;
			lives = data.lives;
			firstEntry = data.firstEntry;
			if (data.timeWhenLeavedTheApp.HasValue) {
				timeWhenLeavedTheApp = data.timeWhenLeavedTheApp;
			}
		}
	}

	public void Die()
	{
		ParticleSystem destruction = Instantiate (PlayerDestruction);
		destruction.Play ();
		destruction.transform.position = player.transform.position;
		player.GetComponent<SoundManager> ().playDestroySound ();
		player.SetActive (false);

		if (lives > 0) {
			lives--;
			if (lives == 0) {
				watchAddForLivesPrompt.SetActive (true);
			}
		}
		livesText.text = lives.ToString ();
		if (wasRestarted) {
			GameOver ();
		}
	}

	public void GameOver()
	{
		// populate the data in game over panel
		// used transform.Find to find inactive objects
		gameOverPanel.transform.Find("ScoreText").GetComponent<Text> ().text = "Score: " + score;
		gameOverPanel.transform.Find("HighscoreText").GetComponent<Text> ().text = "Best: " + highscore;

		if (lives == 0 && hasInternetConnection) {
			gameOverPanel.transform.Find ("Retry").gameObject.SetActive (false);
			gameOverPanel.transform.Find ("OutOfLivesPanel").gameObject.SetActive (true);
		} else if(lives == 0 && !hasInternetConnection) {
			gameOverPanel.transform.Find ("Retry").gameObject.SetActive (false);
			gameOverPanel.transform.Find ("ActivateInternetPanel").gameObject.SetActive (true);
		}

		// display the game over panel
		gameOverPanel.SetActive(true);
	}

	private bool IsCloseToHighscore()
	{
		int differenceBetweenHighscoreAndScore = highscore - score;
		if (highscore > 10 && highscore - score <= 5) {
			return true;
		}
		return false;
	}

	public void GiveLives()
	{
		lives = 10;
		Save ();
		livesText.text = lives.ToString ();
		fullLivesPrompt.SetActive (true);
		wasRestarted = true;
	}

	private void PlayTutorial()
	{
		tutorialPrompt.gameObject.SetActive (true);
	}
		
	public void scoreChanged()
	{
		if (highscore < score && !newHighscore) {
			player.GetComponent<SoundManager> ().playHighscoreSound ();
			StartCoroutine (showHighscoreText ());
			newHighscore = true;
		} else if (newHighscore == true) {
			highscore = score;
		}
	}

	IEnumerator showHighscoreText()
	{
		newHighscoreText.gameObject.SetActive (true);
		yield return new WaitForSeconds (2);
		newHighscoreText.gameObject.SetActive (false);
	}

	public void ShowLeaderboard()
	{
		if (hasInternetConnection) {
			StartCoroutine (populateLeaderboard ());
			leaderboardPanel.SetActive (true);
		} else {
			activateInternetToUseLeaderboardPrompt.SetActive (true);
		}
	}

	IEnumerator populateLeaderboard()
	{
		dreamloLeaderBoard.Score[] scoresArray = leaderboard.ToScoreArray ();
		if (scoresArray.Length > 0) {
			int remainingDots;
			string dots = "";
			int length = scoresArray.Length;
			for (int i = 1; i <= length; i++) {
				dots = "";
				remainingDots = MAX_CHARACTERS_IN_LEADERBOARD_ROW - scoresArray [i-1].playerName.Length -  i.ToString().Length - 8;
				for (int j = 0; j < remainingDots; j++) {
					dots += ".";
				}
				leaderboardResults.text += i + ". " + scoresArray [i - 1].playerName + " " + dots + " " + scoresArray[i-1].score + "\n";
			}
		}
		yield return new WaitForFixedUpdate ();
		leaderboardWrapper.sizeDelta = new Vector2(leaderboardResults.rectTransform.rect.width, leaderboardResults.rectTransform.rect.height);
	}

	public void addScoreToLeaderboard()
	{
		InputField input = GameObject.Find ("NameInput").GetComponent<InputField> ();
		string name = input.text;
		if (name.Length != 0) {
			leaderboard.AddScore (name, score);
			input.text = "";
			StartCoroutine (waitAndLoadScore());
			StartCoroutine (waitAndPopulateLeaderboard());
		}
	}

	IEnumerator waitAndLoadScore()
	{
		wasScoreAdded = false;
		while (!wasScoreAdded) {
			yield return new WaitForSeconds (0.1f);
		}
		leaderboard.LoadScores ();
		wasScoreAdded = false;
	}

	IEnumerator waitAndPopulateLeaderboard()
	{
		leaderboardResults.text = "Loading ...";
		isLeaderboardLoaded = false;
		while (!isLeaderboardLoaded) {
			yield return new WaitForSeconds (0.1f);
		}
		leaderboardResults.text = "";
		StartCoroutine (populateLeaderboard ());
		isLeaderboardLoaded = false;
	}

	public IEnumerator ShowStartWaveText()
	{
		startWaveText.gameObject.SetActive (true);
		yield return new WaitForSeconds (1.5f);
		FadeOut (startWaveText.gameObject, 1f);
		
	}
		
	public void FadeOut(GameObject go, float duration)
	{
		StartCoroutine(FadeOutCR(go, duration));
	}

	private IEnumerator FadeOutCR(GameObject go, float duration)
	{
		float currentTime = 0f;
		Text text = go.GetComponent<Text> ();
		while(currentTime < duration)
		{
			float alpha = Mathf.Lerp(1f, 0f, currentTime/duration);
			text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
			currentTime += Time.deltaTime;
			yield return null;
		}
		go.SetActive (false);
		text.color = new Color(text.color.r, text.color.g, text.color.b, 1); // reset the alpha to 1
		yield break;
	}

	IEnumerator StartCountdownAndPlay()
	{
		countdownText.gameObject.SetActive (true);
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Shot");
		for (int i = 0; i < enemies.Length; i++){
			Destroy(enemies[i]);
		}
		for (int i = 3; i >= 1; i--) {
			countdownText.text = i.ToString();
			yield return new WaitForSeconds (1);
			if (i == 1)
				countdownText.gameObject.SetActive (false);
		}
		StartGame ();
	}

	public void addCoin()
	{
		coins++;
		coinsText.text = coins.ToString ();
		
	}

	public void addScore()
	{
		score++;
		scoreText.text = score.ToString ();
	}

	void ShowAdQuestion()
	{
		rewardAdPrompt.SetActive (true);
		StartCoroutine (SliderAnimation());
	}

	IEnumerator SliderAnimation()
	{
		for (int i = 100; i >= 0; i--) {
			rewardAdPrompt.GetComponentInChildren<Slider> ().value = (float)i ;
			yield return new WaitForSeconds (0.03f);
		}
		if (rewardAdPrompt.activeSelf == true) {
			rewardAdPrompt.SetActive (false);
			GameOver ();
		}
	}

	IEnumerator checkInternetConnectionWithWWW()
	{
		WWW www = new WWW("http://google.com");
		yield return www;
		if (www.error != null) {
			hasInternetConnection = false;
		} else {
			hasInternetConnection = true;
		}
	} 


	// ping the google server for 5 seconds
	// if the response is "positive" we have a working internet connection
	private IEnumerator CheckInternetConnection() {
		Ping pingMasterServer = new Ping("8.8.8.8");
		Debug.Log(pingMasterServer.ip);
		float startTime = Time.time;
		while (!pingMasterServer.isDone && Time.time < startTime + 5.0f) {
			yield return new WaitForSeconds(0.1f);
		}
		if(pingMasterServer.isDone) {
			hasInternetConnection = true;
		} else {
			hasInternetConnection = false;
		}
		Debug.Log (pingMasterServer.time);
	}

	public int GetHighscore()
	{
		return highscore;
	}

	private void SetEnergyRefillTimer() {
		if (firstTimeLeftTheApp.HasValue) {
			TimeSpan timeBetweenLeavingAndEntering = DateTime.Now - firstTimeLeftTheApp.Value;
			int minutes = (int)timeBetweenLeavingAndEntering.TotalMinutes;
			if (minutes > 0) {
				minutes = 30 - (int)(timeBetweenLeavingAndEntering.TotalMinutes % 30);
			} else {
				minutes = 29;
			}
			int seconds = 60 - (int)(timeBetweenLeavingAndEntering.TotalSeconds % 60);
			energyRefillTime = DateTime.Now.AddMinutes (minutes).AddSeconds (seconds);
		} else {
			energyRefillTime = DateTime.Now.AddMinutes (30);
			var countdownTimerText = "30:00";
		}
		// display countdownTimerText somewhere!
	}

	void OnApplicationPause(bool loseFocus) {
		if (loseFocus) {
			SetAllNotifications();
			return;
		}

		// gained focus!
		NotificationManager.CancelAll ();
		CheckIfEnergyWaitTimeOver();
	}

	void OnApplicationQuit()
	{
		SaveFirstTimeLeftTheAppDatetime (DateTime.Now.ToString());
		SetAllNotifications();
	}

	private void SetAllNotifications() {
		timeWhenLeavedTheApp = DateTime.Now;
		Save ();
		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// WE NEED TO INSTALL THE IOS BUILD SUPPORT

//			UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications ();
//			UnityEngine.iOS.NotificationServices.ClearLocalNotifications ();

//			if (energyRefillTime.HasValue) {
//				UnityEngine.iOS.LocalNotification notif = new UnityEngine.iOS.LocalNotification ();
//				notif.fireDate = energyRefillTime.Value;
//				notif.alertBody = "You have more energy!";
//				notif.alertAction = "The wait is over!";
//				notif.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
//				notif.applicationIconBadgeNumber = 1;
//				UnityEngine.iOS.NotificationServices.ScheduleLocalNotification (notif);
//			}
		} else if (Application.platform == RuntimePlatform.Android) {
			if (lives != 10) {
				int multiplier = 10 - lives;
				NotificationManager.SendWithAppIcon (TimeSpan.FromMinutes (multiplier * 30), "You have more lives!", "The wait is over!", new Color (0, 0.6f, 1), NotificationIcon.Message);
			}
		}
	}

	public bool CheckIfEnergyWaitTimeOver() {
		if (!energyRefillTime.HasValue || DateTime.Now < energyRefillTime.Value) {
			return false;
		} else {
			if (timeWhenLeavedTheApp.HasValue) {
				TimeSpan timeBetweenLeavingAndEntering = DateTime.Now - timeWhenLeavedTheApp.Value;
				int livesToAdd = (int)timeBetweenLeavingAndEntering.TotalMinutes / 30;
				int maxLivesToAdd = 10 - lives;
				livesToAdd = Math.Min (livesToAdd, maxLivesToAdd);
				AddALive(livesToAdd);
			}
		}
			
		return true;
	}

	public bool CheckIfEnergyWaitTimeOverAfterApplicationQuit() {
		if (timeWhenLeavedTheApp.HasValue) {
			TimeSpan timeBetweenLeavingAndEntering = DateTime.Now - timeWhenLeavedTheApp.Value;
			int livesToAdd = (int)timeBetweenLeavingAndEntering.TotalMinutes / 30;
			int maxLivesToAdd = 10 - lives;
			livesToAdd = Math.Min (livesToAdd, maxLivesToAdd);
			AddALive(livesToAdd);
			return true;
		}
		return false;
	}

	public void ShowRemainingTimeUntilNextLife() {
		TimeSpan remaining = energyRefillTime.Value - DateTime.Now;
		var timerCountdownText = string.Format("{0:D2}:{1:D2}", remaining.Minutes, remaining.Seconds);
		// use timerCountdownText and display it somewhere!
		watchAddForLivesPrompt.GetComponent<Transform> ().FindChild ("TimeLeft").GetComponent<Text> ().text = timerCountdownText;
		timeUntilNextLife.text = timerCountdownText;
	}

	public void AddALive(int livesToAdd = 1) {

		// WE NEED TO INSTALL THE IOS BUILD SUPPORT

//		UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications ();
//		UnityEngine.iOS.NotificationServices.ClearLocalNotifications ();

		NotificationManager.CancelAll ();

		// Add code to give player more energy!
		if (livesToAdd == 1) {
			lives++;
		} else {
			lives += livesToAdd;
		}
		Save ();
		livesText.text = lives.ToString ();

		energyRefillTime = null;
		Debug.Log ("VIATA DATA: " + livesToAdd);
	}

	public void FillMaxLives()
	{
		int livesToAdd = 10 - lives;
		AddALive (livesToAdd);
	}

	public void toogleSound(bool isSoundOn) {
		if (isSoundOn) {
			AudioListener.pause = false;
		} else {
			AudioListener.pause = true;
		}
		SaveSoundPref (isSoundOn);
	}

	void SaveSoundPref(bool isSoundOn) {
		PlayerPrefs.SetInt ("SoundPref", Convert.ToInt32(isSoundOn));
	}

	void LoadSoundPref() {
		if(PlayerPrefs.HasKey("SoundPref")) {
			soundPref = Convert.ToBoolean(PlayerPrefs.GetInt("SoundPref"));
		}
	}

	void FirstEntryToFalse() {
		PlayerPrefs.SetInt ("FirstEntry", Convert.ToInt32 (false));
	}

	void LoadFirstEntry() {
		if (PlayerPrefs.HasKey ("FirstEntry")) {
			firstEntry = Convert.ToBoolean (PlayerPrefs.GetInt ("FirstEntry"));
		} else {
			firstEntry = true;
		}
	}

	void SaveFirstTimeLeftTheAppDatetime(string datetime) {
		//PlayerPrefs.DeleteAll ();
		if (!PlayerPrefs.HasKey ("FirstTimeLeftTheApp")) {
			PlayerPrefs.SetString ("FirstTimeLeftTheApp", datetime);
		}
	}

	void ChangeFirstTimeLeftTheAppDatetime(string datetime) {
		PlayerPrefs.SetString ("FirstTimeLeftTheApp", datetime);
	}

	void LoadFirstTimeLeftTheAppDatetime() {
		if (PlayerPrefs.HasKey ("FirstTimeLeftTheApp")) {
			firstTimeLeftTheApp = Convert.ToDateTime (PlayerPrefs.GetString ("FirstTimeLeftTheApp"));
		}
	}

	public void ShowAdsCantPlayPromptAndGameOver() {
		adsCantPlayPromptAndGameOver.SetActive (true);
	}

	public void ShowAdsCantPlayPromptAndRestartGame() {
		adsCantPlayPromptAndRestartGame.SetActive (true);
	}

}

[Serializable]
class PlayerData 
{
	public int coins;
	public int highscore;
	public int lives;
	public bool firstEntry;
	public DateTime? timeWhenLeavedTheApp;
}
