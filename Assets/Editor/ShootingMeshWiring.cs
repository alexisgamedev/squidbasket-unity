using Squidbasket.Player;
using UnityEditor;
using UnityEngine;

namespace Squidbasket.EditorTools
{
    /// <summary>
    /// Headless, idempotent wiring for hiding the player's own body/eyes mesh during Shooting:
    /// points <see cref="PlayerStateController"/>'s bodyMeshRenderer/eyesMeshRenderer fields at
    /// the "BodyMesh"/"EyesMesh" children already present on the Player prefab. Unlike the
    /// scene-object wiring scripts in this folder, this edits the prefab asset directly since
    /// BodyMesh/EyesMesh live inside the same prefab as PlayerStateController. Invoke via:
    /// `unity run . --editor-version 6000.0.58f2 -- -executeMethod
    /// Squidbasket.EditorTools.ShootingMeshWiring.Run -logFile -`. Safe to re-run.
    /// </summary>
    public static class ShootingMeshWiring
    {
        private const string PrefabPath = "Assets/Content/PF_Player.prefab";
        private const string BodyMeshObjectName = "BodyMesh";
        private const string EyesMeshObjectName = "EyesMesh";

        public static void Run()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                var playerStateController = prefabRoot.GetComponent<PlayerStateController>();
                if (playerStateController == null)
                {
                    Debug.LogError($"{nameof(ShootingMeshWiring)}: could not find a " +
                                    $"{nameof(PlayerStateController)} on {PrefabPath}. Aborting.");
                    return;
                }

                Renderer bodyMeshRenderer = FindRenderer(prefabRoot.transform, BodyMeshObjectName);
                Renderer eyesMeshRenderer = FindRenderer(prefabRoot.transform, EyesMeshObjectName);

                if (bodyMeshRenderer == null || eyesMeshRenderer == null)
                {
                    Debug.LogError($"{nameof(ShootingMeshWiring)}: could not find a Renderer on " +
                                    $"'{BodyMeshObjectName}' and/or '{EyesMeshObjectName}' under {PrefabPath}. " +
                                    "Aborting.");
                    return;
                }

                var controllerSerialized = new SerializedObject(playerStateController);
                controllerSerialized.FindProperty("bodyMeshRenderer").objectReferenceValue = bodyMeshRenderer;
                controllerSerialized.FindProperty("eyesMeshRenderer").objectReferenceValue = eyesMeshRenderer;
                controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                Debug.Log($"{nameof(ShootingMeshWiring)}: wired {BodyMeshObjectName}/{EyesMeshObjectName} " +
                          $"renderers onto {nameof(PlayerStateController)} in {PrefabPath}.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        // BodyMesh/EyesMesh sit at different depths under the prefab root (EyesMesh nests under
        // an "Eyes" parent), so this searches all descendants by name rather than assuming a path.
        private static Renderer FindRenderer(Transform root, string childName)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == childName)
                {
                    return renderer;
                }
            }

            return null;
        }
    }
}
