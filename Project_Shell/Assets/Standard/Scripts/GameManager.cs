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
        LOADING,            // When the player is transitioning to the next round
        BEGINNING           // When the player enters the game
    }

    public class GameManager : MonoBehaviour {

        public static GameManager Instance;

        [Header("Gameplay Variables")]
        [Tooltip("How many times does the game swap the shells?")]
        [Range(1,100)]
        public int numberOfSwitches = 10;                   // Number of times the shells swap

        [Tooltip("How many sucessful rounds will it take to make the game increase in difficulty?")]
        [Range(1,10)]
        public int turnDifficulty = 5;

        [Header("External Refs")]
        public SoundManager soundPlayer;

        // Private Variables
        private GameState currentState = GameState.BEGINNING;   // The current state the game is in rn
        private InteractShell luckyShell;                   // Keeps track of this round's lucky shell
        private int gameScore;                              // The current score the player has
        private int origNumberOfSwitches;                   // Number of times the shells swap

        public int GetScore {
            get {return gameScore;}
        }

        public GameState GetCurrentState {
            get {return currentState;}
        }

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
            currentState = GameState.BEGINNING;
            gameScore = 0;
            origNumberOfSwitches = numberOfSwitches;
		}

		// When called, randomly picks a shell that is deemed the winning shell. Returns said shell out
        private InteractShell DetermineLuckyShell()
        {
            int randomIndex = Random.Range(0, InteractShell.shellList.Count);
            InteractShell.shellList[randomIndex].MarkAsLucky();
            return InteractShell.shellList[randomIndex];
        }

        // When called, increases the speed of the shells, their arc and the number of switches
        private void IncreaseDifficulty()
        {
            float randMoveSpeedIncrease = Random.Range(1f,2f);
            float randArcHeightIncrease = Random.Range(1f,2f);

            foreach(InteractShell currShell in InteractShell.shellList)
            {
                int randChance = Random.Range(0,2);

                currShell.moveSpeed = Mathf.Clamp(currShell.moveSpeed + randMoveSpeedIncrease, 1f, 100f);

                // This determines if the arc will be negative or not.
                if(randChance == 1)
                {
                    currShell.arcHeight = -Mathf.Clamp(Mathf.Abs(currShell.arcHeight) + randArcHeightIncrease, -5f, 5f);
                }
                else
                {
                    currShell.arcHeight = Mathf.Clamp(Mathf.Abs(currShell.arcHeight) + randArcHeightIncrease, -5f, 5f);
                }
            }
            numberOfSwitches = Mathf.Clamp(numberOfSwitches + 1, 1,100);
        }

        // Called to show the shell that the player should be looking at
        public IEnumerator ShowLuckyShell()
        {
            while(luckyShell.AnimateOpenChest() == false)
            {
                yield return null;
            }
            yield return null;
            while(luckyShell.AnimateCloseChest() == false)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);

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
            if(currentState == GameState.BEGINNING || currentState == GameState.LOADING)
            {
                if(gameScore % turnDifficulty == 0 && gameScore != 0)
                {
                    // Every X sucessful rounds, we increase the difficulty.
                    IncreaseDifficulty();
                    soundPlayer.SpeedUpSong();
                }

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
                // The reason we wait 3 seconds here is because we want to make sure the ShowLuckyShell method
                // completes running its course, since we called it in parallel to this method.
                if(selectedShell.IsWinner == true)
                {
                    currentState = GameState.WIN;
                    gameScore += 1;
                    StartCoroutine(soundPlayer.WinSound());

                    while(selectedShell.AnimateOpenChest() == false)
                    {
                        yield return null;
                    }
                    while(selectedShell.AnimateCloseChest() == false)
                    {
                        yield return null;
                    }
                }
                else
                {
                    currentState = GameState.LOSE;
                    StartCoroutine(soundPlayer.LoseSound());

                    while(luckyShell.AnimateOpenChest() == false)
                    {
                        yield return null;
                    }
                    while(luckyShell.AnimateCloseChest() == false)
                    {
                        yield return null;
                    }

                    // If we lose once, we restart from the start
                    gameScore = 0;
                    numberOfSwitches = origNumberOfSwitches;
                    soundPlayer.RestartSong();
                }
                yield return new WaitForSeconds(1f);

                // We then reset the shells back
                foreach(InteractShell currShell in InteractShell.shellList)
                {
                    currShell.ResetShell();
                }

                // And depending if we won/lost, we proceed accordingly
                if(currentState == GameState.LOSE)
                {
                    currentState = GameState.BEGINNING;
                }
                else if(currentState == GameState.WIN)
                {
                    currentState = GameState.LOADING;
                    yield return null;
                    StartGame();
                }
            }
        }
    }
}
