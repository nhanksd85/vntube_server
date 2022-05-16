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
    public class APKPureController : ApiController
    {
        List<APKPureItem> APKApps = new List<APKPureItem>();
        List<MainChannelAPK> ListAPKRaw = new List<MainChannelAPK>();
        public List<MainChannelAPK> Get(string id0, string id1, string id2, string id3, string id4, string id5)
        {
            //id0=526513b9658a4f22&id1=bc01d0006311&id2=000000000000&id3=ecc11bd7760c3dd687e6&id4=75&id5=NPNStore

            
            string testSecurity = "http://ubc.dauthutruyenhinh.com:2001/tvapi/xphim2.php?name=data_apk_main.txt&id0="
                + id0 + "&id1=" + id1 + "&id2=" + id2 + "&id3=" + id3 + "&id4=" + id4 + "&id5=" + id5;

            string firstRequest = testSecurity;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr1 = new StreamReader(st);
            String firstResponse = sr1.ReadToEnd();
            if (firstResponse.Length < 20)
                return new List<MainChannelAPK>();
            else
            {
                ListAPKRaw = JsonConvert.DeserializeObject<List<MainChannelAPK>>(firstResponse);
                return ListAPKRaw;
            }

            /*
            string PicturePath = "~/data_apk.txt";
            // HttpServerUtility utility = new HttpServerUtility();

            string path = HttpContext.Current.Server.MapPath(PicturePath);
            if (File.Exists(path) == true)
            {
                DateTime lastModified = System.IO.File.GetLastWriteTime(path);
                long local_Time = (long)(lastModified - new DateTime(1970, 1, 1)).TotalMilliseconds; //lastModified.Day + lastModified.Month * 10 + lastModified.Year * 100;
                long current_Time = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;// DateTime.Now.Day + DateTime.Now.Month * 10 + DateTime.Now.Year * 100;

                if (current_Time - local_Time > 150 * 3600 * 1000)
                {
                    getMainAPKChannel("http://ubc.dauthutruyenhinh.com:2080/NPNGoogleStore/npn_google_store.txt");
                    StreamWriter sw = new StreamWriter(path);

                    var json = JsonConvert.SerializeObject(ListAPKRaw);

                    sw.Write(json);
                    sw.Close();
                }
                else
                {
                    StreamReader sr = new StreamReader(path);
                    ListAPKRaw = JsonConvert.DeserializeObject<List<MainChannelAPK>>(sr.ReadToEnd());
                    sr.Close();
                }
            }
            else
            {
                getMainAPKChannel("http://ubc.dauthutruyenhinh.com:2080/NPNGoogleStore/npn_google_store.txt");
                StreamWriter sw = new StreamWriter(path);

                var json = JsonConvert.SerializeObject(ListAPKRaw);

                sw.Write(json);
                sw.Close();
            }
            
            return ListAPKRaw;
            */
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


        public void getMainAPKChannel(string url)
        {

            string firstRequest = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            ListAPKRaw = JsonConvert.DeserializeObject<List<MainChannelAPK>>(firstResponse);
            int i, j;
            for (i = 0; i < ListAPKRaw.Count; i++)
            {
                for (j = 0; j < ListAPKRaw[i].sub_channels.Count; j++)
                {
                    if (ListAPKRaw[i].sub_channels[j].videos.ToString().IndexOf("search") >= 0)
                    {
                        string[] splitData = ListAPKRaw[i].sub_channels[j].videos.ToString().Split(':');
                        ListAPKRaw[i].sub_channels[j].videos = getSearchVideos(splitData[splitData.Length-1]);
                    }
                    else if (ListAPKRaw[i].sub_channels[j].videos.ToString().IndexOf("NPNGoogleStore") >= 0)
                    {
                        ListAPKRaw[i].sub_channels[j].videos = getUBCVideos(ListAPKRaw[i].sub_channels[j].videos.ToString());
                    }

                    else
                    {
                        ListAPKRaw[i].sub_channels[j].videos = getAppFromAPKPure(ListAPKRaw[i].sub_channels[j].videos.ToString());
                    }
                }
            }
        }


        private List<APKPureItem> getSearchVideos(string keyword)
        {
            string firstRequest = "http://hkeyboard.com/api/apkpuresearch?keyword=" + keyword;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            string firstResponse = sr.ReadToEnd();
            List<APKPureItem> responseJSON = JsonConvert.DeserializeObject<List<APKPureItem>>(firstResponse);
            return responseJSON;
        }
        private List<APKPureItem> getUBCVideos(string url)
        {
            string firstRequest = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            string firstResponse = sr.ReadToEnd();
            List<APKPureItem> responseJSON = JsonConvert.DeserializeObject<List<APKPureItem>>(firstResponse);
            return responseJSON;
        }

        private List<APKPureItem> getAppFromAPKPure(string original)
        {

            string firstRequest = original;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            string firstResponse = sr.ReadToEnd();

            firstResponse = firstResponse.Replace("\r\n", "\n");
            string[] splitLines = firstResponse.Split('\n');

            List<string> data = new List<string>();

            for (int i = 0; i < splitLines.Length; i++)
            {
                
                if (splitLines[i].IndexOf("<div class=\"category-template-img\">") >= 0)
                {
                    data.Add(splitLines[i]);
                }
            }

            List<APKPureItem> returnApps = new List<APKPureItem>();

            for (int i = 0; i < data.Count; i++)
            {
                APKPureItem objResult = new APKPureItem();

                objResult.name = getToken(data[i], "title");
                objResult.url_details = "https://apkpure.com" + getToken(data[i], "href") + "/download?from=details";
                objResult.url_download = getDownloadLink(objResult.url_details);

                objResult.url_logo = getToken(data[i], "data-original");
                objResult.url_logo = objResult.url_logo.Replace("w=130", "w=500");
                //APKApps.Add(objResult);
                returnApps.Add(objResult);
            }
            return returnApps;
        }



        public string getToken(string content, string token)
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
        public string getDownloadLink(string url)
        {
            string firstRequest = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            string firstResponse = sr.ReadToEnd();

            int index1 = firstResponse.IndexOf("If the download doesn't start") + "If the download doesn't start".Length;
            int index2 = firstResponse.IndexOf("click here");
            if (index1 < index2)
            {
                string data = firstResponse.Substring(index1, index2 - index1 + 1);
                string link = getToken(data, "href=");
                return link;
            }
            return "";
        }

    }
    public class APKPureItem
    {
        public string name { get; set; }
        public string url_details { get; set; }
        public string url_logo { get; set; }
        public string url_download { get; set; }
    }


    public class SubChannelAPK
    {
        public string sub_channel_name { get; set; }
        public string icon { get; set; }
        public object videos { get; set; }
    }

    public class MainChannelAPK
    {
        public string channel_name { get; set; }
        public string icon { get; set; }
        public List<SubChannelAPK> sub_channels { get; set; }

    }

}
