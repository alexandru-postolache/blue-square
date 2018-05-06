using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BackgroundChanger : MonoBehaviour {

	private int numberOfColors = 9;

	private int numberOfColoursInArray = 8;

	List<Color> colors = new List<Color>();
	Color[][] colorsArray = new Color[10][];

	// Use this for initialization
	void Start () {
		createColorsList ();
		changeBackground ();
	}

	private void createColorsList()
	{
		colors.Add(ConvertColor(193, 228, 155));
		colors.Add(ConvertColor(186, 55, 125));
		colors.Add(ConvertColor(254, 254, 254));
		colors.Add(ConvertColor(191, 36, 37));
		colors.Add(ConvertColor(239, 202, 53));
		colors.Add(ConvertColor(239, 193, 123));
		colors.Add(ConvertColor(75, 18, 72));
		colors.Add(ConvertColor(250, 210, 232));
		colors.Add(ConvertColor(186, 55, 125));
		colors.Add (Color.magenta);

		// the new way for adding colours
		//TODO: convert the above format to this format
		colorsArray [0] = new Color[2];
		colorsArray [0] [0] = ConvertColor (182, 178, 217);
		colorsArray [0] [1] = ConvertColor (192, 173, 33);

		colorsArray [1] = new Color[2];
		colorsArray [1] [0] = ConvertColor (193, 228, 155);
		colorsArray [1] [1] = ConvertColor (202, 202, 170);

		colorsArray [2] = new Color[2];
		colorsArray [2] [0] = ConvertColor (78, 195, 203);
		colorsArray [2] [1] = ConvertColor (255, 232, 130);

		colorsArray [3] = new Color[2];
		colorsArray [3] [0] = ConvertColor (87, 170, 64);
		colorsArray [3] [1] = ConvertColor (194, 225, 247);

		colorsArray [4] = new Color[2];
		colorsArray [4] [0] = ConvertColor (255, 229, 47);
		colorsArray [4] [1] = ConvertColor (153, 139, 124);

		colorsArray [5] = new Color[2];
		colorsArray [5] [0] = ConvertColor (124, 124, 122);
		colorsArray [5] [1] = ConvertColor (238, 235, 228);

		colorsArray [6] = new Color[2];
		colorsArray [6] [0] = ConvertColor (180, 210, 186);
		colorsArray [6] [1] = ConvertColor (167, 197, 92);

		colorsArray [7] = new Color[2];
		colorsArray [7] [0] = ConvertColor (129, 83, 131);
		colorsArray [7] [1] = ConvertColor (186, 212, 236);
	}

	void changeBackground()
	{
		InvokeRepeating ("FadeOut", 0.0f, 3f);
	}

	private void FadeOut()
	{
		StartCoroutine(FadeOutCR());
	}

	private IEnumerator FadeOutCR()
	{
		float duration = 3f;
		GameObject firstBackgroundGo = this.gameObject.transform.GetChild (0).gameObject;

		int colorIndex = Random.Range (0, numberOfColoursInArray);

		float currentTime = 1f;
		MeshRenderer renderer = firstBackgroundGo.GetComponent<MeshRenderer> ();
		Color prevTopColor = renderer.material.GetColor ("_TopColor");
		Color prevBottomColor = renderer.material.GetColor ("_BottomColor");

		while(currentTime < duration)
		{
			float alpha = Mathf.Lerp(1f, 0f, currentTime/duration);
			renderer.material.SetColor("_TopColor", Color.Lerp (colorsArray[colorIndex][1], prevTopColor, alpha));
			renderer.material.SetColor("_BottomColor", Color.Lerp (colorsArray[colorIndex][0], prevBottomColor, alpha));
			currentTime += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	Color ConvertColor (int r, int g, int b) {
		return new Color(r/255f, g/255f, b/255f);
	}
}
