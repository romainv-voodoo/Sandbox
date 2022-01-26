namespace Voodoo.Store
{
    public interface IEditor
    {
        void OnEnable();

        void OnDisable();

        void Controls();

        void OnGUI();
    }
}