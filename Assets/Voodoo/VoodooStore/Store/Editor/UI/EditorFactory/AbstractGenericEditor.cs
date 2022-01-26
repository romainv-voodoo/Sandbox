using System;

namespace Voodoo.Store
{
    public abstract class AbstractGenericEditor<T> : IGenericEditor<T>
    {
        public Type targetType => typeof(T);

        public abstract void OnGUI(T target);
    }
}