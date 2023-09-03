using System.Collections;
using Tetraizor.MonoSingleton;
using Tetraizor.SceneSystem.Utils;
using Tetraizor.Bootstrap.Base;
using Tetraizor.Bootstrap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Tetraizor.SceneSystem
{
    public class SceneSystem : MonoSingleton<SceneSystem>, IPersistentSystem
    {
        #region Properties

        [Header("Scene References")]
        [SerializeField]
        private int _firstSceneIndex = 2;

        private Scene _currentScene;
        [HideInInspector] public UnityEvent<float> SceneLoadStateChangeEvent = new UnityEvent<float>();

        #endregion

        #region Scene Management Methods

        public void SwitchScene(int sceneIndex)
        {
            StartCoroutine(SwitchSceneAsync(sceneIndex));
        }

        public IEnumerator SwitchSceneAsync(int sceneIndex)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentScene);

            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < .89f)
            {
                SceneLoadStateChangeEvent?.Invoke(loadOperation.progress);
                yield return null;
            }

            loadOperation.allowSceneActivation = true;

            while (!loadOperation.isDone)
            {
                SceneLoadStateChangeEvent?.Invoke(loadOperation.progress);
                yield return null;
            }

            _currentScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            SceneManager.SetActiveScene(_currentScene);
        }

        #endregion

        #region Event Callbacks

        private void OnSystemLoadingComplete()
        {
            _currentScene = SceneManager.GetActiveScene();

            // Check if game is not started from Bootstrapper scene.
            var autoSceneChanger = FindObjectOfType<AutoSceneChanger>();

            if (autoSceneChanger == null)
            {
                SwitchScene(_firstSceneIndex);
            }
            else
            {
                DestroyImmediate(autoSceneChanger.gameObject);
                SwitchScene(autoSceneChanger.SceneToAutoLoad);
            }
        }

        #endregion

        #region IPersistentSystem Methods

        public IEnumerator LoadSystem()
        {
            Bootstrapper systemLoader = Bootstrapper.Instance;

            systemLoader.BootCompleteEvent.AddListener(OnSystemLoadingComplete);

            yield return null;
        }

        public IEnumerator UnloadSystem()
        {
            yield return null;
        }

        public string GetName()
        {
            return "Scene Loader Manager";
        }

        #endregion
    }
}