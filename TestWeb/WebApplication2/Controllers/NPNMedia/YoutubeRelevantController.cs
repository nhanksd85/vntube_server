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

namespace WebApplication2.Controllers
{
    public class YoutubeRelevantController : ApiController
    {
        List<YouTubeVideo> links = new List<YouTubeVideo>();
        public List<YouTubeVideo> Get(string ID)
        {
            getLinkFromYoutube("https://www.youtube.com/watch?v=" + ID);
            return links;
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
            string[] splitRes = link.Split('*');
            obj.low_res = splitRes[0];
            obj.mid_res = splitRes[1];
            obj.high_res = splitRes[2];

            List<ResParam> objList = new List<ResParam>();

            obj.parameters = objList;

            return obj;

        }




        private object getListVideo(string url)
        {
            List<YouTubeVideo> resultList = new List<YouTubeVideo>();
            string firstRequest = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();



            string[] m_content_list_news = firstResponse.Split(new[] { "<div class=\"yt-lockup-content\">" }, StringSplitOptions.None);
            for (int i = 1; i < m_content_list_news.Count(); i++)
            {
                YouTubeVideo objResult = new YouTubeVideo();

                int index_url = m_content_list_news[i].IndexOf("href=\"/watch?v=");
                int index_title = m_content_list_news[i].IndexOf(" title=");
                //int index_brief_content = m_content_list_news[i].IndexOf("<div class=\"text");
                //int index_thumb_img = m_content_list_news[i].IndexOf("<img src=");
                // get data for title

                if (index_url > -1)
                {
                    //Url
                    objResult.id = ParserData(index_url, m_content_list_news[i], "href=\"/watch?v=", "href=\"/watch?v=".Length, "\"", 0);

                    objResult.url = "https://www.youtube.com/watch?v=" + objResult.id;
                    objResult.video_logo = "https://i.ytimg.com/vi/" + objResult.id + "/hqdefault.jpg";

                }

                if (index_title > -1)
                {
                    // title
                    objResult.name = ParserData(index_title, m_content_list_news[i], "\"", 1, "\"", 0);
                    objResult.name = WebUtility.HtmlDecode(objResult.name);
                }

                resultList.Add(objResult);
            }
            return resultList;

        }

        private void getLinkFromYoutube(string original)
        {

            string firstRequest = original;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";



            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();


            firstResponse = firstResponse.Replace("\r\n", "\n");
            string[] splitLines = firstResponse.Split('\n');
            string relevant_string = "";

            for (int i = 0; i < splitLines.Length; i++)
            {
                if (splitLines[i].IndexOf("window[\"ytInitialData\"] =") >= 0)
                {
                    relevant_string = splitLines[i];
                    break;
                }
            }

            for (int i = 0; i < relevant_string.Length; i++)
            {
                if (relevant_string[i] == '{')
                {
                    relevant_string = relevant_string.Substring(i);

                    relevant_string = relevant_string.Substring(0, relevant_string.Length - 1);
                    
                    break;
                }
            }

            try
            {

                var jo = JObject.Parse(relevant_string);
                var listRelevant = jo["playerOverlays"]["playerOverlayRenderer"]["endScreen"]["watchNextEndScreenRenderer"]["results"].ToList();

                

                foreach (var item in listRelevant)
                {
                    YouTubeVideo objResult = new YouTubeVideo();
                    objResult.name = getToken2(item.ToString(), "\"simpleText\":");
                    objResult.id = getToken2(item.ToString(), "\"videoId\":");
                    objResult.url = "https://www.youtube.com/watch?v=" + objResult.id;
                    objResult.video_logo = "https://i.ytimg.com/vi/" + objResult.id + "/hqdefault.jpg";
                    if (objResult.id != "")
                    {
                        links.Add(objResult);

                    }
                }

               
                
                
                
                //objResult.name = listRelevant[""].ToString();


            }
            catch (Exception e)
            {
                string error = e.ToString();
            }
            

        }



        public string getToken2(string content, string token)
        {
            int index = content.IndexOf(token);
            if (index > 0)
            {
                int start = 0; int end = 0;
                int i = index + token.Length;
                for (; i < content.Length; i++)
                {
                    if (content[i] == '\"')
                    {
                        start = i + 1;
                        i++;
                        break;
                    }
                }
                for (; i < content.Length; i++)
                {
                    if (content[i] == '\"')
                    {
                        end = i - 1;
                        break;
                    }
                }
                if (start < end)
                    return content.Substring(start, end - start + 1);


            }
            return "";
        }


        public string ParserData(int indextext, string content, string textStart, int indexStart, string textEnd, int indexEnd)
        {
            string result = "";
            try
            {
                content = content.Substring(indextext, content.Length - indextext);
                int index_content_start = content.IndexOf(textStart) + indexStart;
                content = content.Substring(index_content_start, content.Length - index_content_start);
                int index_content_end = content.IndexOf(textEnd) + indexEnd;

                result = content.Substring(0, index_content_end);
            }
            catch
            {
                result = "";
            }
            return result;
        }


    }
   

}
