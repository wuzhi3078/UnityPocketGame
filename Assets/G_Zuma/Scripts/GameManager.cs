using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZumaGame;
namespace ZumaGame {

public enum BallType {
	red = 0,
	blue = 1,
	yellow = 2,
	green = 3
}


public enum GameState {
	beginEnvirment = 0,
	runningTime = 1
}


public class GameManager : MonoBehaviour {
	[SerializeField]
	MapConfig mMapConfig;
	[SerializeField]
	[Header("祖玛球的模子")]
	GameObject mBallMold;

	// 祖玛球的池
	public SCObjectPool<Ball> BallPool { get; private set; }
	// 祖玛球的挂载点
	Transform MapBallMountNode;
	// 有几段珠子
	List<Ball> mSnakeSegment;
	// 游戏状态
	GameState mgameState = GameState.beginEnvirment;
	// 1 第一个珠子推进
	// -1最后的珠子回缩
	// 0 停滞
	int mAttackState = 1;
	// 默认的速度
	float mAttackSpeed = 2f;
	// 消除球
	List<Ball> EliminateQueue = new List<Ball>();
	// 回退球
	List<Ball> FallbackQueue = new List<Ball>();


	void Awake() {
		// 适配屏幕处理
		if (Camera.main.aspect > (2 / 3f)) {
			Camera.main.orthographicSize = 4.8f * (2 / 3f) / Camera.main.aspect;
		}

		mMapConfig.InitRunningConfg();
		GameBulletinBoard.SetGameManager(this);
		GameBulletinBoard.SetMapConfig(mMapConfig);

		MapBallMountNode = transform.Find("Map");
		mSnakeSegment = new List<Ball>();
	}

	// Use this for initialization
	IEnumerator Start() {
		// 初始背景音效
		PlayBakcgroundMusic();

		// 初始对象池
		InitObjectPool();

		// 测试mapconfig是否有数据
		if (Debug.isDebugBuild) Debug.Log(mMapConfig.MapInfo.Length);

		yield return new WaitForSeconds(0.2f);
		mgameState = GameState.runningTime;

		SCSound.monkey_yundong();
		mAttackSpeed = 20;
		yield return new WaitForSeconds(1);
		mAttackSpeed = 2;
	}

	// Update is called once per frame
	void Update() {
		if (mgameState == GameState.runningTime) {
			// 消除球	
			CheckEliminate();
			// 检测球的碰撞插入
			CheckFireBallCross();
			// 有回缩趋势的球的检测
			CheckFallBackBall ();
			// 第一个球的推进
			if (mAttackState == 1) {
				CheckFirstSegmentAttack();
				// 检测失败点
				CheckGameFailurePoint();
			}
			// 当达到终点时队列回缩
			if (mAttackState == -1) {
				CheckLastSegmentAttack();
			}			
			// 判断队列的连接	
			CheckSnakeConnect();			
		}

		// 更新球的Tick
		UpdateAllBallTick ();
		// 更新球的渲染
		UpdateBallRender();
	}

	void PlayBakcgroundMusic() {
		AudioClip clip = SCSound.Single.getSoundClip(SCSound.monkey_background_music);
		AudioSource sound = gameObject.AddComponent<AudioSource>();
		sound.clip = clip;
		sound.loop = true;
		sound.Play();
	}

	void InitObjectPool() {
		BallPool = new SCObjectPool<Ball>(NewInstanceBall, 100);
	}

	Ball NewInstanceBall() {
		GameObject g = Instantiate(mBallMold) as GameObject;
		g.transform.parent = MapBallMountNode;
		Ball ball = new Ball(g);
		return ball;
	}

	Ball CreateActiveBall() {
		Ball ball = BallPool.GetObject().OnInit(GameStrategy());
		return ball;
	}

	Ball CreateActiveBall(BallType t) {
		Ball ball = BallPool.GetObject().OnInit(t);
		return ball;
	}

	// 第一段推进
	void CheckFirstSegmentAttack() {

		//游戏中一段珠子都没有，先初始一段
		if (mSnakeSegment.Count == 0) {
			Ball b = CreateActiveBall();
			mSnakeSegment.Add(b);
			return;
		}

		//如果第一段中第一个珠子已经出了出口，补充新的珠子在洞口
		Ball ptr = mSnakeSegment[0];
		if (ptr.IsNotHoleExit()) {
			Ball b = CreateActiveBall();
			b.processIndex = ptr.processIndex - 1f;
			b.SetNext(ptr);
			ptr.SetPre(b);

			mSnakeSegment[0] = b;
			ptr = b;
		}

		//在出洞口第一颗珠子推动
		ptr.processIndex += Time.deltaTime * mAttackSpeed;

		//所有的右边的珠子都要调整
		while (ptr.Next != null) {
			if (ptr.Next.processIndex < ptr.processIndex + 1) {
				ptr.Next.processIndex = ptr.processIndex + 1;
			}

			ptr = ptr.Next;
		}
	}

	void CheckLastSegmentAttack() {
		int count = mSnakeSegment.Count;

		if (count == 0)
			return;

		Ball ptr = mSnakeSegment[count - 1].Tail;
		Ball kill = null;

		ptr.processIndex -= Time.deltaTime * mAttackSpeed;
		while (ptr.Pre != null) {
			if (ptr.Pre.processIndex > ptr.processIndex - 1) {
				ptr.Pre.processIndex = ptr.processIndex - 1;
			}
			ptr = ptr.Pre;

			// 退出出洞口
			if (ptr.processIndex < 0) {
				kill = ptr;
				break;
			}
		}

		if (kill != null) {
			ptr = kill;
			mSnakeSegment[count - 1] = ptr.Next;
			ptr.Next.SetPre(null);

			do {
				kill = ptr;
				ptr.OnDestroy();
				ptr = ptr.Pre;
			} while (ptr != null);
		}
	}

	void CheckGameFailurePoint() {
		int count = mSnakeSegment.Count;

		if (count == 0)
			return;		

		Ball ptr = mSnakeSegment[count - 1];
		if (ptr.Tail.IsArriveFailurePoint()) {
			SCSound.monkey_shibai();

			mAttackSpeed = 20;
			mAttackState = -1;
			ScheduleOnce.CreateActive(this, 2, () => {
				mAttackSpeed = 2;
				mAttackState = 1;
			});
		}
	}

	void CheckFireBallCross() {
		List<FireBall> list = GameBulletinBoard.FireBallManager.GetActiveFireList();
		float _distance = 0.47f * 0.5f;

		int i = list.Count;
		while (i-- > 0) {
			FireBall fire = list[i];

			bool isHit = false;
			int j = mSnakeSegment.Count;
			while (j-- > 0) {
				Ball ball = mSnakeSegment[j];

				do {
					if (fire.IsCross(ball.getPosition(), _distance)) {
						Ball insert = CreateActiveBall(fire.Type);
						Ball next = ball.Next;
						ball.SetNext(insert);
						insert.SetPre(ball);
						insert.SetNext(next);
						if (next != null) next.SetPre(insert);
						insert.processIndex = ball.processIndex + 1;

						EliminateQueue.Add(insert);

						list.RemoveAt(i);
						GameBulletinBoard.FireBallManager.OnBomb(fire);
						isHit = true;
						break;
					}
					ball = ball.Next;
				} while (ball != null);

				if (isHit) {
					UpdateBallAndBallDistance(mSnakeSegment[j]);
					break;
				}
			}
		}
	}

	void CheckEliminate() {
		bool misEliminate = false;

		int i = EliminateQueue.Count;
		while (i-- > 0) {
			Ball ball = EliminateQueue[i];
			Ball _pre, _next;
			if (ball.TheSameColorQueue(out _pre, out _next) >= 3) {
				misEliminate = true;
				_next.IsNextDelete = true;
				do {
					_pre.IsNextDelete = true;
					_pre = _pre.Next;
				} while (_pre != _next);

				SCSound.monkey_xiaochu1();
			} else {
				SCSound.monkey_qiu();
			}
		}
		EliminateQueue.Clear();

		if (!misEliminate)
			return;

		int x = mSnakeSegment.Count;
		while (x-- > 0) {
			Ball pointer = mSnakeSegment[x];
			Ball head = pointer.Head;
			Ball tail = pointer.Tail;
			bool isExistDelete = false;
			do {
				if (pointer.IsNextDelete) {
					isExistDelete = true;
					if (pointer.Pre != null) pointer.Pre.SetNext(null);
					if (pointer.Next != null) pointer.Next.SetPre(null);
					if (pointer == head) head = null;
					if (pointer == tail) tail = null;

					GameBulletinBoard.Fx.PlayEffectAtPoint (pointer.getPosition());
					pointer.OnDestroy();					
				}
				pointer = pointer.Next;
			} while (pointer != null);

			if (!isExistDelete)
				continue;

			// 回退球
			Ball _backforward = null;
			if (head != null) _backforward = head.Tail;
			else if (x > 0) _backforward = mSnakeSegment[x - 1].Tail;

			if (_backforward != null) {
				Ball _p = null;
				if (tail != null) _p = tail.Head;
				else if (x + 1 <= mSnakeSegment.Count - 1)
					_p = mSnakeSegment[x + 1];

				if (_p != null && _backforward.Type == _p.Type) {
					_p.BallTowards = _backforward;
					FallbackQueue.Add(_p);
				}
			}

			// 队伍分裂
			if (head != null) {
				mSnakeSegment[x] = head;
				if (tail != null && head != tail.Head) {
					mSnakeSegment.Insert(x + 1, tail.Head);
				} else {
				}
			} else {
				if (tail != null) {
					tail = tail.Head;
					mSnakeSegment[x] = tail;
				} else {
					mSnakeSegment.RemoveAt(x);
				}
			}
		}
	}

	static int SortCompare(Ball a, Ball b) {
		if (a.processIndex < b.processIndex) return -1;
		if (a.processIndex > b.processIndex) return 1;
		return 0;
	}

	void CheckSnakeConnect() {
		//应该先排序下
		//mSnakeSegment.Sort (SortCompare);
		int i = mSnakeSegment.Count;
		while (i-- > 1) {
			Ball q = mSnakeSegment[i];
			Ball p = mSnakeSegment[i - 1];
			Ball t = p.Tail;

			//前一段的尾巴碰到了后一段的头
			if (t.processIndex + 1 >= q.processIndex) {
				q.processIndex = t.processIndex + 1;
				UpdateBallAndBallDistance(q);
				t.SetNext(q);
				q.SetPre(t);
				mSnakeSegment.RemoveAt(i);
			}
		}
	}

	// 更新连接的距离
	void UpdateBallAndBallDistance(Ball b) {
		while (b != null) {
			if (b.Next != null &&
				b.Next.processIndex < b.processIndex + 1)
				b.Next.processIndex = b.processIndex + 1;
			b = b.Next;
		}
	}

	// 更新连接的距离
	void UpdateBallConnectDistance(Ball b) {
		while (b != null) {
			if (b.Next != null)
				b.Next.processIndex = b.processIndex + 1;
			b = b.Next;
		}
	}

	void UpdateAllBallTick () {
		float dt = Time.deltaTime;
		int i = mSnakeSegment.Count;
		while(i-- > 0) {
			Ball pointer = mSnakeSegment [i];
			do {
				pointer.Tick (dt);
				pointer = pointer.Next;
			}while (pointer!= null);
		}
	}

	void UpdateBallRender() {
		int i = mSnakeSegment.Count;
		while (i-- > 0) {
			Ball b = mSnakeSegment[i];
			do {
				b.UpdateBall();
				b = b.Next;
			} while (b != null);
		}
	}

	void CheckFallBackBall () {
		int i = FallbackQueue.Count;
		while (i-- > 0) {
			Ball pointer = FallbackQueue [i];
			if (pointer.IsDestroy () || pointer.BallTowards.IsDestroy ()) {
				FallbackQueue.RemoveAt (i);
				continue;
			}

			pointer.processIndex -= Time.deltaTime * 16;
			UpdateBallConnectDistance (pointer);

			if (pointer.processIndex <= pointer.BallTowards.processIndex + 1) {
				EliminateQueue.Add (pointer);
				FallbackQueue.RemoveAt (i);
				continue;
			}			
		}
	}

	BallType GameStrategy() {
		return GameBulletinBoard.GameStrategy();
	}
}

}