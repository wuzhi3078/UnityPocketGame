using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PopStar {
public interface IGamePoolObject {
	void Used ();
	void UnUsed();
}

public class GamePool : MonoBehaviour {
	// 外部设置的变量
	[Header ("精灵集合")]
	public Sprite[] mStarSprites;

	[Header ("消除特效")]
	public GameObject mEliminateEffect;
	
	Dictionary <string, Sprite> mDictionarySprite;
	GameManager mGameManagerScript;

	Transform meliminateEffectRoot = null;
	Transform mgamestarRoot = null;

	public SCObjectPool <NPCStar> NPCStarPool;
	public SCObjectPool <GameObject> EliminateEffect;

	public static GamePool Instance { get; set;}

	// Use this for initialization
	void Awake () {	
		Instance = this;

		InitDictionarySprite ();

		mGameManagerScript = GameObject.Find ("GameManager").GetComponent <GameManager>();
		mgamestarRoot = GameObject.Find ("GameStar").transform;
		meliminateEffectRoot = GameObject.Find ("GameFx").transform;

		InitPool ();
	}

	void InitDictionarySprite () {
		if (mStarSprites == null || mStarSprites.Length == 0)
			throw new UnityException ("please set star sprite");

		mDictionarySprite = new Dictionary<string, Sprite>();

		int i = mStarSprites.Length;
		while (i-- > 0) {
			Sprite e = mStarSprites [i];
			
			if (Debug.isDebugBuild) {
				if (mDictionarySprite.ContainsKey (e.name))
					Debug.LogError ("有重名的精灵: " + e.name);
			}

			mDictionarySprite.Add(e.name, e);
		}
	}

	void InitPool () {
		int line = mGameManagerScript.MapLine;
		int cell = mGameManagerScript.MapCell;
	
		NPCStarPool = new SCObjectPool<NPCStar> (InstanceNpcStar, line * cell);
		EliminateEffect = new SCObjectPool<GameObject> (InstanceEliminateEffect, 15);
	}

	NPCStar InstanceNpcStar () {
		float pixelUtil = mGameManagerScript.PixelPreUtil;

		GameObject g = new GameObject("star");
		g.transform.parent = mgamestarRoot;

		NPCStar star = new NPCStar(g, pixelUtil);
		star.UnUsed();

		return star;
	} 
	

	GameObject InstanceEliminateEffect () {
		GameObject g = Instantiate (mEliminateEffect) as GameObject;
		g.transform.parent = meliminateEffectRoot;
		g.transform.localPosition = Vector3.zero;
		return g;
	}

	/// <summary>
	/// 经过类型装饰
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public NPCStar GetNPCStarFromType (StarType type) {
		NPCStar e = NPCStarPool.GetObject ();
		e.SpriteRenderer.sprite = GetSpriteMapping (GameConfig.Instance.GetComnmonStarMapping (type.ToString()));
		e.Type = type;

		e.Used ();
		return e;
	} 

	public Sprite GetSpriteMapping (string spriteName) {
		return mDictionarySprite [spriteName];
	}
}
}