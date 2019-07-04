/*  This keeps track of all global game items
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    // All of the states the game can be in
    public enum GameState {
        LOSE,               // The player chose incorrectly
        WIN,                // The player chose correctly
        SELECTING,          // When the game asks the player to choose the correct one
        SHUFFLING,          // When the game is moving the shell around
        SHOW,               // When the game shows the player the lucky shell
        START               // When the player starts a new round
    }

    public class GameManager : MonoBehaviour {

        public static GameManager Instance;

        [Header("Gameplay Variables")]
        public GameState currentState = GameState.START;    // The current state the game is in rn

        [Tooltip("How many times does the game swap the shells?")]
        [Range(1,100)]
        public int numberOfSwitches = 10;                   // Number of times the shells swap

        // Private Variables
        private InteractShell luckyShell;

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

        // We set the game state to be starting
		private void Start()
		{
            currentState = GameState.START;
		}

		// When called, randomly picks a shell that is deemed the winning shell. Returns said shell out
        private InteractShell DetermineLuckyShell()
        {
            int randomIndex = Random.Range(0, InteractShell.shellList.Count);
            InteractShell.shellList[randomIndex].isWinner = true;
            return InteractShell.shellList[randomIndex];
        }

        // Called to show the shell that the player should be looking at
        public IEnumerator ShowLuckyShell()
        {
            // The shell should flash 3 times
            for(int i = 0; i < 6; i++)
            {
                luckyShell.ToggleShellColors();
                yield return new WaitForSeconds(0.5f);
            }

            // If the game state is at SHOW, we start the game up
            // Otherwise, we simply leave the method
            if(currentState == GameState.SHOW)
            {
                StartCoroutine(PerformGame());
            }
        }

        // When called, starts up the game
        public void StartGame()
        {
            if(currentState == GameState.START)
            {
                luckyShell = DetermineLuckyShell();

                currentState = GameState.SHOW;
                StartCoroutine(ShowLuckyShell());
            }
        }
    
        // While this is running, the shells will be called to move.
        public IEnumerator PerformGame()
        {
            if(currentState == GameState.SHOW)
            {
                currentState = GameState.SHUFFLING;
                for(int iterating = 0; iterating < numberOfSwitches; ++iterating)
                {
                    // We pick two random shells to move
                    int randomIndex1 = Random.Range(0, InteractShell.shellList.Count);
                    int randomIndex2 = Random.Range(0, InteractShell.shellList.Count);
                    while(randomIndex1 == randomIndex2)
                    {
                        randomIndex2 = Random.Range(0, InteractShell.shellList.Count);
                    }

                    InteractShell currShell1 = InteractShell.shellList[randomIndex1];
                    InteractShell currShell2 = InteractShell.shellList[randomIndex2];
                    currShell1.SwapShellLocation(currShell2);

                    // While we are moving these shells, we wait
                    while(currShell1.IsMoving || currShell2.IsMoving)
                    {
                        yield return null;
                    }
                }
                currentState = GameState.SELECTING;
            }
        }

        // This is called when the player has selected a shell to see if they are right
        public IEnumerator ConcludeGame(InteractShell selectedShell)
        {
            if(currentState == GameState.SELECTING)
            {
                if(selectedShell.isWinner == true)
                {
                    currentState = GameState.WIN;
                }
                else
                {
                    currentState = GameState.LOSE;
                }

                // The reason we wait 3 seconds here is because we want to make sure the ShowLuckyShell method
                // completes running its course, since we called it in parallel to this method.
                yield return new WaitForSeconds(3f);
                ResetGame();
            }
        }

        // When called, resets the game back to the initial state.
        public void ResetGame()
        {
            if(currentState != GameState.SELECTING && currentState != GameState.SHOW)
            {
                foreach(InteractShell currShell in InteractShell.shellList)
                {
                    StartCoroutine(currShell.ResetShell());
                }
                currentState = GameState.START;
            }
        }
    }
}
