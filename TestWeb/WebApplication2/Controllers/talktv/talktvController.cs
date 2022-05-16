using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using WebApplication2.Models;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace WebApplication2.Controllers
{
    public class talktvController : ApiController
    {
        //htv7 : http://talktv.vn/emo.15
        //htv9 : http://talktv.vn/lenghengoi
        //sctv20: http://talktv.vn/emoitv
        public ResResult Get(string original = "", string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string[] splitData = original.Split('?');
            string link = getLinkFromTV101(splitData[0]);

            return formatLink(link);

        }
        //id0=fed851ce640ca9f3&id1=e0aab8018765&id2=545aa6939d1e&id3=6ca0e4ad937626d65d2d&id4=57&id5=86579
        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        private ResResult formatLink(string link)
        {

            ResResult obj = new ResResult();
            obj.low_res = link;
            obj.mid_res = "";
            obj.high_res = "";
            obj.parameters = new List<string>();

            return obj;

        }

        private string getLinkFromTV101(string original)
        {
            
            string firstRequest = original;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();


            try
            {
                Regex expression = new Regex("var data = ({.*})");
                var results = expression.Matches(firstResponse);
                foreach (Match match in results)
                {
                    //links.Add(match.Groups["url"].Value);
                    string abc = match.Groups[0].Value.Replace("var data = ", "");
                    abc = abc.Substring(8, abc.Length - 9);
                    var myObject = JsonConvert.DeserializeObject<TalkTvDataDetails>(abc);
                    return myObject.androidUrl;
                }
            }
            catch { }
            

            
            return "";
        }
    }
    internal class TalkTvData
    {
        public string data { get; set; }
    }
    internal class TalkTvDataDetails
    {
        public string TvUrl { get; set; }
        public string androidUrl { get; set; }
        public string background { get; set; }
        public string blockMsg { get; set; }
        public string channel { get; set; }
        public string currentViewers { get; set; }
        public string hlsSetup { get; set; }
        public string iUin { get; set; }
        public string iosUrl { get; set; }
        public string isTv { get; set; }
        public string live { get; set; }
        public string manifestUrl { get; set; }

        public string redirectType { get; set; }
        public string redirectUrl { get; set; }
        public string requestKey { get; set; }
        public string requestToken { get; set; }
        public string requireDownload { get; set; }

        public string roomId { get; set; }

        public string roomMode { get; set; }
        public string roomType { get; set; }
        public string streamGame { get; set; }
        public string streamGameId { get; set; }
        public string streamGameId2 { get; set; }
        public string streamName { get; set; }
        public string streamSource { get; set; }
        public string trailer { get; set; }
        public string viewCount { get; set; }



    }
}
