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
    public class APKPureSearchController : ApiController
    {
        List<APKPureItem> APKApps = new List<APKPureItem>();
        public List<APKPureItem> Get(string keyword)
        {
            try
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
                string searchLink = "https://apkpure.com/search?q=" + combineKeyWords + "&region=VN";
                getSearchAppFromAPKPure(searchLink);
            }
            catch
            {
                getBackupAPKs(keyword);
            }
            return APKApps;
        }


        private string getBackupAPKs(string keyword)
        {
            string firstRequest = "http://npnlab.chipfc.com/api/apkpuresearch?keyword=" + keyword;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            string firstResponse = sr.ReadToEnd();
            sr.Close();
            response.Close();

            APKApps = JsonConvert.DeserializeObject<List<APKPureItem>>(firstResponse);
            return firstResponse;
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





        private void getSearchAppFromAPKPure(string link)
        {

            string firstRequest = link;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            string firstResponse = sr.ReadToEnd();
            sr.Close();
            response.Close();
            


            firstResponse = firstResponse.Replace("\r\n", "\n");
            string[] splitLines = firstResponse.Split('\n');

            List<string> data = new List<string>();

            for (int i = 0; i < splitLines.Length; i++)
            {

                if (splitLines[i].IndexOf("<dl class=\"search-dl\">") >= 0)
                {
                    data.Add(splitLines[i+ 1]);
                }
            }


            for (int i = 0; i < data.Count; i++)
            {
                APKPureItem objResult = new APKPureItem();

                objResult.name = getToken(data[i], "title");
                objResult.url_details = "https://apkpure.com" + getToken(data[i], "href") + "/download?from=details";
                objResult.url_download = getDownloadLink(objResult.url_details);

                objResult.url_logo = getToken(data[i], "src");
                objResult.url_logo = objResult.url_logo.Replace("w=130", "w=500");
                APKApps.Add(objResult);
            }
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
            sr.Close();
            response.Close();

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
    

}
