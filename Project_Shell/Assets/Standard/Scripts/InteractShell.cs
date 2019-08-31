/*  This defines how an object interacts with the mouse
 * 
 *  Documentation: http://luminaryapps.com/blog/arcing-projectiles-in-unity/
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    public class InteractShell : MonoBehaviour {

        // Static Variables
        public static List<InteractShell> shellList;    // All of the shells know how many shells are there in the game

        private static float origMoveSpeed;              // The orignal move speed of all of the shells
        private static float origArcHeight;              // The orignal arc height of all the shells
        private static float spotLightMaxIntensity = 4;  // The max spot light intensity this goes up to
        private static Light sceneLight;                 // A reference to the scene lighting
        private static float origSceneLightIntensity;    // The original scene lighting intensity

        [Header("General Variables")]

        [Tooltip("How fast does this shell move while being shuffled?")]
        [Range(1f,100f)]
        public float moveSpeed = 1f;                    // How fast does this object move?

        [Tooltip("How high is the arc on this shell's swap? Negative numbers make the shell move below the line!")]
        [Range(-30f,30f)]
        public float arcHeight = 1f;                    // How high is the arch on this object's movement?

        [Header("Visual Variables")]
        public Animator animator;                       // Reference to the chest animations
        public Shader outlineShader;                    // The shader to use when this object is selected
        [Range(0.001f,1f)]
        public float lightTransitionSpeed = 0.1f;       // How fast did the light change?

        [Header("External References")]
        public Light spotLight;                         // Reference to the spot light the shell has
        public GameObject goldPile;                     // Reference to the gold pile the shell has

        // Private Variables
        private SkinnedMeshRenderer[] objRenders;       // Reference to the object renders the shell has
        private Shader origShader;                      // Reference to the shader the shell originaly used

        private bool isWinner = false;                  // This indicates that this shell is the lucky one
        private bool isHovering;                        // Is the player currently selecting this?
        private bool isMoving;                          // Is the object currently moving?
        private Vector3 newLocation;                    // The new destination to move this object towards
        private Vector3 origLocation;                   // The initial location of this object
        private Vector3 startMoveLocation;              // The current location this object is at before moving

        // Getters
        public bool IsMoving {
            get {return isMoving;}
        }
        public bool IsWinner {
            get { return isWinner;}
        }

        // Adds this shell to the list of shells in the game
        // Also sets up the static variables
		private void Awake()
		{
            if(shellList == null)
            {
                shellList = new List<InteractShell>();
                origMoveSpeed = moveSpeed;
                origArcHeight = arcHeight;
            }

            if(sceneLight == null)
            {
                sceneLight = GameObject.FindWithTag("MainLight").GetComponent<Light>();
                origSceneLightIntensity = sceneLight.intensity;
            }

            shellList.Add(this);
		}

        // Sets all of the variables to defaults
		private void Start()
		{
            objRenders = gameObject.transform.GetChild(0).GetComponentsInChildren<SkinnedMeshRenderer>();

            origShader = objRenders[0].material.shader;
            origLocation = gameObject.transform.position;
            newLocation = Vector3.zero;
            startMoveLocation = Vector3.zero;
            isMoving = false;

            goldPile.SetActive(false);
		}

        // Handles movement of the objects
		private void FixedUpdate()
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
                isHovering = false;
                StartCoroutine(GameManager.Instance.ConcludeGame(this));
            }
		}

        // We are hovering over said object
		private void OnMouseOver()
		{
            if(GameManager.Instance.GetCurrentState == GameState.SELECTING)
            {
                if(isHovering == false)
                {
                    HighlightShell();
                    isHovering = true;
                }
            }		
        }

		// We left the mouse from this object
		private void OnMouseExit()
		{
            if(GameManager.Instance.GetCurrentState == GameState.SELECTING)
            {
                if(isHovering == true)
                {
                    DehighlightShell();
                    isHovering = false;
                }
            }		
        }

        // Handles checking if two floats are equal. Returns false if they aren't equal
        private bool FloatEquality(float f1, float f2)
        {
            if(Mathf.Abs(f1 - f2) < 0.1f)
            {
                return false;
            }
            else
            {
                return true;
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
    
        // Makes this shell the lucky shell
        public void MarkAsLucky()
        {
            if(isWinner == false)
            {
                isWinner = true;
                goldPile.SetActive(true);
            }
        }

        // Reverts this shell back to its default values
        public void ResetShell()
        {
            gameObject.transform.position = origLocation;
            DehighlightShell();
            isWinner = false;
            goldPile.SetActive(false);

            // If we lost the game, we also reset its move speeds
            if(GameManager.Instance.GetCurrentState == GameState.LOSE)
            {
                moveSpeed = origMoveSpeed;
                arcHeight = origArcHeight;
            }
        }
    
        // Toggles to highlight the shell
        public void HighlightShell()
        {
            foreach(SkinnedMeshRenderer currRender in objRenders)
            {
                if(currRender.material.shader == origShader)
                {
                    currRender.material.shader = outlineShader;
                }
            }
        }

        // Toggels to dehighlight the shell
        public void DehighlightShell()
        {
            foreach(SkinnedMeshRenderer currRender in objRenders)
            {
                if(currRender.material.shader == outlineShader)
                {
                    currRender.material.shader = origShader;
                }
            }
        }
    
        // Starts the open chest animation
        public bool AnimateOpenChest()
        {
            if(animator.GetBool("isOpen") == false)
            {
                animator.SetBool("isOpen", true);
            }
            spotLight.intensity = Mathf.Lerp(spotLight.intensity, spotLightMaxIntensity, lightTransitionSpeed);
            sceneLight.intensity = Mathf.Lerp(sceneLight.intensity, 0f, lightTransitionSpeed);

            if(FloatEquality(spotLight.intensity, spotLightMaxIntensity) == false)
            {
                sceneLight.intensity = 0f;
                spotLight.intensity = spotLightMaxIntensity;
                return true;
            }
            return false;
        }

        // Starts the close chest animation
        public bool AnimateCloseChest()
        {
            if(animator.GetBool("isOpen") == true)
            {
                animator.SetBool("isOpen", false);
            }
            spotLight.intensity = Mathf.Lerp(spotLight.intensity, 0f, lightTransitionSpeed);
            sceneLight.intensity = Mathf.Lerp(sceneLight.intensity, origSceneLightIntensity, lightTransitionSpeed);

            if(FloatEquality(sceneLight.intensity, origSceneLightIntensity) == false)
            {
                spotLight.intensity = 0f;
                sceneLight.intensity = origSceneLightIntensity;
                return true;
            }
            return false;
        }
    }
}