using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public static AudioManager instance;
	[SerializeField]
	private AudioClip[] music;
	private AudioSource audioSrc;

	void Awake () 
	{
		if (instance == null)
		{
			instance = this;
		} else {
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		audioSrc = GetComponent<AudioSource>();
		audioSrc.volume = PlayerPrefs.GetInt("Volume", 1);
		playMusic(UnityEngine.Random.Range(0, music.Length)); //Plays a random intro song
	}

	void playMusic(int musicID)
	{
		audioSrc.clip = music[musicID];
		audioSrc.Play();
	}

	public void stopMusic()
	{
		audioSrc.Stop();
	}

	// public void PlaySound(string name, float pitch)
	// {
	// 	Sound soundClip = Array.Find(sounds, sound => sound.name == name);
	// 	if (soundClip == null)
	// 		{
	// 			Debug.LogWarning("Sound: " + name + " not found!");
	// 			return;
	// 		}
	// 	soundClip.source.pitch = pitch;
	// 	soundClip.source.Play();
	// }
}
