using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using CVS_MinuteClinicWaitTimeScraper.SimpleHelpers;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using WebUtilsLib;
using SharedLibrary.Models;
using SharedLibrary;
using Newtonsoft.Json;
using System.Collections.Generic;
using BDCExcelManager;
using System.Drawing;

namespace CVS_MinuteClinicWaitTimeScraper
{
    class Program
    {
        public static FlexibleOptions ProgramOptions { get; private set; }

        private static int            _currentExcelRow  { get; set; }

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

            int maxRetries = options.Get ("MAX_RETRIES_PER_PHARMACY", 3);
           
            // Sanity Check
            if (!File.Exists (options["PHARMACIES_LIST_FILE"]))
            {
                logger.Fatal ("Could not find input file of pharmacies. Halting.");
                ConsoleUtils.CloseApplication (-1, true);
            }

            if (String.IsNullOrEmpty(options["PHARMACIES_OUTPUT_FILE"]))
            {
                logger.Fatal ("Could not find 'PHARMACIES_OUTPUT_FILE' parameter on config file. Halting");
                ConsoleUtils.CloseApplication (-2, true);
            }

            string outputPharmaciesFile = options["PHARMACIES_OUTPUT_FILE"] + DateTime.Now.ToString ("yyyyMMdd#hh_mm") + ".xlsx";
            _currentExcelRow = 2;

            // Reading file and Parsing Pharmacies data
            using (ExcelManager excelWriter = new ExcelManager(new FileInfo(outputPharmaciesFile)))
            {
                // Creating Spreadsheet
                Worksheet mainSheet = excelWriter.OpenOrCreateWorksheet ("CVS_DATA");

                // Writing File Header
                WriteFileHeader (mainSheet);

                using (WebRequests client = new WebRequests())
                {
                    client.AllowAutoRedirect = true;                    

                    foreach (string fLine in File.ReadLines (options["PHARMACIES_LIST_FILE"]))
                    {
                        logger.Trace ("Processing Pharmacy : " + fLine);

                        // Get Request for Pharmacy Page
                        int    retries = 1;
                        string htmlPharmacy;
                        string apiResponse;
                        do
                        {
                            try
                            {
                                // Reseting Host to "Website" value, instead of API
                                client.Host = "www.cvs.com";

                                // HTTP GET Request for the Pharmacy Page
                                htmlPharmacy = client.Get (fLine);

                                // Sanity Check
                                if (String.IsNullOrEmpty (htmlPharmacy))
                                {
                                    logger.Warn ("Failed to Fetch HTML Page for {0}. Retry:[{1}]", fLine, retries);
                                    continue;
                                }

                                // Parsing HTML Elements for the API call
                                string postUrl    = Parser.ParseAPICallUrl (htmlPharmacy);
                                string pharmacyId = Parser.ParsePharmacyID (htmlPharmacy);
                                string postBody   = "{\"request\":{\"destination\":{\"minuteClinicID\":[\"" + pharmacyId + "\"]},\"operation\":[\"clinicInfo\",\"waittime\"],\"services\":[\"indicatorMinuteClinicService\"]}}";

                                // API Post                                
                                client.Host    = "services.cvs.com";
                                client.Referer = "http://www.cvs.com";
                                apiResponse    = client.Post (postUrl, postBody);

                                // Serializing Response to Object
                                PharmacyAPI pharmacyAPIModel = JsonConvert.DeserializeObject<PharmacyAPI> (apiResponse);

                                // Writing CSV Output
                                WriteRow (PharmacyToString (pharmacyAPIModel), mainSheet);
                                break;
                            }
                            catch (Exception ex)
                            {
                                logger.Error (ex);
                                continue;
                            }

                        // Retry Loop
                        } while (++retries < maxRetries);
                    } 
                }

                // Saving result
                ApplyTableBorders (mainSheet);
                excelWriter.Save ();
            }

            logger.Info ("End - Press ENTER to halt.");
            Console.ReadLine ();
        }

        private static string PharmacyToString (PharmacyAPI pharmacy)
        {
            StringBuilder strBuilder  = new StringBuilder ();
            List<String>  csvElements = new List<String> ();

            // Reaching Intermediate Root Object
            var locationObject = pharmacy.response.details.locations[0];

            // Adding elements to List of CSV Elements
            csvElements.Add (String.IsNullOrEmpty (locationObject.StoreNumber)                ? "" : locationObject.StoreNumber );
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicName)           ? "" : locationObject.minuteClinicName);
            csvElements.Add (String.IsNullOrEmpty (locationObject.addressLine)                ? "" : locationObject.addressLine);
            csvElements.Add (String.IsNullOrEmpty (locationObject.addressCityDescriptionText) ? "" : locationObject.addressCityDescriptionText);
            csvElements.Add (String.IsNullOrEmpty (locationObject.addressState)               ? "" : locationObject.addressState);

            csvElements.Add (String.IsNullOrEmpty (locationObject.addressZipCode)                    ? "" : locationObject.addressZipCode);
            csvElements.Add (String.IsNullOrEmpty (locationObject.addressCountry)                    ? "" : locationObject.addressCountry);
            csvElements.Add (String.IsNullOrEmpty (locationObject.geographicLatitudePoint)           ? "" : locationObject.geographicLatitudePoint);
            csvElements.Add (String.IsNullOrEmpty (locationObject.geographicLongitudePoint)          ? "" : locationObject.geographicLongitudePoint);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorStoreTwentyFourHoursOpen) ? "" : locationObject.indicatorStoreTwentyFourHoursOpen);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorPrescriptionService)      ? "" : locationObject.indicatorPrescriptionService);

            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorPhotoCenterService)  ? "" : locationObject.indicatorPhotoCenterService);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorFluShotService)      ? "" : locationObject.indicatorFluShotService);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorMinuteClinicService) ? "" : locationObject.indicatorMinuteClinicService);
            csvElements.Add (String.IsNullOrEmpty (locationObject.instorePickupService)         ? "" : locationObject.instorePickupService);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorDriveThruService)    ? "" : locationObject.indicatorDriveThruService);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorH1N1FluShot)         ? "" : locationObject.indicatorH1N1FluShot);

            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorVaccineServiceSupport) ? "" : locationObject.indicatorVaccineServiceSupport);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorPneumoniaShotService)  ? "" : locationObject.indicatorPneumoniaShotService);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorWeeklyAd)              ? "" : locationObject.indicatorWeeklyAd);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorCVSStore)              ? "" : locationObject.indicatorCVSStore);
            csvElements.Add (String.IsNullOrEmpty (locationObject.ageDescription)                 ? "" : locationObject.ageDescription);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorStorePickup)           ? "" : locationObject.indicatorStorePickup);

            csvElements.Add (String.IsNullOrEmpty (locationObject.storeLocationTimeZone) ? "" : locationObject.storeLocationTimeZone);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storePhonenumber)      ? "" : locationObject.storePhonenumber);
            csvElements.Add (String.IsNullOrEmpty (locationObject.pharmacyPhonenumber)   ? "" : locationObject.pharmacyPhonenumber);
            csvElements.Add (String.IsNullOrEmpty (locationObject.indicatorStorePickup)  ? "" : locationObject.indicatorStorePickup);

            // Day-Opening Times (Monday to Sunday)
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[0].Hours) ? "" : locationObject.storeHours.DayHours[0].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[1].Hours) ? "" : locationObject.storeHours.DayHours[1].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[2].Hours) ? "" : locationObject.storeHours.DayHours[2].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[3].Hours) ? "" : locationObject.storeHours.DayHours[3].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[4].Hours) ? "" : locationObject.storeHours.DayHours[4].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[5].Hours) ? "" : locationObject.storeHours.DayHours[5].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.storeHours.DayHours[6].Hours) ? "" : locationObject.storeHours.DayHours[6].Hours);

            // Day-Break Times (Monday to Sunday)
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[0].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[0].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[1].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[1].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[2].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[2].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[3].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[3].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[4].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[4].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[5].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[5].Hours);
            csvElements.Add (String.IsNullOrEmpty (locationObject.minuteClinicNpBreakHours.DayHours[6].Hours) ? "" : locationObject.minuteClinicNpBreakHours.DayHours[6].Hours);

            // Misc Attributes
            csvElements.Add (String.IsNullOrEmpty (locationObject.waittime)             ? "" : locationObject.waittime);
            csvElements.Add (String.IsNullOrEmpty (locationObject.hmpilstatus)          ? "" : locationObject.hmpilstatus);
            csvElements.Add (String.IsNullOrEmpty (locationObject.ageThreshold)         ? "" : locationObject.ageThreshold);
            csvElements.Add (String.IsNullOrEmpty (locationObject.gracePeriod)          ? "" : locationObject.gracePeriod);
            
            // Adding List elements to String Builder
            foreach(String csvElement in csvElements)
            {
                strBuilder.Append (csvElement.Replace(";","") + ";");
            }
            
            // Remove Trailing ';'
            return strBuilder.ToString ().Replace("NULL","").Trim(';');
        }

        private static void WriteFileHeader (Worksheet mainSheet)
        {
            // Adding elements to List of CSV Elements
            mainSheet.Write ("A1", "StoreNumber");
            mainSheet.Write ("B1", "MinuteClinicName");
            mainSheet.Write ("C1", "AddressLine");
            mainSheet.Write ("D1", "AddressCityDescription");
            mainSheet.Write ("E1", "AddressState");

            mainSheet.Write ("F1","AddressZipCode");
            mainSheet.Write ("G1","AddressCountry");
            mainSheet.Write ("H1","Latitude");
            mainSheet.Write ("I1","Longitude");
            mainSheet.Write ("J1","IndicatorOpenTwentyForHours");
            mainSheet.Write ("K1","IndicatorPrescriptionService");

            mainSheet.Write ("L1", "IndicatorPhotoCenterService");
            mainSheet.Write ("M1", "IndicatorFluShotService");
            mainSheet.Write ("N1", "IndicatorMinuteClinicService");
            mainSheet.Write ("O1", "InstorePickupService");
            mainSheet.Write ("P1", "IndicatorDriveThruService");
            mainSheet.Write ("Q1", "IndicatorH1N1FluShot");

            mainSheet.Write ("R1", "IndicatorVaccineServiceSupport");
            mainSheet.Write ("S1", "IndicatorPneumoniaShotService");
            mainSheet.Write ("T1", "IndicatorWeeklyAd");
            mainSheet.Write ("U1", "IndicatorCVSStore");
            mainSheet.Write ("V1", "AgeDescription");
            mainSheet.Write ("W1", "IndicatorStorePickup");

            mainSheet.Write ("X1","StoreLocationTimeZone");
            mainSheet.Write ("Y1","StorePhonenumber");
            mainSheet.Write ("Z1","PharmacyPhonenumber");
            mainSheet.Write ("AA1","IndicatorStorePickup");

            mainSheet.Write ("AB1", "StoreHours.Monday");
            mainSheet.Write ("AC1", "StoreHours.Tuesday");
            mainSheet.Write ("AD1", "StoreHours.Wednesday");
            mainSheet.Write ("AE1", "StoreHours.Thursday");
            mainSheet.Write ("AF1", "StoreHours.Friday");
            mainSheet.Write ("AG1", "StoreHours.Saturday");
            mainSheet.Write ("AH1", "StoreHours.Sunday");

            mainSheet.Write ("AI1", "ClinicBreakHours.Monday");
            mainSheet.Write ("AJ1", "ClinicBreakHours.Tuesday");
            mainSheet.Write ("AK1", "ClinicBreakHours.Wednesday");
            mainSheet.Write ("AL1", "ClinicBreakHours.Thursday");
            mainSheet.Write ("AM1", "ClinicBreakHours.Friday");
            mainSheet.Write ("AN1", "ClinicBreakHours.Saturday");
            mainSheet.Write ("AO1", "ClinicBreakHours.Sunday");

            mainSheet.Write ("AP1", "WaitTime");
            mainSheet.Write ("AQ1", "HMPILStatus");
            mainSheet.Write ("AR1", "AgeThreshold");
            mainSheet.Write ("AS1", "GracePeriod");

            // Applying Style
            mainSheet.EPPlusSheet.Cells["A1:AS1"].Style.Font.Bold = true;
            mainSheet.EPPlusSheet.Cells["A1:AS1"].Style.Font.Color.SetColor (Color.FromArgb (255, 255, 255));
            mainSheet.EPPlusSheet.Cells["A1:AS1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            mainSheet.SetBackgroundColor ("A1:AS1", Color.FromArgb (22, 54, 92));

            // Table Borders
            mainSheet.SetBorderStyle ("A1:AS1", OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            mainSheet.SetBorderColor ("A1:AS1", Color.Black);
            mainSheet.AutofitColumns ("A1:AS1");
        }

        private static void ApplyTableBorders(Worksheet mainSheet)
        {
            // Table Borders
            mainSheet.SetBorderStyle ("A1:AS" + (_currentExcelRow - 1), OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            mainSheet.SetBorderColor ("A1:AS" + (_currentExcelRow - 1), Color.Black);
            mainSheet.AutofitColumns ("A1:AS" + (_currentExcelRow - 1));
        }

        private static void WriteRow (String data, Worksheet mainSheet)
        {
            // Spliting each data point into it's own array element
            string[] dataPoint = data.Split (';');

            // Writing columns
            for(int i = 1 ; i <= dataPoint.Count() / 2; i ++)
            {
                // Translating numbers into an excel range
                string excelCell = GetExcelColumnName (i) + _currentExcelRow;

                mainSheet.Write (excelCell, dataPoint[i - 1]);
            }

            _currentExcelRow++;
        }

        private static string GetExcelColumnName (int columnNumber)
        {
            int dividend      = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar (65 + modulo).ToString () + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}