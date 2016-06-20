using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace ZumaGame { 
public class GamePool : MonoBehaviour {

	void Awake () {
		GameBulletinBoard.SetGamePool (this);
	}
}
}

public class SCObjectPool <T> {

	public int DefaultIncrease = 3;

	List <T> pool = new List<T> ();
	System.Func <T> mDoInstanceFunc;

	public SCObjectPool (System.Func <T> func, int initNumber) {
		mDoInstanceFunc = func;
		InstanceObjectNumber (initNumber);
	}

	public T GetObject () {
		int i = pool.Count;
		while (i-- > 0) {
			T e = pool [i];
			pool.RemoveAt (i);
			return e;
		}

		InstanceObjectNumber (DefaultIncrease);
		return GetObject ();
	}

	public void SetObject (T t) {
		pool.Add (t);
	}

	void InstanceObjectNumber (int n) {

		int i = n;
		while (i-- > 0) {
			pool.Add (mDoInstanceFunc ());
		}
	}
}