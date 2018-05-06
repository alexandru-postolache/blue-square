using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class AdManager : MonoBehaviour {

	[SerializeField] string gameID = "1203400";
	public GameController gameController;

	void Awake () {
		Advertisement.Initialize (gameID, true);
	}

	public void ShowAdAndContinue(string zone = "")
	{
		if(string.Equals(zone, ""))
			zone = null;

		ShowOptions options = new ShowOptions();
		options.resultCallback = AdCallbackToContinue;

		if (Advertisement.IsReady (zone)) {
			Advertisement.Show (zone, options);
		} else {
			gameController.ShowAdsCantPlayPromptAndGameOver ();
		}
	}

	void AdCallbackToContinue(ShowResult result)
	{
		switch (result) {
		case ShowResult.Finished:
			gameController.ContinueGame ();
			break;
		case ShowResult.Skipped:
			gameController.ContinueGame ();
			break;
		case ShowResult.Failed:
			gameController.GoToHomepage ();
			break;
		}
	}

	public void ShowAdAndGiveLifes(string zone = "")
	{
		if(string.Equals(zone, ""))
			zone = null;

		ShowOptions options = new ShowOptions();
		options.resultCallback = AdCallbackToGiveLifes;

		if (Advertisement.IsReady (zone)) {
			Advertisement.Show (zone, options);
		} else {
			gameController.ShowAdsCantPlayPromptAndRestartGame ();
		}
	}

	void AdCallbackToGiveLifes(ShowResult result)
	{
		switch (result) {
		case ShowResult.Finished:
			gameController.GiveLives ();
			break;
		case ShowResult.Skipped:
			gameController.GoToHomepage ();
			break;
		case ShowResult.Failed:
			gameController.GoToHomepage ();
			break;
		}
	}

	IEnumerator ShowAdWhenReady()
	{
		while (!Advertisement.IsReady ())
			yield return null;
		Advertisement.Show ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
