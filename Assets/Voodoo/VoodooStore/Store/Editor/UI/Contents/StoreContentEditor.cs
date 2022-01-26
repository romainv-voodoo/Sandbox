using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class StoreContentEditor : AbstractGenericEditor<VoodooStoreEditor>
    {
        public override void OnGUI(VoodooStoreEditor store)
        {
            Rect labelRect = EditorGUILayout.GetControlRect();
            if (FiltersEditor.filteredPackages?.Count == 0)
            {
                EditorGUI.LabelField(labelRect, "No packages found", ContentHelper.WrapStyleTitle);
                return;
            }

            if (VoodooStore.selection == null || VoodooStore.selection.Count == 0)
            {
                EditorGUI.LabelField(labelRect, "No items selected", ContentHelper.WrapStyleTitle);
            }
        }
    }
}