using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using WebApplication2.Models;
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
    public class vtv3AdaptiveController : ApiController
    {
        public ResResult Get(string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string link = getLinkFromTV101("http://tv.101vn.com/ok/vtv/vtv3_3.php");
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

        private string getLinkFromTV101(string original)
        {
            int pFrom = -1;
            int pTo = -1;
            int i = 0;
            string firstRequest = original;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();
            pFrom = firstResponse.IndexOf("function init()");
            pTo = -1;
            if (pFrom > 0)
            {
                pFrom += "function init()".Length + 1;
                for (i = pFrom; i < firstResponse.Length; i++)
                {
                    if (firstResponse[i] == '\'')
                    {
                        pFrom = i + 1; break;
                    }
                }
                for (i = pFrom + 10; i < firstResponse.Length; i++)
                {
                    if (firstResponse[i] == '\'')
                    {
                        pTo = i; break;
                    }
                }
            }
            string firstResult = "";
            if (pFrom > -1 && pTo > -1)
            {
                firstResult = firstResponse.Substring(pFrom, pTo - pFrom);

            }

            String secondResult = "";

            if (firstResult != "")
            {
                bool isFinishRedirect = true;
                while (isFinishRedirect == true)
                {

                    request = (HttpWebRequest)WebRequest.Create(firstResult);
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                    request.AllowAutoRedirect = false;
                    response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect)
                    {
                        firstResult = response.Headers["Location"].ToString();
                    }
                    else
                    {
                        st = response.GetResponseStream();
                        sr = new StreamReader(st);
                        secondResult = sr.ReadToEnd();
                        isFinishRedirect = false;

                    }
                }
            }
            if (firstResult != "" && secondResult != "")
            {
                String[] resolutionLinks = secondResult.Split('\n');
                if (resolutionLinks.Length >= 7)
                {
                    if (secondResult.IndexOf("http") >= 0)
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
                        string[] splitOriginalLink = firstResult.Split('/');
                        string strReplace = splitOriginalLink[splitOriginalLink.Length - 1];

                        string highResLink = firstResult.Replace(strReplace, highResName);
                        string midResLink = firstResult.Replace(strReplace, midResName);
                        string lowResLink = firstResult.Replace(strReplace, lowResName);

                        return lowResLink + "*" + midResLink + "*" + highResLink;
                    }
                }
                else
                {
                    return firstResult + "**";
                }
            }
            return "**";
        }
    }
}
