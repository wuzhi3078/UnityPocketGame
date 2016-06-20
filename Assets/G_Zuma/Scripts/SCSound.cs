using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZumaGame {

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
	bool misInit = false;
	void InitScriptData () {
		if (misInit) return;
		misInit = true;

		DontDestroyOnLoad (gameObject);
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
		
	public static string monkey_background_music = @"monkey_09";
	public static void monkey_xiaochu1 ()	{Single.Play ("monkey_02");} ///	撞击球消除的音效
    public static void monkey_yundong ()	{Single.Play ("monkey_03");} ///	球列快速运动出现时的音效
    public static void monkey_fashe ()		{Single.Play ("monkey_04");} ///	发射球时的音效
    public static void monkey_qiu ()	    {Single.Play ("monkey_11");} ///	球进入队列
    public static void monkey_zhadan ()		{Single.Play ("monkey_05");} ///	撞击到炸弹球爆炸的音效
    public static void monkey_bianse ()		{Single.Play ("monkey_06");} ///	撞击到变色球的音效
    public static void monkey_xiaochu ()	{Single.Play ("monkey_07");} ///	撞击到消除球的音效
    public static void monkey_huixuan ()	{Single.Play ("monkey_08");} ///	撞击到回旋球的音效
    public static void monkey_shibai ()		{Single.Play ("monkey_12");} ///	失败！球快速进入终点
    public static void monkey_beijing ()	{Single.Play ("monkey_09");} ///	背景音乐
}

}