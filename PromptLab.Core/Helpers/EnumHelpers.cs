using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Helpers
{
	public static class EnumHelpers
	{
		public static string GetDescription(this Enum value)
		{
			var type = value.GetType();

			var name = Enum.GetName(type, value);
			if (name == null) return "";
			var field = type.GetField(name);
			if (field == null) return value.ToString();
			if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
			{
				return attr.Description;
			}

			return value.ToString();
		}
	}
}
