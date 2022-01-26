using System;
using System.IO;
using System.Linq;

namespace Voodoo.Store
{
	[Serializable]
	public class AdditionalContent
	{
		public string folderPath;
		public int size;

		private string name;
		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(name))
				{
					name = folderPath.Split(Path.AltDirectorySeparatorChar).Last();
				}

				return name;
			}
		}
	}
}