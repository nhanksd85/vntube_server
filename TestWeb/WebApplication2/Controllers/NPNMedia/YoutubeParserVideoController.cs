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
    public class YoutubeParserVideoController : ApiController
    {
        List<YouTubeVideo> links = new List<YouTubeVideo>();
        List<MainChannelFromUBC> ListChannelRaw = new List<MainChannelFromUBC>();
        public List<MainChannelFromUBC> Get(string id0, string id1, string id2, string id3, string id4, string id5)
        {
            /*
            string id0 = "526513b9658a4f22";
            string id1 = "bc01d0006311";
            string id2 = "000000000000";
            string id3 = "ecc11bd7760c3dd687e6";
            string id4 = "75";
            string id5 = "NPNStore";
            */
            /*
            string testSecurity = "http://ubc.dauthutruyenhinh.com:2001/tvapi/ust.php?name=npnstore_version.txt&id0="
                + id0 + "&id1=" + id1 + "&id2=" + id2 + "&id3=" + id3 + "&id4=" + id4 + "&id5=" + id5;

            
            string firstRequest = testSecurity;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr1 = new StreamReader(st);
            String firstResponse = sr1.ReadToEnd();
            if (firstResponse.Length < 20)
                return new List<MainChannelFromUBC>();
            */

            //getMainChannel("http://ubc.dauthutruyenhinh.com:2080/NPNMedia/npnmedia_main.txt");
            //getLinkFromYoutube(original);
            try
            {

                // HttpServerUtility utility = new HttpServerUtility();
                string rootData = "~/data/data.txt";
                string path = HttpContext.Current.Server.MapPath(rootData);
                
                if (File.Exists(path) == true)
                {
                    DateTime lastModified = System.IO.File.GetLastWriteTime(path);
                    long local_Time = (long)(lastModified - new DateTime(1970, 1, 1)).TotalMilliseconds; //lastModified.Day + lastModified.Month * 10 + lastModified.Year * 100;
                    long current_Time = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;// DateTime.Now.Day + DateTime.Now.Month * 10 + DateTime.Now.Year * 100;

                    long test_update = (current_Time - local_Time) / 1000;

                    if (test_update > 50 * 60 * 60)//50 hour
                    {
                        //getMainChannel("http://ubc.dauthutruyenhinh.com:2080/NPNMedia/npnmedia_main.txt");
                        getMainChannel("https://ubc-store-tmp1.npnlab.com/youtubevn/npnmedia_main.txt");
                       
                        var json = JsonConvert.SerializeObject(ListChannelRaw);

                        bool isValidData = true;
                        for (int i = 0; i < ListChannelRaw.Count; i++)
                        {
                            for (int j = 0; j < ListChannelRaw[i].sub_channels.Count; j++)
                            {

                                List<YouTubeVideo> listVideo = (List<YouTubeVideo>)(ListChannelRaw[i].sub_channels[j].videos);

                                if (listVideo.Count < 2)
                                {
                                    isValidData = false;
                                    break;
                                }
                            }
                            if (isValidData == false)
                                break;
                        }
                        if (isValidData == true)
                        {
                            StreamWriter sw = new StreamWriter(path);

                            sw.Write(json);
                            sw.Close();
                        }
                    }
                    else
                    {
                        StreamReader sr = new StreamReader(path);
                        ListChannelRaw = JsonConvert.DeserializeObject<List<MainChannelFromUBC>>(sr.ReadToEnd());
                        sr.Close();
                    }
                }
                else
                {
                    //getMainChannel("http://ubc.dauthutruyenhinh.com:2080/NPNMedia/npnmedia_main.txt");
                    getMainChannel("https://ubc-store-tmp1.npnlab.com/youtubevn/npnmedia_main.txt");

                    StreamWriter sw = new StreamWriter(path);

                    var json = JsonConvert.SerializeObject(ListChannelRaw);

                    sw.Write(json);
                    sw.Close();
                }
            }
            catch (Exception e) 
            {
                string PicturePath = "~/data/data.txt";

                string path = HttpContext.Current.Server.MapPath(PicturePath);
                StreamReader sr = new StreamReader(path);
                ListChannelRaw = JsonConvert.DeserializeObject<List<MainChannelFromUBC>>(sr.ReadToEnd());
                sr.Close();
            } 
            return ListChannelRaw;
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
        int main_channel = 0;
        int sub_channel = 0;

        public void getMainChannel(string link)
        {
            string firstRequest = link;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();
            sr.Close();

            ListChannelRaw = JsonConvert.DeserializeObject<List<MainChannelFromUBC>>(firstResponse);
            int i, j;
            for (i = 0; i < ListChannelRaw.Count; i++)
            {
                main_channel = i;
                for (j = 0; j < ListChannelRaw[i].sub_channels.Count; j++)
                {
                    sub_channel = j;
                    Console.WriteLine("Channel " + i.ToString() + "*****" + "Sub channel " + j);
                    ListChannelRaw[i].sub_channels[j].videos = getListVideo2(ListChannelRaw[i].sub_channels[j].videos.ToString(), ListChannelRaw[i].sub_channels[j].sub_channel_name);
                }
            }
           

        }
        private object getListVideo2(string url, string channel_official)
        {
            List<YouTubeVideo> resultList = new List<YouTubeVideo>();
            string firstRequest = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            //request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";

            WebHeaderCollection myWebHeaderCollection = request.Headers;
            myWebHeaderCollection.Add("Accept-Language:vi");


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();
            sr.Close();

            //get Channel owner data

           
            firstResponse = firstResponse.Replace("\r\n", "\n");
            string[] data = firstResponse.Split('\n');

            string relevant = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].IndexOf("ytInitialData") >= 0)
                {
                    relevant = data[i];
                    break;
                }
            }
            int index_data = relevant.IndexOf("ytInitialData");
            relevant = relevant.Substring(index_data);
            bool startChecking = false;
            int counter_brackets = 0;
            for (int i = 0; i < relevant.Length; i++)
            {
                if (relevant[i] == '{') counter_brackets++;
                if (relevant[i] == '}') counter_brackets--;

                if (counter_brackets > 10 && startChecking == false) startChecking = true;

                if (startChecking == true && counter_brackets == 0)
                {
                    relevant = relevant.Substring(0, i + 1) ;
                    break;
                }
            }


            if (relevant.Length > 0)
            {
                int index_response = 0;
                for (int i = 0; i < 100; i++)
                {
                    if (relevant[i] == '{')
                    {
                        index_response = i;
                        break;
                    }
                }
                if (index_response > 0)
                {
                    relevant = relevant.Substring(index_response, relevant.Length - index_response);
                }
            }
        
            try
            {
                JObject jsonObject = (JObject)JObject.Parse(relevant)["contents"]["twoColumnBrowseResultsRenderer"];
                JArray arrJSON = (JArray)jsonObject["tabs"];
                int index_tab = 0;
                for (index_tab = 0; index_tab < arrJSON.Count; index_tab++)
                {
                    jsonObject = (JObject)arrJSON[index_tab];
                    jsonObject = (JObject)jsonObject["tabRenderer"];
                    if (jsonObject.Count == 5)
                        break;
                }
                


                jsonObject = (JObject)(arrJSON[index_tab]);
                jsonObject = (JObject)jsonObject["tabRenderer"];
                jsonObject = (JObject)(jsonObject["content"]["sectionListRenderer"]);
                arrJSON = (JArray)jsonObject["contents"]; jsonObject = (JObject)(arrJSON[0]);

                jsonObject = (JObject)(jsonObject["itemSectionRenderer"]);
                arrJSON = (JArray)jsonObject["contents"]; jsonObject = (JObject)(arrJSON[0]);

                if (index_tab == 0)
                {
                    jsonObject = (JObject)jsonObject["shelfRenderer"]["content"]["horizontalListRenderer"];
                }
                else
                {
                    jsonObject = (JObject)jsonObject["gridRenderer"];
                }


                arrJSON = (JArray)jsonObject["items"];
                for (int i = 0; i < arrJSON.Count; i++)
                {
                    try
                    {
                        jsonObject = (JObject)(arrJSON[i]);
                      
                        
                        String ID = jsonObject["gridVideoRenderer"]["videoId"].ToString();
                        String title = "";



                        if (jsonObject["gridVideoRenderer"]["title"].ToString().IndexOf("simpleText") >= 0) {
                            title = jsonObject["gridVideoRenderer"]["title"]["simpleText"].ToString();
                        } else {
                            JArray jsonTitle = (JArray)jsonObject["gridVideoRenderer"]["title"]["runs"];
                            title = jsonTitle[0]["text"].ToString();
                        }


                          
                        String publishDate = jsonObject["gridVideoRenderer"]["publishedTimeText"]["simpleText"].ToString();
                        String viewCount = jsonObject["gridVideoRenderer"]["shortViewCountText"]["simpleText"].ToString();

                        if (index_tab == 0)
                        {
                            JArray json2 = (JArray)(jsonObject["gridVideoRenderer"]["shortBylineText"]["runs"]);
                            JObject json3 = (JObject)json2[0];
                            channel_official = json3["text"].ToString();
                        }

                        JArray arrJSON_2 = (JArray)jsonObject["gridVideoRenderer"]["thumbnailOverlays"];
                        jsonObject = (JObject)arrJSON_2[0];
                        String duration = jsonObject["thumbnailOverlayTimeStatusRenderer"]["text"]["simpleText"].ToString();

                        YouTubeVideo aVideo = new YouTubeVideo();
                        aVideo.id = ID;
                        aVideo.video_logo = "https://i.ytimg.com/vi/" + ID + "/hqdefault.jpg";
                        aVideo.name = title;
                        aVideo.post_date = publishDate;
                        aVideo.views = viewCount;
                        aVideo.duration = duration;
                        aVideo.channel_name = channel_official;

                        resultList.Add(aVideo);
                    }
                    catch { }

                }
                
                
            }
            catch(Exception e)
            {
                int k = 2;
                
            }


            return resultList;

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
            sr.Close();

            string[] m_content_list_news = firstResponse.Split(new[] { "<div class=\"yt-lockup-content\">" }, StringSplitOptions.None);

            
            try
            {

                for (int i = 1; i < m_content_list_news.Count(); i++)
                {
                    YouTubeVideo objResult = new YouTubeVideo();

                    int index_url = m_content_list_news[i].IndexOf("href=\"/watch?v=");
                    int index_title = m_content_list_news[i].IndexOf(" title=");

                    int index_channel_name = m_content_list_news[i].IndexOf("dir=\"ltr\">");
                    int index_views = m_content_list_news[i].IndexOf("\"yt-lockup-meta-info\"><li>");

                    int index_post_date = m_content_list_news[i].IndexOf("xem</li><li>");

                    int index_duration = m_content_list_news[i].IndexOf("Thời lượng:");


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

                    if (index_channel_name > -1)
                    {
                        objResult.channel_name = ParserData(index_channel_name, m_content_list_news[i], "dir=\"ltr\">", "dir=\"ltr\">".Length, "</a>", 0);
                        objResult.channel_name = WebUtility.HtmlDecode(objResult.channel_name);
                    }

                    if (index_views > -1)
                    {
                        objResult.views = ParserData(index_views, m_content_list_news[i], "\"yt-lockup-meta-info\"><li>", "\"yt-lockup-meta-info\"><li>".Length, "</li>", 0);
                    }

                    if (index_post_date > -1)
                    {
                        objResult.post_date = ParserData(index_post_date, m_content_list_news[i], "xem</li><li>", "xem</li><li>".Length, "</li>", 0);
                    }

                    if (index_duration > -1)
                    {
                        objResult.duration = ParserData(index_duration, m_content_list_news[i], "Thời lượng:", "Thời lượng:".Length, ".", 0);
                    }

                    getChannelAvatar(objResult.id, ref objResult);
                    resultList.Add(objResult);
                }
            }
            catch
            {
                return null;
            }
            return resultList;
        
        }

        public String getChannelAvatar(String id, ref YouTubeVideo objVideo)
        {
            string firstRequest = "https://www.youtube.com/watch?v=" + id;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            int index_channel_avatar = firstResponse.IndexOf("https://yt3.ggpht.com");
            int index_subcriber_counter = firstResponse.IndexOf("subscriberCountText\\\":{\\\"simpleText\\\":\\\"");

            String strChannelAvatar = "";
            if (index_channel_avatar > -1)
            {
                strChannelAvatar = ParserData(index_channel_avatar, firstResponse, "https://yt3.ggpht.com", 0, "\"", 0);
                strChannelAvatar = strChannelAvatar.Replace("48", "88");
                objVideo.channel_avatar_url = strChannelAvatar;
            }

            if (index_subcriber_counter > -1)
            {
                objVideo.channel_subscriber = ParserData(index_channel_avatar, firstResponse, "subscriberCountText\\\":{\\\"simpleText\\\":\\\"", "subscriberCountText\\\":{\\\"simpleText\\\":\\\"".Length, "\\\"}", 0) + "đăng kí";
            }



            return strChannelAvatar;
        }

        private void getLinkFromYoutube(string original)
        {

            string firstRequest = original;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            //////////////
            // THUAN PARSER
           
            string[] m_content_list_news = firstResponse.Split(new[] { "<div class=\"yt-lockup-content\">" }, StringSplitOptions.None);
            for (int i = 1; i < m_content_list_news.Count(); i++)
            {
                YouTubeVideo objResult = new YouTubeVideo();

                int index_url = m_content_list_news[i].IndexOf("href=\"/watch?v=");
                int index_title = m_content_list_news[i].IndexOf(" title=");

                int index_channel_name = m_content_list_news[i].IndexOf("dir=\"ltr\">");

                

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

               


                links.Add(objResult);
            }

         
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
    public class YouTubeVideo
    {
        
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string video_logo { get; set; }

        public string channel_name { get; set; }

        public string views { get; set; }
        public string post_date { get; set; }

        public string duration { get; set; }
        public string channel_avatar_url { get; set; }
        public string channel_subscriber { get; set; }



    }

    public class SubChannelFromUBC
    {
        public string sub_channel_name { get; set; }
        public string icon { get; set; }
        public object videos { get; set; }
    }

    public class MainChannelFromUBC
    {
        public string channel_name { get; set; }
        public string icon { get; set; }
        public List<SubChannelFromUBC> sub_channels { get; set; }

    }

}
