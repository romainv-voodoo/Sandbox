using System;

namespace Voodoo.Store
{
    public interface IEditorTarget
    {
        System.Type targetType { get; }
    }
}