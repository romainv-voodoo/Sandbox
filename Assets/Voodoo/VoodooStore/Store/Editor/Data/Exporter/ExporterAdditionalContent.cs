using System;

namespace Voodoo.Store
{
	[Serializable]
	public class ExporterAdditionalContent
	{
		public AdditionalContent additionalContent;
		public AdditionalContentState status;
		public bool existOnline;
		public bool existLocal;
		public int sizeDiff;
		
		public ExporterAdditionalContent()
		{
			additionalContent = new AdditionalContent
			{
				folderPath = "",
				size = 0
			};
			status = 0;
			existOnline = false;
			existLocal = false;
			sizeDiff = 0;
		}
		
		//Constructor for already existing additional content
		public ExporterAdditionalContent(AdditionalContent source)
		{
			additionalContent = new AdditionalContent
			{
				folderPath = PathHelper.GetMacPath(source.folderPath),
				size = source.size
			};
			status = 0;
			existOnline = true;
			existLocal = false;
			sizeDiff = -source.size;
		}
		
		//Constructor for new additional content
		public ExporterAdditionalContent(string folderPath, int folderSize = 0, AdditionalContentState state = AdditionalContentState.UPDATE)
		{
			additionalContent = new AdditionalContent
			{
				folderPath = PathHelper.GetMacPath(folderPath),
				size = folderSize
			};
			status = state;
			existOnline = false;
			existLocal = true;
			sizeDiff = folderSize;
		}
	}

	[Serializable]
	public enum AdditionalContentState
	{
		KEEP = 0,
		UPDATE = 1,
		REMOVE = 2
	}
}