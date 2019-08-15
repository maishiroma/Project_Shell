/*  This is uses to play the sound effects that are in the game
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    public class SoundManager : MonoBehaviour {

        [Header("Music Refs")]
        public AudioClip BGM_Start;             // The BGM music that will play for this game
        public AudioClip BGM_Loop;
        public AudioClip winSFX;                // The win sound effect when the player selects the right shell
        public AudioClip loseSFX;               // The lose sound effect when the player selects the wrong shell

        [Range(0f,1f)]
        public float SFXVolume;                 // How loud is the SFX?

		// Private Variables
        private AudioSource bgmPlayer;          // Reference to the audio source that is only playing the BGM
        private AudioSource sfxPlayer;          // Reference to the audio source that is only plaing sound effects

        // Gets all of the references and starts the song
		private void Start()
		{
            bgmPlayer = GetComponents<AudioSource>()[0];
            sfxPlayer = GetComponents<AudioSource>()[1];

            StartCoroutine(StartSong());
		}

        // A special method that plays the start of the song and loops the rest of the song
        private IEnumerator StartSong()
        {
            bgmPlayer.PlayOneShot(BGM_Start);
            bgmPlayer.clip = BGM_Loop;
            while(bgmPlayer.isPlaying)
            {
                yield return null;
            }
            bgmPlayer.Play();
            bgmPlayer.loop = true;
        }

        // Called when the player won the round
        public IEnumerator WinSound()
        {
            bgmPlayer.Pause();
            yield return null;

            sfxPlayer.PlayOneShot(winSFX);
            while(sfxPlayer.isPlaying)
            {
                yield return null;
            }
            bgmPlayer.UnPause();
        }

        // Called when the player lost the round
        public IEnumerator LoseSound()
        {
            bgmPlayer.Pause();
            yield return null;

            sfxPlayer.PlayOneShot(loseSFX);
            while(sfxPlayer.isPlaying)
            {
                yield return null;
            }            
            bgmPlayer.Stop();
        }

        // Called to reset the entire song
        // Also resets the pitch as well.
        public void RestartSong()
        {
            sfxPlayer.Stop();
            bgmPlayer.Stop();
            bgmPlayer.pitch = 1f;

            StartCoroutine(StartSong());
        }

        // Upon being called, this will increase the pitch by 0.1f
        public void SpeedUpSong()
        {
            if(bgmPlayer.pitch > 3f)
            {
                bgmPlayer.pitch += 0.1f;
            }
        }
	}
}