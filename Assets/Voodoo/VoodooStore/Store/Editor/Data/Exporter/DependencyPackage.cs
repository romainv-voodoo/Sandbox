namespace Voodoo.Store
{
	[System.Serializable]
	public class DependencyPackage
	{
		public string name;
		public string Name => name;
		public bool isSelected;

		public DependencyPackage()
		{
			name = "";
			isSelected = false;
		}

		public DependencyPackage(DependencyPackage source)
		{
			name = source.name;
			isSelected = source.isSelected;
		}
	}
}