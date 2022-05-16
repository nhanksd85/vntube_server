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
    public class PhimMoiSearchController : ApiController
    {
        public List<MovieHDItem> Get(string keyword)
        {
            string[] splitData = keyword.Split(' ');
            string combineKeyWords = "";
            for (int i = 0; i < splitData.Length; i++)
            {
                if (splitData[i] != "")
                {
                    combineKeyWords += splitData[i].TrimStart().TrimEnd() + "+";
                }
            }

            return getLinkFromPhimMoiSearch("http://www.phimmoi.net/tim-kiem/" + combineKeyWords + "/page-1.html");

        }

        private List<MovieHDItem> getLinkFromPhimMoiSearch(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
