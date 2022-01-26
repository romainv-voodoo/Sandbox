using System;
using System.Collections.Generic;

namespace Voodoo.Store
{
    public static class EditorRetailer
    {
        private static List<IEditorFactory> factories = new List<IEditorFactory>();

        public static void AddFactory(IEditorFactory factory)
        {
            for (int i = 0; i < factories.Count; i++)
            {
                if (factories[i].Context == factory.Context)
                {
                    return;
                }
            }

            factories.Add(factory);
        }

        public static IGenericEditor<T> ContentEditorFor<T>(int context)
        {
            foreach (IEditorFactory factory in factories)
            {
                if (factory.Context == context)
                {
                    return factory.Mount<T>();
                }
            }

            return null;
        }

        public static void OnGUI<T>(int context, T value) 
        {
            foreach (IEditorFactory factory in factories)
            {
                if (factory.Context == context)
                {
                    factory.OnGUI<T>(value);
                }
            }
        }

        public static void Clear()
        {
            factories?.Clear();
        }
    }
}