using UnityEngine;
using System.Collections;
using UnityEditor;
namespace ZumaGame { 
public class LevelLayoutEditor {

	[MenuItem ("Tools/Zuma/LevelConfig2Json", false, 1001)]
	public static void GameLevelLayout () {
		string assetPath = @"Assets/Resources/map.asset";

		if (Selection.activeGameObject == null) 
			throw new UnityException ("must select Map");

		Transform map = Selection.activeGameObject.transform;

		MapConfig info = AssetDatabase.LoadAssetAtPath (assetPath, typeof (MapConfig)) as MapConfig;
		bool isExist = (info != null); 

		if (info == null) {
			Debug.Log ("AssetDatabase not exist MapConfig");
			info = ScriptableObject.CreateInstance <MapConfig>() as MapConfig;			
		}

		info.MapInfo = new Vector3 [map.childCount];
		for (int i =0, L = map.childCount; i < L; ++i) {
			Transform t = map.GetChild (i);
			info.MapInfo [i] = t.localPosition;
		}
		
		if ( !isExist ) {
			AssetDatabase.CreateAsset (info, assetPath);
			return;
		}

		// 这里很重要，如果没有告诉unity已经被改变。它只写入内存，没有写入磁盘
		EditorUtility.SetDirty (info);
		AssetDatabase.SaveAssets();
	}

	[MenuItem ("Tools/Zuma/", false, 1002)]
	public static void GameLevelLayoutBreak () {
	}
}
}