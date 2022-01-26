namespace Voodoo.Store
{
    public interface IEditorFactory
    {
        int Context { get; }

        IGenericEditor<T> Mount<T>();
        void OnGUI<T>(T value);
    }
}