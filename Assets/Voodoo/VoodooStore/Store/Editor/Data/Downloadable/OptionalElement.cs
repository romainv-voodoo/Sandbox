using System.IO;
using System.Linq;

namespace Voodoo.Store
{
	public class OptionalElement
	{
		public string   path;
		public bool     isSelected;
		public int      size;

		private string  name;
		public string   Name => name;


		public OptionalElement(string path, int size = 0, bool isSelected = true)
		{
			this.path = path;
			this.size = size;
			this.isSelected = isSelected;
			
			name = path.Split(Path.AltDirectorySeparatorChar).Last();
		}
	}
}