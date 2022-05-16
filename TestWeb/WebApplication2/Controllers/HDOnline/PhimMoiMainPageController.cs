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
using Newtonsoft.Json.Linq;
using System.Web;
using WebApplication2.Controllers.HDOnline;

namespace WebApplication2.Controllers
{
    public class PhimMoiMainPageController : ApiController
    {
        List<MainChannelHDOnline> ListChannelRaw = new List<MainChannelHDOnline>();
        public List<MainChannelHDOnline> Get(string id0, string id1, string id2, string id3, string id4, string id5)
        {

            string testSecurity = "http://ubc.dauthutruyenhinh.com:2001/tvapi/ust.php?name=npnstore_version.txt&id0="
                + id0 + "&id1=" + id1 + "&id2=" + id2 + "&id3=" + id3 + "&id4=" + id4 + "&id5=" + id5;

            string firstRequest = testSecurity;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr1 = new StreamReader(st);
            String firstResponse = sr1.ReadToEnd();
            if (firstResponse.Length < 20)
                return new List<MainChannelHDOnline>();


            string PicturePath = "~/data_PhimMoi.txt";

            string path = HttpContext.Current.Server.MapPath(PicturePath);
            if (File.Exists(path) == true)
            {
                DateTime lastModified = System.IO.File.GetLastWriteTime(path);
                long local_Time = (long)(lastModified - new DateTime(1970, 1, 1)).TotalMilliseconds; //lastModified.Day + lastModified.Month * 10 + lastModified.Year * 100;
                long current_Time = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;// DateTime.Now.Day + DateTime.Now.Month * 10 + DateTime.Now.Year * 100;

                if (current_Time - local_Time > 100 * 3600 * 1000) //100hours ~ 4 days
                {
                    getMainChannel("http://ubc.dauthutruyenhinh.com:2080/HDOnline/phimmoi_main.txt");
                    StreamWriter sw = new StreamWriter(path);

                    var json = JsonConvert.SerializeObject(ListChannelRaw);

                    sw.Write(json);
                    sw.Close();
                }
                else
                {
                    StreamReader sr = new StreamReader(path);
                    ListChannelRaw = JsonConvert.DeserializeObject<List<MainChannelHDOnline>>(sr.ReadToEnd());
                    sr.Close();
                }
            }
            else
            {
                getMainChannel("http://ubc.dauthutruyenhinh.com:2080/HDOnline/phimmoi_main.txt");
                StreamWriter sw = new StreamWriter(path);

                var json = JsonConvert.SerializeObject(ListChannelRaw);

                sw.Write(json);
                sw.Close();
            }
            return ListChannelRaw;

        }

        private void getMainChannel(string link)
        {
            string firstRequest = link;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            ListChannelRaw = JsonConvert.DeserializeObject<List<MainChannelHDOnline>>(firstResponse);
            int i, j;
            for (i = 0; i < ListChannelRaw.Count; i++)
            {
                for (j = 0; j < ListChannelRaw[i].sub_channels.Count; j++)
                {
                    ListChannelRaw[i].sub_channels[j].videos = getListVideo(ListChannelRaw[i].sub_channels[j].videos.ToString());
                }
            }

        }
        private object getListVideo(string v)
        {
            return getLinkFromHDOnlinePageMovie(v);
        }

        private List<MovieHDItem> getLinkFromHDOnlinePageMovie(string movieUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(movieUrl);

            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String strResponse = sr.ReadToEnd();
            List<MovieHDItem> links = new List<MovieHDItem>();
            List<String> SplitPP = new List<String>();
            SplitFilm2(strResponse, "<li class=\"movie-item\">", "</li>", ref SplitPP);
            foreach (String i in SplitPP)
            {
                MovieHDItem objItem = new MovieHDItem();
                objItem.MovieTitle = getToken(i, "title=\"", "\"");
                objItem.MovieLogo = getToken(i, "background:url(", ")").Replace("thumb", "medium");
                objItem.MovieUrl = "http://www.phimmoi.net/" + getToken(i, "href=\"", "\"") + "xem-phim.html";
                links.Add(objItem);
            }

            return links;
        }
        private static void SplitFilm2(string text, string startString, string endString, ref List<string> splitPP)
        {
            int startIndex = 0; int endIndex = 0;
            while (text.IndexOf(startString, startIndex + 1) > 0)
            {
                startIndex = text.IndexOf(startString, startIndex + 1);
                endIndex = text.IndexOf(endString, startIndex + startString.Length);
                splitPP.Add(text.Substring(startIndex, endIndex - startIndex + endString.Length));
            }
        }
        private string getToken(string text, string startString, string endString)
        {
            int startIndex = 0; int endIndex = 0;

            startIndex = text.IndexOf(startString, startIndex + 1);
            endIndex = text.IndexOf(endString, startIndex + startString.Length);
            if (endIndex > startIndex)
                return text.Substring(startIndex + startString.Length, endIndex - startIndex - startString.Length);
            return "";

        }

    }
}
