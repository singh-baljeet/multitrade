using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace ValleyDreamsIndia.Common
{
    public class MessagePart
    {
        public string MessageId { get; set; }
        public int MessagePartId { get; set; }
        public string MessageText { get; set; }
    }

    public class MessageData
    {
        public string MobileNumber { get; set; }
        public List<MessagePart> MessageParts { get; set; }
    }

    public class SmsResponseFormatted
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string JobId { get; set; }
        public List<MessageData> MessageData { get; set; }
    }

    public static class SmsProvider
    {
        public static string SendSms(string phoneNumber, string textMessage, string alternateNumber = "")
        {
            string sURL;
            
            StreamReader objReader;
            string user = "multitrade";
            string password = "ML2518T";
            string msisdn = phoneNumber;
            string sid = "BETHUE";
            string msgText = textMessage;
            msgText = WebUtility.UrlEncode(msgText);
            sURL = string.Format("http://49.50.88.174/vendorsms/pushsms.aspx?user={0}&password={1}&msisdn={2}&sid={3}&msg={4}&fl=0&gwid=2", user, password, msisdn, sid, msgText);
            if (alternateNumber != "")
            {
                msisdn = msisdn + "," + alternateNumber;
                sURL = string.Format("http://49.50.88.174/vendorsms/pushsms.aspx?user={0}&password={1}&msisdn={2}&sid={3}&msg={4}&fl=0&gwid=2", user, password, msisdn, sid, msgText);
            }

            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(sURL);
            try
            {
                Stream objStream;
                objStream = wrGETURL.GetResponse().GetResponseStream();
                objReader = new StreamReader(objStream);
                string response = objReader.ReadToEnd();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                SmsResponseFormatted smsResponseList = (SmsResponseFormatted)json_serializer.DeserializeObject(response);
                objReader.Close();
                return smsResponseList.ErrorMessage;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

    }
}