using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace WebApplication2.Controllers
{
    public class TiviController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "tivi1", "tivi2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            string link = getlinkVTV1();
            return link;
            // return "thuan";
        }

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




        private string getlinkVTV1()
        {
            string url = "http://vtv.vn/truyen-hinh-truc-tuyen/vtv1.htm";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Referer = "http://vtv.vn/";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstStep = sr.ReadToEnd();
            //richVTVGo.Text = firstStep;

            int pFrom = firstStep.IndexOf("<iframe src=") + "<iframe src=".Length + 1;
            int pTo = firstStep.IndexOf("</iframe>");
            if (pFrom > 0 && pTo > 0)
            {
                String frameLink = firstStep.Substring(pFrom, pTo - pFrom);
                int nextPoint = frameLink.IndexOf("\"");
                frameLink = frameLink.Substring(0, nextPoint);
             //   txtVTV_VN.Text = frameLink;

                request = (HttpWebRequest)WebRequest.Create(frameLink);
                request.Referer = "http://vtv.vn/";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                response = (HttpWebResponse)request.GetResponse();
                st = response.GetResponseStream();
                sr = new StreamReader(st);
                String secondStep = sr.ReadToEnd();


                pFrom = secondStep.IndexOf("<iframe");
                pTo = secondStep.IndexOf("</iframe>");

                String m3u8 = secondStep.Substring(pFrom, pTo - pFrom);
                pFrom = m3u8.IndexOf("live=") + "live=".Length;
                pTo = m3u8.IndexOf("m3u8") + "m3u8".Length;
                String linkM3U8 = m3u8.Substring(pFrom, pTo - pFrom);
                //linkM3U8 = HttpServerUtility.UrlDecode(linkM3U8);
                linkM3U8 = linkM3U8.Replace("%2F", "/");
                linkM3U8 = linkM3U8.Replace("%3A", ":");
                linkM3U8 = linkM3U8.Replace("%3D", "=");

                return linkM3U8;
             //   txtVTV_VN.Text = linkM3U8;
            }
            return "";
        }

    }
}
