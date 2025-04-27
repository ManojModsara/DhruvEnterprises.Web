using System;
namespace DhruvEnterprises.API
{ 
    public static class Extensions
    {
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
            string cmpid="",
            string rrrop1="",
            string uid="",
            string requestdate=""

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
                      .Replace("[RRROP1]", "[rrrop1]")
                     .Replace("[CMPID]", "[cmpid]")
                     .Replace("[UID]", "[uid]")
                     .Replace("[VVT]", "[vvt]")
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
                     .Replace("[rrrop1]", rrrop1)
                     .Replace("[cmpid]",cmpid)
                     .Replace("[uid]", uid)
                     .Replace("[vvt]", requestdate)

                     ;
            return url;

        }

        public static string GetSubstring(this string originalstr, string prestr = null, string poststr = null, int premargin = 0, int postmargin = 0, StringComparison comparison = StringComparison.InvariantCulture)
        {
            string subString = string.Empty;
            try
            {
                var fromLength = (!string.IsNullOrEmpty(prestr) ? prestr.ToString().ToLower() != "none" ? prestr : string.Empty : string.Empty).Length;
                var startIndex = !string.IsNullOrEmpty(prestr)
                    ? prestr.ToLower() != "none" ? originalstr.IndexOf(prestr, comparison) + fromLength : 0
                    : 0;

                if (startIndex < fromLength) { throw new ArgumentException("prestr: Failed to find "); }

                var endIndex = !string.IsNullOrEmpty(poststr) ?
                                poststr.ToLower() != "none" ? originalstr.IndexOf(poststr, startIndex, comparison) : originalstr.Length :
                                originalstr.Length;

                if (endIndex < 0) { throw new ArgumentException("poststr:Failed to find"); }

                startIndex += premargin;
                endIndex += postmargin;

                subString = originalstr.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception ex)
            {
                
            }

            return subString;
        }

        public static string GetSplitstringByIndex(this string originalstr, string separator = null, int index = 0)
        {
            separator = separator?.Replace("\r","")?.Replace("\t", "")?.Replace("\n", "")?.Trim();
            string subString = string.Empty;
            try
            {
                subString = originalstr.Split(Convert.ToChar(separator))[index];
            }
            catch (Exception ex)
            {

             
            }
          
            return subString;
        }

        public static string ToPlainText(this string str)  
        { 

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = str.Replace("<", " ")
                         .Replace(">", " ")
                         .Replace(",", " ")
                         .Replace("\r\n", " ")
                         .Replace("\r", " ")
                         .Replace("\t", " ");
            }

            return str.Trim();
        }


        public static string ReplacePlanApiURL(this string url,
            string uuu,
            string ppp,
            string mmm,
            string ooo = "",
            string ccc = ""
            )
        {

            url = url.Replace(";", "&")
                     .Replace("[UUU]", "[uuu]")
                     .Replace("[PPP]", "[ppp]")
                     .Replace("[MMM]", "[mmm]")
                     .Replace("[OOO]", "[ooo]")
                     .Replace("[CCC]", "[ccc]");



            url = url.Replace("[uuu]", uuu)
                     .Replace("[ppp]", ppp)
                     .Replace("[mmm]", mmm)
                     .Replace("[ooo]", ooo)
                     .Replace("[ccc]", ccc);
            return url;

        }


    }
}
