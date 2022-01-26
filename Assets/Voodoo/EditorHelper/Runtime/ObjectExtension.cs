using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Voodoo.Utils
{
    public static class ObjectExtension
    {
        public static object DeepCopy(this object obj)
        {
            using (var ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(ms);
            }
        }
    }
}
