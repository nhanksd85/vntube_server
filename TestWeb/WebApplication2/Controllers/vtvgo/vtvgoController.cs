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
    public class vtvgoController : ApiController
    {
        List<string> links = new List<string>();
        public ResResult Get(string original, string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string link = getLinkFromVTVGO(original);
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
            */
            ResParam obj2 = new ResParam();
            obj2.header = "Referer"; obj2.value = "http://vtvgo.vn/";
            //objList.Add(obj1);
            objList.Add(obj2);
            
            obj.parameters = objList;

            return obj;

        }

        private string getLinkFromVTVGO(string original)
        {

            string firstRequest = original;


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            

            int index = firstResponse.IndexOf("m3u8");
            int start_index = 0;
            int stop_index = 0;
            if (index > 0)
            {

                for (int i = index; i < index + 50; i++)
                {
                    if (firstResponse[i] == '\'')
                    {
                        stop_index = i;
                        break;
                    }
                }

                for (int i = index; i > index - 300; i--)
                {
                    if (firstResponse[i] == '\'')
                    {
                        start_index = i + 1;
                        break;
                    }
                }

            }
            if (start_index > 0 && stop_index > 0 && start_index < stop_index)
            {
                string url = firstResponse.Substring(start_index, stop_index - start_index);


                if (url != "")
                {
                    bool isFinishRedirect = true;
                    string resolution_response = "";
                    //loop until resolution data is received
                    while (isFinishRedirect == true)
                    {

                        request = (HttpWebRequest)WebRequest.Create(url);
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                        request.Referer = "http://vtvgo.vn/";
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
                return "**";
            }
            return "**";
        }
    }
    
}
