using System.Net;


namespace DhruvEnterprises.Web.LIBS
{
    public partial class Utilities
    {
       

        public static string GetIP()
        {
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();
            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
        }

      
    }

}
