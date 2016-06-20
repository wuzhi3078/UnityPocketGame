using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZumaGame {
public class Ball {

	GameObject mBall;
	SpriteRenderer mRender;
	BallType mType;
	Transform transform;

	public BallType Type { get { return mType;} }

	public Ball (GameObject g) {
		mBall = g;
		transform = g.transform;
		InitInstance ();
	}

	public Ball OnInit (BallType t) {
		mType = t;
		mRender.sprite = GameBulletinBoard.MapConfig.GetSpriteFromType (t);
		mBall.SetActive (true);

		Pre = null;
		Next = null;
		IsNextDelete = false;

		return this;
	}

	public void Tick (float dt) {
		transform.localEulerAngles += Vector3.forward * dt * 200;
	}

	public void UpdateBall () {
		transform.localPosition = GameBulletinBoard.MapConfig.GetPoint (processIndex);
	}

	public void OnDestroy () {
		mBall.SetActive (false);
		GameBulletinBoard.GameManager.BallPool.SetObject (this);
	}

	public bool IsDestroy () {
		return !mBall.activeSelf;
	}
	
	// 路径的进度
	public float processIndex { get ; set;}	

	// 设置回退趋势球
	public Ball BallTowards;
	
	void InitInstance () {
		mRender = mBall.GetComponent <SpriteRenderer> ();
	}

	public Vector3 getPosition () {
		return transform.position;
	}

	// ------------------------------------------
	// 查找前面与后面串连起来的同Type
	// ------------------------------------------

	// 查找队列
	public int TheSameColorQueue(out Ball _pre, out Ball _next) {
		Ball ptr;
		int Count = 1;

		_pre = this;
		_next = this;

		ptr = this;
		do {
			if (ptr.Pre != null && ptr.Pre.Type == Type) {
				_pre = ptr.Pre;
				Count++;
				ptr = ptr.Pre;
			}
			else
				break;
		} while (ptr != null);

		ptr = this;
		do {
			if (ptr.Next != null && ptr.Next.Type == Type) {
				_next = ptr.Next;
				Count++;
				ptr = ptr.Next;
			}
			else
				break;
		} while (ptr != null);

		return Count;
	}

	// 标志删除
	public bool IsNextDelete = false;
	
	/// <summary>
	/// 珠子的串联
	/// </summary>

	public Ball Pre { get ; private set;}
	public Ball Next { get; private set;}

	public void SetNext (Ball t) { Next = t;}
	public void SetPre (Ball t) { Pre = t;}

	public Ball Head {
		get {
			Ball ptr = this;
			do {
				if (ptr.Pre == null)
					return ptr;

				ptr = ptr.Pre;
			} while (true);
		}
	}

	public Ball Tail {
		get {
			Ball ptr = this;
			do {
				if (ptr.Next == null) return ptr;
				ptr = ptr.Next;
			} while (true);
		}
	}
}
}
