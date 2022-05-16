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

namespace WebApplication2.Controllers
{
    public class vtvgtController : ApiController
    {
        List<string> links = new List<string>();
        public ResResult Get(string original, string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string link = getLinkFromVTVGT(original);
            return formatLink(link);
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
            /*
            ResParam obj1 = new ResParam();
            obj1.key = "User-Agent"; obj1.value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            ResParam obj2 = new ResParam();
            obj2.key = "Referer"; obj2.value = "http://vtvgo.vn/";
            objList.Add(obj1);
            objList.Add(obj2);
            */
            obj.parameters = objList;

            return obj;

        }

        private string getLinkFromVTVGT(string original)
        {
            
            string firstRequest = "https://www.vtvgiaitri.vn/live/vtv1";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            //get json array for m3u8 links

            int index = firstResponse.IndexOf("\"channel\":{\"items\":");
            if (index > 0)
            {
                int end_index = 0;
                for (int i = index; i < firstResponse.Length; i++)
                {
                    if (firstResponse[i] == ']')
                    {
                        index = index + "\"channel\":{\"items\":".Length;
                        end_index = i;
                        break;
                    }
                }
                try
                {
                    string jsonarray = firstResponse.Substring(index, end_index - index + 1);
                    var list = JsonConvert.DeserializeObject<List<ChannelItem>>(jsonarray);
                    string url = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (original.IndexOf(list[i].slug) >=0)
                        {
                            url = list[i].url;
                            break;
                        }
                    }
                    if (url != "")
                    {
                        bool isFinishRedirect = true;
                        string resolution_response = "";
                        //loop until resolution data is received
                        while (isFinishRedirect == true)
                        {

                            request = (HttpWebRequest)WebRequest.Create(url);
                            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                            request.AllowAutoRedirect = false;
                            try
                            {
                                response = (HttpWebResponse)request.GetResponse();
                                if (response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect)
                                {
                                    url = response.Headers["Location"].ToString();
                                }
                                else
                                {
                                    st = response.GetResponseStream();
                                    sr = new StreamReader(st);
                                    resolution_response = sr.ReadToEnd();
                                    isFinishRedirect = false;

                                }
                            }
                            catch
                            {
                                resolution_response = "";
                                isFinishRedirect = false;
                            }
                        }//end while

                        if (url != "" && resolution_response != "")
                        {
                            String[] resolutionLinks = resolution_response.Split('\n');
                            if (resolutionLinks.Length >= 7)
                            {
                                if (resolution_response.IndexOf("http") >= 0)
                                {
                                    int lastRes = resolutionLinks.Length - 2;
                                    return resolutionLinks[lastRes - 4] + "*" + resolutionLinks[lastRes - 2] + "*" + resolutionLinks[lastRes];
                                }
                                else
                                {
                                    int lastRes = resolutionLinks.Length - 2;
                                    //replace the firstResult
                                    string highResName = resolutionLinks[lastRes];
                                    string midResName = resolutionLinks[lastRes - 2];
                                    string lowResName = resolutionLinks[lastRes - 4];
                                    //get the original string
                                    string[] splitOriginalLink = url.Split('/');
                                    string strReplace = splitOriginalLink[splitOriginalLink.Length - 1];

                                    string highResLink = url.Replace(strReplace, highResName);
                                    string midResLink = url.Replace(strReplace, midResName);
                                    string lowResLink = url.Replace(strReplace, lowResName);

                                    return lowResLink + "*" + midResLink + "*" + highResLink;
                                }
                            }
                            else
                            {
                                return url + "**";
                            }
                        }


                    }
                    else
                    {
                        return "**";
                    }
                    
                }
                catch (Exception e)
                {
                }
            }
            return "**";
        }
    }
    public class ChannelItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string slug { get; set; }
        public string index { get; set; }
        public string is_active { get; set; }
        public string web_logo { get; set; }
        public string mobile_logo { get; set; }
        public string view { get; set; }
        public string channel_id { get; set; }
    }
}
