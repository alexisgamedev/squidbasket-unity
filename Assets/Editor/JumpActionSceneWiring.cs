using Squidbasket.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Squidbasket.EditorTools
{
    /// <summary>
    /// One-off headless wiring for the new Jump mechanic: points
    /// <see cref="PlayerInputSource"/>'s new `jumpAction` field at the Player map's already-bound
    /// Jump action. Invoke via: `unity run . --editor-version 6000.0.58f2 -- -executeMethod
    /// Squidbasket.EditorTools.JumpActionSceneWiring.Run -logFile -`. Safe to re-run.
    /// </summary>
    public static class JumpActionSceneWiring
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerMap = "Player";

        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var inputSource = Object.FindFirstObjectByType<PlayerInputSource>();
            if (inputSource == null)
            {
                Debug.LogError($"{nameof(JumpActionSceneWiring)}: could not find {nameof(PlayerInputSource)} " +
                                $"in {ScenePath}. Aborting.");
                return;
            }

            InputActionReference reference = FindActionReference("Jump");
            if (reference == null)
            {
                Debug.LogError($"{nameof(JumpActionSceneWiring)}: could not find an " +
                                $"{nameof(InputActionReference)} for action 'Jump' in {InputActionsPath}.");
                return;
            }

            var serialized = new SerializedObject(inputSource);
            serialized.FindProperty("jumpAction").objectReferenceValue = reference;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(JumpActionSceneWiring)}: wired jumpAction on {nameof(PlayerInputSource)} in {ScenePath}.");
        }

        private static InputActionReference FindActionReference(string actionName)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(InputActionsPath))
            {
                if (asset is InputActionReference reference &&
                    reference.action.name == actionName &&
                    reference.action.actionMap.name == PlayerMap)
                {
                    return reference;
                }
            }

            return null;
        }
    }
}
