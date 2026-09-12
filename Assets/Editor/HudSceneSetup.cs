using Squidbasket.Input;
using Squidbasket.Scoring;
using Squidbasket.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Squidbasket.EditorTools
{
    /// <summary>
    /// Headless, idempotent scene wiring for the Score/Streak HUD and Restart action (issues #2,
    /// #3). Builds a screen-space Canvas (Score text, Streak text, Restart button) and a
    /// standalone Restart-key handler, driven from the command line since no interactive Editor
    /// session is available here: `unity run . --editor-version 6000.0.58f2 --
    /// -executeMethod Squidbasket.EditorTools.HudSceneSetup.Run -logFile -`. Safe to re-run —
    /// it's a no-op if the HUD already exists in the scene.
    /// </summary>
    public static class HudSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";

        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (Object.FindFirstObjectByType<Canvas>() != null)
            {
                Debug.Log($"{nameof(HudSceneSetup)}: a Canvas already exists in the scene — skipping (already wired).");
                return;
            }

            var scoreSystem = Object.FindFirstObjectByType<ScoreSystem>();
            var inputSource = Object.FindFirstObjectByType<PlayerInputSource>();

            if (scoreSystem == null || inputSource == null)
            {
                Debug.LogError($"{nameof(HudSceneSetup)}: could not find a {nameof(ScoreSystem)} and/or " +
                                $"{nameof(PlayerInputSource)} in {ScenePath}. Aborting.");
                return;
            }

            EnsureEventSystem();

            Canvas canvas = CreateCanvas();
            Text scoreText = CreateText(canvas.transform, "ScoreText", "Score: 0",
                new Vector2(20, -20), new Vector2(220, 40));
            Text streakText = CreateText(canvas.transform, "StreakText", "Streak: 0",
                new Vector2(20, -60), new Vector2(220, 40));
            CreateRestartButton(canvas.transform, scoreSystem);

            var hudGo = new GameObject("Score Hud");
            var hud = hudGo.AddComponent<ScoreHud>();
            var hudSerialized = new SerializedObject(hud);
            hudSerialized.FindProperty("scoreSystem").objectReferenceValue = scoreSystem;
            hudSerialized.FindProperty("scoreText").objectReferenceValue = scoreText;
            hudSerialized.FindProperty("streakText").objectReferenceValue = streakText;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(HudSceneSetup)}: HUD, Restart button, and Restart key handler wired into {ScenePath}.");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            return canvas;
        }

        private static Text CreateText(Transform parent, string name, string initialText, Vector2 topLeftOffset, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = topLeftOffset;
            rect.sizeDelta = size;

            var text = go.GetComponent<Text>();
            text.text = initialText;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;

            return text;
        }

        private static void CreateRestartButton(Transform parent, ScoreSystem scoreSystem)
        {
            var go = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -110);
            rect.sizeDelta = new Vector2(140, 40);

            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<Text>();
            label.text = "Restart";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;

            var button = go.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, scoreSystem.Restart);
        }
    }
}
