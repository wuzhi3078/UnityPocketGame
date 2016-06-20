using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace ZumaGame { 
[System.Serializable]
public class BallTypeSprite {
	public BallType Type;
	public Sprite Sprite;
}

public class MapConfig : ScriptableObject {
	[Header ("关卡地图的信息")]
	public Vector3[] MapInfo;

	public BallTypeSprite [] mBallTypeSprite;

	public Vector3 GetMapPointInfo (int x) {
		return MapInfo [x];
	}

	public Vector3 GetPoint (float p) {
		int x = Mathf.FloorToInt (p); 
		return Vector3.Lerp (MapInfo [x], MapInfo [x + 1], p - x);
	}

	public void InitRunningConfg () {
		Length = MapInfo.Length;
		FailurePoint = Length - 2 - 0.1f;

		mDicBallSprite = new Dictionary<BallType, Sprite>();
		for (int i = 0; i < mBallTypeSprite.Length; ++i) {
			mDicBallSprite.Add (mBallTypeSprite[i].Type, mBallTypeSprite [i].Sprite);
		}
	}
	
	public int Length {get ; private set;}
	public float FailurePoint { get ;private set;}

	Dictionary <BallType, Sprite> mDicBallSprite;
	public Sprite GetSpriteFromType (BallType t) {
		return mDicBallSprite [t];
	}
}
}