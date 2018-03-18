using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValleyDreamsIndia;
using ValleyDreamsIndia.AuthData;
using ValleyDreamsIndia.Common;
using ValleyDreamsIndia.Models;

namespace ValleyDreamsIndia.Controllers.Members
{

    [CustomAuthorize]
    public class ProfileController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        public ProfileController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult ViewProfile(int currentId = 0)
        {
            ViewBag.Title = "Admin: Profile";
            try
            {
                if(currentId == 0)
                {
                    return View("~/Views/Members/Profile/ViewProfile.cshtml",
                    GetPersonalAndUserDetails(CurrentUser.CurrentUserId));
                }
                else
                {
                    return View("~/Views/Members/Profile/ViewProfile.cshtml",
                    GetPersonalAndUserDetails(currentId));
                }
                
            }
            catch(Exception ex)
            {
                throw new  Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Edit(int currentId = 0)
        {
            ViewBag.Title = "Admin: Profile Settings";
            try
            {
                if(currentId == 0)
                {
                    return View("~/Views/Members/Profile/Edit.cshtml", GetPersonalAndUserDetails(CurrentUser.CurrentUserId));
                }
                else
                {
                    return View("~/Views/Members/Profile/Edit.cshtml", GetPersonalAndUserDetails(currentId));
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult Edit(UsersPersonalModelView usersPersonalModelView,HttpPostedFileBase memberImage)
        {
            ViewBag.Title = "Admin: Profile Settings";
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

                personalDetails.BirthDate = usersPersonalModelView.PersonalDetails.BirthDate;
                personalDetails.PhoneNumber2 = usersPersonalModelView.PersonalDetails.PhoneNumber2;
                personalDetails.Email = usersPersonalModelView.PersonalDetails.Email;
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

                return RedirectToAction("ViewProfile", new { currentId= usersPersonalModelView.UserDetails.Id });
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
                .First(x => x.Id == userDetailsId && x.Deleted == 0);
            usersPersonalModelView.PersonalDetails = _valleyDreamsIndiaDBEntities.PersonalDetails
                .First(x => x.UsersDetailsId == userDetailsId && x.Deleted == 0);
            usersPersonalModelView.BankDetails = _valleyDreamsIndiaDBEntities.BankDetails
                .First(x => x.UsersDetailsId == userDetailsId && x.Deleted == 0);
            usersPersonalModelView.ContributionDetails = _valleyDreamsIndiaDBEntities.ContributionDetails
                .Where(x => x.UserDetailsId == userDetailsId).OrderByDescending(x=>x.Id).FirstOrDefault();
            return usersPersonalModelView;
        }

    }

}