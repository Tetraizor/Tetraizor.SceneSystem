using System.Collections;
using System;

using Tetraizor.MonoSingleton;
using Tetraizor.SceneSystem.Utils;
using Tetraizor.Bootstrap.Base;
using Tetraizor.Bootstrap;
using Tetraizor.DebugUtils;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetraizor.SceneSystem
{
    #region Delegates
    public delegate void SceneLoadHandler(object sender, SceneLoadEventArgs e);
    public class SceneLoadEventArgs : EventArgs
    {
        public int SceneIndex { get; set; }
    }

    public delegate void SceneLoadProgressHandler(object sender, SceneLoadProgressEventArgs e);
    public class SceneLoadProgressEventArgs : EventArgs
    {
        public int SceneIndex { get; set; }
        public float Progress { get; set; }
    }
    #endregion

    public class SceneLoadingSystem : MonoSingleton<SceneLoadingSystem>, IPersistentSystem
    {
        #region Properties

        [Header("Scene References")]
        [SerializeField]
        private int _firstSceneIndex = 2;

        private Scene _currentScene;

        [Header("Events")]
        public SceneLoadProgressHandler SceneLoadStateChanged;

        public SceneLoadHandler SceneUnloadCompleted;
        public SceneLoadHandler SceneUnloadStarted;

        public SceneLoadHandler SceneLoadCompleted;
        public SceneLoadHandler SceneLoadStarted;

        #endregion

        #region Scene Management Methods

        public void SwitchScene(int sceneIndex)
        {
            StartCoroutine(SwitchSceneAsync(sceneIndex));
        }

        public IEnumerator SwitchSceneAsync(int sceneIndex)
        {
            SceneUnloadStarted?.Invoke(this, new SceneLoadEventArgs { SceneIndex = _currentScene.buildIndex });
            SceneLoadStarted?.Invoke(this, new SceneLoadEventArgs { SceneIndex = sceneIndex });

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentScene);

            loadOperation.allowSceneActivation = false;
            print("Test1");

            while (loadOperation.progress < .89f)
            {
                SceneLoadStateChanged?.Invoke(this, new SceneLoadProgressEventArgs { SceneIndex = sceneIndex, Progress = loadOperation.progress });
                yield return null;
            }
            print("Test2");

            loadOperation.allowSceneActivation = true;

            while (!loadOperation.isDone)
            {
                SceneLoadStateChanged?.Invoke(this, new SceneLoadProgressEventArgs { SceneIndex = sceneIndex, Progress = loadOperation.progress });
                yield return null;
            }

            print("Test3");

            _currentScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            SceneManager.SetActiveScene(_currentScene);

            SceneUnloadCompleted?.Invoke(this, new SceneLoadEventArgs { SceneIndex = _currentScene.buildIndex });
            SceneLoadCompleted?.Invoke(this, new SceneLoadEventArgs { SceneIndex = sceneIndex });

            DebugBus.LogPrint($"Finished loading scene {SceneManager.GetSceneByBuildIndex(sceneIndex).name}.");
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