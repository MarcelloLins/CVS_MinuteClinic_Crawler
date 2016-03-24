using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    #region ** Class Attributes and Sub-Classes **

    public class PharmacyAPI
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public Header header   { get; set; }
        public Details details { get; set; }
    }

    public class Header
    {
        public string statusCode { get; set; }
        public string statusDesc { get; set; }
    }

    public class Details
    {
        public Location[] locations { get; set; }
    }

    public class Location
    {
        public string StoreNumber                                { get; set; }
        public string minuteClinicID                             { get; set; }
        public int storeType                                     { get; set; }
        public string minuteClinicName                           { get; set; }
        public string pharmacyNCPDPProviderIdentifier            { get; set; }
        public string addressLine                                { get; set; }
        public string addressCityDescriptionText                 { get; set; }
        public string addressState                               { get; set; }
        public string addressZipCode                             { get; set; }
        public string addressCountry                             { get; set; }
        public string geographicLatitudePoint                    { get; set; }
        public string geographicLongitudePoint                   { get; set; }
        public string indicatorStoreTwentyFourHoursOpen          { get; set; }
        public string indicatorPrescriptionService               { get; set; }
        public string indicatorPhotoCenterService                { get; set; }
        public string indicatorFluShotService                    { get; set; }
        public string indicatorMinuteClinicService               { get; set; }
        public string instorePickupService                       { get; set; }
        public string indicatorDriveThruService                  { get; set; }
        public string indicatorPharmacyTwentyFourHoursOpen       { get; set; }
        public string rxConvertedFlag                            { get; set; }
        public string indicatorCircularConverted                 { get; set; }
        public string indicatorH1N1FluShot                       { get; set; }
        public string indicatorRxFluFlag                         { get; set; }
        public string indicatorHhcService                        { get; set; }
        public string indicatorWicService                        { get; set; }
        public string snapIndicator                              { get; set; }
        public string indicatorVaccineServiceSupport             { get; set; }
        public string indicatorPneumoniaShotService              { get; set; }
        public string indicatorWeeklyAd                          { get; set; }
        public string indicatorCVSStore                          { get; set; }
        public string ageDescription                             { get; set; }
        public string indicatorStorePickup                       { get; set; }
        public string storeLocationTimeZone                      { get; set; }
        public string storePhonenumber                           { get; set; }
        public string pharmacyPhonenumber                        { get; set; }
        public Storehours storeHours                             { get; set; }
        public Minuteclinicnpbreakhours minuteClinicNpBreakHours { get; set; }
        public Pharmacyhours pharmacyHours                       { get; set; }
        public Minuteclinichours minuteClinicHours               { get; set; }
        public string adVersionCdCurrent                         { get; set; }
        public string adVersionCdNext                            { get; set; }
        public int locationID                                    { get; set; }
        public string waittime                                   { get; set; }
        public string hmpilstatus                                { get; set; }
        public string ageThreshold                               { get; set; }
        public string gracePeriod                                { get; set; }
        public Reasonforvisit[] reasonforvisits                  { get; set; }
    }

    public class Storehours
    {
        public Dayhour[] DayHours { get; set; }
    }

    public class Dayhour
    {
        public string Day   { get; set; }
        public string Hours { get; set; }
    }

    public class Minuteclinicnpbreakhours
    {
        public Dayhour1[] DayHours { get; set; }
    }

    public class Dayhour1
    {
        public string Day   { get; set; }
        public string Hours { get; set; }
    }

    public class Pharmacyhours
    {
        public Dayhour2[] DayHours { get; set; }
    }

    public class Dayhour2
    {
        public string Day   { get; set; }
        public string Hours { get; set; }
    }

    public class Minuteclinichours
    {
        public Dayhour3[] DayHours { get; set; }
    }

    public class Dayhour3
    {
        public string Day   { get; set; }
        public string Hours { get; set; }
    }

    public class Reasonforvisit
    {
        public string name { get; set; }
        public string id   { get; set; }
    }

    #endregion
}
