using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DawnAccountGen.Classes
{
    internal class Settings
    {
        public required string CapsolverKey { get; set; }
        public required string CatchallDomain { get; set; }
        public required string TwoLetterCountryCode { get; set; }
        public required string DawnReferralCode { get; set; }
        public required string GrassReferralCode { get; set; }
        public required string InputProxyFile { get; set; }
        public required string OutputFolder { get; set; }
    }
}
