using AVGD.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

public static class StringExtensions
{
    /// <summary>
    /// 获取 SqlValue 里面字段的别名
    /// </summary>
    /// <param name="name">表单值</param>
    /// <param name="sqlValue"> Sql 值</param>
    /// <returns> c1 as `c11`  返回 c1</returns>
    /// http://stackoverflow.com/questions/1297455/using-a-custom-field-in-where-clause-of-sql-query#comment1128450_1297461
    public static string GetFieldSqlByName(this string sqlValue,string name)
    {
        /* select c1,c2 from sys_users */
        if (!sqlValue.Contains("as", StringComparison.OrdinalIgnoreCase))
        {
            return name;
        }
        string[] Fields = sqlValue.Split(',');
        int length = Fields.Length;
        for (int index = 0; index < length; index++)
        {
            string value = Fields[index].Replace("\n", " ").Replace("\t", " ").Replace("\r", " ").Trim();
            if (value.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                if (value.Contains("case", StringComparison.OrdinalIgnoreCase) && value.Contains("when", StringComparison.OrdinalIgnoreCase) && value.Contains("end", StringComparison.OrdinalIgnoreCase))
                {
                    /* CASE a.IsCheckState  WHEN 0 THEN  '等待买家付款' WHEN 6 THEN '订单退款中' end END AS '订单状态' -----> 要 a.IsCheckState */
                    //TODO :  修改bug  a.IsCheckState 是数字的话 前台显示汉字 然乎模糊查询不能查找到对应的语句
                    /* CASE a.IsCheckState  WHEN 0 THEN  '等待买家付款' WHEN 6 THEN '订单退款中' end END AS '订单状态' -----> 要  CASE a.IsCheckState  WHEN 0 THEN  '等待买家付款' WHEN 6 THEN '订单退款中' end END */
                    if (value.Contains("as", StringComparison.OrdinalIgnoreCase))
                    {
                        if (index == 0)
                        {
                            /* Select c1 As c11 , c2 as c22 from sys_users ----->要 Select c1 As c11 中的c1  */
                            return value.Substring(value.IndexOf("select", StringComparison.OrdinalIgnoreCase) + 6, value.IndexOf(" as", StringComparison.OrdinalIgnoreCase) - 6);
                        }
                        /* select c1 as c11,c2 as c22 , c3 as c33 from sys_users ------>要 c2 as c22 或者 c3 as c33 from sys_users 中的 c2 或者 c3*/
                        return value.Substring(0, value.IndexOf(" as", StringComparison.OrdinalIgnoreCase));
                    }
                    return value.Substring(value.IndexOf("case", StringComparison.OrdinalIgnoreCase) + 4, value.IndexOf(" when ", StringComparison.OrdinalIgnoreCase) - 4);
                }
                if (value.Contains("as", StringComparison.OrdinalIgnoreCase))
                {
                    if (index == 0)
                    {
                    /* Select c1 As c11 , c2 as c22 from sys_users ----->要 Select c1 As c11 中的c1  */
                    return value.Substring(value.IndexOf("select", StringComparison.OrdinalIgnoreCase) + 6, value.IndexOf(" as", StringComparison.OrdinalIgnoreCase) - 6);
                    }
                    /* select c1 as c11,c2 as c22 , c3 as c33 from sys_users ------>要 c2 as c22 或者 c3 as c33 from sys_users 中的 c2 或者 c3*/
                    return value.Substring(0, value.IndexOf(" as", StringComparison.OrdinalIgnoreCase));
                }
                else { 
                    /* 没有 as 的情况 */
                    /* Select c1  c11 , c2  c22 from sys_users ----->要 Select c1 As c11 中的c1  */
                    var symbols = value.GetSymbol();
                    if (index == 0) {                       
                        return value.Substring(value.IndexOf("select", StringComparison.OrdinalIgnoreCase) + 6, value.IndexOf(symbols, StringComparison.OrdinalIgnoreCase) - 6);
                    }
                    /* select c1  c11,c2  c22 , c3  c33 from sys_users ------>要 c2  c22 或者 c3  c33 from sys_users 中的 c2 或者 c3*/
                    return value.Substring(0, value.IndexOf(symbols, StringComparison.OrdinalIgnoreCase));
                }
            }
        }
        return name;
    }

    /// <summary>
    ///  获取 SqlValue 里面字段的 值
    /// </summary>
    /// <param name="sqlValue">Sql 语句 CASE a.IsCheckState WHEN 0 THEN	'等待买家付款' WHEN 1 THEN '买家已付款' END AS '订单状态',</param>
    /// <param name="value">等待买家付款</param>
    /// <returns>返回 THEN 对应的数字0</returns>
    /// 	CASE a.IsCheckState WHEN 0 THEN	'等待买家付款' WHEN 1 THEN '买家已付款' END AS '订单状态',
    /// 	字段是 a.IsCheckState 
    /// 	要获取对应  等待买家付款 的值 0
    public static string GetFieldValueByValue(this string sqlValue, string name,string value)
    {
        /* select c1,c2 from sys_users */
        if (!sqlValue.Contains("case", StringComparison.OrdinalIgnoreCase) && !sqlValue.Contains("when", StringComparison.OrdinalIgnoreCase) && !sqlValue.Contains("end", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }
        string[] Fields = sqlValue.Split(',');
        int length = Fields.Length;
        List<string> valueList = new List<string>();
        for (int index = 0; index < length; index++)
        {
            string field = Fields[index];
            if (!field.Contains(name))
            {
                continue;
            }
            /*找到对应 case Then end 的语句 进行分割 查找索要的值*/
            string[] when = field.Split("when", RegexOptions.IgnoreCase);
            foreach (var item in when)
            {
                if (item.IndexOf(value, StringComparison.OrdinalIgnoreCase) > -1)
                {
                   valueList.Add(item.Substring(0, item.IndexOf("then", StringComparison.OrdinalIgnoreCase)));
                }
            }
            return string.Join(",", valueList.ToArray());
        }
        return string.Join(",", valueList.ToArray());
    }

    public static string GetSymbol(this string @string)
    { 
        /*as 关键字的  */
      string[] symbols={"\"","\'","`"};
      foreach (var item in symbols)
      {
          if (@string.IndexOf(item, StringComparison.OrdinalIgnoreCase) > 0)
          {
              return item;
          }
      }
      return " ";
    }

    public static string GetMD5String(this string  input)
    {
        if (null != input && !"".Equals(input))
        {
            string s = null;
            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8',
                   '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            try
            {
                MD5 md = MD5.Create();
                byte[] tmp = md.ComputeHash(Encoding.UTF8.GetBytes(input));
                char[] str = new char[16 * 2];
                int k = 0;
                for (int i = 0; i < 16; i++)
                {

                    byte byte0 = tmp[i];
                    str[k++] = hexDigits[byte0 >> 4 & 0xf];

                    str[k++] = hexDigits[byte0 & 0xf];
                }
                s = new String(str);

            }
            catch (Exception e)
            {
                BugLog.Write(e.ToString());
            }
            return s;
        }
        else
        {
            return null;
        }
    }

    public static bool InArray(this string[] array, string key)
    {
        foreach (var item in array)
        {
            if (key.Contains(item))
            {
                return true;
            }
        }
        return false;    
    }

    public static string toStringMergeChar(this string[] items, char str)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < items.Length; i++)
        {
            sb.AppendFormat(items[i] + str);
        }
        return sb.ToString().Substring(0, sb.ToString().Length - 1);
    }


    public static string[] toStringArray(this string str)
    {
        return str.Split(',').ToArray();
    }

    public static string sumField(this string[] value)
    {
        string sum = string.Empty;
        foreach (var item in value)
        {
            if (item.Contains("金") || item.Contains("价"))
            {
                sum += string.Format(" ROUND(sum(xiaoji.{0}),2) as {1},", item, item);
            }
            else
                sum += string.Format("sum(xiaoji.{0}) as {1},", item, item);
        }
        if (string.IsNullOrWhiteSpace(sum))
        {
            return sum;
        }
        else
        {
            return sum.TrimEnd(',');
        }

    }

	public static bool IsEmpty(this string value)
	{
		return value == null || value.Length == 0;
	}

	public static bool IsNotEmpty(this string value)
	{
		return !value.IsEmpty();
	}

	public static string IfEmpty(this string value, string defaultValue)
	{
		if (!value.IsNotEmpty())
		{
			return defaultValue;
		}
		return value;
	}

	public static string FormatWith(this string value, params object[] parameters)
	{
		return string.Format(value, parameters);
	}

	public static string TrimToMaxLength(this string value, int maxLength)
	{
		if (value != null && value.Length > maxLength)
		{
			return value.Substring(0, maxLength);
		}
		return value;
	}

	public static string TrimToMaxLength(this string value, int maxLength, string suffix)
	{
		if (value != null && value.Length > maxLength)
		{
			return value.Substring(0, maxLength) + suffix;
		}
		return value;
	}

	public static bool Contains(this string inputValue, string comparisonValue, StringComparison comparisonType)
	{
		return inputValue.IndexOf(comparisonValue, comparisonType) != -1;
	}

	public static XDocument ToXDocument(this string xml)
	{
		return XDocument.Parse(xml);
	}

	public static XmlDocument ToXmlDOM(this string xml)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		return xmlDocument;
	}

	public static XPathNavigator ToXPath(this string xml)
	{
		XPathDocument xPathDocument = new XPathDocument(new StringReader(xml));
		return xPathDocument.CreateNavigator();
	}

	public static string Reverse(this string value)
	{
		if (value.IsEmpty() || value.Length == 1)
		{
			return value;
		}
		char[] array = value.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static string EnsureStartsWith(this string value, string prefix)
	{
		if (!value.StartsWith(prefix))
		{
			return prefix + value;
		}
		return value;
	}

	public static string EnsureEndsWith(this string value, string suffix)
	{
		if (!value.EndsWith(suffix))
		{
			return value + suffix;
		}
		return value;
	}

    //public static string Repeat(this string value, int repeatCount)
    //{
    //    StringBuilder sb = new StringBuilder();
    //    repeatCount.Times(delegate
    //    {
    //        sb.Append(value);
    //    });
    //    return sb.ToString();
    //}

	public static bool IsNumeric(this string value)
	{
		float num;
		return float.TryParse(value, out num);
	}

	public static string ExtractDigits(this string value)
	{
		return string.Join(null, Regex.Split(value, "[^\\d]"));
	}

	public static string ConcatWith(this string value, params string[] values)
	{
		return value + string.Concat(values);
	}

	public static Guid ToGuid(this string value)
	{
		return new Guid(value);
	}

	public static Guid ToGuidSave(this string value)
	{
		return value.ToGuidSave(Guid.Empty);
	}

	public static Guid ToGuidSave(this string value, Guid defaultValue)
	{
		if (value.IsEmpty())
		{
			return defaultValue;
		}
		try
		{
			return value.ToGuid();
		}
		catch
		{
		}
		return defaultValue;
	}

	public static string GetBefore(this string value, string x)
	{
		int num = value.IndexOf(x);
		if (num != -1)
		{
			return value.Substring(0, num);
		}
		return string.Empty;
	}

	public static string GetBetween(this string value, string x, string y)
	{
		int num = value.IndexOf(x);
		int num2 = value.LastIndexOf(y);
		if (num == -1 || num == -1)
		{
			return string.Empty;
		}
		int num3 = num + x.Length;
		if (num3 < num2)
		{
			return value.Substring(num3, num2 - num3).Trim();
		}
		return string.Empty;
	}

	public static string GetAfter(this string value, string x)
	{
		int num = value.LastIndexOf(x);
		if (num == -1)
		{
			return string.Empty;
		}
		int num2 = num + x.Length;
		if (num2 < value.Length)
		{
			return value.Substring(num2).Trim();
		}
		return string.Empty;
	}

	public static string Join<T>(string separator, T[] value)
	{
		if (value == null || value.Length == 0)
		{
			return string.Empty;
		}
		if (separator == null)
		{
			separator = string.Empty;
		}
		Converter<T, string> converter = (T o) => o.ToString();
		return string.Join(separator, Array.ConvertAll<T, string>(value, converter));
	}

	public static string Remove(this string value, params char[] removeCharc)
	{
		if (!string.IsNullOrEmpty(value) && removeCharc != null)
		{
			Array.ForEach<char>(removeCharc, delegate(char c)
			{
				value = value.Remove(new string[]
				{
					c.ToString()
				});
			});
		}
		return value;
	}

	public static string Remove(this string value, params string[] strings)
	{
		return strings.Aggregate(value, (string current, string c) => current.Replace(c, string.Empty));
	}

	public static bool IsEmptyOrWhiteSpace(this string value)
	{
		if (!value.IsEmpty())
		{
			return value.All((char t) => char.IsWhiteSpace(t));
		}
		return true;
	}

	public static bool IsNotEmptyOrWhiteSpace(this string value)
	{
		return !value.IsEmptyOrWhiteSpace();
	}

	public static string IfEmptyOrWhiteSpace(this string value, string defaultValue)
	{
		if (!value.IsEmptyOrWhiteSpace())
		{
			return value;
		}
		return defaultValue;
	}

	public static string ToUpperFirstLetter(this string value)
	{
		if (value.IsEmptyOrWhiteSpace())
		{
			return string.Empty;
		}
		char[] array = value.ToCharArray();
		array[0] = char.ToUpper(array[0]);
		return new string(array);
	}

	public static byte[] GetBytes(this string data)
	{
		return Encoding.Default.GetBytes(data);
	}

	public static byte[] GetBytes(this string data, Encoding encoding)
	{
		return encoding.GetBytes(data);
	}

	public static string ToTitleCase(this string value)
	{
		return CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value);
	}

	public static string ToPlural(this string singular)
	{
		int num = singular.LastIndexOf(" of ");
		if (num > 0)
		{
			return singular.Substring(0, num) + singular.Remove(0, num).ToPlural();
		}
		if (singular.EndsWith("sh"))
		{
			return singular + "es";
		}
		if (singular.EndsWith("ch"))
		{
			return singular + "es";
		}
		if (singular.EndsWith("us"))
		{
			return singular + "es";
		}
		if (singular.EndsWith("ss"))
		{
			return singular + "es";
		}
		if (singular.EndsWith("y"))
		{
			return singular.Remove(singular.Length - 1, 1) + "ies";
		}
		if (singular.EndsWith("o"))
		{
			return singular.Remove(singular.Length - 1, 1) + "oes";
		}
		return singular + "s";
	}

	public static string ToHtmlSafe(this string s)
	{
		return s.ToHtmlSafe(false, false);
	}

	public static string ToHtmlSafe(this string s, bool all)
	{
		return s.ToHtmlSafe(all, false);
	}

	public static string ToHtmlSafe(this string s, bool all, bool replace)
	{
		if (s.IsEmptyOrWhiteSpace())
		{
			return string.Empty;
		}
		int[] source = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15,
			16,
			17,
			18,
			19,
			20,
			21,
			22,
			23,
			24,
			25,
			26,
			28,
			29,
			30,
			31,
			34,
			39,
			38,
			60,
			62,
			123,
			124,
			125,
			126,
			127,
			160,
			161,
			162,
			163,
			164,
			165,
			166,
			167,
			168,
			169,
			170,
			171,
			172,
			173,
			174,
			175,
			176,
			177,
			178,
			179,
			180,
			181,
			182,
			183,
			184,
			185,
			186,
			187,
			188,
			189,
			190,
			191,
			215,
			247,
			192,
			193,
			194,
			195,
			196,
			197,
			198,
			199,
			200,
			201,
			202,
			203,
			204,
			205,
			206,
			207,
			208,
			209,
			210,
			211,
			212,
			213,
			214,
			215,
			216,
			217,
			218,
			219,
			220,
			221,
			222,
			223,
			224,
			225,
			226,
			227,
			228,
			229,
			230,
			231,
			232,
			233,
			234,
			235,
			236,
			237,
			238,
			239,
			240,
			241,
			242,
			243,
			244,
			245,
			246,
			247,
			248,
			249,
			250,
			251,
			252,
			253,
			254,
			255,
			256,
			8704,
			8706,
			8707,
			8709,
			8711,
			8712,
			8713,
			8715,
			8719,
			8721,
			8722,
			8727,
			8730,
			8733,
			8734,
			8736,
			8743,
			8744,
			8745,
			8746,
			8747,
			8756,
			8764,
			8773,
			8776,
			8800,
			8801,
			8804,
			8805,
			8834,
			8835,
			8836,
			8838,
			8839,
			8853,
			8855,
			8869,
			8901,
			913,
			914,
			915,
			916,
			917,
			918,
			919,
			920,
			921,
			922,
			923,
			924,
			925,
			926,
			927,
			928,
			929,
			931,
			932,
			933,
			934,
			935,
			936,
			937,
			945,
			946,
			947,
			948,
			949,
			950,
			951,
			952,
			953,
			954,
			955,
			956,
			957,
			958,
			959,
			960,
			961,
			962,
			963,
			964,
			965,
			966,
			967,
			968,
			969,
			977,
			978,
			982,
			338,
			339,
			352,
			353,
			376,
			402,
			710,
			732,
			8194,
			8195,
			8201,
			8204,
			8205,
			8206,
			8207,
			8211,
			8212,
			8216,
			8217,
			8218,
			8220,
			8221,
			8222,
			8224,
			8225,
			8226,
			8230,
			8240,
			8242,
			8243,
			8249,
			8250,
			8254,
			8364,
			8482,
			8592,
			8593,
			8594,
			8595,
			8596,
			8629,
			8968,
			8969,
			8970,
			8971,
			9674,
			9824,
			9827,
			9829,
			9830
		};
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (all || source.Contains((int)c))
			{
				stringBuilder.Append("&#" + (int)c + ";");
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		if (!replace)
		{
			return stringBuilder.ToString();
		}
		return stringBuilder.Replace("", "<br />").Replace("\n", "<br />").Replace(" ", "&nbsp;").ToString();
	}

	public static bool IsMatchingTo(this string value, string regexPattern)
	{
		return value.IsMatchingTo(regexPattern, RegexOptions.None);
	}

	public static bool IsMatchingTo(this string value, string regexPattern, RegexOptions options)
	{
		return Regex.IsMatch(value, regexPattern, options);
	}

	public static string ReplaceWith(this string value, string regexPattern, string replaceValue)
	{
		return value.ReplaceWith(regexPattern, replaceValue, RegexOptions.None);
	}

	public static string ReplaceWith(this string value, string regexPattern, string replaceValue, RegexOptions options)
	{
		return Regex.Replace(value, regexPattern, replaceValue, options);
	}

	public static string ReplaceWith(this string value, string regexPattern, MatchEvaluator evaluator)
	{
		return value.ReplaceWith(regexPattern, RegexOptions.None, evaluator);
	}

	public static string ReplaceWith(this string value, string regexPattern, RegexOptions options, MatchEvaluator evaluator)
	{
		return Regex.Replace(value, regexPattern, evaluator, options);
	}

	public static MatchCollection GetMatches(this string value, string regexPattern)
	{
		return value.GetMatches(regexPattern, RegexOptions.None);
	}

	public static MatchCollection GetMatches(this string value, string regexPattern, RegexOptions options)
	{
		return Regex.Matches(value, regexPattern, options);
	}

	public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern)
	{
		return value.GetMatchingValues(regexPattern, RegexOptions.None);
	}

	public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern, RegexOptions options)
	{
		return from Match match in value.GetMatches(regexPattern, options)
		where match.Success
		select match.Value;
	}

	public static string[] Split(this string value, string regexPattern)
	{
		return value.Split(regexPattern, RegexOptions.None);
	}

	public static string[] Split(this string value, string regexPattern, RegexOptions options)
	{
		return Regex.Split(value, regexPattern, options);
	}

	public static string[] GetWords(this string value)
	{
		return value.Split("\\W");
	}

	public static string GetWordByIndex(this string value, int index)
	{
		string[] words = value.GetWords();
		if (index < 0 || index > words.Length - 1)
		{
			throw new IndexOutOfRangeException("The word number is out of range.");
		}
		return words[index];
	}

	[Obsolete("Please use RemoveAllSpecialCharacters instead")]
	public static string AdjustInput(this string value)
	{
		return string.Join(null, Regex.Split(value, "[^a-zA-Z0-9]"));
	}

	public static string RemoveAllSpecialCharacters(this string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char current in from c in value
		where (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
		select c)
		{
			stringBuilder.Append(current);
		}
		return stringBuilder.ToString();
	}

	public static string SpaceOnUpper(this string value)
	{
		return Regex.Replace(value, "([A-Z])(?=[a-z])|(?<=[a-z])([A-Z]|[0-9]+)", " $1$2").TrimStart(new char[0]);
	}

	public static byte[] ToBytes(this string value)
	{
		return value.ToBytes(null);
	}

	public static byte[] ToBytes(this string value, Encoding encoding)
	{
		encoding = (encoding ?? Encoding.Default);
		return encoding.GetBytes(value);
	}

	public static string EncodeBase64(this string value)
	{
		return value.EncodeBase64(null);
	}

	public static string EncodeBase64(this string value, Encoding encoding)
	{
		encoding = (encoding ?? Encoding.UTF8);
		byte[] bytes = encoding.GetBytes(value);
		return Convert.ToBase64String(bytes);
	}

	public static string DecodeBase64(this string encodedValue)
	{
		return encodedValue.DecodeBase64(null);
	}

	public static string DecodeBase64(this string encodedValue, Encoding encoding)
	{
		encoding = (encoding ?? Encoding.UTF8);
		byte[] bytes = Convert.FromBase64String(encodedValue);
		return encoding.GetString(bytes);
	}
}
