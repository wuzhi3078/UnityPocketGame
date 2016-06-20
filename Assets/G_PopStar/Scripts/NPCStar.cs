using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace PopStar {
/// <summary>
/// 游戏星星
/// </summary>
public class NPCStar : IMapContent, IGamePoolObject {
	
	public NPCStar (GameObject g, float boxSize) {
		transform = g.transform;
		PixelUtil = boxSize;

		SpriteRenderer = g.AddComponent<SpriteRenderer>();
		Box2d = g.AddComponent<BoxCollider2D>();
		Box2d.isTrigger = true;
		Box2d.size = Vector2.one * boxSize;
	}

	public StarType Type;	
	public MazeMapPoint Point ;
	public BoxCollider2D Box2d { get; private set;} 
	public SpriteRenderer SpriteRenderer{ get; private set;} 
	public Transform transform { get; private set;} 
	public float PixelUtil;

	bool misMoving = false;

	// 能否触发消除
	//		正在移动不可点击
	public bool CanTriggerMatch () {
		return (!this.misMoving);
	}

	// 类型相同才被搜索
	public bool IsSameAsType (NPCStar p) {
		return p.Type == this.Type;
	}

	// 能通过搜索
	public bool IsPassTestSearch () {
		return !this.misMoving;
	}

	public void InitUsed (MazeMapPoint point) {
		transform.localPosition = new Vector3 (PixelUtil * point.y, PixelUtil * point.x, 0);
		Point = point;
	}

	public void OnDestroy () {
		//GameObject.Destroy (transform.gameObject);
		UnUsed ();
		GamePool.Instance.NPCStarPool.SetObject (this);
	}

	public void Used() {
		transform.gameObject.SetActive (true);
	}

	public void UnUsed() {
		transform.gameObject.SetActive (false);
	}

	public void BeforeOnMove () {
		misMoving = true;
	}

	public void AfterOnMove () {
		misMoving = false;
	}
}
}