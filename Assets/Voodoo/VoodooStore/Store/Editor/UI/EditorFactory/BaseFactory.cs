using System.Collections.Generic;

namespace Voodoo.Store
{
    public class BaseFactory : IEditorFactory
    {
        public int context;
        public List<IEditorTarget> factors;

        public int Context => context;
        
        public IGenericEditor<T> Mount<T>()
        {
            foreach (IEditorTarget factor in factors)
            {
                if (factor.targetType == typeof(T))
                {
                    return factor as IGenericEditor<T>;
                }
            }

            return null;
        }

        public void OnGUI<T>(T value)
        {
            IGenericEditor<T> editor = Mount<T>();
            if (editor == null)
            {
                return;
            }

            editor.OnGUI(value);
        }
    }
}