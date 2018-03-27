using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
            int countLeftTeam = 0, countRightTeam = 0;

            var response = _valleyDreamsIndiaDBEntities.CountPlacementSideFunc(1);
            foreach (var res in response)
            {
                countLeftTeam = Convert.ToInt32(res.LeftNodes);
                countRightTeam = Convert.ToInt32(res.RightNodes);
            }

            ViewBag.UserConnected = countLeftTeam + countRightTeam;
            ViewBag.ProfileCompleted = Convert.ToInt32(ViewBag.UserConnected) - 30;
            ViewBag.OnlineUsers = Convert.ToInt32(ViewBag.UserConnected) - 50;
            ViewBag.HappyClients = Convert.ToInt32(ViewBag.UserConnected) - 20;

            return View();
        }

        
        [HttpGet]
        public ActionResult achievers()
        {
            List<AchieverDetail> achieverDetailList = _valleyDreamsIndiaDBEntities.AchieverDetails.ToList();
            return View(achieverDetailList);
        }


        [HttpGet]
        public ActionResult gallery()
        {
            List<GalleryDetail> galleryDetailList = _valleyDreamsIndiaDBEntities.GalleryDetails.ToList();
            return View(galleryDetailList);
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
                    CurrentUser.CurrentUserId = userDetail.Id;
                    //var authTicket = new FormsAuthenticationTicket(1, userDetail.UserName, DateTime.Now, DateTime.Now.AddMinutes(20), false, userDetail.Id.ToString());
                    //string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    //var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    //HttpContext.Response.Cookies.Add(authCookie);
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
        public JsonResult ContactUs(string name,string email,string phone,string message)
        {
            try
            {
                string phoneNumber1 = "919888540973,919646744247";
                string textMessage = String.Format("Contacted By :- {0}, \n Email Id : {1} \n Phone : {2} \n Message : {3}.",
                    name, email, phone, message);
                string smsStatus = SmsProvider.SendSms(phoneNumber1, textMessage);

                //const string SERVER = "relay-hosting.secureserver.net";
                //MailMessage oMail = new System.Web.Mail.MailMessage();
                //oMail.From = "emailaddress@domainname";
                //oMail.To = "emailaddress@domainname";
                //oMail.Subject = "Test email subject";
                //oMail.BodyFormat = MailFormat.Html; // enumeration
                //oMail.Priority = MailPriority.High; // enumeration
                //oMail.Body = "Sent at: " + DateTime.Now;
                //SmtpMail.SmtpServer = SERVER;
                //SmtpMail.Send(oMail);



                //MailMessage Msg = new MailMessage();
                //Msg.From = new MailAddress("baljeetpnf@gmail.com");
                //Msg.To.Add("bethuelinfo@gmail.com");
                //Msg.Subject = "Contact Us";
                //string msg = String.Format("Name : {0} , Email : {1} , Phone Number: {2} , Message : {3}", 
                //    name, email,phone, message);
                //Msg.Body = msg;
                //SmtpClient smtp = new SmtpClient();
                //smtp.Host = "smtp.gmail.com";
                //smtp.Port = 587;
                //smtp.Credentials = new System.Net.NetworkCredential("bethuelinfo@gmail.com", "iqbalbtl");
                //smtp.EnableSsl = true;
                //smtp.Send(Msg);
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

       
        [HttpPost]
        public ActionResult LogOut()
        {
            //Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-1);
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}   