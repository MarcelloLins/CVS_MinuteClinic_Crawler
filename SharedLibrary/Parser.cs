using HtmlAgilityPack;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class Parser
    {
        #region ** XPaths **

        private static string _XPATH_STATES     = "//div[@class='states-wrap']/div[@class='column']/ul/li//a";
        private static string _XPATH_CITIES     = "//div[@class='city-wrap']/div[@class='column']/ul/li//a";
        private static string _XPATH_CITY_ERROR = "//div[@class='error']";
        private static string _XPATH_PHARMACIES = "//a[@title='Clinic Hours']";

        private static string _XPATH_PHARMACY_ADDRESS           = "//div[@id='content']//div[@class='dMarginBot'][1]";
        private static string _XPATH_PHARMACY_MINUTECLINICHOURS = "//div[@id='content']//div[@class='dBox02'][1]/div[@class='dMarginBot2'][1]";
        private static string _XPATH_PHARMACY_LUNCHHOURS        = "//div[@id='content']//div[@class='dBox02'][2]/div[@class='dMarginBot2'][1]";
        private static string _XPATH_PHARMACY_WAITTIMES         = "//div[@class='wait_time_clinic']";

        private static string _XPATH_API_SCRIPT_CALL            = "//script[contains(text(),'var vhmpl')]";

        #endregion

        #region ** Pharmacy Tracing Steps **

        public static IEnumerable<String> ParseStates(string htmlResponse)
        {
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            foreach (string stateLink in map.DocumentNode.SelectNodes (_XPATH_STATES).Select(t => t.Attributes["href"].Value))
            {
                // Assembling full url
                string fullStateLink = "http://www.cvs.com" + stateLink;

                yield return fullStateLink;
            }
        }

        public static IEnumerable<String> ParseCities(string htmlResponse)
        {
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            foreach (string cityLink in map.DocumentNode.SelectNodes (_XPATH_CITIES).Select (t => t.Attributes["href"].Value))
            {
                // Assembling full url
                string fullCityLink = "http://www.cvs.com" + cityLink;

                yield return fullCityLink;
            }
        }

        public static bool IsCityError (string htmlResponse)
        {
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            return map.DocumentNode.SelectSingleNode (_XPATH_CITY_ERROR) != null;
        }

        public static IEnumerable<String> ParsePharmacyUrls (string htmlResponse)
        {
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            foreach (string pharmacyUrl in map.DocumentNode.SelectNodes (_XPATH_PHARMACIES).Select (t => t.Attributes["href"].Value))
            {
                // Assembling full url
                string fullPharmacyLink = "http://www.cvs.com" + pharmacyUrl;

                yield return fullPharmacyLink;
            }
        }

        #endregion

        #region ** Pharmacy Data Parsing **
        
        public static string ParseAPICallUrl(string htmlResponse)
        {
            // Loading HtmlMap
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            // Reaching Script Node
            var scriptNode = map.DocumentNode.SelectSingleNode (_XPATH_API_SCRIPT_CALL);

            string scriptUrl = scriptNode.InnerText;

            return scriptUrl.Replace ("var vhmpl='", "").Trim ().Replace ("'", "");
        }

        public static string ParsePharmacyID (string htmlResponse)
        {
            // Loading HtmlMap
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            // Reaching Script Node
            var scriptNode = map.DocumentNode.SelectSingleNode ("//div[@class='wait_time_hmpl_dtl']");

            return scriptNode.Attributes["id"].Value.Replace ("wait_time_hmpl_", "");
        }

        #endregion
    }
}
