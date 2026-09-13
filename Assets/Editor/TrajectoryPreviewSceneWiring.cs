using Squidbasket.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Squidbasket.EditorTools
{
    /// <summary>
    /// Headless, idempotent wiring for the Shot trajectory preview: adds
    /// <see cref="TrajectoryPreview"/> to the scene's existing "LineRenderer" GameObject and
    /// points <see cref="PlayerStateController"/>'s trajectoryPreview field at it. Invoke via:
    /// `unity run . --editor-version 6000.0.58f2 -- -executeMethod
    /// Squidbasket.EditorTools.TrajectoryPreviewSceneWiring.Run -logFile -`. Safe to re-run.
    /// </summary>
    public static class TrajectoryPreviewSceneWiring
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";
        private const string LineRendererObjectName = "LineRenderer";

        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject lineRendererGo = GameObject.Find(LineRendererObjectName);
            if (lineRendererGo == null)
            {
                Debug.LogError($"{nameof(TrajectoryPreviewSceneWiring)}: could not find a GameObject named " +
                                $"'{LineRendererObjectName}' in {ScenePath}. Aborting.");
                return;
            }

            var playerStateController = Object.FindFirstObjectByType<PlayerStateController>();
            if (playerStateController == null)
            {
                Debug.LogError($"{nameof(TrajectoryPreviewSceneWiring)}: could not find a " +
                                $"{nameof(PlayerStateController)} in {ScenePath}. Aborting.");
                return;
            }

            var preview = lineRendererGo.GetComponent<TrajectoryPreview>();
            if (preview == null)
            {
                preview = lineRendererGo.AddComponent<TrajectoryPreview>();
            }

            var controllerSerialized = new SerializedObject(playerStateController);
            controllerSerialized.FindProperty("trajectoryPreview").objectReferenceValue = preview;
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(TrajectoryPreviewSceneWiring)}: wired {nameof(TrajectoryPreview)} onto " +
                      $"'{LineRendererObjectName}' and assigned it on {nameof(PlayerStateController)} in {ScenePath}.");
        }
    }
}
