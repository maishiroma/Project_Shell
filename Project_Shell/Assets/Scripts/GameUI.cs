/*  Handles all of the UI in the game
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MattScripts {

    public class GameUI : MonoBehaviour {

        public static GameUI Instance;

        public Button startButton;                      // Reference to the Start Button
        public TextMeshProUGUI gameMessage;             // Reference to the game message

        // Singleton Pattern
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Depending on the game state, this changes the game UI
		private void Update()
		{
            switch(GameManager.Instance.currentState)
            {
                case GameState.START:
                    startButton.gameObject.SetActive(true);
                    gameMessage.text = "Welcome to Shell Game!";
                    break;
                case GameState.SHUFFLING:
                    startButton.gameObject.SetActive(false);
                    gameMessage.text = "Keep an eye on the prize!";
                    break;
                case GameState.SELECTING:
                    startButton.gameObject.SetActive(false);
                    gameMessage.text = "Which one is the lucky object?";
                    break;
            }
		}
	}
}
