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
        public DashboardController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
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
                .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.LegId != CurrentUser.CurrentUserId).Count();

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