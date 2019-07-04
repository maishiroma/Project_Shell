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
        public static float origMoveSpeed;              // The orignal move speed of all of the shells
        public static float origArcHeight;              // The orignal arc height of all the shells

        [Header("General Variables")]
        public bool isWinner = false;                   // This indicates that this shell is the lucky one

        [Tooltip("How fast does this shell move while being shuffled?")]
        [Range(1f,100f)]
        public float moveSpeed = 1f;                    // How fast does this object move?

        [Tooltip("How high is the arc on this shell's swap? Negative numbers make the shell move below the line!")]
        [Range(-5f,5f)]
        public float arcHeight = 1f;                    // How high is the arch on this object's movement?

        [Header("Visual Variables")]
        public Material selectedColor;                  // The color to use when this object is selected

        // Private Variables
        private MeshRenderer objRender;

        private bool isMoving;                          // IS the object currently moving?
        private Vector3 newLocation;                    // The new destination to move this object towards
        private Vector3 origLocation;                   // The initial location of this object
        private Vector3 startMoveLocation;              // The current location this object is at before moving
        private Material origColor;                     // The original color of the object

        // Getter
        public bool IsMoving {
            get {return isMoving;}
        }

        // Adds this shell to the list of shells in the game
		private void Awake()
		{
            if(shellList == null)
            {
                shellList = new List<InteractShell>();
                origMoveSpeed = moveSpeed;
                origArcHeight = arcHeight;
            }

            shellList.Add(this);
		}

        // Sets all of the variables to defaults
		private void Start()
		{
            objRender = gameObject.GetComponent<MeshRenderer>();

            origLocation = gameObject.transform.position;
            newLocation = Vector3.zero;
            startMoveLocation = Vector3.zero;
            origColor = objRender.material;
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
            if(GameManager.Instance.GetCurrentState == GameState.SELECTING)
            {
                // When we selected a shell, we show the answer and determine if the player has picked the right shell.
                StartCoroutine(GameManager.Instance.ShowLuckyShell());
                StartCoroutine(GameManager.Instance.ConcludeGame(this));
            }
		}

        // We are hovering over said object
		private void OnMouseEnter()
		{
            if(GameManager.Instance.GetCurrentState == GameState.SELECTING)
            {
                objRender.material = selectedColor;
            }		
        }

        // We left the mouse from this object
		private void OnMouseExit()
		{
            if(GameManager.Instance.GetCurrentState == GameState.SELECTING)
            {
                objRender.material = origColor;
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
    
        // Reverts this shell back to its default values
        public void ResetShell()
        {
            gameObject.transform.position = origLocation;
            objRender.material = origColor;
            isWinner = false;

            // If we lost the game, we also reset its move speeds
            if(GameManager.Instance.GetCurrentState == GameState.LOSE)
            {
                moveSpeed = origMoveSpeed;
                arcHeight = origArcHeight;
            }
        }
    
        // Toggles between the original color and the selected color. Used in an Coroutine to make a flash effect
        public void ToggleShellColors()
        {
            if(objRender.material == origColor)
            {
                objRender.material = selectedColor;
            }
            else
            {
                objRender.material = origColor;
            }
        }
    }
}