using Squidbasket.Input;
using Squidbasket.Player;
using Squidbasket.Scoring;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Squidbasket.EditorTools
{
    /// <summary>
    /// One-off headless re-wiring for the IPlayerInputSource removal: points
    /// <see cref="PlayerInputSource"/>'s new <see cref="InputActionReference"/> fields
    /// at the Player action map, and re-assigns the renamed `input` field on
    /// <see cref="PlayerStateController"/>/<see cref="RestartInputHandler"/> now that the old
    /// `inputSourceBehaviour` field name/type is gone. Invoke via: `unity run . --editor-version
    /// 6000.0.58f2 -- -executeMethod Squidbasket.EditorTools.InputSourceRefactorSceneWiring.Run
    /// -logFile -`. Safe to re-run.
    /// </summary>
    public static class InputSourceRefactorSceneWiring
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerMap = "Player";

        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var inputSource = Object.FindFirstObjectByType<PlayerInputSource>();
            var playerStateController = Object.FindFirstObjectByType<PlayerStateController>();
            var restartInputHandler = Object.FindFirstObjectByType<RestartInputHandler>();

            if (inputSource == null || playerStateController == null || restartInputHandler == null)
            {
                Debug.LogError($"{nameof(InputSourceRefactorSceneWiring)}: could not find " +
                                $"{nameof(PlayerInputSource)}/{nameof(PlayerStateController)}/" +
                                $"{nameof(RestartInputHandler)} in {ScenePath}. Aborting.");
                return;
            }

            var inputSourceSerialized = new SerializedObject(inputSource);
            AssignActionReference(inputSourceSerialized, "moveAction", "Move");
            AssignActionReference(inputSourceSerialized, "lookAction", "Look");
            AssignActionReference(inputSourceSerialized, "shootAction", "Shoot");
            AssignActionReference(inputSourceSerialized, "resetAction", "Reset");
            AssignActionReference(inputSourceSerialized, "restartAction", "Restart");
            inputSourceSerialized.ApplyModifiedPropertiesWithoutUndo();

            var playerStateSerialized = new SerializedObject(playerStateController);
            playerStateSerialized.FindProperty("input").objectReferenceValue = inputSource;
            playerStateSerialized.ApplyModifiedPropertiesWithoutUndo();

            var restartSerialized = new SerializedObject(restartInputHandler);
            restartSerialized.FindProperty("input").objectReferenceValue = inputSource;
            restartSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(InputSourceRefactorSceneWiring)}: re-wired {ScenePath} for the direct " +
                      $"{nameof(PlayerInputSource)} references.");
        }

        private static void AssignActionReference(SerializedObject target, string fieldName, string actionName)
        {
            InputActionReference reference = FindActionReference(actionName);
            if (reference == null)
            {
                Debug.LogError($"{nameof(InputSourceRefactorSceneWiring)}: could not find an " +
                                $"{nameof(InputActionReference)} for action '{actionName}' in {InputActionsPath}.");
                return;
            }

            target.FindProperty(fieldName).objectReferenceValue = reference;
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
