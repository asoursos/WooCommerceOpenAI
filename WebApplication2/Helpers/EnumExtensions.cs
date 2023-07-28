using System.ComponentModel;

namespace WebApplication2.Helpers;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes?[0].Description ?? value.ToString();
    }
}
