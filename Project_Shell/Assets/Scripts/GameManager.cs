/*  This keeps track of all global game items
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    public enum GameState {
        SELECTING,
        SHUFFLING,
        START
    }

    public class GameManager : MonoBehaviour {

        public static GameManager Instance;

        public GameState currentState = GameState.START;
        public int numberOfSwitches = 10;                   // Number of times the shells swap

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

		// When called, randomly picks a shell that is deemed the winning shell.
		private void DetermineLuckyShell()
        {
            int randomIndex = Random.Range(0, InteractShell.shellList.Count);
            InteractShell.shellList[randomIndex].isWinner = true;
            print(InteractShell.shellList[randomIndex].name + " is the lucky one.");
        }

        // When called, resets all of the shells to their normal states and positions
        private void ResetShells()
        {
            foreach(InteractShell currentShell in InteractShell.shellList)
            {
                currentShell.transform.position = currentShell.GetOrigLocation;

                if(currentShell.isWinner == true)
                {
                    currentShell.isWinner = false;
                    print(currentShell.name + " was reset back to normal.");
                }
            }
        }

        // While this is running, the shells will be called to move.
        private IEnumerator PerformGame()
        {
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
    
        // When called, starts up the game
        public void StartGame()
        {
            if(currentState == GameState.START)
            {
                ResetShells();
                DetermineLuckyShell();
                currentState = GameState.SHUFFLING;

                StartCoroutine(PerformGame());
            }
        }
    
        // When called, resets the game.
        public void ResetGame()
        {
            if(currentState == GameState.SELECTING)
            {
                ResetShells();
                currentState = GameState.START;
            }
        }
    }
}
