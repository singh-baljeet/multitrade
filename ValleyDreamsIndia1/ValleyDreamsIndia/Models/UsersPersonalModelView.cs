using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ValleyDreamsIndia.Models
{
    public class UsersPersonalModelView
    {
        public UsersDetail UserDetails { get; set; }
        public PersonalDetail PersonalDetails { get; set; }
        public BankDetail BankDetails { get; set; }
        public ContributionDetail ContributionDetails { get; set; }
        public RenewalPinDetail RenewalPinDetails { get; set; }
        public string JavascriptToRun { get; set; }
    }
}