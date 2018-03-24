using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ValleyDreamsIndia.Common;
using ValleyDreamsIndia.Models;
using static ValleyDreamsIndia.Common.TreeStructure;

namespace ValleyDreamsIndia.Controllers
{
    public class SuperAdminController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        string password = "";
        string transactionpassword = "";
        string username = "";
        string smsstatus = "";

        public SuperAdminController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }


        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.ErrorMessage = false;
            return View();
        }

        [HttpPost]
        public ActionResult Login(UsersDetail userDetail)
        {
            UsersDetail usrDetail = _valleyDreamsIndiaDBEntities.UsersDetails
                                        .Where(x => x.UserName == userDetail.UserName && x.Password == userDetail.Password && x.Deleted == -1).FirstOrDefault();
            if (usrDetail != null)
            {
                FormsAuthentication.SetAuthCookie(usrDetail.UserName, false);
                var authTicket = new FormsAuthenticationTicket(1, usrDetail.UserName, DateTime.Now
                    , DateTime.Now.AddMinutes(20), false, usrDetail.Id.ToString());
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                HttpContext.Response.Cookies.Add(authCookie);
                return RedirectToAction("Dashboard", "SuperAdmin");
            }
            else
            {
                ViewBag.ErrorMessage = true;
                return View();
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
        public ActionResult ForgotPassword(string username, string mobilenumber)
        {
            UsersDetail usrDetail = _valleyDreamsIndiaDBEntities.UsersDetails
                                        .Where(x => x.UserName == username && x.Deleted == -1).FirstOrDefault();
            if (usrDetail != null)
            {
                PersonalDetail personalDetail = _valleyDreamsIndiaDBEntities.PersonalDetails
                    .Where(x => x.PhoneNumber1 == mobilenumber && x.UsersDetailsId == usrDetail.Id).FirstOrDefault();
                if(personalDetail != null)
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
                        return View();
                    }
                    catch(Exception ex)
                    {
                        ViewBag.SuccessMessage = false;
                        ViewBag.ErrorMessage = false;
                        ViewBag.MobileErrorMessage = false;
                        ViewBag.SmsStatusMessage = true;
                        return View();
                    }
                }
                else
                {
                    ViewBag.SuccessMessage = false;
                    ViewBag.SmsStatusMessage = false;
                    ViewBag.ErrorMessage = false;
                    ViewBag.MobileErrorMessage = true;
                    return View();
                }
            }
            else
            {
                ViewBag.SuccessMessage = false;
                ViewBag.SmsStatusMessage = false;
                ViewBag.MobileErrorMessage = false;
                ViewBag.ErrorMessage = true;
                return View();
            }
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult CreateMember()
        {
            ViewBag.ShowMessage = false;

            
            ViewBag.Title = "SuperAdmin: Add New Member";
            UsersPersonalModelView usersPersonalModelView = new UsersPersonalModelView();
            return View(usersPersonalModelView);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult CreateMember(UsersPersonalModelView usersPersonalModelView)
        {

            //int serialNumber = Convert.ToInt32(_valleyDreamsIndiaDBEntities.UsersDetails.Max(x => x.SrNo).Value);

            UsersDetail userDetail = new UsersDetail();
            userDetail.SponsoredId = CurrentUser.CurrentUserId;
            userDetail.IsPinUsed = 1;
            userDetail.Password = Guid.NewGuid().ToString().Substring(0, 6);
            userDetail.Deleted = 0;
            userDetail.CreatedOn = DateTime.Now;
            userDetail.SrNo = 0;

            
            _valleyDreamsIndiaDBEntities.Entry(userDetail).State = EntityState.Added;
            _valleyDreamsIndiaDBEntities.SaveChanges();

            string sponserId = usersPersonalModelView.UserDetails.UserName;
            int legId = CurrentUser.CurrentUserId;

            if (sponserId != "" || sponserId.Length > 0)
            {
                legId = _valleyDreamsIndiaDBEntities.UsersDetails
                .Where(x => x.UserName == sponserId).FirstOrDefault().Id;
            }

            int lastleg = legId;
            if (usersPersonalModelView.PersonalDetails.PlacementSide == "LEFT")
            {
                var leftLegRes = _valleyDreamsIndiaDBEntities.GetLastLeftPlacementSideRecords(legId);
                if (leftLegRes != null)
                {
                    foreach (var left in leftLegRes)
                    {
                        if (left != null)
                        {
                            lastleg = left.Value;
                        }
                    }
                }
            }
            if (usersPersonalModelView.PersonalDetails.PlacementSide == "RIGHT")
            {
                var rightLegRes = _valleyDreamsIndiaDBEntities.GetLastRightPlacementSideRecords(legId);
                if (rightLegRes != null)
                {
                    foreach (var right in rightLegRes)
                    {
                        if (right != null)
                        {
                            lastleg = right.Value;
                        }
                    }
                }
            }
            

            usersPersonalModelView.PersonalDetails.UsersDetailsId = userDetail.Id;
            usersPersonalModelView.PersonalDetails.JoinedOn = DateTime.Now.ToString();
            usersPersonalModelView.PersonalDetails.CreatedOn = DateTime.Now;
            usersPersonalModelView.PersonalDetails.SponsoredId = CurrentUser.CurrentUserId;
            usersPersonalModelView.PersonalDetails.LegId = lastleg;
            usersPersonalModelView.PersonalDetails.Deleted = 0;
            usersPersonalModelView.PersonalDetails.ProfilePic = "/UploadedTeamImages/default1_avatar_small.png";
            _valleyDreamsIndiaDBEntities.PersonalDetails.Add(usersPersonalModelView.PersonalDetails);
            _valleyDreamsIndiaDBEntities.SaveChanges();


            usersPersonalModelView.BankDetails.UsersDetailsId = userDetail.Id;
            usersPersonalModelView.BankDetails.CreatedOn = DateTime.Now;
            usersPersonalModelView.BankDetails.TransactionPassword = Guid.NewGuid().ToString().Substring(0, 6);
            usersPersonalModelView.BankDetails.Deleted = 0;
            _valleyDreamsIndiaDBEntities.BankDetails.Add(usersPersonalModelView.BankDetails);
            _valleyDreamsIndiaDBEntities.SaveChanges();

            ContributionDetail contributionDetails = new ContributionDetail();
            contributionDetails.ContribNumber = 1;
            contributionDetails.ContribDate = DateTime.Now;
            contributionDetails.ContribAmount = 1000;
            contributionDetails.NextContribNumber = 2;
            contributionDetails.NextContribDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 20);
            contributionDetails.RemainingContrib = 15 - 1;
            contributionDetails.UserDetailsId = userDetail.Id;
            contributionDetails.SponsoredId = CurrentUser.CurrentUserId;
            contributionDetails.PayedBy = "ADMIN";
            contributionDetails.IsCompleted = 0;
            _valleyDreamsIndiaDBEntities.ContributionDetails.Add(contributionDetails);
            _valleyDreamsIndiaDBEntities.SaveChanges();


            ViewBag.ShowMessage = true;
            ViewBag.Username = username = userDetail.UserName;
            ViewBag.Password = password = userDetail.Password;
            ViewBag.TransactionPassword = transactionpassword = usersPersonalModelView.BankDetails.TransactionPassword;
            string fullname = usersPersonalModelView.PersonalDetails.FirstName + " " + usersPersonalModelView.PersonalDetails.LastName;
            string sponsorId = usersPersonalModelView.UserDetails.UserName;
            string srno = usersPersonalModelView.PersonalDetails.Id.ToString();

            string textMessage = String.Format("Welcome to Bethuel Multi Trade Pvt. Ltd. \n\n Dear ({0}), \n Sr. No : {1} \n Sponsor ID : {2} \n User ID : {3} \n Password : {4} \n Txn Password : {5}",
                fullname, srno, sponsorId, username, password, transactionpassword);

            string phoneNumber1 = usersPersonalModelView.PersonalDetails.PhoneNumber1;
            string phoneNumber2 = "919888540973,919646744247";
            string smsStatus = SmsProvider.SendSms(phoneNumber1, textMessage, phoneNumber2);
            if (smsStatus == "Success")
            {
                smsstatus = "Credentials Sent To Your Registered Mobile Number Successfully";
            }
            ViewBag.SmsStatus = smsstatus;

            UsersPersonalModelView usersPersonalModelView1 = new UsersPersonalModelView();
            return View("~/Views/SuperAdmin/CreateMember.cshtml", usersPersonalModelView1);
        }


        [CustomAuthorize]
        [HttpGet]
        public ActionResult AssignPin()
        {
            return View();
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult AssignPin(string sponsoredId, int totalPin, string pinType, string transactionPassword)
        {
            var getUser = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.UserName == sponsoredId).FirstOrDefault();

            var sender = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId).FirstOrDefault();

            var isTransactionPasswordExists = _valleyDreamsIndiaDBEntities.BankDetails
                .Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0 && x.TransactionPassword == transactionPassword).FirstOrDefault();
            if (isTransactionPasswordExists != null)
            {

                if (pinType == "NEW")
                {
                    UsersDetail userDetail = new UsersDetail();

                    for (int i = 1; i <= totalPin; i++)
                    {
                        userDetail.PinCreatedOn = DateTime.Now;
                        userDetail.Deleted = 0;
                        userDetail.SponsoredId = getUser.Id;
                        userDetail.IsPinUsed = 0;
                        userDetail.PinType = pinType;
                        _valleyDreamsIndiaDBEntities.UsersDetails.Add(userDetail);
                        _valleyDreamsIndiaDBEntities.SaveChanges();
                    }
                }
                if (pinType == "RENEW")
                {
                    RenewalPinDetail renewalPinDetail = new RenewalPinDetail();

                    for (int i = 1; i <= totalPin; i++)
                    {
                        renewalPinDetail.PinCreatedOn = DateTime.Now;
                        renewalPinDetail.Deleted = 0;
                        renewalPinDetail.SponsoredId = getUser.Id;
                        renewalPinDetail.IsPinUsed = 0;
                        _valleyDreamsIndiaDBEntities.RenewalPinDetails.Add(renewalPinDetail);
                        _valleyDreamsIndiaDBEntities.SaveChanges();
                    }
                }

                string receiverusername = sponsoredId;
                string receiverfullname = getUser.PersonalDetails
                    .Where(x => x.UsersDetailsId == getUser.Id)
                    .FirstOrDefault().FirstName + " " + getUser.PersonalDetails
                    .Where(x => x.UsersDetailsId == getUser.Id).FirstOrDefault().LastName;
                string senderusername = sender.UsersDetail.UserName;
                string receiverphonenumber = getUser.PersonalDetails
                    .Where(x => x.UsersDetailsId == getUser.Id).FirstOrDefault().PhoneNumber1;

                string textMessage = String.Format("Dear ({0}), ({1}) has sucessfully transferred {2} pins to the user ({3})",
                    receiverfullname, senderusername, totalPin, receiverusername);

                string phoneNumber2 = "919888540973,919646744247";
                string smsStatus = SmsProvider.SendSms(receiverphonenumber, textMessage, phoneNumber2);
                if (smsStatus == "Success")
                {
                    smsstatus = "Credentials Sent To Your Registered Mobile Number Successfully";
                }
                ViewBag.SmsStatus = smsstatus;


                @ViewBag.Message = $"{pinType} type pins transfered successfully";
            }
            else
            {
                ViewBag.Message = "Wrong transactional password entered";
            }
            
            return View();

        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult GetAllUsers()
        {
            ViewBag.Searched = "ALL";
            UserPersonalListModelView userPersonalListModelView = new UserPersonalListModelView();
            List<PersonalDetail> personalDetailList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x=>x.Deleted == 0).ToList();
            userPersonalListModelView.PersonalDetails = personalDetailList;
            return View(userPersonalListModelView);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult SearchByPlacementSide(string placementSide)
        {
            if (placementSide == "" || placementSide == String.Empty)
            {
                return RedirectToAction("GetAllUsers");
            }

            ViewBag.Title = "SuperAdmin: View Team";

            var count = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.LegId == CurrentUser.CurrentUserId
                && x.PlacementSide == placementSide && x.Deleted == 0).FirstOrDefault();

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
                    objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                }
                if (placementSide == "RIGHT")
                {
                    var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                    foreach (var res in response)
                    {
                        personalIdList.Add(res.Value);
                    }
                    objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                }
            }

            UserPersonalListModelView userPersonalListModelView = new UserPersonalListModelView();
            userPersonalListModelView.PersonalDetails = objList;

            ViewBag.Searched = placementSide;
            return View("GetAllUsers", userPersonalListModelView);
        }


        [CustomAuthorize]
        [HttpPost]
        public ActionResult SearchByMemberId(string memberId)
        {
            if(memberId == ""  || memberId == string.Empty || memberId == null)
            {
                return RedirectToAction("GetAllUsers");
            }
            UserPersonalListModelView userPersonalListModelView = new UserPersonalListModelView();
            List<PersonalDetail> personalDetailList = _valleyDreamsIndiaDBEntities.PersonalDetails.
                Where(x=>x.UsersDetail.UserName == memberId && x.Deleted == 0).ToList();
            userPersonalListModelView.PersonalDetails = personalDetailList;
            ViewBag.Searched = memberId;
            return View("GetAllUsers",userPersonalListModelView);
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult ViewProfile(int currentId = 0)
        {
            ViewBag.Title = "SuperAdmin: Profile";
            try
            {
                if (currentId == 0)
                {
                    return View(GetPersonalAndUserDetails(CurrentUser.CurrentUserId));
                }
                else
                {
                    return View(GetPersonalAndUserDetails(currentId));
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult EditProfile(int currentId = 0)
        {
            ViewBag.Title = "SuperAdmin: Profile Settings";
            try
            {
                if (currentId == 0)
                {
                    return View(GetPersonalAndUserDetails(CurrentUser.CurrentUserId));
                }
                else
                {
                    return View(GetPersonalAndUserDetails(currentId));
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult MemberEditProfile(int memberId)
        {
            ViewBag.Message = "";
            return View(GetPersonalAndUserDetails(memberId));
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult MemberEditProfile(UsersPersonalModelView usersPersonalModelView, HttpPostedFileBase memberImage)
        {
            ViewBag.Title = "SuperAdmin: Profile Settings";
            try
            {
                PersonalDetail personalDetails = _valleyDreamsIndiaDBEntities.PersonalDetails
                    .Where(x => x.UsersDetailsId == usersPersonalModelView.UserDetails.Id).FirstOrDefault();

                BankDetail bankDetails = _valleyDreamsIndiaDBEntities.BankDetails
                    .Where(x => x.UsersDetailsId == usersPersonalModelView.UserDetails.Id).FirstOrDefault();

                if (memberImage != null)
                {
                    string randomImageName = Guid.NewGuid().ToString().Substring(0, 5) + memberImage.FileName;
                    personalDetails.ProfilePic = "/UploadedTeamImages/" + randomImageName;
                    memberImage.SaveAs(Server.MapPath("~/UploadedTeamImages/") + randomImageName);
                }

                personalDetails.Gender = usersPersonalModelView.PersonalDetails.Gender;
                personalDetails.FirstName = usersPersonalModelView.PersonalDetails.FirstName;
                personalDetails.LastName = usersPersonalModelView.PersonalDetails.LastName;
                personalDetails.FatherName = usersPersonalModelView.PersonalDetails.FatherName;
                personalDetails.PhoneNumber1 = usersPersonalModelView.PersonalDetails.PhoneNumber1;
                personalDetails.BirthDate = usersPersonalModelView.PersonalDetails.BirthDate;
                personalDetails.PhoneNumber2 = usersPersonalModelView.PersonalDetails.PhoneNumber2;
                personalDetails.Email = usersPersonalModelView.PersonalDetails.Email;

                personalDetails.Address = usersPersonalModelView.PersonalDetails.Address;
                personalDetails.State = usersPersonalModelView.PersonalDetails.State;
                personalDetails.District = usersPersonalModelView.PersonalDetails.District;
                personalDetails.City = usersPersonalModelView.PersonalDetails.City;
                personalDetails.PinCode = usersPersonalModelView.PersonalDetails.PinCode;
                personalDetails.UpdatedOn = DateTime.Now;

                _valleyDreamsIndiaDBEntities.Entry(personalDetails).State = EntityState.Modified;


                bankDetails.NomineeName = usersPersonalModelView.BankDetails.NomineeName;
                bankDetails.NomineeRelation = usersPersonalModelView.BankDetails.NomineeRelation;
                bankDetails.BankName = usersPersonalModelView.BankDetails.BankName;
                bankDetails.AccountHolderName = usersPersonalModelView.BankDetails.AccountHolderName;
                bankDetails.AccountNumber = usersPersonalModelView.BankDetails.AccountNumber;
                bankDetails.IFSCCode = usersPersonalModelView.BankDetails.IFSCCode;
                bankDetails.PANNumber = usersPersonalModelView.BankDetails.PANNumber;
                bankDetails.UpdatedOn = DateTime.Now;
                _valleyDreamsIndiaDBEntities.Entry(bankDetails).State = EntityState.Modified;

                _valleyDreamsIndiaDBEntities.SaveChanges();

                ViewBag.Message = "Record Updated Successfully";
                return View(GetPersonalAndUserDetails(Convert.ToInt32(personalDetails.UsersDetailsId)));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        [CustomAuthorize]
        [HttpPost]
        public ActionResult EditProfile(UsersPersonalModelView usersPersonalModelView, HttpPostedFileBase memberImage)
        {
            ViewBag.Title = "SuperAdmin: Profile Settings";
            try
            {
                PersonalDetail personalDetails = _valleyDreamsIndiaDBEntities.PersonalDetails
                    .Where(x => x.UsersDetailsId == usersPersonalModelView.UserDetails.Id).FirstOrDefault();

                BankDetail bankDetails = _valleyDreamsIndiaDBEntities.BankDetails
                    .Where(x => x.UsersDetailsId == usersPersonalModelView.UserDetails.Id).FirstOrDefault();

                if (memberImage != null)
                {
                    string randomImageName = Guid.NewGuid().ToString().Substring(0, 5) + memberImage.FileName;
                    personalDetails.ProfilePic = "/UploadedTeamImages/" + randomImageName;
                    memberImage.SaveAs(Server.MapPath("~/UploadedTeamImages/") + randomImageName);
                }

                personalDetails.FirstName = usersPersonalModelView.PersonalDetails.FirstName;
                personalDetails.LastName = usersPersonalModelView.PersonalDetails.LastName;
                personalDetails.FatherName = usersPersonalModelView.PersonalDetails.FatherName;
                personalDetails.BirthDate = usersPersonalModelView.PersonalDetails.BirthDate;
                personalDetails.PhoneNumber1 = usersPersonalModelView.PersonalDetails.PhoneNumber1;
                personalDetails.PhoneNumber2 = usersPersonalModelView.PersonalDetails.PhoneNumber2;
                personalDetails.Email = usersPersonalModelView.PersonalDetails.Email;
                personalDetails.Address = usersPersonalModelView.PersonalDetails.Address;
                personalDetails.State = usersPersonalModelView.PersonalDetails.State;
                personalDetails.District = usersPersonalModelView.PersonalDetails.District;
                personalDetails.City = usersPersonalModelView.PersonalDetails.City;
                personalDetails.PinCode = usersPersonalModelView.PersonalDetails.PinCode;
                personalDetails.UpdatedOn = DateTime.Now;

                _valleyDreamsIndiaDBEntities.Entry(personalDetails).State = EntityState.Modified;


                bankDetails.NomineeName = usersPersonalModelView.BankDetails.NomineeName;
                bankDetails.NomineeRelation = usersPersonalModelView.BankDetails.NomineeRelation;
                bankDetails.BankName = usersPersonalModelView.BankDetails.BankName;
                bankDetails.AccountHolderName = usersPersonalModelView.BankDetails.AccountHolderName;
                bankDetails.AccountNumber = usersPersonalModelView.BankDetails.AccountNumber;
                bankDetails.IFSCCode = usersPersonalModelView.BankDetails.IFSCCode;
                bankDetails.PANNumber = usersPersonalModelView.BankDetails.PANNumber;
                bankDetails.UpdatedOn = DateTime.Now;
                _valleyDreamsIndiaDBEntities.Entry(bankDetails).State = EntityState.Modified;

                _valleyDreamsIndiaDBEntities.SaveChanges();

                return RedirectToAction("ViewProfile", new { currentId = usersPersonalModelView.UserDetails.Id });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private UsersPersonalModelView GetPersonalAndUserDetails(int userDetailsId)
        {
            UsersPersonalModelView usersPersonalModelView = new UsersPersonalModelView();
            usersPersonalModelView.UserDetails = _valleyDreamsIndiaDBEntities.UsersDetails
                .First(x => x.Id == userDetailsId);
            usersPersonalModelView.PersonalDetails = _valleyDreamsIndiaDBEntities.PersonalDetails
                .First(x => x.UsersDetailsId == userDetailsId && x.Deleted == 0);
            usersPersonalModelView.BankDetails = _valleyDreamsIndiaDBEntities.BankDetails
                .First(x => x.UsersDetailsId == userDetailsId && x.Deleted == 0);
            return usersPersonalModelView;
        }

        private void GetUserInfo(int currentId)
        {
            ViewBag.DirectTeam = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.SponsoredId == currentId && x.LegId != currentId).Count();

            var myUserList = _valleyDreamsIndiaDBEntities.UsersDetails.
                Where(x => x.SponsoredId == currentId && x.IsPinUsed == 1);

            int countLeftTeam = 0, countRightTeam = 0;

            var response = _valleyDreamsIndiaDBEntities.CountPlacementSideFunc(currentId);
            foreach (var res in response)
            {
                countLeftTeam = Convert.ToInt32(res.LeftNodes);
                countRightTeam = Convert.ToInt32(res.RightNodes);
            }

            ViewBag.LeftTeam = countLeftTeam;
            ViewBag.RightTeam = countRightTeam;

        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Tree()
        {
            Parent parentResult = TreeDetails(CurrentUser.CurrentUserId);
            return View(parentResult);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult TreeByUserId(string userId)
        {
            ViewBag.Title = "Admin: Tree";
            int currentUserId = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.UserName == userId).FirstOrDefault().Id;
            Parent parentResult = TreeDetails(currentUserId);
            return View("Tree", parentResult);
        }

        public Parent TreeDetails(int currentId)
        {
            var UserDetailsResults = _valleyDreamsIndiaDBEntities.UsersDetails.First(x => x.Id == currentId);
            ViewBag.UserName = UserDetailsResults.UserName;
            var PersonalDetails = UserDetailsResults.PersonalDetails.Where(x => x.UsersDetailsId == currentId && x.Deleted == 0).FirstOrDefault();
            ViewBag.FullName = PersonalDetails.FirstName + " " + PersonalDetails.LastName;
            ViewBag.Sponsor = UserDetailsResults.UsersDetail1.UserName;
            try
            {
                var SponsorPersonalDetail = UserDetailsResults.UsersDetail1.PersonalDetails.Where(x => x.Deleted == 0).FirstOrDefault();
                if(SponsorPersonalDetail != null)
                {
                    ViewBag.SponsorName = SponsorPersonalDetail.FirstName + " " + SponsorPersonalDetail.LastName;
                }
                else
                {
                    ViewBag.SponsorName = "NA";
                }
            }
            catch(Exception ex)
            {
                ViewBag.SponsorName = "NA";
            }

            GetUserInfo(currentId);

            var parentResult = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.UsersDetailsId == currentId && x.Deleted == 0)
                .Select(x => new TreeStructure.Parent
                {
                    Detail = new TreeStructure.SelfDetails
                    {
                        Name = x.FirstName + " " + x.LastName,
                        UserName = x.UsersDetail.UserName,
                        SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails.
                        Where(y => y.Id.ToString() == x.UsersDetail.SponsoredId.ToString()).FirstOrDefault().UserName
                    },
                }
            ).FirstOrDefault();

            var childernPlacementSide = _valleyDreamsIndiaDBEntities.PersonalDetails
                 .Where(x => x.LegId == currentId && x.Deleted == 0);
            foreach (var children in childernPlacementSide)
            {
                if (children.PlacementSide == "LEFT")
                {
                    parentResult.LeftChildren = new Children
                    {
                        Detail =
                        new TreeStructure.SelfDetails
                        {
                            Name = children.FirstName + " " + children.LastName,
                            UserName = children.UsersDetail.UserName,
                            SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails
                            .Where(y => y.Id == children.SponsoredId).FirstOrDefault().UserName
                        },
                    };

                    var leftSubChildernPlacementSide = _valleyDreamsIndiaDBEntities.PersonalDetails
                        .Where(x => x.LegId == children.UsersDetailsId && x.Deleted == 0);
                    foreach (var subChildren in leftSubChildernPlacementSide)
                    {
                        if (subChildren.PlacementSide == "LEFT")
                        {
                            parentResult.LeftChildren.LeftSubChildren =
                                new TreeStructure.SelfDetails
                                {
                                    Name = subChildren.FirstName + " " + subChildren.LastName,
                                    UserName = subChildren.UsersDetail.UserName,
                                    SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails
                                    .Where(y => y.Id == subChildren.SponsoredId).FirstOrDefault().UserName
                                };
                        }
                        if (subChildren.PlacementSide == "RIGHT")
                        {
                            parentResult.LeftChildren.RightSubChildren =
                                new TreeStructure.SelfDetails
                                {
                                    Name = subChildren.FirstName + " " + subChildren.LastName,
                                    UserName = subChildren.UsersDetail.UserName,
                                    SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails
                                    .Where(y => y.Id == subChildren.SponsoredId).FirstOrDefault().UserName
                                };
                        }
                    }
                }
                if (children.PlacementSide == "RIGHT")
                {
                    parentResult.RightChildren = new Children
                    {
                        Detail =
                        new TreeStructure.SelfDetails
                        {
                            Name = children.FirstName + " " + children.LastName,
                            UserName = children.UsersDetail.UserName,
                            SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails
                            .Where(y => y.Id == children.SponsoredId).FirstOrDefault().UserName
                        }
                    };

                    var rightSubChildernPlacementSide = _valleyDreamsIndiaDBEntities.PersonalDetails
                        .Where(x => x.LegId == children.UsersDetailsId && x.Deleted == 0);
                    foreach (var subChildren in rightSubChildernPlacementSide)
                    {
                        if (subChildren.PlacementSide == "LEFT")
                        {
                            parentResult.RightChildren.LeftSubChildren =
                                new TreeStructure.SelfDetails
                                {
                                    Name = subChildren.FirstName + " " + subChildren.LastName,
                                    UserName = subChildren.UsersDetail.UserName,
                                    SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails
                                    .Where(y => y.Id == subChildren.SponsoredId).FirstOrDefault().UserName
                                };
                        }
                        if (subChildren.PlacementSide == "RIGHT")
                        {
                            parentResult.RightChildren.RightSubChildren =
                                new TreeStructure.SelfDetails
                                {
                                    Name = subChildren.FirstName + " " + subChildren.LastName,
                                    UserName = subChildren.UsersDetail.UserName,
                                    SponsorName = _valleyDreamsIndiaDBEntities.UsersDetails
                                    .Where(y => y.Id == subChildren.SponsoredId).FirstOrDefault().UserName
                                };
                        }
                    }
                }
            }

            
            return parentResult;

        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Dashboard()
        {
            ViewBag.Title = "SuperAdmin: Dashboard";
            var UserDetailsResults = _valleyDreamsIndiaDBEntities.UsersDetails.First(x => x.Id == CurrentUser.CurrentUserId);
            ViewBag.UserName = UserDetailsResults.UserName;
            var PersonalDetails = UserDetailsResults.PersonalDetails
                .Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId).FirstOrDefault();
            ViewBag.FullName = PersonalDetails.FirstName + " " + PersonalDetails.LastName;
            ViewBag.Status = (UserDetailsResults.Deleted == -1) ? "Active" : "InActive";
            ViewBag.Sponsor = UserDetailsResults.UsersDetail1.UserName;
            ViewBag.DOJ = Convert.ToDateTime(UserDetailsResults.CreatedOn).ToString("dd/MM/yyyy");

            ViewBag.DirectTeam = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.LegId != CurrentUser.CurrentUserId && x.Deleted == 0).Count();

            var myUserList = _valleyDreamsIndiaDBEntities.UsersDetails
                .Where(x => x.SponsoredId == CurrentUser.CurrentUserId && x.IsPinUsed == 1);

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
            
            return View();
        }


        [CustomAuthorize]
        [HttpPost]
        public ActionResult Print(string hdnSearched)
        {
            UserPersonalListModelView userPersonalListModelView = new UserPersonalListModelView();
            List<PersonalDetail> personalDetailList = new List<PersonalDetail>();
            if (hdnSearched == "ALL")
            {
                personalDetailList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x=>x.Deleted==0).ToList();
                ViewBag.PlacementSidePrint = "";
            }
            else if(hdnSearched == "LEFT" || hdnSearched == "RIGHT")
            {
                var count = _valleyDreamsIndiaDBEntities.PersonalDetails
                .Where(x => x.LegId == CurrentUser.CurrentUserId
                && x.PlacementSide == hdnSearched).FirstOrDefault();

                List<int> personalIdList = new List<int>();
                List<PersonalDetail> objList = new List<PersonalDetail>();

                if (count != null)
                {
                    if (hdnSearched == "LEFT")
                    {
                        var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                        foreach (var res in response)
                        {
                            personalIdList.Add(res.Value);
                        }
                        ViewBag.PlacementSidePrint = "LEFT";
                        objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                    }
                    if (hdnSearched == "RIGHT")
                    {
                        var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                        foreach (var res in response)
                        {
                            personalIdList.Add(res.Value);
                        }
                        ViewBag.PlacementSidePrint = "RIGHT";
                        objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                    }
                }

                personalDetailList = objList;
            }
            
            else
            {
                personalDetailList = _valleyDreamsIndiaDBEntities.PersonalDetails.
                Where(x => x.UsersDetail.UserName == hdnSearched && x.Deleted == 0).ToList();
                ViewBag.PlacementSidePrint = "";
            }

            userPersonalListModelView.PersonalDetails = personalDetailList;
            return View(userPersonalListModelView);
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            ViewBag.Title = "SuperAdmin: Change Password";
            ViewBag.ErrorMessage = false;
            ViewBag.TransactionErrorMessage = false;
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmNewPassword)
        {
            ViewBag.TransactionErrorMessage = false;
            ViewBag.TransactionError = "";
            try
            {
                if (NewPassword == ConfirmNewPassword)
                {
                    UsersDetail usersDetail = _valleyDreamsIndiaDBEntities.UsersDetails.First(x => x.Id == CurrentUser.CurrentUserId && x.Deleted == -1);
                    if (usersDetail.Password == OldPassword)
                    {
                        usersDetail.Password = NewPassword;
                        usersDetail.UpdatedOn = DateTime.Now;
                        _valleyDreamsIndiaDBEntities.Entry(usersDetail).State = EntityState.Modified;
                        _valleyDreamsIndiaDBEntities.SaveChanges();

                        string phoneNumber = (usersDetail.PersonalDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId).FirstOrDefault()).PhoneNumber1;

                        string textMessage = String.Format("Your new password is {0}", NewPassword);

                        string smsStatus = SmsProvider.SendSms(phoneNumber, textMessage);
                        if (smsStatus == "Success")
                        {
                            ViewBag.SmsStatus = "New Password Sent To Your Registered Mobile Number Successfully";
                        }
                        else
                        {
                            ViewBag.SmsStatus = "Sended Sms Failed";
                        }

                        ViewBag.ErrorMessage = true;
                        ViewBag.Error = "Password Updated";
                        return View();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = true;
                        ViewBag.Error = "Old Password Mismatched";
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = true;
                    ViewBag.Error = "New And Confirm New Password Mismatched";
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [Route("SuperAdmin/transactionpassword")]
        [HttpPost]
        public ActionResult ChangePassword(string OldTransactionPassword, string NewTransactionPassword, string ConfirmTransactionNewPassword, string  isTransaction)
        {
            ViewBag.Title = "SuperAdmin: Change Password";
            ViewBag.ErrorMessage = false;

            try
            {
                if (NewTransactionPassword == ConfirmTransactionNewPassword)
                {
                    BankDetail bankDetail = _valleyDreamsIndiaDBEntities.BankDetails.First(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0);
                    if (bankDetail.TransactionPassword == OldTransactionPassword)
                    {
                        bankDetail.TransactionPassword = NewTransactionPassword;
                        bankDetail.UpdatedOn = DateTime.Now;
                        _valleyDreamsIndiaDBEntities.Entry(bankDetail).State = EntityState.Modified;
                        _valleyDreamsIndiaDBEntities.SaveChanges();
                        ViewBag.TransactionErrorMessage = true;

                        string phoneNumber = (_valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId).FirstOrDefault()).PhoneNumber1;

                        string textMessage = String.Format("Your new transaction password is {0}", NewTransactionPassword);

                        string smsStatus = SmsProvider.SendSms(phoneNumber, textMessage);
                        if (smsStatus == "Success")
                        {
                            ViewBag.SmsStatus = "New Transaction Password Sent To Your Registered Mobile Number Successfully";
                        }
                        else
                        {
                            ViewBag.SmsStatus = "Sended Sms Failed";
                        }

                        ViewBag.TransactionError = "Transaction Password Updated";
                        return View();
                    }
                    else
                    {
                        ViewBag.TransactionErrorMessage = true;
                        ViewBag.TransactionError = "Transaction Old Password Mismatched";
                        return View();
                    }
                }
                else
                {
                    ViewBag.TransactionErrorMessage = true;
                    ViewBag.TransactionError = "New And Confirm Transaction New Password Mismatched";
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Contribution()
        {
            List<int> personalIdList = new List<int>();

            List<PersonalDetail> objList = new List<PersonalDetail>();

            var response = _valleyDreamsIndiaDBEntities.GetPlacementSideRecords((int)CurrentUser.CurrentUserId);
            foreach (var res in response)
            {
                personalIdList.Add(res.Value);
            }

            objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();

            ViewBag.ContributionSearchedPlacementSide = "";
            
            GetUserInfo();

            return View(objList);
        }

        private void GetUserInfo()
        {

            var UserDetailsResults = _valleyDreamsIndiaDBEntities.UsersDetails.First(x => x.Id == CurrentUser.CurrentUserId);
            ViewBag.UserName = UserDetailsResults.UserName;
            var PersonalDetails = UserDetailsResults.PersonalDetails.Where(x => x.UsersDetailsId == CurrentUser.CurrentUserId && x.Deleted == 0).FirstOrDefault();
            ViewBag.FullName = PersonalDetails.FirstName + " " + PersonalDetails.LastName;
            ViewBag.Sponsor = UserDetailsResults.UsersDetail1.UserName;
            var SponsorPersonalDetail = UserDetailsResults.UsersDetail1.PersonalDetails.Where(x=>x.Deleted ==0).FirstOrDefault();
            if(SponsorPersonalDetail != null)
            {
                ViewBag.SponsorName = SponsorPersonalDetail.FirstName + " " + SponsorPersonalDetail.LastName;
            }
            

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

        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult SearchContributionByPlacementSide(string placementSide)
        {
            if (placementSide == "")
            {
                return RedirectToAction("Contribution");
            }

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
                    objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                }
                if (placementSide == "RIGHT")
                {
                    var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                    foreach (var res in response)
                    {
                        personalIdList.Add(res.Value);
                    }
                    objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                }
            }

            GetUserInfo();

            ViewBag.ContributionSearchedPlacementSide = placementSide;
            return View("Contribution", objList);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult SearchContributionByMemberId(string memberId)
        {
            if (memberId == "")
            {
                return RedirectToAction("Contribution");
            }

            List<PersonalDetail> objList = _valleyDreamsIndiaDBEntities.PersonalDetails.
               Where(x => x.UsersDetail.UserName == memberId && x.Deleted == 0).ToList();

            GetUserInfo();

            ViewBag.ContributionSearchedPlacementSide = memberId;
            return View("Contribution", objList);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult ContributionPrint(string hdnContributionSearched)
        {
            List<PersonalDetail> personalDetailList = new List<PersonalDetail>();
            if (hdnContributionSearched == "")
            {
                List<int> personalIdList = new List<int>();

                List<PersonalDetail> objList = new List<PersonalDetail>();

                var response = _valleyDreamsIndiaDBEntities.GetPlacementSideRecords((int)CurrentUser.CurrentUserId);
                foreach (var res in response)
                {
                    personalIdList.Add(res.Value);
                }
                ViewBag.PlacementSideContributionPrint = "";
                objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                personalDetailList = objList;

            }
            else if (hdnContributionSearched == "LEFT" || hdnContributionSearched == "RIGHT")
            {
                var count = _valleyDreamsIndiaDBEntities.PersonalDetails
               .Where(x => x.LegId == CurrentUser.CurrentUserId
               && x.PlacementSide == hdnContributionSearched).FirstOrDefault();

                List<int> personalIdList = new List<int>();
                List<PersonalDetail> objList = new List<PersonalDetail>();

                if (count != null)
                {
                    if (hdnContributionSearched == "LEFT")
                    {
                        var response = _valleyDreamsIndiaDBEntities.GetLeftSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                        foreach (var res in response)
                        {
                            personalIdList.Add(res.Value);
                        }
                        ViewBag.PlacementSideContributionPrint = "LEFT";
                        objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                    }
                    if (hdnContributionSearched == "RIGHT")
                    {
                        var response = _valleyDreamsIndiaDBEntities.GetRightSidePlacementRecords(count.UsersDetailsId, (int)CurrentUser.CurrentUserId);
                        foreach (var res in response)
                        {
                            personalIdList.Add(res.Value);
                        }
                        ViewBag.PlacementSideContributionPrint = "RIGHT";
                        objList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => personalIdList.Contains(x.Id) && x.Deleted == 0).ToList();
                    }
                }

                personalDetailList = objList;
            }

            else
            {
                List<PersonalDetail> objList = _valleyDreamsIndiaDBEntities.PersonalDetails.
               Where(x => x.UsersDetail.UserName == hdnContributionSearched && x.Deleted == 0).ToList();
                ViewBag.PlacementSideContributionPrint = "";
                personalDetailList = objList;
            }

            return View(personalDetailList);
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult RenewalContribution(string memberid = "")
        {
            UsersPersonalModelView usersPersonalModelView = GetContributionData(memberid);
            return View(usersPersonalModelView);
        }

        private UsersPersonalModelView GetContributionData(string memberid = "")
        {
            UsersPersonalModelView usersPersonalModelView = null;

            ViewBag.Title = "SuperAdmin: Renewal";
            usersPersonalModelView = new UsersPersonalModelView();
            usersPersonalModelView.RenewalPinDetails = _valleyDreamsIndiaDBEntities.RenewalPinDetails.Where(x => x.SponsoredId == CurrentUser.CurrentUserId).FirstOrDefault();
            if (usersPersonalModelView.RenewalPinDetails == null)
            {
                usersPersonalModelView.RenewalPinDetails = new RenewalPinDetail();
            }
            usersPersonalModelView.UserDetails = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.Id == CurrentUser.CurrentUserId).FirstOrDefault();

            usersPersonalModelView.ContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails.Where(x => x.UserDetailsId == CurrentUser.CurrentUserId).OrderByDescending(x => x.NextContribNumber).FirstOrDefault();
            if (usersPersonalModelView.ContributionDetails == null)
            {
                usersPersonalModelView.ContributionDetails = new ContributionDetail();
            }

            if (memberid != null && memberid != "" && memberid != string.Empty)
            {
                var otherMemberUserDetails = _valleyDreamsIndiaDBEntities.UsersDetails.Where(x => x.UserName == memberid).FirstOrDefault();
                var otherMemberContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails.Where(x => x.UserDetailsId == otherMemberUserDetails.Id).OrderByDescending(x => x.NextContribNumber).FirstOrDefault();
                ViewBag.OtherContributionNumber = otherMemberContributionDetails.NextContribNumber;
                ViewBag.OtherContributionDate = otherMemberContributionDetails.NextContribDate;
                ViewBag.OtherSponsoredId = otherMemberUserDetails.UsersDetail1.UserName;
            }

            return usersPersonalModelView;
        }

        [CustomAuthorize]
        [HttpGet]
        public JsonResult GetMemberDetailsById(string memberId)
        {
            UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails
                .Where(x => x.UserName == memberId && x.Deleted == 0).FirstOrDefault();
            if (userDetail != null)
            {
                int renewPins = userDetail.RenewalPinDetails.Where(x => x.IsPinUsed == 0).Count();
                if (renewPins >= 0)
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
            }
            return Json("False", JsonRequestBehavior.AllowGet);
        }


        [CustomAuthorize]
        [HttpPost]
        public ActionResult OtherContribution(string transactionPassword, string memberid)
        {
            try
            {
                if(transactionpassword == "" || memberid == "")
                {
                    return RedirectToAction("RenewalContribution");
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
                    contributionDetails.PayedBy = "ADMIN";
                    contributionDetails.IsCompleted = (contributionDetails.ContribNumber != 15) ? 0 : 1;
                    _valleyDreamsIndiaDBEntities.ContributionDetails.Add(contributionDetails);
                    _valleyDreamsIndiaDBEntities.SaveChanges();


                    RenewalPinDetail renewalPinDetails = _valleyDreamsIndiaDBEntities.RenewalPinDetails
                        .Where(x => x.SponsoredId == otherMemberUserDetails.Id && x.IsPinUsed == 0)
                        .OrderBy(x => x.PinCreatedOn).FirstOrDefault();

                    renewalPinDetails.IsPinUsed = 1;
                    _valleyDreamsIndiaDBEntities.Entry(renewalPinDetails).State = System.Data.Entity.EntityState.Modified;
                    _valleyDreamsIndiaDBEntities.SaveChanges();

                    return RedirectToAction("RenewalContribution");
                }

                return RedirectToAction("RenewalContribution");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [CustomAuthorize]
        [HttpGet]
        public ActionResult Gallery()
        {
            List<GalleryDetail> galleryList = _valleyDreamsIndiaDBEntities.GalleryDetails.ToList();
            return View(galleryList);
        }


        [CustomAuthorize]
        [HttpPost]
        public ActionResult Gallery(HttpPostedFileBase galleryImage)
        {
            GalleryDetail galleryDetail = new GalleryDetail();

            if (galleryImage != null)
            {
                string randomImageName = Guid.NewGuid().ToString().Substring(0, 5) + galleryImage.FileName;
                galleryDetail.Pic = "/GalleryImages/" + randomImageName;
                galleryImage.SaveAs(Server.MapPath("~/GalleryImages/") + randomImageName);
            }

            galleryDetail.CreatedOn = DateTime.Now;
            galleryDetail.Deleted = 0;

            _valleyDreamsIndiaDBEntities.Entry(galleryDetail).State = EntityState.Added;
            _valleyDreamsIndiaDBEntities.SaveChanges();

            ViewBag.Message = "Gallery Detail Submitted Successfully";
            List<GalleryDetail> galleryDetailList = _valleyDreamsIndiaDBEntities.GalleryDetails.ToList();
            return View(galleryDetailList);
        }


        [CustomAuthorize]
        [HttpGet]
        public ActionResult DeleteGallery(int galleryId)
        {
            GalleryDetail galleryDetail = _valleyDreamsIndiaDBEntities.GalleryDetails.Where(x => x.Id == galleryId).FirstOrDefault();
            if (galleryDetail != null)
            {
                _valleyDreamsIndiaDBEntities.Entry(galleryDetail).State = EntityState.Deleted;
                _valleyDreamsIndiaDBEntities.SaveChanges();
            }

            return RedirectToAction("Gallery");
        }


        [CustomAuthorize]
        [HttpGet]
        public ActionResult Achiever()
        {
            List<AchieverDetail> achieverList = _valleyDreamsIndiaDBEntities.AchieverDetails.OrderByDescending(x=>x.Id).Take(12).ToList();
            return View(achieverList);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult Achiever(string drawDate, HttpPostedFileBase achieverImage)
        {
            AchieverDetail achieverDetail = new AchieverDetail();

            if (achieverImage != null)
            {
                string randomImageName = Guid.NewGuid().ToString().Substring(0, 5) + achieverImage.FileName;
                achieverDetail.AchieverImage = "/AchieverImages/" + randomImageName;
                achieverImage.SaveAs(Server.MapPath("~/AchieverImages/") + randomImageName);
            }
            
            achieverDetail.Month = Convert.ToDateTime(drawDate).Month.ToString();
            achieverDetail.Year = Convert.ToDateTime(drawDate).Year.ToString();
            achieverDetail.CreatedOn = Convert.ToDateTime(drawDate);
            achieverDetail.Deleted = 0;

            _valleyDreamsIndiaDBEntities.Entry(achieverDetail).State = EntityState.Added;
            _valleyDreamsIndiaDBEntities.SaveChanges();

            ViewBag.Message = "Achiever Detail Submitted Successfully";
            List<AchieverDetail> achieverList = _valleyDreamsIndiaDBEntities.AchieverDetails.OrderByDescending(x => x.Id).Take(12).ToList();
            return View(achieverList);
        }


        [CustomAuthorize]
        [HttpGet]
        public ActionResult DeleteAchiever(int achieverId)
        {
            AchieverDetail achieverDetail = _valleyDreamsIndiaDBEntities.AchieverDetails.Where(x => x.Id == achieverId).FirstOrDefault();
            if(achieverDetail != null)
            {
                _valleyDreamsIndiaDBEntities.Entry(achieverDetail).State = EntityState.Deleted;
                _valleyDreamsIndiaDBEntities.SaveChanges();
            }

            return RedirectToAction("Achiever");
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult ViewReward(int memberId)
        {
            int LeftTeamCount = 0;
            int RightTeamCount = 0;
            var response = _valleyDreamsIndiaDBEntities.CountPlacementSideFunc(memberId);

            foreach (var res in response)
            {
                LeftTeamCount = Convert.ToInt32(res.LeftNodes);
                RightTeamCount = Convert.ToInt32(res.RightNodes);
            }

            List<MemberRewardDetail> memberRewardList = new List<MemberRewardDetail>();

            if (LeftTeamCount >= 215000 && RightTeamCount >= 215000)
            {
                var result = getMemberRewardList(215000, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }

            }
            if (LeftTeamCount >= 110000 && RightTeamCount >= 110000)
            {
                var result = getMemberRewardList(110000, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 51000 && RightTeamCount >= 51000)
            {
                var result = getMemberRewardList(51000,memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 25000 && RightTeamCount >= 25000)
            {
                var result = getMemberRewardList(25000,memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 10000 && RightTeamCount >= 10000)
            {
                var result = getMemberRewardList(10000, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 3200 && RightTeamCount >= 3200)
            {
                var result = getMemberRewardList(3200, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 2040 && RightTeamCount >= 2040)
            {
                var result = getMemberRewardList(2040,memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 1020 && RightTeamCount >= 1020)
            {
                var result = getMemberRewardList(1020, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 500 && RightTeamCount >= 500)
            {
                var result = getMemberRewardList(500, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 129 && RightTeamCount >= 129)
            {
                var result = getMemberRewardList(129, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }

            }
            if (LeftTeamCount >= 38 && RightTeamCount >= 38)
            {
                var result = getMemberRewardList(38, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 16 && RightTeamCount >= 16)
            {
                var result = getMemberRewardList(16, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 2 && RightTeamCount >= 2)
            {
                var result = getMemberRewardList(2, memberId);
                if (result.Count != 0)
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }

            List<MemberRewardModel> memberRewardModelList = new List<MemberRewardModel>();

            List<RewardDetail> rewardList = _valleyDreamsIndiaDBEntities.RewardDetails.ToList();

            foreach (var reward in rewardList)
            {
                if (memberRewardList.Where(x => x.Pairs == reward.Pairs).Count() > 0)
                {
                    memberRewardModelList.Add(new MemberRewardModel
                    {
                        RewardDetails = reward,
                        MemberRewardDetails = memberRewardList.Where(x => x.Pairs == reward.Pairs).FirstOrDefault()
                    });
                }
                else
                {
                    memberRewardModelList.Add(new MemberRewardModel
                    {
                        RewardDetails = reward,
                        MemberRewardDetails = new MemberRewardDetail
                        {
                            Status = "Not Achieved",
                            PaidStatus = "Not Paid",
                            PaidDate = null,
                            PaidRemarks = "A/C Cleared"
                        }
                    });
                }
            }

            return View(memberRewardModelList);
        }
        private List<MemberRewardDetail> getMemberRewardList(int paris, int memberId)
        {
            return _valleyDreamsIndiaDBEntities.MemberRewardDetails
                                                 .Where(x => x.UserDetailsId == memberId
                                                 && x.Pairs == paris).ToList();
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult ClearPayment(int userid,int pairs, string paidstatus, string paiddate,string paidremarks) 
        {
            MemberRewardDetail memberReward = _valleyDreamsIndiaDBEntities.MemberRewardDetails
                                                                        .Where(x => x.UserDetailsId == userid && x.Pairs == pairs).FirstOrDefault();

            memberReward.PaidStatus = paidstatus;
            memberReward.PaidDate = Convert.ToDateTime(paiddate);
            memberReward.PaidRemarks = paidremarks;

            _valleyDreamsIndiaDBEntities.Entry(memberReward).State = EntityState.Modified;
            _valleyDreamsIndiaDBEntities.SaveChanges();

            return RedirectToAction("ViewReward", new { memberId = memberReward.UserDetailsId });

        }


        [CustomAuthorize]
        [HttpGet]
        public ActionResult GetAllDeletedUsers()
        {
            UserPersonalListModelView userPersonalListModelView = new UserPersonalListModelView();
            List<PersonalDetail> personalDetailList = _valleyDreamsIndiaDBEntities.PersonalDetails.Where(x => x.Deleted == 1).ToList();
            userPersonalListModelView.PersonalDetails = personalDetailList;
            return View(userPersonalListModelView);
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult MemberEditDeletedProfile(int memberId)
        {
            ViewBag.ShowMessage = false;
            ViewBag.Message = "";
            return View(GetPersonalDeletedAndUserDetails(memberId));
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult MemberEditDeletedProfile(UsersPersonalModelView usersPersonalModelView, HttpPostedFileBase memberImage)
        {
            ViewBag.Title = "SuperAdmin: Profile Settings";
            try
            {
                string sponserId = usersPersonalModelView.UserDetails.UserName;
                int legId = CurrentUser.CurrentUserId;

                if (sponserId != "" || sponserId.Length > 0)
                {
                    legId = _valleyDreamsIndiaDBEntities.UsersDetails
                    .Where(x => x.UserName == sponserId).FirstOrDefault().Id;
                }

                int lastleg = legId;
                if (usersPersonalModelView.PersonalDetails.PlacementSide == "LEFT")
                {
                    var leftLegRes = _valleyDreamsIndiaDBEntities.GetLastLeftPlacementSideRecords(legId);
                    if (leftLegRes != null)
                    {
                        foreach (var left in leftLegRes)
                        {
                            if (left != null)
                            {
                                lastleg = left.Value;
                            }
                        }
                    }
                }
                if (usersPersonalModelView.PersonalDetails.PlacementSide == "RIGHT")
                {
                    var rightLegRes = _valleyDreamsIndiaDBEntities.GetLastRightPlacementSideRecords(legId);
                    if (rightLegRes != null)
                    {
                        foreach (var right in rightLegRes)
                        {
                            if (right != null)
                            {
                                lastleg = right.Value;
                            }
                        }
                    }
                }

                UsersDetail userDetail = _valleyDreamsIndiaDBEntities.UsersDetails
                    .Where(x => x.Id == usersPersonalModelView.UserDetails.Id).FirstOrDefault();

                userDetail.Password = Guid.NewGuid().ToString().Substring(0, 6);
                userDetail.SponsoredId = CurrentUser.CurrentUserId;
                userDetail.PinType = null;
                userDetail.IsPinUsed = 1;
                userDetail.CreatedOn = DateTime.UtcNow;
                _valleyDreamsIndiaDBEntities.Entry(userDetail).State = EntityState.Modified;
                _valleyDreamsIndiaDBEntities.SaveChanges();

                PersonalDetail personalDetails = _valleyDreamsIndiaDBEntities.PersonalDetails
                    .Where(x => x.UsersDetailsId == usersPersonalModelView.UserDetails.Id && x.Deleted == 1).FirstOrDefault();

                
                if (memberImage != null)
                {
                    string randomImageName = Guid.NewGuid().ToString().Substring(0, 5) + memberImage.FileName;
                    personalDetails.ProfilePic = "/UploadedTeamImages/" + randomImageName;
                    memberImage.SaveAs(Server.MapPath("~/UploadedTeamImages/") + randomImageName);
                }

                personalDetails.SponsoredId = CurrentUser.CurrentUserId;
                personalDetails.JoinedOn = DateTime.Now.ToString();
                personalDetails.LegId = lastleg;
                personalDetails.PlacementSide = usersPersonalModelView.PersonalDetails.PlacementSide;
                personalDetails.Gender = usersPersonalModelView.PersonalDetails.Gender;
                personalDetails.FirstName = usersPersonalModelView.PersonalDetails.FirstName;
                personalDetails.LastName = usersPersonalModelView.PersonalDetails.LastName;
                personalDetails.FatherName = usersPersonalModelView.PersonalDetails.FatherName;
                personalDetails.BirthDate = usersPersonalModelView.PersonalDetails.BirthDate;
                personalDetails.PhoneNumber1 = usersPersonalModelView.PersonalDetails.PhoneNumber1;
                personalDetails.PhoneNumber2 = usersPersonalModelView.PersonalDetails.PhoneNumber2;
                personalDetails.Email = usersPersonalModelView.PersonalDetails.Email;
                personalDetails.Address = usersPersonalModelView.PersonalDetails.Address;
                personalDetails.State = usersPersonalModelView.PersonalDetails.State;
                personalDetails.District = usersPersonalModelView.PersonalDetails.District;
                personalDetails.City = usersPersonalModelView.PersonalDetails.City;
                personalDetails.PinCode = usersPersonalModelView.PersonalDetails.PinCode;
                personalDetails.Deleted = 0;
                personalDetails.CreatedOn = DateTime.Now;
                personalDetails.UpdatedOn = DateTime.Now;

                _valleyDreamsIndiaDBEntities.Entry(personalDetails).State = EntityState.Modified;
                _valleyDreamsIndiaDBEntities.SaveChanges();

                usersPersonalModelView.BankDetails.UsersDetailsId = userDetail.Id;
                usersPersonalModelView.BankDetails.CreatedOn = DateTime.Now;
                usersPersonalModelView.BankDetails.TransactionPassword = Guid.NewGuid().ToString().Substring(0, 6);
                usersPersonalModelView.BankDetails.Deleted = 0;
                _valleyDreamsIndiaDBEntities.BankDetails.Add(usersPersonalModelView.BankDetails);
                _valleyDreamsIndiaDBEntities.SaveChanges();

                ContributionDetail contributionDetails = new ContributionDetail();
                contributionDetails.ContribNumber = 1;
                contributionDetails.ContribDate = DateTime.Now;
                contributionDetails.ContribAmount = 1000;
                contributionDetails.NextContribNumber = 2;
                contributionDetails.NextContribDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 20);
                contributionDetails.RemainingContrib = 15 - 1;
                contributionDetails.UserDetailsId = userDetail.Id;
                contributionDetails.SponsoredId = CurrentUser.CurrentUserId;
                contributionDetails.PayedBy = "ADMIN";
                contributionDetails.IsCompleted = 0;
                _valleyDreamsIndiaDBEntities.ContributionDetails.Add(contributionDetails);
                _valleyDreamsIndiaDBEntities.SaveChanges();


                ViewBag.ShowMessage = true;
                ViewBag.Username = username = userDetail.UserName;
                ViewBag.Password = password = userDetail.Password;
                ViewBag.TransactionPassword = transactionpassword = usersPersonalModelView.BankDetails.TransactionPassword;
                string fullname = usersPersonalModelView.PersonalDetails.FirstName + " " + usersPersonalModelView.PersonalDetails.LastName;
                string sponsorId = usersPersonalModelView.UserDetails.UserName;
                string srno = personalDetails.Id.ToString();

                string textMessage = String.Format("Welcome to Bethuel Multi Trade Pvt. Ltd. \n\n Dear ({0}), \n Sr. No : {1} \n Sponsor ID : {2} \n User ID : {3} \n Password : {4} \n Txn Password : {5}",
                    fullname, srno, sponsorId, username, password, transactionpassword);

                string phoneNumber1 = usersPersonalModelView.PersonalDetails.PhoneNumber1;
                string phoneNumber2 = "919888540973,919646744247";
                string smsStatus = SmsProvider.SendSms(phoneNumber1, textMessage);
                if (smsStatus == "Success")
                {
                    smsstatus = "Credentials Sent To Your Registered Mobile Number Successfully";
                }
                ViewBag.SmsStatus = smsstatus;

                ViewBag.Message = "Record Updated Successfully";
                return View(GetPersonalAndUserDetails(Convert.ToInt32(personalDetails.UsersDetailsId)));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private UsersPersonalModelView GetPersonalDeletedAndUserDetails(int userDetailsId)
        {
            UsersPersonalModelView usersPersonalModelView = new UsersPersonalModelView();
            usersPersonalModelView.UserDetails = _valleyDreamsIndiaDBEntities.UsersDetails
                .First(x => x.Id == userDetailsId);
            usersPersonalModelView.PersonalDetails = _valleyDreamsIndiaDBEntities.PersonalDetails
                .First(x => x.UsersDetailsId == userDetailsId && x.Deleted == 1);
            usersPersonalModelView.BankDetails = new BankDetail();
            return usersPersonalModelView;
        }

        [HttpPost]
        public ActionResult LogOut()
        {
            Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-1);
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "SuperAdmin");
        }
    }
}

//try
//{
//    // Your code...
//    // Could also be before try if you know the exception occurs in SaveChanges


//}
//catch (DbEntityValidationException e)
//{
//    foreach (var eve in e.EntityValidationErrors)
//    {
//        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            eve.Entry.Entity.GetType().Name, eve.Entry.State);
//        foreach (var ve in eve.ValidationErrors)
//        {
//            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
//                ve.PropertyName, ve.ErrorMessage);
//        }
//    }
//    throw;
//}

