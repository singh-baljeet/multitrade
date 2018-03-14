using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValleyDreamsIndia;
using ValleyDreamsIndia.Common;
using ValleyDreamsIndia.Models;

namespace ValleyDreamsIndia.Controllers.Members
{
    [CustomAuthorize]
    public class WalletController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        public WalletController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }

        private ActionResult TransferPinsData()
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.Id == CurrentUser.CurrentUserId && x.Deleted == 0).FirstOrDefault();
            PersonalDetail personalDetail = userDetail.PersonalDetails.First(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0);
            ViewBag.TotalJoiningPins = userDetail.UsersDetails1.Where(x => x.PinType == "NEW" && x.IsPinUsed == 0).Count();
            ViewBag.TotalRenewPins = userDetail.RenewalPinDetails.Where(x => x.IsPinUsed == 0).Count();
            if ((Convert.ToInt32(ViewBag.TotalJoiningPins) == 0) && (Convert.ToInt32(ViewBag.TotalRenewPins) == 0))
            {
                ViewBag.Message = "You have no new and renewal pins.";
                return View("~/Views/Members/Dashboard/NoPins.cshtml");
                //return Redirect(ControllerContext.HttpContext.Request.UrlReferrer.ToString());
            }
            return View("~/Views/Members/Wallet/Transfer.cshtml", new Tuple<UsersDetail, PersonalDetail>(userDetail, personalDetail));
        }

        [HttpGet]
        public ActionResult Transfer()
        {
            ViewBag.Title = "Admin: Wallet";
            try
            {
                return TransferPinsData();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Transfer(string sponsoredId, int totalPin, string pinType, string transactionPassword)
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.Id == CurrentUser.CurrentUserId && x.Deleted == 0).FirstOrDefault();


            int countPins = 0;
            if(pinType == "NEW")
            {
                countPins = userDetail.UsersDetails1.Where(x => x.PinType == pinType && x.IsPinUsed == 0).Count();
            }
            if(pinType == "RENEW")
            {
                countPins = userDetail.RenewalPinDetails.Where(x => x.IsPinUsed == 0).Count();
            }

            if (countPins != 0)
            {
                try
                {
                    var getUser = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.UserName == sponsoredId).FirstOrDefault();

                    var isTransactionPasswordExists = _valleyDreamsIndiaDBEntities.BankDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0 && x.TransactionPassword == transactionPassword).FirstOrDefault();
                    if (isTransactionPasswordExists != null)
                    {
                        if(pinType == "NEW")
                        {
                            var userDetailList = _valleyDreamsIndiaDBEntities.UsersDetails
                            .Where(x => x.SponsoredId == CurrentUser.CurrentUserId
                             && x.PinType == pinType && x.IsPinUsed == 0).Take(totalPin);
                            foreach (var usr in userDetailList)
                            {
                                usr.SponsoredId = getUser.Id;
                                _valleyDreamsIndiaDBEntities.Entry<UsersDetail>(usr).State = EntityState.Modified;

                            }
                        }
                        if(pinType == "RENEW")
                        {
                            var renewPinDetailList = _valleyDreamsIndiaDBEntities.RenewalPinDetails
                            .Where(x => x.SponsoredId == CurrentUser.CurrentUserId
                             && x.IsPinUsed == 0).Take(totalPin);
                            foreach (var renew in renewPinDetailList)
                            {
                                renew.SponsoredId = getUser.Id;
                                _valleyDreamsIndiaDBEntities.Entry<RenewalPinDetail>(renew).State = EntityState.Modified;
                            }
                        }
                        _valleyDreamsIndiaDBEntities.SaveChanges();
                    }

                 @ViewBag.Message = $"{pinType} type pins transfered successfully";
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                @ViewBag.Message = $"You have no {pinType} type pins";
                
            }
            return TransferPinsData();
        }
        
        [HttpGet]
        public JsonResult WalletCheckPins()
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.Id == CurrentUser.CurrentUserId && x.Deleted == 0).FirstOrDefault();

            int newPins = userDetail.UsersDetails1.Where(x => x.PinType == "NEW" && x.IsPinUsed == 0).Count();
            int renewPins = userDetail.RenewalPinDetails.Where(x => x.IsPinUsed == 0).Count();

            if(newPins >0 || renewPins > 0)
            {
                return Json("True", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("False", JsonRequestBehavior.AllowGet);
            }
        }
    }
}