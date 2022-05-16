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
using System;
using System.Collections.Generic;

namespace WebApplication2.Controllers
{
    public class HDOnlineRelevantController : ApiController
    {
        private string regex_tilte = @"bxitem-txt"" title=""[\S\D\W]{0,70}""> ";
        private string regex_url_movie = @"<a href=""[\S\D\W]{0,100}"" class=""bxitem-link"">";
        private string regex_url_image = @"<img src=""[\S\D\W]{0,200}"" alt=""";
        public List<MovieHDItem> Get(string movieUrl)
        {

            return getLinkFromHDORelevant(movieUrl);

        }

        private List<MovieHDItem> getLinkFromHDORelevant(string movieUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(movieUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String strResponse = sr.ReadToEnd();
            //TODO: Parse data here and add to links
            List<MovieHDItem> links = new List<MovieHDItem>();

            string s = strResponse;
            string[] splitData = s.Split(new string[] { "key_similar_id" }, StringSplitOptions.None);
            splitData[splitData.Length - 1] = splitData[splitData.Length - 1].Substring(0, splitData[splitData.Length - 1].IndexOf("<div class=\"tn-main-l\""));
            for (int r = 1; r < splitData.Length; r++)
            {
                MovieHDItem objItem = new MovieHDItem();
                objItem.MovieTitle = findInfoNearMovie(splitData[r], regex_tilte, DEFINE_RELEVANT.REGEX_TITLE);
                objItem.MovieUrl = findInfoNearMovie(splitData[r], regex_url_movie, DEFINE_RELEVANT.REGEX_URL_MOVIE);
                objItem.MovieLogo = findInfoNearMovie(splitData[r], regex_url_image, DEFINE_RELEVANT.REGEX_URL_IMAGE);
                links.Add(objItem);
            }
            return links;
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
                    case DEFINE_RELEVANT.REGEX_URL_MOVIE:
                        k = k.Remove(0, 9);
                        k = k.Remove(k.Length - 22, 22);
                        k = "http://hdonline.vn" + k;
                        break;
                    case DEFINE_RELEVANT.REGEX_URL_IMAGE:
                        k = k.Remove(0, 10);
                        k = k.Remove(k.Length - 7, 7);
                        break;
                    case DEFINE_RELEVANT.REGEX_TITLE:
                        k = k.Remove(0, 19);
                        k = k.Remove(k.Length - 3, 3);
                        break;
                    default:
                        break;
                }
            }
            return k;
        }

    }
    public class DEFINE_RELEVANT
    {
        public const int REGEX_URL_MOVIE = 0;
        public const int REGEX_URL_IMAGE = 1;
        public const int REGEX_TITLE = 2;
    }
}