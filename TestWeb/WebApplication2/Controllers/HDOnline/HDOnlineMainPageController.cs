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
    public class HDOnlineMainPageController : ApiController
    {

        private string regex_url_movie = @"<div class=""tn-bxitem"">[\S\D\W]{0,100}\W{2}class";
        private string regex_url_image = @"<img src[\S\D\W]{0,500}width=";
        private string regex_year = @"<p>Năm sản xuất: \d{0,4} </p>";
        private string regex_rate = @"Đánh giá:[\S\D\W]{0,5}</p>";

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
            

            string PicturePath = "~/data_HDOnline.txt";

            string path = HttpContext.Current.Server.MapPath(PicturePath);
            if (File.Exists(path) == true)
            {
                DateTime lastModified = System.IO.File.GetLastWriteTime(path);
                long local_Time = (long)(lastModified - new DateTime(1970, 1, 1)).TotalMilliseconds; //lastModified.Day + lastModified.Month * 10 + lastModified.Year * 100;
                long current_Time = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;// DateTime.Now.Day + DateTime.Now.Month * 10 + DateTime.Now.Year * 100;

                if (current_Time - local_Time > 100 * 3600 * 1000) //100hours ~ 4 days
                {
                    getMainChannel("http://ubc.dauthutruyenhinh.com:2080/HDOnline/hdonline_main.txt");
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
                getMainChannel("http://ubc.dauthutruyenhinh.com:2080/HDOnline/hdonline_main.txt");
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
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String strResponse = sr.ReadToEnd();
            List<MovieHDItem> links = new List<MovieHDItem>();

            List<String> SplitPP = new List<String>();
            SplitFilm2(strResponse, "<li>\n<div class=\"tn-bxitem\">", "</div>\n</div>\n</div>\n", ref SplitPP);

            foreach (String i in SplitPP)
            {
                MovieHDItem objItem = new MovieHDItem();
                objItem.MovieUrl = getToken(i, "<a href=\"", "\"");
                if (objItem.MovieUrl.IndexOf("http://hdonline.vn") < 0) objItem.MovieUrl = "http://hdonline.vn" + objItem.MovieUrl;
                objItem.MovieLogo = getToken(i, "<img src=\"", "\"");
                objItem.MovieTitle = getToken(i, "alt=\"", "\"");
                objItem.MovieYear = getToken(i, "<p>Năm sản xuất:", "</p>").TrimStart().TrimEnd();
                objItem.MovieDescription = getToken(i, "tn-contentdecs mb10\">", "</div>").TrimStart().TrimEnd();
                if (i.IndexOf("Đánh giá") >= 0) objItem.MovieRating = getToken(i, "<p>Đánh giá:", "</p>").TrimStart().TrimEnd();
                else objItem.MovieRating = "0";
                if (i.IndexOf("Số Tập") >= 0)
                {
                    objItem.MovieCurrentEp = getToken(i, "Số Tập:", "/").TrimStart().TrimEnd();
                    objItem.MovieTotalEp = getToken(i, "Số Tập:", "<").TrimStart().TrimEnd();
                }
                else
                {
                    objItem.MovieCurrentEp = "1";
                    objItem.MovieTotalEp = "1";
                }
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

        private String findInfoNearMovie(string text, string expr, int key)
        {
            String k = "";
            MatchCollection mc = Regex.Matches(text, expr);
            foreach (Match m in mc)
            {
                k = m.ToString();
                switch (key)
                {
                    case DEFINE.REGEX_URL_MOVIE:
                        k = k.Remove(0, 33);
                        k = k.Remove(k.Length - 7, 7);
                        if (k.IndexOf("http://hdonline.vn") < 0) k = "http://hdonline.vn" + k;
                        break;
                    case DEFINE.REGEX_URL_IMAGE:
                        k = k.Remove(0, 10);
                        k = k.Remove(k.Length - 8, 8);
                        break;
                    case DEFINE.REGEX_RATE:
                        k = k.Remove(0, 10);
                        k = k.Remove(k.Length - 5, 5);
                        break;
                    case DEFINE.REGEX_YEAR:
                        k = k.Remove(0, 17);
                        k = k.Remove(k.Length - 5, 5);
                        break;
                    default:
                        break;
                }
            }
            return k;
        }
        public class DEFINE
        {
            public const int REGEX_URL_MOVIE = 0;
            public const int REGEX_URL_IMAGE = 1;
            public const int REGEX_RATE = 2;
            public const int REGEX_YEAR = 3;
        }
    }
    public class SubChannelHDOnline
    {
        public string sub_channel_name { get; set; }
        public string icon { get; set; }
        public object videos { get; set; }
    }

    public class MainChannelHDOnline
    {
        public string channel_name { get; set; }
        public string icon { get; set; }
        public List<SubChannelHDOnline> sub_channels { get; set; }

    }

}
