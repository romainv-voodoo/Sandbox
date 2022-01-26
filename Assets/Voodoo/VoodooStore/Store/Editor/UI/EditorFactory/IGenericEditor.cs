namespace Voodoo.Store
{
    public interface IGenericEditor<T> : IEditorTarget
    {
        void OnGUI(T target);
    }
}