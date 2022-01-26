using System;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Voodoo.Distribution
{
    [Serializable]
	public class Translator : MonoBehaviour
	{
		[SerializeField]
		TMP_Text _textMesh;
		[SerializeField]
		string _defaultValue;
		
		[Tooltip("If the translation should be checked at Start")]
		public bool AtStart = true;
		
		[Tooltip("If the translation should be checked on Enable")]
		public bool AtEnable;

		public string key;

		[SerializeField]
		TMP_Text TextMesh => _textMesh ?? (_textMesh = GetComponent<TMP_Text>());

		// Use this for initialization
		void Start()
		{
			if (AtStart)
			{
				Translate();
			}
			
			// can't do this otherwise we remove all the center/right aligned text for normal languages when switching.
			// to do this properly we would need to store the current alignment of the text and see if we need to change it 
			// Localization.languageChanged += UpdateRightToLeft;
		}

		void OnEnable()
		{
			if (AtEnable)
			{
				Translate();
			}
		}

		public void Translate(string language = null)
		{
			TextMesh.text = Localization.GetTranslation(key, language);
#if UNITY_EDITOR
			EditorUtility.SetDirty(TextMesh);
#endif
		}

		void UpdateRightToLeft()
		{
			if (Localization.IsRightToLeftLanguage)
			{
				TextMesh.alignment = TextAlignmentOptions.Right;
				TextMesh.isRightToLeftText = true;
			}
			else
			{
				TextMesh.alignment = TextAlignmentOptions.Left;
				TextMesh.isRightToLeftText = false;
			}
		}
	}
}