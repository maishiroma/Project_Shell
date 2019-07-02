/*  This defines how an object interacts with the mouse
 * 
 *  Documentation: http://luminaryapps.com/blog/arcing-projectiles-in-unity/
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    public class InteractShell : MonoBehaviour {

        public static List<InteractShell> shellList;    // All of the shells know how many shells are there in the game

        public bool isWinner = false;                   // This indicates that this shell is the lucky one
        public float moveSpeed = 1f;                    // How fast does this object move?
        public float arcHeight = 1f;                    // How high is the arch on this object's movement?

        // Private Variables
        private bool isMoving;                          // IS the object currently moving?
        private Vector3 newLocation;                    // The new destination to move this object towards
        private Vector3 origLocation;                   // The initial location of this object
        private Vector3 startMoveLocation;              // The current location this object is at before moving

        public bool IsMoving {
            get {return isMoving;}
        }

        public Vector3 GetOrigLocation {
            get { return origLocation; }
        }

        // Adds this shell to the list of shells in the game
		private void Awake()
		{
            if(shellList == null)
            {
                shellList = new List<InteractShell>();
            }

            shellList.Add(this);
		}

        // Sets all of the variables to defaults
		private void Start()
		{
            origLocation = gameObject.transform.position;
            newLocation = Vector3.zero;
            startMoveLocation = Vector3.zero;
            isMoving = false;
		}

        // Handles movement of the objects
		private void Update()
		{
            if(isMoving == true)
            {
                // This logic calculates the movement of the shell in a curve
                float xDiff = newLocation.x - startMoveLocation.x;
                float nextXPos = Mathf.MoveTowards(transform.position.x, newLocation.x, moveSpeed * Time.deltaTime);

                // For these next two calculations, if we get a NaN, we set the value to 0
                float newYPos = Mathf.Lerp(startMoveLocation.y, newLocation.y, (nextXPos - startMoveLocation.x) / xDiff);
                if(float.IsNaN(newYPos))
                {
                    newYPos = 0;
                }

                float arc = arcHeight * (nextXPos - startMoveLocation.x) * (nextXPos - newLocation.x) / (-0.25f * xDiff * xDiff);
                if(float.IsNaN(arc))
                {
                    arc = 0;
                }

                // We then move the object to this position
                gameObject.transform.position = new Vector3(nextXPos, newYPos + arc, 0);
                if(gameObject.transform.position == newLocation)
                {
                    // Once we hit the new location, we stop moving
                    isMoving = false;
                }
            }
		}

		// We have selected this object
		private void OnMouseUp()
		{
            if(GameManager.Instance.currentState == GameState.SELECTING)
            {
                if(isWinner == true)
                {
                    Debug.Log("You win!");
                }
                else
                {
                    Debug.Log("You lose...");
                }
                GameManager.Instance.ResetGame();
            }
		}

        // We are hovering over said object
		private void OnMouseEnter()
		{
            if(GameManager.Instance.currentState == GameState.SELECTING)
            {
                Debug.Log("We selected " + name);
            }		
        }

        // We left the mouse from this object
		private void OnMouseExit()
		{
            if(GameManager.Instance.currentState == GameState.SELECTING)
            {
                Debug.Log("We selected " + name);
            }		
        }
	
        // Swaps this and the other shell's location
        public void SwapShellLocation(InteractShell otherShell)
        {
            if(isMoving == false && otherShell.isMoving == false)
            {
                startMoveLocation = gameObject.transform.position;
                otherShell.startMoveLocation = otherShell.transform.position;

                newLocation = otherShell.startMoveLocation;
                otherShell.newLocation = startMoveLocation;

                isMoving = true;
                otherShell.isMoving = true;
            }
        }
    }
}

