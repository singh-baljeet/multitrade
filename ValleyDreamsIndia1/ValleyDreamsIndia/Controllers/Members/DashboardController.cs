using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using System.Security.Claims;
using ValleyDreamsIndia.Common;

namespace ValleyDreamsIndia.Controllers.Members
{
    [CustomAuthorize]
    public class DashboardController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        Dictionary<string, int> pairDic = new Dictionary<string, int>();

        public DashboardController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
            pairDic = new Dictionary<string, int>()
            {
                { "2",1500 },
                {"16",5000 },
                {"38",27000 },
                {"129",55000 },
                {"500",100000 },
                {"1020",250000 },
                { "2040",700000 },
                {"3200",900000 },
                {"10000",1500000 },
                {"25000",2500000 },
                {"51000",4000000 },
                {"110000",7000000 },
                {"215000",10000000 }
            };
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Index()
        {
            var UserDetailsResults = _valleyDreamsIndiaDBEntities.UsersDetails.First(x => x.Id == CurrentUser.CurrentUserId);
            ViewBag.UserName = UserDetailsResults.UserName;
            var PersonalDetails = UserDetailsResults.PersonalDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId).FirstOrDefault();
            ViewBag.FullName = PersonalDetails.FirstName + " " + PersonalDetails.LastName;
            ViewBag.Status = (UserDetailsResults.Deleted ==0) ? "Active": "InActive";
            ViewBag.Sponsor = UserDetailsResults.UsersDetail1.UserName;
            ViewBag.DOJ = Convert.ToDateTime(UserDetailsResults.CreatedOn);

            ViewBag.DirectTeam = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.LegId != CurrentUser.CurrentUserId && x.Deleted == 0).Count();

            var myUserList = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.IsPinUsed == 1);

            int countLeftTeam = 0, countRightTeam = 0;

            var response = _valleyDreamsIndiaDBEntities.CountPlacementSideFunc(CurrentUser.CurrentUserId);
            foreach (var res in response)
            {
                countLeftTeam = Convert.ToInt32(res.LeftNodes);
                countRightTeam = Convert.ToInt32(res.RightNodes);
            }

            ViewBag.LeftTeam = countLeftTeam;
            ViewBag.RightTeam = countRightTeam;


            var countPairs = _valleyDreamsIndiaDBEntities.MemberRewardDetails.Where(x => x.UserDetailsId == CurrentUser.CurrentUserId).ToList();

            string achievedPairs = "";
            foreach(var pair in countPairs)
            {
                achievedPairs += pair.Pairs + ";";
            }

            achievedPairs = achievedPairs.Substring(0, achievedPairs.Length - 1);

            int totalIncome = 0;

            foreach(var dic in pairDic)
            {
                if (achievedPairs.Contains(dic.Key))
                {
                    totalIncome += dic.Value; 
                }
            }

            ViewBag.TotalIncome = totalIncome;

            ViewBag.MyTeam = Convert.ToInt32(ViewBag.LeftTeam) + Convert.ToInt32(ViewBag.RightTeam);

            ViewBag.NewPins = UserDetailsResults.UsersDetails1.Where(x => x.SponsoredId == CurrentUser.CurrentUserId
                                                && x.PinType == "NEW" && x.IsPinUsed == 0).Count();

            ViewBag.RenewalPins = UserDetailsResults.RenewalPinDetails.Where(x => x.SponsoredId == CurrentUser.CurrentUserId
                                               && x.IsPinUsed == 0).Count();
            ViewBag.Title = "Admin: Dashboard";
            return View("~/Views/Members/Dashboard/Index.cshtml");
        }
    }
}