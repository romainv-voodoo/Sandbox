using UnityEditor;

namespace Voodoo.Store 
{
	public class FuzzySearchWindow : EditorWindow
	{
		bool disposed;

		public FuzzySearchWidgetVST Widget { get; set; }

		public virtual void OnGUI()
		{
			Widget?.OnGUI();
		}
	}
}