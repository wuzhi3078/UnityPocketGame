using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace PopStar {
/// <summary>
/// 地图坐标点
/// </summary>
public struct MazeMapPoint {
	public int x;
	public int y;

	static MazeMapPoint _zero = new MazeMapPoint () { x = 0, y = 0};
	public static MazeMapPoint Zero {
		get {
			return _zero;
		}
	}
}

/// <summary>
/// 地图格子内容的接口
/// </summary>
public interface IMapContent {
	

}

/// <summary>
/// 地图
/// </summary>
/// <typeparam name="T"></typeparam>
public class MazeMap <T> {
	// 是否检查边界等错误
	bool misCheckError = true;

	// 二维迷宫的格子的内容
	T [,] map = null;

	// 辅助标志
	SearchFlagArrayHelper mapflag = null;

	// 辅助搜索的队列
	SearchQueueHelper <MazeMapPoint> queue = null;

	// 迷宫最大的行数与列数
	int maxMapLine = 1;
	int maxMapCell = 1;

	public MazeMap (int line, int cell) {
		this.maxMapLine = line;
		this.maxMapCell = cell;

		map = new T [line, cell];
		mapflag = new SearchFlagArrayHelper (line, cell);
		queue = new SearchQueueHelper<MazeMapPoint> (line * cell + 1);
	}

	// 是否开启边界检查
	public void setCheckError (bool mbool) { this.misCheckError = mbool;}

	// 地图放置内容
	public void setMap (T content, int line, int cell) {
		if (this.misCheckError) AssertOutBounds (line, cell);

		this.map [line, cell] = content;
	}
	 
	public int GetMaxLine { get { return maxMapLine;} }

	public int GetMaxCell { get { return maxMapCell;} }

	// 获取地图中的内容
	public T getMap (int line , int cell) {
		if (this.misCheckError) AssertOutBounds (line, cell);

		return map [line, cell];
	}

	// 轮询处理地图中的内容
	public void LoopHandleMapContent (System.Action <T, int, int> handleFunction) {
		for (int i = 0; i < maxMapLine; ++i)
			for (int j = 0; j < maxMapCell; ++j) {
				handleFunction (getMap (i, j), i, j);
			}
	}

	// 轮询处理地图中的内容
	public void LoopHandleMapContent (System.Action <T> handleFunction) {
		for (int i = 0; i < maxMapLine; ++i)
			for (int j = 0; j < maxMapCell; ++j) {
				handleFunction (getMap (i, j));
			}
	}

	// 轮询处理地图中的内容,附带中断
	public void LoopHandleMapContent (System.Func <T, bool> handleFunction) {
		bool isRT = false;
		for (int i = 0; i < maxMapLine; ++i) {
			if (isRT) break;
			for (int j = 0; j < maxMapCell; ++j) {
				isRT = handleFunction (getMap (i, j));
				if (isRT) break;
			}
		}
	}

	// 轮询处理地图中的内容,附带中断
	public void LoopHandleMapContentWithRT (System.Action <T> handleFunction) {
		IsLoopRT = false;
		for (int i = 0; i < maxMapLine; ++i) {
			if (IsLoopRT) break;
			for (int j = 0; j < maxMapCell; ++j) {
				handleFunction (getMap (i, j));
				if (IsLoopRT) break;
			}
		}
	}

	public bool IsLoopRT { get; set;}

	public List <T> OnSearchMap (int pointx, int pointy, System.Func <T, bool> IsSearchPass) {
		List <T> list = new List<T> ();

		queue.Reset ();
		mapflag.ClearMapFlag ();
		
		queue.AddTail (new MazeMapPoint (){x = pointx, y = pointy});
		mapflag.SetMapFlag (pointx, pointy);
		list.Add (getMap (pointx, pointy));

		int[] tx = new int[] {-1, 0, 0, 1 };
		int[] ty = new int[] { 0, -1, 1, 0};

		while (queue.IsNotEmpty ()) {
			MazeMapPoint head = queue.GetHead ();

			for (int i=0; i<4; ++i) {
				int dx = head.x + tx [i];
				int dy = head.y + ty [i];

				// 已经过界
				if (dx < 0 || dx >= maxMapLine || 
					dy < 0 || dy >= maxMapCell)
					continue;

				// 已经访问过了
				if (mapflag.IsMapFlag (dx, dy)) continue;

				T ptr = getMap (dx, dy);
				// 没有存在星星
				if (ptr == null) continue;
				
				// 星星是否通过搜索
				if (IsSearchPass (ptr)) {
					queue.AddTail (new MazeMapPoint () { x = dx, y = dy});
					mapflag.SetMapFlag (dx, dy);

					list.Add (ptr);	
				}					
			}
			
			queue.Next ();
		}

		return list;
	}

	// 越界断言
	void AssertOutBounds (int line, int cell) {
		if (this.misCheckError) {
			if (line < 0 || line >= maxMapLine) Error ("< 0 || >= line");
			if (cell < 0 || cell >= maxMapCell) Error ("< 0 || >= cell");
		}
	}

	void Error (string e) {
		throw new UnityException (e);
	}
}

/// <summary>
/// 搜索辅助标志帮助
/// </summary>
public class SearchFlagArrayHelper {
	public void ClearMapFlag () {
		for (int i= 0 ; i< line; ++i)
			for (int j = 0; j< cell; ++j)
				mapflag [i, j] = false;
	}
	public void SetMapFlag (int x, int y) {
		mapflag [x, y] = true;
	}
	public bool IsMapFlag (int x, int y) {
		return mapflag [x, y];
	}

	// 辅助标志
	bool [,] mapflag = null;
	int line ,cell;

	public SearchFlagArrayHelper (int xL, int xC) {
		line =xL;
		cell =xC;
		mapflag = new bool [line, cell];
	}
}


/// <summary>
/// 地图搜索时辅助的队列
/// </summary>
public class SearchQueueHelper <T> {

	int head = 0;
	int tail = 0;

	T[] mqueue = null;

	public int Head {get { return head;} }

	public int Tail { get { return tail;} }

	public void Next () {
		head++;
	}

	public void AddTail (T t) {
		mqueue [tail] = t;

		tail++;
	}

	public T GetHead () {
		return mqueue [head];
	}

	public bool IsNotEmpty () {
		return (head < tail);
	}

	public void Reset () {
		head = 0;
		tail = 0;
	}

	public SearchQueueHelper (int length) {
		mqueue = new T [length];
	}
}
}