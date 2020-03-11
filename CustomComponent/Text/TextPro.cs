using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

	public class TextPro : Text
	{
		public string TestNewProperty;
		protected override void OnEnable()
		{
			base.OnEnable();
			if (!Application.isPlaying && Application.isEditor) return;
			
		}
	}
