using UnityEngine;
using System.Collections;
using LitJson;
namespace PopStar {
public class GameConfig : MonoBehaviour {

	static GameConfig _instance = null;
	public static GameConfig Instance {
		get {
			if (_instance == null) {
				GameObject g = GameObject.Find("GameEnvironment/GameConfig");
				_instance = g.GetComponent <GameConfig>();
				_instance.InitConfigTable ();

			}

			return _instance;
		}
	}


	// 配置表
	[Header ("游戏配置文件")]
	public TextAsset mGameConfigJsonFile;
	
	void InitConfigTable () {
		jsonRoot = JsonMapper.ToObject (mGameConfigJsonFile.text);
	}

	// 根对象
	JsonData jsonRoot;
	// 普通星星的精灵映射
	JsonData jsonCommonStar;

	/// <summary>
	/// 获取普通星星的渲染精灵
	/// </summary>
	public string GetComnmonStarMapping (string mCommonTypesStar) {
		if (jsonCommonStar == null)
			jsonCommonStar = jsonRoot ["CommonTypesStar"];

		return jsonCommonStar [mCommonTypesStar].ToString();
	}
}
}