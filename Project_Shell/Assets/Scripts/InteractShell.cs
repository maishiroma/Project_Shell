/*  This defines how an object interacts with the mouse
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    public class InteractShell : MonoBehaviour {

        public static List<InteractShell> shellList;    // All of the shells know how many shells are there in the game

        public bool isWinner = false;                   // This indicates that this shell is the lucky one

        // Adds this shell to the list of shells in the game
		private void Awake()
		{
            if(shellList == null)
            {
                shellList = new List<InteractShell>();
            }

            shellList.Add(this);
		}

		// We have selected this object
		private void OnMouseUp()
		{
            Debug.Log("We selected " + name);
		}

        // We are hovering over said object
		private void OnMouseEnter()
		{
            Debug.Log("We are over " + name);
		}

        // We left the mouse from this object
		private void OnMouseExit()
		{
            Debug.Log("We exited " + name);
		}
	
        // When called, randomly picks a shell that is deemed the winning shell.
        public void DetermineLuckyShell()
        {
            int randomIndex = Random.Range(0, shellList.Count);
            shellList[randomIndex].isWinner = true;
            print(shellList[randomIndex].name + " is the lucky one.");
        }

        // When called, resets all of the shells to their normal states
        public void ResetShells()
        {
            foreach(InteractShell currentShell in shellList)
            {
                if(currentShell.isWinner == true)
                {
                    currentShell.isWinner = false;
                    print(currentShell.name + " was reset back to normal.");
                    break;
                }
            }
        }
    }
}

