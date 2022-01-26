using System.Linq;
using System.Reflection;

namespace Voodoo.CI
{
    public static class ObjectExtensions
    {
        public static T1 CopyFrom<T1, T2>(this T1 obj, T2 otherObject)
            where T1 : class
            where T2 : class
        {
            var srcFields = otherObject.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            var destFields = obj.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in srcFields)
            {
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null)
                    dest.SetValue(obj, property.GetValue(otherObject));
            }

            return obj;
        }
    }
}