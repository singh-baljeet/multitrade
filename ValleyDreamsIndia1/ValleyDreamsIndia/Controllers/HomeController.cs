using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ValleyDreamsIndia.Common;

namespace ValleyDreamsIndia.Controllers
{
    public class HomeController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;

        public HomeController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ErrorMessage = false;
            ViewBag.SmsStatusMessage = false;
            ViewBag.MobileErrorMessage = false;
            ViewBag.SuccessMessage = false;
            return View();
        }

        [HttpGet]
        public ActionResult achievers()
        {
            return View();
        }

        [HttpGet]
        public ActionResult plan()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Login(string Username, string Password)
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.UserName == Username && x.Password == Password && x.Deleted == 0).FirstOrDefault();
            if(userDetail != null)
            {
                    FormsAuthentication.SetAuthCookie(userDetail.UserName, false);
                    var authTicket = new FormsAuthenticationTicket(1, userDetail.UserName, DateTime.Now, DateTime.Now.AddMinutes(20), false, userDetail.Id.ToString());
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    HttpContext.Response.Cookies.Add(authCookie);
                    return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                ViewBag.ErrorMessage = true;
                return Json("Invalid Login Attempt", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            ViewBag.ErrorMessage = false;
            ViewBag.SmsStatusMessage = false;
            ViewBag.MobileErrorMessage = false;
            ViewBag.SuccessMessage = false;
            return View();
        }

        [HttpPost]
        public JsonResult ForgotPassword(string username, string mobilenumber)
        {
            UsersDetail usrDetail = _valleyDreamsIndiaDBEntities.UsersDetails
                                        .Where(x => x.UserName == username && x.Deleted == 0).FirstOrDefault();
            if (usrDetail != null)
            {
                PersonalDetail personalDetail = _valleyDreamsIndiaDBEntities.PersonalDetails
                    .Where(x => x.PhoneNumber1 == mobilenumber && x.UsersDetailsId == usrDetail.Id).FirstOrDefault();
                if (personalDetail != null)
                {
                    string fullName = personalDetail.FirstName + " " + personalDetail.LastName;
                    string userName = usrDetail.UserName;
                    string recoveredPassword = usrDetail.Password;
                    string textMessage = String.Format("Dear ({0}), Your password for the ({1}) is {2}", fullName, userName, recoveredPassword);
                    try
                    {
                        SmsProvider.SendSms(mobilenumber, textMessage);
                        ViewBag.SuccessMessage = true;
                        ViewBag.ErrorMessage = false;
                        ViewBag.SmsStatusMessage = false;
                        ViewBag.MobileErrorMessage = false;

                        return Json("Password Sent To Your Mobile Number", JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.SuccessMessage = false;
                        ViewBag.ErrorMessage = false;
                        ViewBag.MobileErrorMessage = false;
                        ViewBag.SmsStatusMessage = true;
                        return Json("Sms Sending Failed", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ViewBag.SuccessMessage = false;
                    ViewBag.SmsStatusMessage = false;
                    ViewBag.ErrorMessage = false;
                    ViewBag.MobileErrorMessage = true;
                    return Json("Entered Mobile Number Is Not Registered", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                ViewBag.SuccessMessage = false;
                ViewBag.SmsStatusMessage = false;
                ViewBag.MobileErrorMessage = false ;
                ViewBag.ErrorMessage = true;
                return Json("Invalid Login Attempt", JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult LogOut()
        {
            Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-1);
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}   