namespace Voodoo.Store
{
	[System.Serializable]
	public class ExporterLabel
	{
		public string labelName;
		public bool isSelected;

		public ExporterLabel()
		{
			labelName = "";
			isSelected = false;
		}

		public ExporterLabel(ExporterLabel source)
		{
			labelName = source.labelName;
			isSelected = source.isSelected;
		}
	}
}