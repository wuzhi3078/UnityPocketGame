using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace PopStar {
public class SCSound : MonoBehaviour {
	static SCSound _single = null;
	public static SCSound Single {
		get {
			if (_single == null) {
				GameObject g = new GameObject ("SCSound");
				_single = g.AddComponent <SCSound> ();
				_single.InitScriptData ();
			}
			return _single;
		}
	}

	const int maxTempChannel = 16;
	//AudioSource[] tempChannel = new AudioSource[maxTempChannel];

	bool misInit = false;
	void InitScriptData () {
		if (misInit) return;
		misInit = true;

		DontDestroyOnLoad (gameObject);

		//for (int i=0; i<maxTempChannel; ++i) {
		//	GameObject g = new GameObject ("Sound" + i);
		//	g.transform.parent = _single.transform;
		//	AudioSource source = g.AddComponent <AudioSource> ();
		//	tempChannel [i] = source;
		//}
	}

	void Awake () {
		_single = this;
		InitScriptData ();
	}

	public void Play (string fileID) {
		AudioSource.PlayClipAtPoint (getSoundClip (fileID), Vector3.zero);
	}

	public class SoundEffect {
		void Update () {

		}
	}

	string defaultPath = "Sound/";
	public AudioClip getSoundClip(string str) {
		string s = defaultPath + str;
		AudioClip clip = Resources.Load(s, typeof(AudioClip)) as AudioClip;
		return clip;
	}

	
	public static void game_xiaochu01 () {Single.Play ("popanimal_02");} ///	点击砖块消除的音效1(消除3个已下时播放)
	public static void game_xiaochu02 () {Single.Play ("popanimal_03");} ///	点击砖块消除的音效2(消除4--5个下时播放)
	public static void game_xiaochu03 () {Single.Play ("popanimal_04");} ///	点击砖块消除的音效3(消除6个以上时播放)
	public static void game_cuowu	  () {Single.Play ("popanimal_05");} ///	点中不符合条件消除的砖块的音效
	public static void game_bianse	  () {Single.Play ("popanimal_06");} ///	点击变色道具的音效
	public static void game_huanwei   () {Single.Play ("popanimal_07");} ///	点击换位道具的音效
	public static void game_beijing   () {Single.Play ("popanimal_08");} ///	背景音乐
	public static void game_gaibian   () {Single.Play ("popanimal_12");} ///	当画面中已经没有可消失的砖块，重新改变砖块时播放
}
}