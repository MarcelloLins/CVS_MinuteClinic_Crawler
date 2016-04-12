using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using CSV_MinuteClinicCrawler.SimpleHelpers;
using System.Threading.Tasks;
using WebUtilsLib;
using SharedLibrary;
using System.IO;
using System.Text;

namespace CSV_MinuteClinicCrawler
{
    class Program
    {
        public static FlexibleOptions ProgramOptions { get; private set; }

        #region ** Private Attributes **

        private static string _cvsMinuteClinicHomePageUrl = "http://www.cvs.com/minuteclinic/clinic-locator";

        #endregion

        /// <summary>
        /// Main program entry point.
        /// </summary>
        static void Main (string[] args)
        {
            // set error exit code
            System.Environment.ExitCode = -50;
            try
            {
                // load configurations
                ProgramOptions = ConsoleUtils.Initialize (args, true);           

                // start execution
                ExecuteTask (ProgramOptions);

                // check before ending for waitForKeyBeforeExit option
                if (ProgramOptions.Get ("waitForKeyBeforeExit", false))
                    ConsoleUtils.WaitForAnyKey ();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger ().Fatal (ex);

                // check before ending for waitForKeyBeforeExit option
                if (ProgramOptions != null && ProgramOptions.Get ("waitForKeyBeforeExit", false))
                    ConsoleUtils.WaitForAnyKey ();

                ConsoleUtils.CloseApplication (-60, true);
            }
            // set success exit code
            ConsoleUtils.CloseApplication (0, false);
        }
        
        static Logger logger = LogManager.GetCurrentClassLogger ();
        static DateTime started = DateTime.UtcNow;

        private static void ExecuteTask (FlexibleOptions options)
        {
           logger.Info ("Start");
           
            using (WebRequests client = new WebRequests ())
            {
                using (StreamWriter fWriter = new StreamWriter (options["PHARMACIES_LIST_FILE"], false, Encoding.UTF8))
                {
                    logger.Info ("Executing Request for HomePage");
                    string homeResponse = client.Get (_cvsMinuteClinicHomePageUrl);

                    // Sanity Check
                    if (String.IsNullOrEmpty (homeResponse))
                    {
                        logger.Fatal ("Failed to fetch HomePage. Please check if the site is responding on your browser");
                        ConsoleUtils.CloseApplication (-1, true);
                    }

                    // Extracting Urls of each of the States
                    foreach (string stateUrl in Parser.ParseStates (homeResponse))
                    {
                        logger.Info ("\t=> Processing State : " + stateUrl.Replace ("http://www.cvs.com/minuteclinic/clinics/", "").Split (';')[0]);

                        // Executing Request for State Page
                        string statePageResponse = client.Get (stateUrl);

                        // Sanity Check
                        if (String.IsNullOrEmpty (homeResponse))
                        {
                            logger.Error ("Failed to fetch StatePage. Please check if the site is responding on your browser");
                        }
                        else
                        {
                            // Parsing City Urls
                            foreach (string cityUrl in Parser.ParseCities (statePageResponse))
                            {
                                logger.Info ("\t\t=> Processing City : " + cityUrl.Split ('/')[6]);

                                // Executing Request for City Page
                                string cityPageResponse = client.Get (cityUrl);

                                // Sanity Check
                                if (String.IsNullOrEmpty (cityPageResponse) || Parser.IsCityError(cityPageResponse))
                                {
                                    logger.Error ("Failed to fetch CityPage. Please check if the site is responding on your browser");
                                }
                                else
                                {
                                    foreach (string pharmacyUrl in Parser.ParsePharmacyUrls (cityPageResponse))
                                    {
                                        logger.Info ("\t\t\t=> Processing Pharmacy : " + pharmacyUrl.Split ('/')[7]);

                                        fWriter.WriteLine (pharmacyUrl);
                                    }
                                }
                            }
                        }
                    }
                }
            }

		   logger.Info ("End - Press ENTER to halt.");
           Console.ReadLine ();
        }
    }
}