using System.Collections.Generic;

namespace Voodoo.Store
{
	public static class MarkdownHelper
	{
		public static string ToBold(this string value)
		{
			return "*" + value + "*";
		}
   
		public static string ToItalic(this string value)
		{
			return "_" + value + "_";
		}
   
		public static string ToStrike(this string value)
		{
			return "~" + value + "~";
		}
   
		public static string ToInline(this string value)
		{
			return "`inline" + value + "`";
		}

		public static string ToLink(this string value)
		{
			return "<" + value + "/>";
		}
   
		public static string ToQuote(this string value)
		{
			return ">" + value;
		}

		public static string ToBulletList(this List<string> values)
		{
			string result= string.Empty;

			for (int i = 0; i < values.Count; i++)
			{
				result +="• " + values[i] + "\n";
			}

			return result;
		}
	}
}
