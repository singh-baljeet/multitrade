using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValleyDreamsIndia.Common;
using ValleyDreamsIndia.Models;

namespace ValleyDreamsIndia.Controllers.Members
{
    [CustomAuthorize]
    public class RenewalController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        public RenewalController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }

        public ActionResult Index()
        {
            return View();
        }

        private UsersPersonalModelView GetContributionData(string memberid = "")
        {
            UsersPersonalModelView usersPersonalModelView = null;
            var IsRenewPinAvailable = _valleyDreamsIndiaDBEntities.RenewalPinDetails
                .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.IsPinUsed == 0).Count();
            if (IsRenewPinAvailable != 0)
            {
                ViewBag.Title = "Admin: Renewal";
                usersPersonalModelView = new UsersPersonalModelView();
                usersPersonalModelView.RenewalPinDetails = _valleyDreamsIndiaDBEntities
                    .RenewalPinDetails.Where(x => x.SponsoredId == CurrentUser.CurrentUserId).FirstOrDefault();
                usersPersonalModelView.UserDetails = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.Id == CurrentUser.CurrentUserId).FirstOrDefault();
                usersPersonalModelView.ContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails.Where(x => x.UserDetailsId == CurrentUser.CurrentUserId).OrderByDescending(x => x.NextContribNumber).FirstOrDefault();
                ViewBag.RenewalPinAvailable = IsRenewPinAvailable;

                if (memberid != null && memberid != "" && memberid != string.Empty)
                {
                    var otherMemberUserDetails = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.UserName == memberid).FirstOrDefault();
                    var otherMemberContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails.Where(x => x.UserDetailsId == otherMemberUserDetails.Id).OrderByDescending(x => x.NextContribNumber).FirstOrDefault();
                    ViewBag.OtherContributionNumber = otherMemberContributionDetails.NextContribNumber;
                    ViewBag.OtherContributionDate = otherMemberContributionDetails.NextContribDate;
                    ViewBag.OtherSponsoredId = otherMemberUserDetails.UsersDetail1.UserName;
                }
            }
            return usersPersonalModelView;
        }

        [HttpGet]
        public ActionResult Contribution(string memberid="")
        {
            UsersPersonalModelView usersPersonalModelView = GetContributionData(memberid);
            if(usersPersonalModelView != null)
            {
                return View("~/Views/Members/Renewal/Contribution.cshtml", usersPersonalModelView);
            }
            else
            {
                ViewBag.Message = "You have no renewal pins";
                return View("~/Views/Members/Dashboard/NoPins.cshtml");
                //return Redirect(ControllerContext.HttpContext.Request.UrlReferrer.ToString());
            }
        }

        [HttpPost]
        public ActionResult ContributionPost(string transactionPassword)
        {
            try
            {
                BankDetail bankDetail = _valleyDreamsIndiaDBEntities.BankDetails.First(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0);
                if (bankDetail.TransactionPassword == transactionPassword)
                {

                    ContributionDetail ContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails
                                                                                .Where(x => x.UserDetailsId == CurrentUser.CurrentUserId)
                                                                                .OrderByDescending(x => x.NextContribNumber).FirstOrDefault();

                    ContributionDetail contributionDetails = new ContributionDetail();
                    contributionDetails.ContribNumber = ContributionDetails.NextContribNumber;
                    contributionDetails.ContribDate = DateTime.Now;
                    contributionDetails.ContribAmount = 1000;
                    contributionDetails.NextContribNumber = ContributionDetails.NextContribNumber + 1;
                    contributionDetails.NextContribDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 20);
                    contributionDetails.RemainingContrib = 15 - ContributionDetails.NextContribNumber;
                    contributionDetails.UserDetailsId = CurrentUser.CurrentUserId;
                    contributionDetails.SponsoredId = bankDetail.UsersDetail.SponsoredId;
                    contributionDetails.PayedBy = "SELF";
                    contributionDetails.IsCompleted = (contributionDetails.ContribNumber != 15) ? 0 : 1;
                    _valleyDreamsIndiaDBEntities.ContributionDetails.Add(contributionDetails);
                    _valleyDreamsIndiaDBEntities.SaveChanges();


                    RenewalPinDetail renewalPinDetail = _valleyDreamsIndiaDBEntities.RenewalPinDetails.Where(x => x.SponsoredId == CurrentUser.CurrentUserId
                                               && x.IsPinUsed == 0).OrderBy(x => x.PinCreatedOn).FirstOrDefault();

                    renewalPinDetail.IsPinUsed = 1;
                    _valleyDreamsIndiaDBEntities.Entry(renewalPinDetail).State = System.Data.Entity.EntityState.Modified;
                    _valleyDreamsIndiaDBEntities.SaveChanges();


                    ViewBag.OwnRenewalMessage = "Renewal Transfer Successfully ";
                    UsersPersonalModelView usersPersonalModelView = GetContributionData();
                    return View("~/Views/Members/Renewal/Contribution.cshtml", usersPersonalModelView);

                }

                ViewBag.OwnRenewalMessage = "Mismatched Transaction Password";
                UsersPersonalModelView usersPersonalModelView1 = GetContributionData();
                return View("~/Views/Members/Renewal/Contribution.cshtml", usersPersonalModelView1);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult OtherContribution(string transactionPassword, string memberid)
        {
            try
            {
                if (transactionPassword == "" || memberid == "")
                {
                    return RedirectToAction("Contribution");
                }
                var otherMemberUserDetails = _valleyDreamsIndiaDBEntities.UsersDetails
                    .Where(x => x.UserName == memberid).FirstOrDefault();

                BankDetail bankDetail = _valleyDreamsIndiaDBEntities.BankDetails
                    .First(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0);

                if (bankDetail.TransactionPassword == transactionPassword)
                {

                    ContributionDetail ContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails
                                                                                .Where(x => x.UserDetailsId == otherMemberUserDetails.Id)
                                                                                .OrderByDescending(x => x.NextContribNumber).FirstOrDefault();

                    ContributionDetail contributionDetails = new ContributionDetail();
                    contributionDetails.ContribNumber = ContributionDetails.NextContribNumber;
                    contributionDetails.ContribDate = DateTime.Now;
                    contributionDetails.ContribAmount = 1000;
                    contributionDetails.NextContribNumber = ContributionDetails.NextContribNumber + 1;
                    contributionDetails.NextContribDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 20);
                    contributionDetails.RemainingContrib = 15 - ContributionDetails.NextContribNumber;
                    contributionDetails.UserDetailsId = otherMemberUserDetails.Id;
                    contributionDetails.SponsoredId = CurrentUser.CurrentUserId;
                    contributionDetails.PayedBy = "SPONSOR";
                    contributionDetails.IsCompleted = (contributionDetails.ContribNumber != 15) ? 0 : 1;
                    _valleyDreamsIndiaDBEntities.ContributionDetails.Add(contributionDetails);
                    _valleyDreamsIndiaDBEntities.SaveChanges();


                    RenewalPinDetail renewalPinDetails = _valleyDreamsIndiaDBEntities.RenewalPinDetails
                        .Where(x => x.SponsoredId == otherMemberUserDetails.Id && x.IsPinUsed == 0)
                        .OrderBy(x => x.PinCreatedOn).FirstOrDefault();

                    renewalPinDetails.IsPinUsed = 1;
                    _valleyDreamsIndiaDBEntities.Entry(renewalPinDetails).State = System.Data.Entity.EntityState.Modified;
                    _valleyDreamsIndiaDBEntities.SaveChanges();

                    return RedirectToAction("Contribution");
                }

                return RedirectToAction("Contribution");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public JsonResult RenewalCheckPins()
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.Id == CurrentUser.CurrentUserId && x.Deleted == 0).FirstOrDefault();

            int renewPins = userDetail.RenewalPinDetails1.Where(x => x.IsPinUsed == 0).Count();

            if (renewPins > 0)
            {
                return Json("True", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("False", JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetMemberDetailsById(string memberId)
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails
               .Where(x => x.UserName == memberId && x.Deleted == 0).FirstOrDefault();
            if (userDetail != null)
            {
                int renewPins = userDetail.RenewalPinDetails.Where(x => x.IsPinUsed == 0).Count();
                if (renewPins > 0)
                {
                    var otherMemberContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails
                        .Where(x => x.UserDetailsId == userDetail.Id)
                        .OrderByDescending(x => x.NextContribNumber).FirstOrDefault();

                    var jsonResult = new
                    {
                        ContributionNo = otherMemberContributionDetails.NextContribNumber,
                        ContributionDate = DateTime.Now.ToString("dd/MM/yyyy"),
                        SponsoredId = otherMemberContributionDetails.UsersDetail.UsersDetail1.UserName,
                        AvailableRenewalPins = renewPins
                    };

                    return Json(jsonResult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("False", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("False", JsonRequestBehavior.AllowGet);
        }



    }
}