using UnityEngine;
using System.Collections;
namespace ZumaGame { 
/// <summary>
/// 游戏公告板，大家都看到的东西。
/// 需要权限才能写上,这里权限制作约定
/// </summary>
public class GameBulletinBoard {

	public static GamePool GamePool { get ; private set; }	
	public static void SetGamePool (GamePool ptr) {GamePool = ptr; }

	public static GameManager GameManager { get ; private set; }	
	public static void SetGameManager (GameManager ptr) { GameManager = ptr;} 

	public static MapConfig MapConfig { get; private set;}
	public static void SetMapConfig (MapConfig ptr) { MapConfig = ptr;}

	public static FireBallManager FireBallManager { get; private set;}
	public static void SetFireBallManager (FireBallManager ptr) { FireBallManager = ptr;}

	public static Fx Fx { get; private set;}
	public static void SetFx (Fx ptr) { Fx = ptr;}

	// 游戏策略
	public static BallType GameStrategy () {
		if (typeNumber < 0) {
			typeNumber = UnityEngine.Random.Range (0, 2);
			type = UnityEngine.Random.Range (0, 3);
		}

		typeNumber--;
		return (BallType) type;
	}

	static int type;
	static int typeNumber= 0;
}

/// <summary>
/// 游戏帮助作用
/// 主要是解耦
/// </summary>
public static class GameHelper {

	// 不再是在开始洞的出口
	public static bool IsNotHoleExit (this Ball ptr) {
		return ptr.processIndex >= 1.0f;
	}

	// 到达了失败洞口
	public static bool IsArriveFailurePoint (this Ball ball) {
		return ball.processIndex >= GameBulletinBoard.MapConfig.FailurePoint;
	}	
}
}