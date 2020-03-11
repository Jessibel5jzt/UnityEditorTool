using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

	public class ComponentsReplace : EditorWindow
	{
		[MenuItem("Tools/脚本批量替换工具", false, 1)]
		public static void Open()
		{
			GetWindowWithRect(typeof(ComponentsReplace), new Rect(0, 0, 300, 400), false, "脚本批量替换工具");
		}

		void OnGUI()
		{
			if (GUILayout.Button("Text替换为TextPro"))
			{
				TextToPro();
			}

		}
		public static Text ChangeT(GameObject tGo)
		{
			Text t = tGo.GetComponent<Text>();
			Undo.RecordObject(tGo, tGo.name);
			DestroyImmediate(tGo.GetComponent<Text>());
			var tp = tGo.AddComponent<TextPro>();
			tp.text = t.text;
			tp.font = t.font;
			tp.fontStyle = t.fontStyle;
			tp.fontSize = t.fontSize;
			tp.lineSpacing = t.lineSpacing;
			tp.supportRichText = t.supportRichText;
			tp.alignment = t.alignment;
			tp.alignByGeometry = t.alignByGeometry;
			tp.horizontalOverflow = t.horizontalOverflow;
			tp.verticalOverflow = t.verticalOverflow;
			tp.resizeTextForBestFit = t.resizeTextForBestFit;
			tp.resizeTextMinSize = t.resizeTextMinSize;
			tp.resizeTextMaxSize = t.resizeTextMaxSize;
			tp.color = t.color;
			tp.material = t.material.name.CompareTo("Default UI Material") == 0 ? null : t.material;
			tp.raycastTarget = t.raycastTarget;
			tp.TestNewProperty = "测试用自定义新属性";
			EditorUtility.SetDirty(tGo);
			return tp;
		}



		private static int _replaceCount = 0;
		public static Text ChangeTextReferenceRelationship(Text item)
		{
			if (item != null && item.GetType() != typeof(TextPro))
			{
				_replaceCount += 1;
				var timetextGo = item.gameObject;
				return ChangeT(timetextGo);
			}
			return null;
		}
		public static void TextToPro()
		{
			var gpList = GetAllObjsOfType<GamePanel>();
			foreach (var item in gpList)
			{
				item.Timetext = ChangeTextReferenceRelationship(item.Timetext);
				item.Gradetext = ChangeTextReferenceRelationship(item.Gradetext);
			}

			var admList = GetAllObjsOfType<AudioMgr>();
			foreach (var item in admList)
			{
				item.musicTypetxt = ChangeTextReferenceRelationship(item.musicTypetxt);
				item.musicNametxt = ChangeTextReferenceRelationship(item.musicNametxt);
			}

			var ddList = GetAllObjsOfType<Dropdown>();
			foreach (var item in ddList)
			{
				item.captionText = ChangeTextReferenceRelationship(item.captionText);
				item.itemText = ChangeTextReferenceRelationship(item.itemText);
			}

			var ipfList = GetAllObjsOfType<InputField>();
			foreach (var item in ipfList)
			{
				item.textComponent = ChangeTextReferenceRelationship(item.textComponent);
				item.placeholder = ChangeTextReferenceRelationship(item.placeholder as Text);
			}

			var tList = GetAllObjsOfType<Text>();
			for (int i = 0; i < tList.Count; i++)
			{
				if (tList[i].GetType() != typeof(TextPro))
				{
					tList[i] = ChangeT(tList[i].gameObject);
				}
			}

			Debug.Log($"替换成功，共替换{_replaceCount}处。");
			_replaceCount = 0;
		}
		public static List<T> GetAllObjsOfType<T>(bool onlyRoot = false) where T : Component
		{
			T[] Objs = (T[])Resources.FindObjectsOfTypeAll(typeof(T));

			List<T> returnObjs = new List<T>();

			foreach (T Obj in Objs)
			{
				if (onlyRoot)
				{
					if (Obj.transform.parent != null)
					{
						continue;
					}
				}

				if (Obj.hideFlags == HideFlags.NotEditable || Obj.hideFlags == HideFlags.HideAndDontSave)
				{
					continue;
				}

				if (Application.isEditor)
				{
					//检测资源是否存在，不存在会返回null或empty的字符串，存在会返回文件名
					string sAssetPath = AssetDatabase.GetAssetPath(Obj.transform.root.gameObject);
					if (!string.IsNullOrEmpty(sAssetPath))
					{
						continue;
					}
				}

				returnObjs.Add(Obj);
			}

			return returnObjs;
		}

	}
