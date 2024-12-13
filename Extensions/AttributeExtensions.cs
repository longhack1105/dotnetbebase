using System.ComponentModel;
using System.Reflection;

namespace DotnetBeBase.Extensions
{
    public static class AttributeExtensions
    {
        public static string ToDescriptionString<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
    }
}
