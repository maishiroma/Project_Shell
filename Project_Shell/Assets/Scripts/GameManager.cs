/*  This keeps track of all global game items
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MattScripts {

    public class GameManager : MonoBehaviour {

        public static GameManager Instance;

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
    }
}
