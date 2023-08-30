using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Tetraizor.SceneSystem.Utils
{
    [DefaultExecutionOrder(-1)]
    public class AutoSceneChanger : MonoBehaviour
    {
        [HideInInspector]
        public int SceneToAutoLoad = 0;

        private void Awake()
        {
            if (SceneSystem.Instance == null)
            {
                // Set DontDestroyOnLoad for SceneSystem to understand there is an AutoScene operation.
                DontDestroyOnLoad(this);

                SceneToAutoLoad = SceneManager.GetActiveScene().buildIndex;

                // Get back to Bootstrapper scene.
                SceneManager.LoadScene(0);
                
                Debug.Log("Systems are not loaded. Reloading scene from boot.");
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}