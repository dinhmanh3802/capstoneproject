using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
	public static string GetDisplayName(this Enum enumValue)
	{
		return enumValue.GetType()
						.GetMember(enumValue.ToString())
						.First()
						.GetCustomAttribute<DisplayAttribute>()?
						.Name ?? enumValue.ToString();
	}
	public static string GetDisplayGender(this Enum enumValue)
	{
		var enumMember = enumValue.GetType().GetMember(enumValue.ToString());
		if (enumMember.Length > 0)
		{
			var displayAttr = enumMember[0].GetCustomAttribute<DisplayAttribute>();
			if (displayAttr != null)
			{
				return displayAttr.Name;
			}
		}
		return enumValue.ToString(); // Trả về tên mặc định nếu không có Display
	}
}
