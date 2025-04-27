using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DhruvEnterprises.Core
{
    public static class Extensions
    {
        public static string GetEnumDisplayName(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .Name;
        }
        public static void AddOrReplace(this IDictionary<string, object> DICT, string key, object value)
        {
            if (DICT.ContainsKey(key))
                DICT[key] = value;
            else
                DICT.Add(key, value);
        }

        public static T GetObjectOrDefault<T>(this IDictionary<string, object> DICT, string key)
        {
            if (DICT.ContainsKey(key))
                return (T)Convert.ChangeType(DICT[key], typeof(T));
            else
                return default(T);
        }

        public static dynamic NewGetObjectOrDefault(this Dictionary<string, object> DICT, string key)
        {
            if (DICT != null && DICT.ContainsKey(key))
                return DICT[key];
            else
                return null;
        }

        /// <summary>
        /// Extension method to return an enum value of type T for the given string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Extension method to return an enum value of type T for the given int.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this int value)
        {
            var name = Enum.GetName(typeof(T), value);
            return name.ToEnum<T>();
        }


        #region "String"

        public static string ToSelfURL(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string outputStr = text.Trim().Replace(":", "").Replace("&", "").Replace(" ", "-").Replace("'", "").Replace(",", "").Replace("(", "").Replace(")", "").Replace("--", "").Replace(".", "");
            return Regex.Replace(outputStr.Trim().ToLower().Replace("--", ""), "[^a-zA-Z0-9_-]+", "", RegexOptions.Compiled);
        }

        public static string TrimLength(this string input, int length, bool Incomplete = true)
        {
            if (String.IsNullOrEmpty(input)) { return String.Empty; }
            return input.Length > length ? String.Concat(input.Substring(0, length), Incomplete ? "..." : "") : input;
        }

        public static string ToTitle(this string input)
        {
            return String.IsNullOrEmpty(input) ? String.Empty : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static bool ContainsAny(this string input, params string[] values)
        {
            return String.IsNullOrEmpty(input) ? false : values.Any(S => input.Contains(S));
        }

        #endregion

        #region "Collection"
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, DataTableServerSide searchCriteria, Type[] childTypes = null)
        {
            return (IQueryable<T>)CustomPredicate.BuildOrderBy(source, searchCriteria, childTypes);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source != null && source.Count() >= 0)
            {
                foreach (T element in source)
                {
                    action(element);
                }
            }
        }

        public static bool IsNotNullAndNotEmpty<T>(this ICollection<T> source)
        {
            return source != null && source.Count() > 0;
        }

        #endregion
        public static string ReplaceURL(this string url,
             string uuu,
             string ppp,
             string rrr,
             string mmm,
             string ooo,
             string ccc,
             string aaa,
             string ttt,
             string sss,
             string vvv,

             string eee,
             string ddd,
             string hhh,
             string nnn,

             string fff1,
             string fff2,
             string fff3,
             string fff4,

             string ffft,
             string fffr,
             string kkkn,
             string kkkp,
             string kkks,
             string kkk1,
             string kkk2,
             string cmpid = "",
             string rrrop1 = "",
              string uid = ""

             )
        {
            url = url.Replace("[UUU]", "[uuu]")
                     .Replace("[PPP]", "[ppp]")
                     .Replace("[RRR]", "[rrr]")
                     .Replace("[MMM]", "[mmm]")
                     .Replace("[OOO]", "[ooo]")
                     .Replace("[CCC]", "[ccc]")
                     .Replace("[AAA]", "[aaa]")
                     .Replace("[TTT]", "[ttt]")
                     .Replace("[SSS]", "[sss]")
                     .Replace("[VVV]", "[vvv]")
                     .Replace("[EEE]", "[eee]")
                     .Replace("[DDD]", "[ddd]")
                     .Replace("[HHH]", "[hhh]")
                     .Replace("[NNN]", "[nnn]")
                     .Replace("[FFF1]", "[fff1]")
                     .Replace("[FFF2]", "[fff2]")
                     .Replace("[FFF3]", "[fff3]")
                     .Replace("[FFF4]", "[fff4]")
                     .Replace("[FFFT]", "[ffft]")
                     .Replace("[FFFR]", "[fffr]")
                     .Replace("[KKKn]", "[kkkn]")
                     .Replace("[KKKp]", "[kkkp]")
                     .Replace("[KKKS]", "[kkks]")
                     .Replace("[KKK1]", "[kkk1]")
                     .Replace("[KKK2]", "[kkk2]")
                     .Replace("[CMPID]", "[cmpid]")
                     .Replace("[RRROP1]", "[rrrop1]")
                     .Replace("[UID]", "[uid]")
                     ;



            url = url.Replace("[uuu]", uuu)
                     .Replace("[ppp]", ppp)
                     .Replace("[rrr]", rrr)
                     .Replace("[mmm]", mmm)
                     .Replace("[ooo]", ooo)
                     .Replace("[ccc]", ccc)
                     .Replace("[aaa]", aaa)
                     .Replace("[ttt]", ttt)
                     .Replace("[sss]", sss)
                     .Replace("[vvv]", vvv)
                     .Replace("[eee]", eee)
                     .Replace("[ddd]", ddd)
                     .Replace("[hhh]", hhh)
                     .Replace("[nnn]", nnn)
                     .Replace("[fff1]", fff1)
                     .Replace("[fff2]", fff2)
                     .Replace("[fff3]", fff3)
                     .Replace("[fff4]", fff4)
                     .Replace("[ffft]", ffft)
                     .Replace("[fffr]", fffr)
                     .Replace("[kkkn]", kkkn)
                     .Replace("[kkkp]", kkkp)
                     .Replace("[kkks]", kkks)
                     .Replace("[kkk1]", kkk1)
                     .Replace("[kkk2]", kkk2)
                     .Replace("[cmpid]", cmpid)
                     .Replace("[rrrop1]", rrrop1)
                     .Replace("[uid]", uid)
                     ;
            return url;

        }
    }
}
