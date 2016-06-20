using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;
namespace PopStar {
/// <summary>
/// 星星的类型
/// </summary>
public enum StarType {

	// 基本的5种颜色
	Red = 0,
	Yellow = 1,
	Blue = 2,
	Green = 3,
	Purple = 4,

	// 功能性	
	Transform = 5,// 地图变换
	Eliminate = 6 // 清除一种颜色

}


public class GameManager : MonoBehaviour {
	// 外部公开的成员
	public int MapLine { get { return this.mapLine; } }
	public int MapCell { get { return this.mapCell; } }
	public float PixelPreUtil { get { return this.pixelEveryStar; } }

	// 游戏的全局状态
	protected enum GameState {

		Rotation,
		InputHandle
	}

	// 地图矩阵最大行
	// 地图矩阵最大列。{0,0}在左下角
	int mapLine = 8;
	int mapCell = 6;
	// 每个星星块子的Sprite大小
	float pixelEveryStar = 0.85f;

	// 星星地图矩阵
	MazeMap<NPCStar> MMap = null;
	// 间隔更新
	SCSchedule updateBlankSpace;
	// 出题策略
	GameQuestionRule mQuestionRule;

	void Awake() {
		// 适配屏幕处理
		if (Camera.main.aspect > (2 / 3f)) {
			Camera.main.orthographicSize = 4.8f * (2 / 3f) / Camera.main.aspect;
		}
	}

	// Use this for initialization
	void Start() {
		// 初始背景音效
		PlayBackgroundMusic();

		// 初始配置
		mQuestionRule = new GameQuestionRule(MapCell);

		// 初始地图
		InitMap();

		// 星星的移到适当的左下角去	
		AdaptationLeftDownPosition();

		// 测试搜索功能
		// TestSeatrch ();	

		// 游戏开始
		updateBlankSpace = new SCSchedule(this, "UpdateBlankSpace", 0.1f, 0.1f);
		updateBlankSpace.beginSchedule();
	}

	/// <summary>
	/// 初始化游戏矩阵地图,实际性放置星星
	/// </summary>
	void InitMap() {
		int line = mapLine;
		int cell = mapCell;
		MMap = new MazeMap<NPCStar>(line, cell);

		for (int i = 0; i < line; ++i) {
			for (int j = 0; j < cell; ++j) {

				NPCStar star = GamePool.Instance.GetNPCStarFromType(GetGameStarTypeStrategy(j));
				star.InitUsed(new MazeMapPoint() { x = i, y = j });

				MMap.setMap(star, i, j);
			}
		}
	}

	void AdaptationLeftDownPosition() {
		GameObject.Find("GameStar").transform.position =
			new Vector3(-pixelEveryStar * (mapCell - 1) * 0.5f, -pixelEveryStar * (mapLine - 1) * 0.5f - 0.4f, 0);
	}

	//void TestSeatrch () {
	//	NPCStar e = MMap.getMap (0,0);
	//	List <NPCStar> queue = MMap.OnSearchMap (0,0, (p) => {
	//		return p.IsPassTestSearch () && p.IsSameAsType (e);
	//	});

	//	Debug.Log ("search result : " + queue.Count);
	//	foreach (NPCStar e1 in queue) {
	//		Debug.Log ("x: "+ e1.Point.x + " y:" + e1.Point.y);
	//	}
	//}

	void PlayBackgroundMusic() {
		AudioClip clip = SCSound.Single.getSoundClip("popanimal_08");
		AudioSource sound = gameObject.AddComponent<AudioSource>();
		sound.clip = clip;
		sound.loop = true;
		sound.Play();
	}

	// Update is called once per frame
	void Update() {

		bool misTouch = false;
		Vector3 mTouchPosition = Vector3.zero;

		if (Input.GetMouseButtonDown(0)) {
			mTouchPosition = Input.mousePosition;
			misTouch = true;
		}

		//if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began) {
		//	mTouchPosition = Input.GetTouch (0).position;
		//	misTouch = true;
		//}

		if (misTouch) {
			Vector3 touchPosition = Camera.main.ScreenToWorldPoint(mTouchPosition);

			NPCStar touchObject = null;
			MMap.LoopHandleMapContentWithRT((e) => {
				if (touchObject == null && e != null) {
					if (e.Box2d.OverlapPoint(touchPosition)) {
						touchObject = e;
						MMap.IsLoopRT = true;
					}
				}
			});

			if (touchObject != null) {
				Debug.Log("TOUCH: " + touchObject.Point.x + " " + touchObject.Point.y);
				if (touchObject.CanTriggerMatch()) {
					HandleClearMap(touchObject);
				}
			}
		}
	}

	void UpdateBlankSpace() {
		int[] topNum = new int[MapCell];

		for (int j = 0; j < MapCell; ++j) {
			for (int i = 0; i < MapLine; ++i) {
				NPCStar e = MMap.getMap(i, j);
				if (e == null) {
					// if is Empty then FallDown

					NPCStar up = null;
					for (int k = i + 1; k < MapLine; ++k) {
						up = MMap.getMap(k, j);
						if (up != null) {
							MMap.setMap(null, k, j);

							SetMapContent(up, i, j, true, up.Point);
							break;
						}
					}

					if (up == null) {
						//最上面为空,以下操作是填充新的格子到上面

						topNum[j]++;

						up = GamePool.Instance.GetNPCStarFromType(GetGameStarTypeStrategy(j));

						SetMapContent(up, i, j, true, new MazeMapPoint() { x = MapLine + topNum[j], y = j });
					}
				}
			}
		}
	}

	/// <summary>
	/// 搜索消除
	/// </summary>
	/// <param name="p">开始搜索的星星</param>
	void HandleClearMap(NPCStar p) {
		List<NPCStar> list = MMap.OnSearchMap(p.Point.x, p.Point.y, (e) => {
			return p.IsPassTestSearch() && p.IsSameAsType(e);
		});

		int mCount = list.Count;

		if (mCount >= 3) {
			int L = list.Count;
			while (L-- > 0) {
				NPCStar e = list[L];
				e.OnDestroy();
				PlayEliminateEffect(e.transform.position);
				MMap.setMap(null, e.Point.x, e.Point.y);
			}

			PlayEliminateSoundEffect(mCount);
			GameUI.SetGameAddScore(ScoringComputation(list.Count));
		} else {

			SCSound.game_cuowu();
		}
	}

	/// 计算得分
	int ScoringComputation(int length) {
		int score = 0;
		if (length < 5) score = 5 * length;
		else if (length < 10) score = 10 * length;
		else if (length < 15) score = 15 * length;
		else score = 20 * length;

		return score;
	}

	// 播放消除音效
	void PlayEliminateSoundEffect(int count) {
		if (count >= 6) SCSound.game_xiaochu03();
		else if (count >= 4) SCSound.game_xiaochu02();
		else if (count >= 3) SCSound.game_xiaochu01();
		else { }
	}

	/// <summary>
	/// 延时操作
	/// </summary>
	/// <param name="time">延时时间</param>
	/// <param name="doing">等待时间之后的操作</param>
	/// <returns></returns>
	IEnumerator WaitForDoing(float time, System.Action doing) {
		yield return new WaitForSeconds(time);
		doing();
	}

	/// <summary>
	/// 在p处播放一个消除特效
	/// </summary>
	/// <param name="p">世界位置</param>
	void PlayEliminateEffect(Vector3 p) {
		GameObject fx = GamePool.Instance.EliminateEffect.GetObject();
		p.z = fx.transform.position.z;
		fx.transform.position = p;
		fx.SetActive(true);

		StartCoroutine(WaitForDoing(0.5f, () => {
			fx.SetActive(false);
			GamePool.Instance.EliminateEffect.SetObject(fx);
		}));
	}

	/// <summary>
	/// 获取出现星星类型的策略
	/// </summary>
	StarType GetGameStarTypeStrategy(int col) {
		//int rnd = UnityEngine.Random.Range(0, 5);
		//return (StarType) rnd;

		return mQuestionRule.GetRuleType(col);
	}

	/// <summary>
	/// 地图坐标{x,y}时的位置信息
	/// </summary>
	public Vector3 GetMapCoordPosition(int x, int y) {
		return new Vector3(pixelEveryStar * y, pixelEveryStar * x, 0);
	}

	/// <summary>
	/// 设置地图的星星内容，附带移动过程
	/// </summary>
	public void SetMapContent(NPCStar p, int i, int j, bool withMove, MazeMapPoint initCoord) {
		MMap.setMap(p, i, j);

		if (p == null) {
			return;
		}

		p.Point = new MazeMapPoint() { x = i, y = j };
		p.transform.localPosition = GetMapCoordPosition(initCoord.x, initCoord.y);

		if (withMove) {
			p.BeforeOnMove();
			p.transform.DOLocalMove(GetMapCoordPosition(i, j), 0.35f).OnComplete(() => {
				p.AfterOnMove();
			});
		} else {
			p.transform.localPosition = GetMapCoordPosition(i, j);
		}
	}
}


/// <summary>
/// 辅助间隔更新类
/// </summary>
public class SCSchedule {
	MonoBehaviour script;
	string updateFunctionName;
	float delayTime;
	float rateTime;

	public SCSchedule(MonoBehaviour mono, string func, float delay, float updateRate) {
		script = mono;
		updateFunctionName = func;
		delayTime = delay;
		rateTime = updateRate;
	}

	public void beginSchedule() {
		script.InvokeRepeating(updateFunctionName, delayTime, rateTime);
	}

	public bool IsUpdate() {
		return script.IsInvoking(updateFunctionName);
	}

	public void stopSchedule() {
		script.CancelInvoke(updateFunctionName);
	}
}


/// <summary>
/// 出题规则
/// </summary>
public class GameQuestionRule {
	int mColumn;
	int maxNum;
	int[,] Array;
	int[] Length;

	public GameQuestionRule(int column, int meanwhileMaxNumber = 4) {
		mColumn = column;
		maxNum = meanwhileMaxNumber;

		Array = new int[mColumn, maxNum];
		Length = new int[mColumn];

		Init();
	}

	public StarType GetRuleType(int col) {
		if (Length[col] < 0) {
			InitColumn(col);
		}

		return (StarType)Array[col, Length[col]--];
	}

	void Init() {
		int i = mColumn;
		while (i-- > 0) {
			InitColumn(i);
		}
	}

	void InitColumn(int col) {
		int t = UnityEngine.Random.Range((int)StarType.Red, (int)StarType.Purple);
		int n = UnityEngine.Random.Range(0, maxNum - 1);
		for (int i = 0; i <= n; ++i)
			Array[col, i] = t;
		Length[col] = n;
	}
}
}