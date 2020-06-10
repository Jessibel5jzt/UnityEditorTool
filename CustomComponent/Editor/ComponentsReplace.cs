using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

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
				if (EditorUtility.DisplayDialog("确认替换", "是否确认替换场景内所有Text组件为TextPro？", "确定", "取消"))
				{
					TextToPro();
				}
			}

			if (GUILayout.Button("TextPro替换为Text"))
			{
				if (EditorUtility.DisplayDialog("确认替换", "此举可能导致TextPro组件的新增属性丢失，若未使用新增属性可完整替换，是否确认替换？", "确定", "取消"))
				{
					ProToText();
				}
			}

		}



		/// <summary>
		/// 计数
		/// </summary>
		private static int _replaceCount = 0;
		/// <summary>
		/// 查找组件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onlyRoot">是否仅父节点</param>
		/// <returns></returns>
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


		#region TextToPro
		public static void TextToPro()
		{
			_replaceCount = 0;

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
					ChangeT(tList[i].gameObject);
				}
			}

			Debug.Log($"Text已全部替换为TextPro，共替换{_replaceCount}处。");
		}
		public static TextPro ChangeTextReferenceRelationship(Text item)
		{
			if (item == null) return null;
			if (item.GetType() == typeof(TextPro)) return item as TextPro;

			var timetextGo = item.gameObject;
			return ChangeT(timetextGo);
		}
		public static TextPro ChangeT(GameObject tGo)
		{
			var t = tGo.GetComponent<Text>();
			var enabled = t.enabled;
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
			tp.enabled = enabled;
			EditorUtility.SetDirty(tGo);
			_replaceCount += 1;
			return tp;
		}
		#endregion

		#region ProToText
		private void ProToText()
		{
			_replaceCount = 0;

			var ddList = GetAllObjsOfType<Dropdown>();
			foreach (var item in ddList)
			{
				item.captionText = ChangeTextProReferenceRelationship(item.captionText);
				item.itemText = ChangeTextProReferenceRelationship(item.itemText);
			}

			var ipfList = GetAllObjsOfType<InputField>();
			foreach (var item in ipfList)
			{
				item.textComponent = ChangeTextProReferenceRelationship(item.textComponent);
				item.placeholder = ChangeTextProReferenceRelationship(item.placeholder as Text);
			}

			var tList = GetAllObjsOfType<TextPro>();
			for (int i = 0; i < tList.Count; i++)
			{
				ChangeTP(tList[i].gameObject);
			}

			Debug.Log($"TextPro已全部替换为Text，共替换{_replaceCount}处。");
		}
		public static Text ChangeTextProReferenceRelationship(Text item)
		{
			if (item != null)
			{
				var timetextGo = item.gameObject;
				return ChangeTP(timetextGo);
			}
			return null;
		}
		public static Text ChangeTP(GameObject tGo)
		{
			var tp = tGo.GetComponent<Text>();
			var enabled = tp.enabled;
			Undo.RecordObject(tGo, tGo.name);
			DestroyImmediate(tGo.GetComponent<Text>());
			var t = tGo.AddComponent<Text>();
			t.text = tp.text;
			t.font = tp.font;
			t.fontStyle = tp.fontStyle;
			t.fontSize = tp.fontSize;
			t.lineSpacing = tp.lineSpacing;
			t.supportRichText = tp.supportRichText;
			t.alignment = tp.alignment;
			t.alignByGeometry = tp.alignByGeometry;
			t.horizontalOverflow = tp.horizontalOverflow;
			t.verticalOverflow = tp.verticalOverflow;
			t.resizeTextForBestFit = tp.resizeTextForBestFit;
			t.resizeTextMinSize = tp.resizeTextMinSize;
			t.resizeTextMaxSize = tp.resizeTextMaxSize;
			t.color = tp.color;
			t.material = tp.material.name.CompareTo("Default UI Material") == 0 ? null : tp.material;
			t.raycastTarget = tp.raycastTarget;
			t.enabled = enabled;
			EditorUtility.SetDirty(tGo);
			_replaceCount += 1;
			return t;
		}
		#endregion

	}