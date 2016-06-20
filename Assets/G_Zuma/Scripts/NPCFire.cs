using UnityEngine;
using System.Collections;
namespace ZumaGame { 
public class NPCFire : MonoBehaviour {

	bool misTouchState = false;
	Transform mDirectionNode;
	Vector3 touchPosition;
	
	SpriteRenderer fireBall;
	BallType newBallType;

	// Use this for initialization
	void Start () {
		mDirectionNode = transform.Find ("arrow");
		mDirectionNode.gameObject.SetActive (false);
		fireBall = transform.Find ("bullet").GetComponent <SpriteRenderer> ();

		fireBall.gameObject.SetActive (false);
		RefreshNewBall();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0) && fireBall.gameObject.activeSelf) {
			misTouchState = true;
			touchPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			mDirectionNode.gameObject.SetActive (true);

			GaveDirections ();
		}

		if (Input.GetMouseButton (0) && misTouchState) {
			touchPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);

			GaveDirections ();
		}

		if (Input.GetMouseButtonUp (0) && misTouchState) {
			touchPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			misTouchState = false;
			mDirectionNode.gameObject.SetActive (false);

			OnFire ();
		}
	}

	// 指示方向
	void GaveDirections () {
		Vector3 right = touchPosition - transform.position;
		transform.right = new Vector3 (right.x, right.y, 0);
	}

	// 反射一个球
	void OnFire () {
		SCSound.monkey_fashe();
		GameBulletinBoard.FireBallManager.OnFire (newBallType, transform.right);

		fireBall.gameObject.SetActive (false);
		ScheduleOnce.CreateActive (this, 0.1f, RefreshNewBall);		
	}

	// 刷新一个新球
	void RefreshNewBall () {
		if (!fireBall.gameObject.activeSelf) {
			newBallType = GameBulletinBoard.GameStrategy();
			fireBall.sprite = GameBulletinBoard.MapConfig.GetSpriteFromType (newBallType);
			fireBall.gameObject.SetActive (true);
		}
	}
}
}