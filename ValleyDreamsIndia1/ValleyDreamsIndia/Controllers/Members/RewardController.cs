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
    public class RewardController : Controller
    {
        ValleyDreamsIndiaDBEntities _valleyDreamsIndiaDBEntities = null;
        public RewardController()
        {
            _valleyDreamsIndiaDBEntities = new ValleyDreamsIndiaDBEntities();
        }

        [CustomAuthorize]
        [HttpGet]
        public ActionResult Index()
        {
            int LeftTeamCount = 0;
            int RightTeamCount = 0;
            var response = _valleyDreamsIndiaDBEntities.CountPlacementSideFunc(CurrentUser.CurrentUserId);

            foreach (var res in response)
            {
                LeftTeamCount = Convert.ToInt32(res.LeftNodes);
                RightTeamCount = Convert.ToInt32(res.RightNodes);
            }

            List<MemberRewardDetail> memberRewardList = new List<MemberRewardDetail>();

            if(LeftTeamCount >= 215000 && RightTeamCount >= 215000)
            {
                var result = getMemberRewardList(215000);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 215000, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }

            }
            if(LeftTeamCount >= 110000 && RightTeamCount >= 110000)
            {
                var result = getMemberRewardList(110000);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 110000, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 51000 && RightTeamCount >= 51000)
            {
                var result = getMemberRewardList(51000);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 51000, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 25000 && RightTeamCount >= 25000)
            {
                var result = getMemberRewardList(25000);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 25000, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 10000 && RightTeamCount >= 10000)
            {
                var result = getMemberRewardList(10000);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 10000, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 3200 && RightTeamCount >= 3200)
            {
                var result = getMemberRewardList(3200);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 3200, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 2040 && RightTeamCount >= 2040)
            {
                var result = getMemberRewardList(2040);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 2040, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 1020 && RightTeamCount >= 1020)
            {
                var result = getMemberRewardList(1020);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 1020, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 500 && RightTeamCount >= 500)
            {
                var result = getMemberRewardList(500);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 500, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 129 && RightTeamCount >= 129)
            {
                var result = getMemberRewardList(129);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 129, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }

            }
            if (LeftTeamCount >= 38 && RightTeamCount >= 38)
            {
                var result = getMemberRewardList(38);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 38, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 16 && RightTeamCount >= 16)
            {
                var result = getMemberRewardList(16);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 16, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }
            if (LeftTeamCount >= 2 && RightTeamCount >= 2)
            {
                var result = getMemberRewardList(2);
                if (result.Count == 0)
                {
                    memberRewardList.Add(SaveMemberRewardDetail(LeftTeamCount, RightTeamCount, 2, "Achieved", "Not Paid", "DD/MM/YYYY", "A/C Cleared"));
                }
                else
                {
                    memberRewardList.Add(result.FirstOrDefault());
                }
            }

            List<MemberRewardModel> memberRewardModelList = new List<MemberRewardModel>();

            List<RewardDetail> rewardList = _valleyDreamsIndiaDBEntities.RewardDetails.ToList();

            foreach(var reward in rewardList)
            {
                if(memberRewardList.Where(x=>x.Pairs == reward.Pairs).Count() > 0)
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

        private List<MemberRewardDetail> getMemberRewardList(int paris)
        {
            return _valleyDreamsIndiaDBEntities.MemberRewardDetails
                                                 .Where(x => x.UserDetailsId == CurrentUser.CurrentUserId
                                                 && x.Pairs == paris).ToList();
        }

        private MemberRewardDetail SaveMemberRewardDetail(int lftcount,int rgtcount, int pairs, string status, string paidstatus, string paiddate, string paidremarks)
        {
            MemberRewardDetail memberReward = new MemberRewardDetail
            {
                UserDetailsId = CurrentUser.CurrentUserId,
                LeftTeamCount = lftcount,
                RightTeamCount = rgtcount,
                Pairs = pairs,
                Status = status,
                PaidStatus = paidstatus,
                PaidDate = null,
                PaidRemarks = paidremarks
            };

            var savedData = _valleyDreamsIndiaDBEntities.MemberRewardDetails.Add(memberReward);
            _valleyDreamsIndiaDBEntities.SaveChanges();
            return savedData;

        }
    }
}