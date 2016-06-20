using UnityEngine;
using System.Collections;
namespace ZumaGame { 
public class Fx : MonoBehaviour {
	[SerializeField]
	[Header("mold of fx")]
	GameObject mold;

	SCObjectPool<GameObject> pool;

	void Start () {
		GameBulletinBoard.SetFx (this);
		pool = new SCObjectPool<GameObject> (OnNewInstanceFx, 15);
	}
	
	GameObject OnNewInstanceFx () {
		GameObject g = Instantiate (mold) as GameObject;
		g.transform.parent = transform;
		g.transform.localPosition = Vector3.zero;
		return g;
	}

	public void PlayEffectAtPoint(Vector3 p) {
		GameObject g = pool.GetObject();
		p.z = g.transform.position.z;
		g.transform.position = p;
		g.SetActive(true);

		ScheduleOnce.CreateActive(this, 0.5f, () => {
			g.SetActive(false);
			pool.SetObject(g);
		});
	}
}
}