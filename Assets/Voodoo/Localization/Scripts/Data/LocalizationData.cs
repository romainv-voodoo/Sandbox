using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Distribution
{
    [Serializable]
    public class LocalizationData : ScriptableObject
    {
		public int selectedSheetIndex = -1;
		public List<LocalizationSheet> sheetVersions = new List<LocalizationSheet>();

		public LocalizationSheet SelectedSheet => selectedSheetIndex < 0 ? null : sheetVersions[Mathf.Clamp(selectedSheetIndex, 0, sheetVersions.Count-1)];
    }

	[Serializable]
	public class LocalizationSheet
	{
		public string name;
		public string[] languageCodes;
		public LocalizationRow[] translationRows;
	}

	[Serializable]
	public class LocalizationRow
	{
		public string key;
		public string[] translations;
	}
}
