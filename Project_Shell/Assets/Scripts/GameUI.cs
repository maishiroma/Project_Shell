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

        [Header("GUI References")]
        public Button startButton;                      // Reference to the Start Button
        public TextMeshProUGUI gameMessage;             // Reference to the game message
        public TextMeshProUGUI gameScore;

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
            switch(GameManager.Instance.GetCurrentState)
            {
                case GameState.START:
                    startButton.gameObject.SetActive(true);
                    gameMessage.text = "Ready?";
                    gameScore.text = "Streak: " + GameManager.Instance.GetScore;
                    break;
                case GameState.SHOW:
                    startButton.gameObject.SetActive(false);
                    gameMessage.text = "Here's your target this round!";
                    break;
                case GameState.SHUFFLING:
                    gameMessage.text = "Keep an eye on the prize!";
                    break;
                case GameState.SELECTING:
                    gameMessage.text = "Which one is the lucky object?";
                    break;
                case GameState.WIN:
                    gameMessage.text = "Correct choice!";
                    break;
                case GameState.LOSE:
                    gameMessage.text = "Too bad...";
                    break;
            }
		}
	}
}
