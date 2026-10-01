using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicalMouseDemo.Editor
{
    [InitializeOnLoad]
    public static class DemoChecksMenu
    {
        static DemoChecksMenu() { EditorApplication.playModeStateChanged += OnState; }
        [MenuItem("Tools/Physical Mouse Demo/Run Play Mode Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="PhysicalMouseDemo")
            { Debug.LogWarning("Open PhysicalMouseDemo and leave Play mode before running checks.");return; }
            SessionState.SetBool("PhysicalMouseDemo.RunChecks",true);EditorApplication.EnterPlaymode();
        }
        private static void OnState(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("PhysicalMouseDemo.RunChecks",false))return;
            SessionState.SetBool("PhysicalMouseDemo.RunChecks",false);
            new GameObject("AutomatedDemoChecks").AddComponent<DemoPlayChecks>();
        }
    }
}
