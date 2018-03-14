using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ValleyDreamsIndia.Common
{
    public class TreeStructure
    {
        public class Parent
        {
            public SelfDetails Detail { get; set; }

            public Children LeftChildren { get; set; }
            public Children RightChildren { get; set; }
        }

        public class Children
        {
            public SelfDetails Detail { get; set; }
            public SelfDetails LeftSubChildren { get; set; }
            public SelfDetails RightSubChildren { get; set; }
        }

        public class SelfDetails
        {
            public string UserName { get; set; }
            public string Name { get; set; }
            public string SponsorName { get; set; }
        }
    }
}