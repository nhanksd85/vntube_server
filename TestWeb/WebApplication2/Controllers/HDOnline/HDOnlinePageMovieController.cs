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
    public class HDOnlinePageMovieController : ApiController
    {
        private string regex_url_movie = @"<div class=""tn-bxitem"">[\S\D\W]{0,100}\W{2}class";
        private string regex_url_image = @"<img src[\S\D\W]{0,500}width=";
        private string regex_year = @"<p>Năm sản xuất: \d{0,4} </p>";
        private string regex_rate = @"Đánh giá:[\S\D\W]{0,5}</p>";
        public List<MovieHDItem> Get(string movieUrl)
        {

            return getLinkFromHDOnlinePageMovie(movieUrl);

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
                objItem.MovieUrl = findInfoNearMovie(i, regex_url_movie, DEFINE.REGEX_URL_MOVIE);
                objItem.MovieLogo = findInfoNearMovie(i, regex_url_image, DEFINE.REGEX_URL_IMAGE);
                objItem.MovieTitle = getToken(i, "alt=\"", "\"");
                objItem.MovieYear = findInfoNearMovie(i, regex_year, DEFINE.REGEX_YEAR);
                objItem.MovieDescription = getToken(i, "tn-contentdecs mb10\">","</div>").TrimStart().TrimEnd();
                if (i.IndexOf("Đánh giá") >= 0) objItem.MovieRating = findInfoNearMovie(i, regex_rate, DEFINE.REGEX_RATE);
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
}