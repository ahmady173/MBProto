using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace MBProtoLib.Utils
{
    internal class SMS
    {
        public static void SendSms(string to, string text)
        {
            //new Thread(() =>
            //{
            //    try
            //    {
            
            //        //var durl = "http://172.16.32.29:9501/api?action=sendmessage&username=cwm&password=Abc123??&recipient=" + to + "&messagetype=SMS:TEXT&messagedata=" + text;
            //        //System.Net.WebRequest req = System.Net.WebRequest.Create(durl);

            //        //System.Net.WebResponse resp = req.GetResponse();
            //        //System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            //        //var res = sr.ReadToEnd().Trim();

            //    }
            //    catch
            //    {

            //    }
            //}).Start();

            ////////new Thread(() =>
            ////////{
            ////////    try
            ////////    {
            ////////        //  var smsUserName = "d.callwithme";
            ////////        //  var smsPassword = "8191";
            ////////        //  var smsFrom = "2184015000";

            ////////        //  Authentication.SmsProxy.Send sms = new Authentication.SmsProxy.Send();
            ////////        //sms.SendSimpleSMS2(smsUserName, smsPassword, to, smsFrom, text, false);

            ////////        smsService.smsSoapClient client = new smsService.smsSoapClient();
            ////////        var result = client.doSendArraySMS("carw", "123@qwe",
            ////////            new smsService.ArrayOfString() { "1000534834" },
            ////////            new smsService.ArrayOfString() { to },
            ////////            new smsService.ArrayOfString() { text },
            ////////            new smsService.ArrayOfBoolean() { true },
            ////////            new smsService.ArrayOfBoolean() { false },
            ////////            new smsService.ArrayOfBoolean() { false },
            ////////            new smsService.ArrayOfString() { "" });

            ////////        result = result.Replace("Send OK.<ReturnIDs>", "").Replace("</ReturnIDs>", "");

            ////////        Thread.Sleep(4000);

            ////////        var del = client.doGetDelivery("carw", result);

            ////////        if (!del.Equals("2"))
            ////////        {
            ////////            try
            ////////            {
            ////////                var nurl = "http://smsgate.callwithme.local:9501/api?action=sendmessage&username=car-boy&password=84015028@@@@@@&recipient=" + to + "&messagetype=SMS:TEXT&messagedata=" + text;
            ////////                System.Net.WebRequest req = System.Net.WebRequest.Create(nurl);

            ////////                System.Net.WebResponse resp = req.GetResponse();
            ////////                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            ////////                var res = sr.ReadToEnd().Trim();
            ////////            }
            ////////            catch
            ////////            {
            ////////            }
            ////////        }
            ////////    }
            ////////    catch
            ////////    {
            ////////    }
            ////////}).Start();

            //new Thread(() =>
            //{
            //    try
            //    {


            //        var nurl = "http://smsgate.callwithme.local:9501/api?action=sendmessage&username=car-boy&password=84015028@@@@@@&recipient=" + to + "&messagetype=SMS:TEXT&messagedata=" + text;
            //        System.Net.WebRequest req = System.Net.WebRequest.Create(nurl);

            //        System.Net.WebResponse resp = req.GetResponse();
            //        System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            //        var res = sr.ReadToEnd().Trim();

            //    }
            //    catch
            //    {

            //    }
            //}).Start();

        }

        public static string GenerateSmsCode()
        {
            string _numbers = "123456789";
            Random random = new Random(DateTime.Now.Millisecond);

            StringBuilder builder = new StringBuilder(5);
            string numberAsString = "";

            for (var i = 0; i < 5; i++)
            {
                builder.Append(_numbers[random.Next(0, _numbers.Length)]);
            }

            numberAsString = builder.ToString();
            return numberAsString;

        }
    }
}