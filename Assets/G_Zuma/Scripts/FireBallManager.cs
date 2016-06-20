using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZumaGame { 
public class FireBall {
	GameObject mGameObject;
	Transform mTransform;
	SpriteRenderer sprite;

	public BallType Type {
		get;
		private set;
	}

	public FireBall (GameObject g) {
		mGameObject = g;
		mTransform = g.transform;
		sprite = g.GetComponent <SpriteRenderer> ();
	}

	public void OnInit (BallType type, Vector3 right) {
		Type = type;
		mGameObject.SetActive (true);
		mTransform.localPosition = Vector3.zero;
		mTransform.right = right;
		sprite.sprite = GameBulletinBoard.MapConfig.GetSpriteFromType (type);
	}

	public void Tick (float dt) {
		mTransform.position += mTransform.right * dt * 10;
	}

	public bool IsCross (Vector3 p, float r) {
		Vector3 point = mTransform.position;
		point.z = p.z;
		return Vector3.Distance (point, p) <= r;
	}

	public bool IsOutOfBounds () {
		if (mTransform.position.x > 4 || mTransform.position.x < -4 ||
			mTransform.position.y > 6 || mTransform.position.y < -6)
			return true;
		return false;
	}

	public void OnDestroy () {
		mGameObject.SetActive (false);
	}
}


public class FireBallManager : MonoBehaviour {
	[SerializeField]
	[Header("mold of fire-balls")]
	GameObject ballMold;
	// 发射球的池
	SCObjectPool<FireBall> mPool;
	// 发射球的挂载点
	Transform FireBallNode;
	// 管理活动球的队列
	List <FireBall> mActiveBallQueue;

	void Awake() { GameBulletinBoard.SetFireBallManager(this); }

	// Use this for initialization
	void Start() {
		mActiveBallQueue = new List<FireBall> ();
		FireBallNode = transform;
		mPool = new SCObjectPool<FireBall> (OnNewInstanceFireBall, 3);
	}

	// Update is called once per frame
	void Update() {
		float dt = Time.deltaTime;
		int i = mActiveBallQueue.Count;
		while (i-- > 0) {
			FireBall fire = mActiveBallQueue[i];
			fire.Tick (dt);
			if (fire.IsOutOfBounds ()) {				
				mActiveBallQueue.RemoveAt (i);
				OnBomb (fire);		
			}
		}
	}

	FireBall OnNewInstanceFireBall () {
		GameObject g = Instantiate (ballMold) as GameObject;
		g.transform.parent = transform;
		return new FireBall (g);
	}

	// 发射一颗子弹
	public void OnFire (BallType type, Vector3 r) {
		FireBall fire = mPool.GetObject ();
		fire.OnInit (type, r);
		mActiveBallQueue.Add (fire);
	}

	// 子弹爆炸失效
	public void OnBomb (FireBall fire) {
		fire.OnDestroy ();
		mPool.SetObject (fire);	
	}

	public List<FireBall> GetActiveFireList () {
		return this.mActiveBallQueue;
	}
}
}