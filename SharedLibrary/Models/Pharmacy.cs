using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class Pharmacy
    {
        public string url               { get; set; }
        public string address           { get; set; }
        public string minuteClinicHours { get; set; }
        public string lunchHours        { get; set; }
        public int    waitTime          { get; set; }
    }
}
