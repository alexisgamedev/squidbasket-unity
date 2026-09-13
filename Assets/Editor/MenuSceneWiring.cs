using Squidbasket.Camera;
using Squidbasket.Input;
using Squidbasket.Player;
using Squidbasket.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Squidbasket.EditorTools
{
    /// <summary>
    /// Headless, idempotent wiring for the Esc pause menu: builds the Sensitivity slider,
    /// Trajectory-preview toggle and Quit button under "Menu Canvas"/"Panel" (the Restart button
    /// already exists there, wired to ScoreSystem.Restart), then points MenuWindow's fields at
    /// the "Menu" input action and the Restart button. Invoke via: `unity run . --editor-version
    /// 6000.0.58f2 -- -executeMethod Squidbasket.EditorTools.MenuSceneWiring.Run -logFile -`.
    /// Safe to re-run.
    /// </summary>
    public static class MenuSceneWiring
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerMap = "Player";
        private const string MenuCanvasName = "Menu Canvas";
        private const string PanelName = "Panel";
        private const string RestartButtonName = "RestartButton";

        public static void Run()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject menuCanvasGo = GameObject.Find(MenuCanvasName);
            if (menuCanvasGo == null)
            {
                Debug.LogError($"{nameof(MenuSceneWiring)}: could not find '{MenuCanvasName}' in {ScenePath}. Aborting.");
                return;
            }

            var menuWindow = menuCanvasGo.GetComponent<MenuWindow>();
            if (menuWindow == null)
            {
                Debug.LogError($"{nameof(MenuSceneWiring)}: '{MenuCanvasName}' has no {nameof(MenuWindow)}. Aborting.");
                return;
            }

            Transform panel = menuCanvasGo.transform.Find(PanelName);
            if (panel == null)
            {
                Debug.LogError($"{nameof(MenuSceneWiring)}: could not find '{PanelName}' under '{MenuCanvasName}'. Aborting.");
                return;
            }

            Transform restartButtonT = panel.Find(RestartButtonName);
            if (restartButtonT == null)
            {
                Debug.LogError($"{nameof(MenuSceneWiring)}: could not find '{RestartButtonName}' under '{PanelName}'. Aborting.");
                return;
            }

            var thirdPerson = Object.FindFirstObjectByType<ThirdPersonCamera>();
            var firstPerson = Object.FindFirstObjectByType<FirstPersonCamera>();
            var trajectoryPreview = Object.FindFirstObjectByType<TrajectoryPreview>();
            var playerInput = Object.FindFirstObjectByType<PlayerInputSource>();

            if (thirdPerson == null || firstPerson == null || trajectoryPreview == null || playerInput == null)
            {
                Debug.LogError($"{nameof(MenuSceneWiring)}: could not find " +
                                $"{nameof(ThirdPersonCamera)}/{nameof(FirstPersonCamera)}/{nameof(TrajectoryPreview)}/" +
                                $"{nameof(PlayerInputSource)} in {ScenePath}. Aborting.");
                return;
            }

            DefaultControls.Resources resources = GetDefaultResources();

            ((RectTransform)restartButtonT).anchoredPosition = new Vector2(0, 180);

            Slider slider = FindOrCreateSlider(panel, resources);
            Toggle toggle = FindOrCreateToggle(panel, resources);
            Button quitButton = FindOrCreateQuitButton(panel, resources);

            AddDynamicPersistentListener(slider, "m_OnValueChanged", thirdPerson, nameof(ThirdPersonCamera.SetSensitivity));
            AddDynamicPersistentListener(slider, "m_OnValueChanged", firstPerson, nameof(FirstPersonCamera.SetSensitivity));
            // Unlike Slider/Button, Toggle exposes onValueChanged as a plain public field (no
            // m_ prefix) rather than a private-field-plus-property pair.
            AddDynamicPersistentListener(toggle, "onValueChanged", trajectoryPreview, nameof(TrajectoryPreview.SetPreviewEnabled));
            AddDynamicPersistentListener(quitButton, "m_OnClick", menuWindow, nameof(MenuWindow.Quit));

            InputActionReference menuActionRef = FindActionReference("Menu");
            if (menuActionRef == null)
            {
                Debug.LogError($"{nameof(MenuSceneWiring)}: could not find an {nameof(InputActionReference)} " +
                                $"for action 'Menu' in {InputActionsPath}.");
                return;
            }

            var menuWindowSerialized = new SerializedObject(menuWindow);
            menuWindowSerialized.FindProperty("toggleMenuAction").objectReferenceValue = menuActionRef;
            menuWindowSerialized.FindProperty("firstSelectable").objectReferenceValue = restartButtonT.GetComponent<Button>();
            menuWindowSerialized.FindProperty("playerInput").objectReferenceValue = playerInput;
            menuWindowSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(MenuSceneWiring)}: built and wired the pause menu in {ScenePath}.");
        }

        private static Slider FindOrCreateSlider(Transform panel, DefaultControls.Resources resources)
        {
            Transform existingSlider = panel.Find("SensitivitySlider");
            if (existingSlider != null)
            {
                return existingSlider.GetComponent<Slider>();
            }

            if (panel.Find("SensitivityLabel") == null)
            {
                GameObject labelGo = DefaultControls.CreateText(resources);
                labelGo.name = "SensitivityLabel";
                labelGo.transform.SetParent(panel, false);
                var labelRect = (RectTransform)labelGo.transform;
                labelRect.sizeDelta = new Vector2(280, 24);
                labelRect.anchoredPosition = new Vector2(0, 100);
                var labelText = labelGo.GetComponent<Text>();
                labelText.text = "Camera Sensitivity";
                labelText.alignment = TextAnchor.MiddleCenter;
            }

            GameObject sliderGo = DefaultControls.CreateSlider(resources);
            sliderGo.name = "SensitivitySlider";
            sliderGo.transform.SetParent(panel, false);
            var sliderRect = (RectTransform)sliderGo.transform;
            sliderRect.sizeDelta = new Vector2(280, 20);
            sliderRect.anchoredPosition = new Vector2(0, 60);

            var slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0.5f;
            slider.maxValue = 5f;
            slider.wholeNumbers = false;
            slider.value = 1.5f;
            return slider;
        }

        private static Toggle FindOrCreateToggle(Transform panel, DefaultControls.Resources resources)
        {
            Transform existing = panel.Find("TrajectoryToggle");
            if (existing != null)
            {
                return existing.GetComponent<Toggle>();
            }

            GameObject toggleGo = DefaultControls.CreateToggle(resources);
            toggleGo.name = "TrajectoryToggle";
            toggleGo.transform.SetParent(panel, false);
            var toggleRect = (RectTransform)toggleGo.transform;
            toggleRect.sizeDelta = new Vector2(280, 32);
            toggleRect.anchoredPosition = new Vector2(0, -20);

            var toggle = toggleGo.GetComponent<Toggle>();
            toggle.isOn = true;

            Transform labelT = toggleGo.transform.Find("Label");
            var labelText = labelT != null ? labelT.GetComponent<Text>() : null;
            if (labelText != null)
            {
                labelText.text = "Show Trajectory Preview";
            }

            return toggle;
        }

        private static Button FindOrCreateQuitButton(Transform panel, DefaultControls.Resources resources)
        {
            Transform existing = panel.Find("QuitButton");
            if (existing != null)
            {
                return existing.GetComponent<Button>();
            }

            GameObject buttonGo = DefaultControls.CreateButton(resources);
            buttonGo.name = "QuitButton";
            buttonGo.transform.SetParent(panel, false);
            var buttonRect = (RectTransform)buttonGo.transform;
            buttonRect.sizeDelta = new Vector2(280, 64);
            buttonRect.anchoredPosition = new Vector2(0, -100);

            Text buttonText = buttonGo.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "Quit";
            }

            return buttonGo.GetComponent<Button>();
        }

        // UnityEditor.Events.UnityEventTools only offers static-argument persistent listeners
        // (Add{Int,Float,Bool}PersistentListener), which would freeze the target at whatever
        // constant we wired here. A dynamic listener that forwards the event's own runtime
        // argument (what dragging a component into the Inspector slot produces — see
        // RestartButton's existing onClick -> ScoreSystem.Restart wiring, PersistentListenerMode
        // 0/EventDefined) has no public API, so it's built directly via SerializedProperty,
        // matching that same on-disk shape.
        private static void AddDynamicPersistentListener(Object component, string eventFieldName, Object target, string methodName)
        {
            var serializedComponent = new SerializedObject(component);
            SerializedProperty calls = serializedComponent.FindProperty(eventFieldName)
                .FindPropertyRelative("m_PersistentCalls").FindPropertyRelative("m_Calls");

            for (int i = 0; i < calls.arraySize; i++)
            {
                SerializedProperty existingCall = calls.GetArrayElementAtIndex(i);
                if (existingCall.FindPropertyRelative("m_Target").objectReferenceValue == target &&
                    existingCall.FindPropertyRelative("m_MethodName").stringValue == methodName)
                {
                    return;
                }
            }

            int index = calls.arraySize;
            calls.InsertArrayElementAtIndex(index);
            SerializedProperty call = calls.GetArrayElementAtIndex(index);
            call.FindPropertyRelative("m_Target").objectReferenceValue = target;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue =
                $"{target.GetType().FullName}, {target.GetType().Assembly.GetName().Name}";
            call.FindPropertyRelative("m_MethodName").stringValue = methodName;
            call.FindPropertyRelative("m_Mode").intValue = 0; // PersistentListenerMode.EventDefined
            call.FindPropertyRelative("m_CallState").intValue = 2; // UnityEventCallState.RuntimeOnly

            serializedComponent.ApplyModifiedPropertiesWithoutUndo();
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

        private static DefaultControls.Resources GetDefaultResources()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
                dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
                mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd"),
            };
        }
    }
}
