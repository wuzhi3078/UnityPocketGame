using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace PopStar {
public class GameUI : MonoBehaviour {

	public Text mgameScore;

	static GameUI _single = null;
	public static GameUI Single {
		get {
			return _single;
		}
	}

	GameData gData = null;

	void Awake () {
		_single = this;
		gData = new GameData ();
	}

	void OnSetGameAddScore (int x) {
		gData.gGameScore += x;
		mgameScore.text = string.Format("score: {0}", gData.gGameScore);
	}
	
	public static void SetGameAddScore (int x) {
		Single.OnSetGameAddScore (x);
	}
}


public class GameData {
	public int gGameScore = 0;
}
}