using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValleyDreamsIndia.Common;

namespace ValleyDreamsIndia.Controllers
{
    [CustomAuthorize]
    public class CheckContributionController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        public CheckContributionController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Index()
        {
            //var myUserList1 = _valleyDreamsIndiaDBEntities.ContributionDetails
            //    .Where(x => x.SponsoredId == CurrentUser.CurrentUserId);

            //List<IQueryable<PersonalDetail>> objList = new List<IQueryable<PersonalDetail>>();

            //var ownObj = _valleyDreamsIndiaDBEntities.PersonalDetails
            //    .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.LegId == CurrentUser.CurrentUserId);
            //objList.Add(ownObj);

            //foreach (var usr in myUserList1)
            //{
            //    var obj = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => x.UsersDetailsId == usr.UserDetailsId);
            //    objList.Add(obj);
            //}



            List<int> personalIdList = new List<int>();

            List<PersonalDetail> objList = new List<PersonalDetail>();

            var response = _valleyDreamsIndiaDBEntities.GetPlacementSideRecords((int)CurrentUser.CurrentUserId);
            foreach (var res in response)
            {
                personalIdList.Add(res.Value);
            }

            objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id)).ToList();


            GetUserInfo();

            ViewBag.ContributionPlacementSide = "";

            return View(objList);
        }

        private void  GetUserInfo()
        {

            var UserDetailsResults = _valleyDreamsIndiaDBEntities.UsersDetails.First(x => x.Id == CurrentUser.CurrentUserId);
            ViewBag.UserName = UserDetailsResults.UserName;
            var PersonalDetails = UserDetailsResults.PersonalDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId).FirstOrDefault();
            ViewBag.FullName = PersonalDetails.FirstName + " " + PersonalDetails.LastName;
            ViewBag.Sponsor = UserDetailsResults.UsersDetail1.UserName;
            var SponsorPersonalDetail = UserDetailsResults.UsersDetail1.PersonalDetails.FirstOrDefault();
            ViewBag.SponsorName = SponsorPersonalDetail.FirstName + " " + SponsorPersonalDetail.LastName;

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

        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult SearchByPlacementSide(string placementSide)
        {
            if(placementSide == "")
            {
                return RedirectToAction("Index");
            }
            //var myUserList = _valleyDreamsIndiaDBEntities.ContributionDetails
            //    .Where(x => x.SponsoredId == CurrentUser.CurrentUserId);

            //List<IQueryable<PersonalDetail>> objList = new List<IQueryable<PersonalDetail>>();

            //var ownObj = _valleyDreamsIndiaDBEntities.PersonalDetails
            //    .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.LegId == CurrentUser.CurrentUserId && x.PlacementSide== placementSide);
            //objList.Add(ownObj);

            //foreach (var usr in myUserList)
            //{
            //    var obj = _valleyDreamsIndiaDBEntities.PersonalDetails
            //        .Where(x => x.UsersDetailsId == usr.UserDetailsId  && x.PlacementSide == placementSide );
            //    objList.Add(obj);
            //}

            GetUserInfo();

            var count = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.LegId == CurrentUser.CurrentUserId
                && x.PlacementSide == placementSide).FirstOrDefault();

            List<int> personalIdList = new List<int>();
            List<PersonalDetail> objList = new List<PersonalDetail>();

            if (count != null)
            {
                if (placementSide == "LEFT")
                {
                    var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                    foreach (var res in response)
                    {
                        personalIdList.Add(res.Value);
                    }
                    objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id)).ToList();
                }
                if (placementSide == "RIGHT")
                {
                    var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                    foreach (var res in response)
                    {
                        personalIdList.Add(res.Value);
                    }
                    objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id)).ToList();
                }
            }

            ViewBag.ContributionPlacementSide = placementSide;

            return View("Index", objList);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult SearchByMemberId(string memberId)
        {
            if (memberId == "")
            {
                return RedirectToAction("Index");
            }

            List<PersonalDetail> objList = _valleyDreamsIndiaDBEntities.PersonalDetails.
                Where(x => x.UsersDetail.UserName == memberId).ToList();


            GetUserInfo();

            ViewBag.ContributionPlacementSide = "";
            return View("Index", objList);
        }

    }
}