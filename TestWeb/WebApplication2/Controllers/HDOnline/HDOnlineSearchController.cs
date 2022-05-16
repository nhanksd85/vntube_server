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
    public class HDOnlineSearchController : ApiController
    {
        //http://hdonline.vn/tim-kiem/dai-chien-thai-binh-duong.html
 
        private string regex_split_para = @"<div class=""tn-bx[\S\D\W]{0,2200}</div>";
        private string regex_url_film = @"<div class=""tn-bxitem"">[\S\D\W]{0,100}\W{2}class";
        private string regex_url_image = @"<img src[\S\D\W]{0,500}width=";
        private string regex_name_vi = @"<p class=""name-vi"">[\S\D\W]{0,100}</p> <p class=""name-en""";
        private string regex_name_en = @"</p> <p class=""name-en"">[\S\D\W]{0,50}</p> <p>N";
        private string regex_content = @"<div class=""tn-contentdecs mb10""[\S\D\W]{0,500}</div> <div class=""clearfix"">";
        private string regex_year = @"<p>Năm sản xuất: \d{0,4} </p>";
        private string regex_name_en_so_tap = @"</p> <p class=""name-en"">[\S\D\W]{0,50}</p> <p>S";
        private string regex_so_tap = @"</p> <p>Số[\S\D\W]{0,50}</p> <p>N";

        private string regex_rate = @"Đánh giá:[\S\D\W]{0,5}</p>";


        public List<MovieHDItem> Get(string keyword)
        {
            string[] splitData = keyword.Split(' ');
            string combineKeyWords = "";
            for (int i = 0; i < splitData.Length; i++)
            {
                if (splitData[i] != "")
                {
                    combineKeyWords += splitData[i].TrimStart().TrimEnd() + "-";
                }
            }
 
            return getLinkFromHDOSearch("http://hdonline.vn/tim-kiem/" + combineKeyWords + ".html");
             
        }
 
 
 
        private List<MovieHDItem> getLinkFromHDOSearch(string urlSearch)
        {
             
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlSearch);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String strResponse = sr.ReadToEnd();
 
            List<MovieHDItem> links = new List<MovieHDItem>();
 
            //TODO: Parse data here and add to links
            string s = strResponse;
            int SearchCountIndex = s.IndexOf(@"Có <strong>") + 11;
            int SearchCount = 0;
            for (int i = SearchCountIndex; i < SearchCountIndex + 3; i++)
            {
                int TempNum = (int)(s[i]) - 48;
                if ((TempNum >= 0) && (TempNum <= 9)) SearchCount = SearchCount * 10 + TempNum;
            }
            List<String> SplitPP = new List<String>();
            SplitFilm2(strResponse, "<li>\n<div class=\"tn-bxitem\">", "</b></div>\n</div>\n</div>\n", ref SplitPP);

            if (SearchCount != 0)
            {
                foreach (String j in SplitPP)
                {
                    MovieHDItem objItem = new MovieHDItem();
                    objItem.MovieUrl = findInfoNearMovie(j, regex_url_film, DEFINE.REGEX_URL_FILM);
                    objItem.MovieLogo = findInfoNearMovie(j, regex_url_image, DEFINE.REGEX_IMAGE);
                    objItem.MovieTitle = getToken(j, "<p class=\"name-en\">", "</p>");
                    objItem.MovieDescription = getToken(j, "tn-contentdecs mb10\">", "</div>").TrimStart().TrimEnd();
                    if (j.IndexOf("Đánh giá") >= 0) objItem.MovieRating = findInfoNearMovie(j, regex_rate, DEFINE.REGEX_RATE);
                    else objItem.MovieRating = "0";
                    if (j.IndexOf("Số Tập") >= 0)
                    {
                        objItem.MovieCurrentEp = getToken(j, "Số Tập:", "/").TrimStart().TrimEnd();
                        objItem.MovieTotalEp = getToken(j, "Số Tập:", "<").TrimStart().TrimEnd();
                    }
                    else
                    {
                        objItem.MovieCurrentEp = "1";
                        objItem.MovieTotalEp = "1";
                    }
                    objItem.MovieYear = findInfoNearMovie(j, regex_year, DEFINE.REGEX_YEAR);
                    objItem.getM3U82();
                    links.Add(objItem);
                }
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
                    case DEFINE.REGEX_NAME_EN_SO_TAP:
                        k = k.Remove(0, 24);
                        k = k.Remove(k.Length - 9, 9);
                        break;
                    case DEFINE.REGEX_URL_FILM:
                        k = k.Remove(0, 33);
                        k = k.Remove(k.Length - 7, 7);
                        if (k.IndexOf("http://hdonline.vn") < 0) k = "http://hdonline.vn" + k;
                        break;
                    case DEFINE.REGEX_IMAGE:
                        k = k.Remove(0, 10);
                        k = k.Remove(k.Length - 8, 8);
                        break;
                    case DEFINE.REGEX_NAME_VI:
                        k = k.Remove(0, 19);
                        k = k.Remove(k.Length - 23, 23);
                        break;
                    case DEFINE.REGEX_NAME_EN:
                        k = k.Remove(0, 24);
                        k = k.Remove(k.Length - 9, 9);
                        break;
                    case DEFINE.REGEX_CONTENT:
                        k = k.Remove(0, 34);
                        k = k.Remove(k.Length - 30, 30);
                        break;
                    case DEFINE.REGEX_YEAR:
                        k = k.Remove(0, 17);
                        k = k.Remove(k.Length - 5, 5);
                        break;
                    case DEFINE.REGEX_SO_TAP:
                        k = k.Remove(0, 16);
                        k = k.Remove(k.Length - 10, 10);
                        break;
                    case DEFINE.REGEX_RATE:
                        k = k.Remove(0, 10);
                        k = k.Remove(k.Length - 5, 5);
                        break;
                    default:
                        break;
                }
            }
            return k;
        }
 
 
    }
    public class DEFINE
    {
        public const int REGEX_NAME_EN_SO_TAP = 0;
        public const int REGEX_URL_FILM = 1;
        public const int REGEX_IMAGE = 2;
        public const int REGEX_NAME_VI = 3;
        public const int REGEX_NAME_EN = 4;
        public const int REGEX_CONTENT = 5;
        public const int REGEX_YEAR = 6;
        public const int REGEX_SO_TAP = 7;
        public const int REGEX_RATE = 8;
    }
}