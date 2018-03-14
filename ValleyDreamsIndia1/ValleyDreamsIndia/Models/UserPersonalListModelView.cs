using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ValleyDreamsIndia.Models
{
    public class UserPersonalListModelView
    {
        public PersonalDetail PersonalDetail { get; set; }
        public List<PersonalDetail> PersonalDetails { get; set; }
        public int Left  { get; set; }
        public int Right { get; set; }
        public int Direct { get; set; }
    }
}