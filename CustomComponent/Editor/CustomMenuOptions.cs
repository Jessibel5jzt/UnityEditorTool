using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

	public class CustomMenuOptions : Editor
	{
		public const int GO_ID = 0;
		private const float kWidth = 500f;
		private const float kThickHeight = 150f;
		private const float kThinHeight = 100f;
		private const int fontSize = 64;

		#region TextPro
		private static readonly string fontsPath = "Assets/platform/Resources/default/Fonts/";
		private static readonly string defFont = "AlibabaPuHuiTi";
		private static readonly string deFontPath = $"{fontsPath}{defFont}/{defFont}-{FontMgr.FontWeight.Regular}.{FontMgr.FontEXT.otf}";
		
		[MenuItem("GameObject/FFTAIUI/TextPro", false, priority = GO_ID)]
		static void CreateTextPro(MenuCommand menuCommand)
		{
			GameObject go = TextProGO();
			PlaceUIElementRoot(go, menuCommand);
		}

		static GameObject TextProGO()
		{
			GameObject go = new GameObject("TextPro");
			var rt = go.AddComponent<RectTransform>();
			var tp = go.AddComponent<TextPro>();
			var ct = go.AddComponent<CtrlText>();

			//以下自定义组件的默认属性
			rt.sizeDelta = new Vector2(kWidth, kThinHeight);//宽高
			tp.font = AssetDatabase.LoadAssetAtPath<Font>(deFontPath);//字体
			tp.raycastTarget = false;//关闭射线检测
			tp.fontSize = fontSize;//字体大小
			tp.text = "New TextPro";//Text默认值
			tp.color = Color.black;//颜色
			tp.TestNewProperty = "测试用自定义新属性";//test
			return go;
		}
		#endregion

		#region ButtonPro
		[MenuItem("GameObject/FFTAIUI/ButtonPro", false, priority = GO_ID + 1)]
		static void CreateButtonPro(MenuCommand menuCommand)
		{
			GameObject go = DefaultControls.CreateButton(GetStandardResources());
			go.name = "ButtonPro";

			GameObject.DestroyImmediate(go.transform.Find("Text").gameObject);
			GameObject goTP = TextProGO();
			SetParentAndAlign(goTP, go);

			var childText = goTP.GetComponent<TextPro>();
			childText.text = "ButtonPro";
			childText.alignment = TextAnchor.MiddleCenter;

			var btRT = go.GetComponent<RectTransform>();
			btRT.sizeDelta = new Vector2(kWidth, kThinHeight);

			var childTextRT = goTP.GetComponent<RectTransform>();
			childTextRT.anchorMin = Vector2.zero;
			childTextRT.anchorMax = Vector2.one;
			childTextRT.sizeDelta = Vector2.zero;

			PlaceUIElementRoot(go, menuCommand);

		}
		#endregion

		#region TogglePro
		[MenuItem("GameObject/FFTAIUI/TogglePro", false, priority = GO_ID + 2)]
		static void CreateTogglePro(MenuCommand menuCommand)
		{
			GameObject go = DefaultControls.CreateToggle(GetStandardResources());
			go.name = "TogglePro";

			GameObject.DestroyImmediate(go.transform.Find("Label").gameObject);
			GameObject goTP = TextProGO();
			SetParentAndAlign(goTP, go);
			goTP.name = "Label";

			var childText = goTP.GetComponent<TextPro>();
			childText.text = "TogglePro";
			//childText.alignment = TextAnchor.MiddleCenter;

			var tgRT = go.GetComponent<RectTransform>();
			tgRT.sizeDelta = new Vector2(kWidth, kThinHeight);

			var bgRect = go.transform.Find("Background").GetComponent<RectTransform>();
			bgRect.anchorMin = new Vector2(0f, 1f);
			bgRect.anchorMax = new Vector2(0f, 1f);
			bgRect.anchoredPosition = new Vector2(50f, -50f);
			bgRect.sizeDelta = new Vector2(100f, 100f);

			var checkmarkRect = bgRect.transform.Find("Checkmark").GetComponent<RectTransform>();
			checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
			checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
			checkmarkRect.anchoredPosition = Vector2.zero;
			checkmarkRect.sizeDelta = new Vector2(100f, 100f);


			var childTextRT = goTP.GetComponent<RectTransform>();
			childTextRT.anchorMin = new Vector2(0f, 0f);
			childTextRT.anchorMax = new Vector2(1f, 1f);
			childTextRT.offsetMin = new Vector2(115f, 3f);
			childTextRT.offsetMax = new Vector2(-25f, -6f);

			PlaceUIElementRoot(go, menuCommand);

		}
		#endregion

		#region InputFieldPro
		[MenuItem("GameObject/FFTAIUI/InputFieldPro", false, priority = GO_ID + 3)]
		static void CreateInputFieldPro(MenuCommand menuCommand)
		{
			GameObject go = DefaultControls.CreateInputField(GetStandardResources());
			go.name = "InputFieldPro";
			var ipf = go.GetComponent<InputField>();
			GameObject.DestroyImmediate(go.transform.Find("Placeholder").gameObject);
			GameObject.DestroyImmediate(go.transform.Find("Text").gameObject);
			GameObject gopTP = TextProGO();
			SetParentAndAlign(gopTP, go);
			gopTP.name = "Placeholder";

			GameObject gotTP = TextProGO();
			SetParentAndAlign(gotTP, go);


			var childText = gotTP.GetComponent<TextPro>();
			childText.text = "";
			childText.supportRichText = false;

			var placeholder = gopTP.GetComponent<TextPro>();
			placeholder.text = "Enter text...";
			placeholder.fontStyle = FontStyle.Italic;
			Color placeholderColor = childText.color;
			placeholderColor.a *= 0.5f;
			placeholder.color = placeholderColor;

			var btRT = go.GetComponent<RectTransform>();
			btRT.sizeDelta = new Vector2(kWidth, kThinHeight);


			RectTransform textRectTransform = childText.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;
			textRectTransform.offsetMin = new Vector2(50, 5);
			textRectTransform.offsetMax = new Vector2(-50, -6);

			RectTransform placeholderRectTransform = placeholder.GetComponent<RectTransform>();
			placeholderRectTransform.anchorMin = Vector2.zero;
			placeholderRectTransform.anchorMax = Vector2.one;
			placeholderRectTransform.sizeDelta = Vector2.zero;
			placeholderRectTransform.offsetMin = new Vector2(50, 5);
			placeholderRectTransform.offsetMax = new Vector2(-50, -6);

			ipf.textComponent = childText;
			ipf.placeholder = placeholder;

			PlaceUIElementRoot(go, menuCommand);

		}
		#endregion

		#region DropdownPro
		[MenuItem("GameObject/FFTAIUI/DropdownPro", false, priority = GO_ID + 4)]
		static void CreateDropdownPro(MenuCommand menuCommand)
		{
			GameObject go = DefaultControls.CreateDropdown(GetStandardResources());
			go.name = "DropdownPro";
			var dd = go.GetComponent<Dropdown>();
			var arrow = dd.transform.Find("Arrow");
			var template = dd.transform.Find("Template");
			var viewport = template.Find("Viewport");
			var content = viewport.Find("Content");
			var item = content.Find("Item");
			var itemBackground = item.Find("Item Background");
			var itemCheckmark = item.Find("Item Checkmark");

			GameObject.DestroyImmediate(go.transform.Find("Label").gameObject);
			GameObject.DestroyImmediate(item.Find("Item Label").gameObject);
			GameObject goilbTP = TextProGO();
			SetParentAndAlign(goilbTP, item.gameObject);
			goilbTP.name = "Item Label";

			GameObject golbTP = TextProGO();
			SetParentAndAlign(golbTP, go);
			golbTP.transform.SetAsFirstSibling();
			golbTP.name = "Label";

			var goilbText = goilbTP.GetComponent<TextPro>();
			goilbText.text = "Option A";
			goilbText.alignment = TextAnchor.MiddleLeft;

			var golbText = golbTP.GetComponent<TextPro>();
			golbText.text = "Option A";
			golbText.alignment = TextAnchor.MiddleLeft;

			dd.captionText = golbText;
			dd.itemText = goilbText;

			var ddRT = go.GetComponent<RectTransform>();
			ddRT.sizeDelta = new Vector2(kWidth, kThinHeight);

			RectTransform labelRT = golbTP.GetComponent<RectTransform>();
			labelRT.anchorMin = Vector2.zero;
			labelRT.anchorMax = Vector2.one;
			labelRT.offsetMin = new Vector2(35, 4);
			labelRT.offsetMax = new Vector2(-100, -5);

			RectTransform arrowRT = arrow.GetComponent<RectTransform>();
			arrowRT.anchorMin = new Vector2(1, 0.5f);
			arrowRT.anchorMax = new Vector2(1, 0.5f);
			arrowRT.sizeDelta = new Vector2(100, 100);
			arrowRT.anchoredPosition = new Vector2(-75, 0);

			RectTransform templateRT = template.GetComponent<RectTransform>();
			templateRT.anchorMin = new Vector2(0, 0);
			templateRT.anchorMax = new Vector2(1, 0);
			templateRT.pivot = new Vector2(0.5f, 1);
			templateRT.anchoredPosition = new Vector2(0, 10);
			templateRT.sizeDelta = new Vector2(0, 750);

			RectTransform viewportRT = viewport.GetComponent<RectTransform>();
			viewportRT.anchorMin = new Vector2(0, 0);
			viewportRT.anchorMax = new Vector2(1, 1);
			viewportRT.sizeDelta = new Vector2(-90, 0);
			viewportRT.pivot = new Vector2(0, 1);

			RectTransform contentRT = content.GetComponent<RectTransform>();
			contentRT.anchorMin = new Vector2(0f, 1);
			contentRT.anchorMax = new Vector2(1f, 1);
			contentRT.pivot = new Vector2(0.5f, 1);
			contentRT.anchoredPosition = new Vector2(0, 0);
			contentRT.sizeDelta = new Vector2(0, 140);

			RectTransform itemRT = item.GetComponent<RectTransform>();
			itemRT.anchorMin = new Vector2(0, 0.5f);
			itemRT.anchorMax = new Vector2(1, 0.5f);
			itemRT.sizeDelta = new Vector2(0, 100);

			RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
			itemBackgroundRT.anchorMin = Vector2.zero;
			itemBackgroundRT.anchorMax = Vector2.one;
			itemBackgroundRT.sizeDelta = Vector2.zero;

			RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
			itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
			itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
			itemCheckmarkRT.sizeDelta = new Vector2(100, 100);
			itemCheckmarkRT.anchoredPosition = new Vector2(50, 0);

			RectTransform itemLabelRT = goilbTP.GetComponent<RectTransform>();
			itemLabelRT.anchorMin = Vector2.zero;
			itemLabelRT.anchorMax = Vector2.one;
			itemLabelRT.offsetMin = new Vector2(100, 3);
			itemLabelRT.offsetMax = new Vector2(-50, -6);

			PlaceUIElementRoot(go, menuCommand);

		}
		#endregion


		static public GameObject CreateNewUI()
		{
			// Root for the UI
			var root = new GameObject("Canvas");
			root.layer = LayerMask.NameToLayer("UI");
			Canvas canvas = root.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			root.AddComponent<CanvasScaler>();
			root.AddComponent<GraphicRaycaster>();

			// Works for all stages.
			StageUtility.PlaceGameObjectInCurrentStage(root);
			bool customScene = false;
			PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
			{
				root.transform.SetParent(prefabStage.prefabContentsRoot.transform, false);
				customScene = true;
			}

			Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

			// If there is no event system add one...
			// No need to place event system in custom scene as these are temporary anyway.
			// It can be argued for or against placing it in the user scenes,
			// but let's not modify scene user is not currently looking at.
			if (!customScene)
				CreateEventSystem(false);
			return root;
		}
		private static void CreateEventSystem(bool select)
		{
			CreateEventSystem(select, null);
		}
		private static void CreateEventSystem(bool select, GameObject parent)
		{
			StageHandle stage = parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
			var esys = stage.FindComponentOfType<EventSystem>();
			if (esys == null)
			{
				var eventSystem = new GameObject("EventSystem");
				if (parent == null)
					StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
				else
					GameObjectUtility.SetParentAndAlign(eventSystem, parent);
				esys = eventSystem.AddComponent<EventSystem>();
				eventSystem.AddComponent<StandaloneInputModule>();

				Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
			}

			if (select && esys != null)
			{
				Selection.activeGameObject = esys.gameObject;
			}
		}
		private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
		{
			GameObject parent = menuCommand.context as GameObject;
			bool explicitParentChoice = true;
			if (parent == null)
			{
				parent = GetOrCreateCanvasGameObject();
				explicitParentChoice = false;

				// If in Prefab Mode, Canvas has to be part of Prefab contents,
				// otherwise use Prefab root instead.
				PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
					parent = prefabStage.prefabContentsRoot;
			}
			if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
			{
				// Create canvas under context GameObject,
				// and make that be the parent which UI element is added under.
				GameObject canvas = CreateNewUI();
				canvas.transform.SetParent(parent.transform, false);
				parent = canvas;
			}

			// Setting the element to be a child of an element already in the scene should
			// be sufficient to also move the element to that scene.
			// However, it seems the element needs to be already in its destination scene when the
			// RegisterCreatedObjectUndo is performed; otherwise the scene it was created in is dirtied.
			SceneManager.MoveGameObjectToScene(element, parent.scene);

			Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);

			if (element.transform.parent == null)
			{
				Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
			}

			GameObjectUtility.EnsureUniqueNameForSibling(element);

			// We have to fix up the undo name since the name of the object was only known after reparenting it.
			Undo.SetCurrentGroupName("Create " + element.name);

			GameObjectUtility.SetParentAndAlign(element, parent);
			if (!explicitParentChoice) // not a context click, so center in sceneview
				SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

			Selection.activeGameObject = element;
		}

		static public GameObject GetOrCreateCanvasGameObject()
		{
			GameObject selectedGo = Selection.activeGameObject;

			// Try to find a gameobject that is the selected GO or one if its parents.
			Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
			if (IsValidCanvas(canvas))
				return canvas.gameObject;

			// No canvas in selection or its parents? Then use any valid canvas.
			// We have to find all loaded Canvases, not just the ones in main scenes.
			Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
			for (int i = 0; i < canvasArray.Length; i++)
				if (IsValidCanvas(canvasArray[i]))
					return canvasArray[i].gameObject;

			// No canvas in the scene at all? Then create a new one.
			return CreateNewUI();
		}
		static bool IsValidCanvas(Canvas canvas)
		{
			if (canvas == null || !canvas.gameObject.activeInHierarchy)
				return false;

			// It's important that the non-editable canvas from a prefab scene won't be rejected,
			// but canvases not visible in the Hierarchy at all do. Don't check for HideAndDontSave.
			if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
				return false;

			if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle())
				return false;

			return true;
		}
		private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
		{
			SceneView sceneView = SceneView.lastActiveSceneView;

			// Couldn't find a SceneView. Don't set position.
			if (sceneView == null || sceneView.camera == null)
				return;

			// Create world space Plane from canvas position.
			Vector2 localPlanePosition;
			Camera camera = sceneView.camera;
			Vector3 position = Vector3.zero;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
			{
				// Adjust for canvas pivot
				localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
				localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

				localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
				localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

				// Adjust for anchoring
				position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
				position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

				Vector3 minLocalPosition;
				minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
				minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

				Vector3 maxLocalPosition;
				maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
				maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

				position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
				position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
			}

			itemTransform.anchoredPosition = position;
			itemTransform.localRotation = Quaternion.identity;
			itemTransform.localScale = Vector3.one;
		}
		private static void SetParentAndAlign(GameObject child, GameObject parent)
		{
			if (parent == null)
				return;

			child.transform.SetParent(parent.transform, false);
			SetLayerRecursively(child, parent.layer);
		}

		private static void SetLayerRecursively(GameObject go, int layer)
		{
			go.layer = layer;
			Transform t = go.transform;
			for (int i = 0; i < t.childCount; i++)
				SetLayerRecursively(t.GetChild(i).gameObject, layer);
		}

		static private DefaultControls.Resources s_StandardResources;
		private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
		private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
		private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
		private const string kKnobPath = "UI/Skin/Knob.psd";
		private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
		private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
		private const string kMaskPath = "UI/Skin/UIMask.psd";
		static private DefaultControls.Resources GetStandardResources()
		{
			if (s_StandardResources.standard == null)
			{
				s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
				s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
				s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
				s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
				s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
				s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
				s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
			}
			return s_StandardResources;
		}

	}

	[CustomEditor(typeof(TextPro), true)]
	[CanEditMultipleObjects]
	public class TextProEditor : UnityEditor.UI.TextEditor
	{
		SerializedProperty TestANewField;

		protected override void OnEnable()
		{
			base.OnEnable();
			TestANewField = serializedObject.FindProperty("TestNewProperty");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			base.OnInspectorGUI();
			EditorGUILayout.PropertyField(TestANewField);
			serializedObject.ApplyModifiedProperties();
		}
	}