using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountGen.Classes
{
    internal class Settings
    {
        public required string CapsolverKey { get; set; }
        public required string CatchallDomain { get; set; }
        public required string TwoLetterCountryCode { get; set; }
        public required string DawnReferralCodesFile { get; set; }
        public required string GrassReferralCodesFile { get; set; }
        public required string InputProxyFile { get; set; }
        public required string InputLoggedInAccountsFile { get; set; }
        public required string InputEmails { get; set; }
        public required string OutputFolder { get; set; }
        public required string ImapHost { get; set; }
        public required string ImapPort { get; set; }
        public required string ImapUsername { get; set; }
        public required string ImapPassword { get; set; }
        public required int GenDelay { get; set; }
    }
}
