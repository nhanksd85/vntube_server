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
    public class HotMovieController : ApiController
    {
        public ResResult Get( string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            //string[] splitData = original.Split('?');
            string link = getLinkFromTV101();// "KSD Test get=link";// //getlinkVTV1();

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
            obj.parameters = new List<string>();

            return obj;

        }

        private string getLinkFromTV101()
        {
            string original = "http://tv.keonhacai.com/resource/talk_2.php?id=thegioionline&kenh=vtc7";
            int pFrom = -1;
            int pTo = -1;
            int i = 0;
            string firstRequest = original;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();
            pFrom = firstResponse.IndexOf("var sv =");
            pTo = -1;
            if (pFrom > 0)
            {
                pFrom += "var sv =".Length + 1;
                for (i = pFrom; i < firstResponse.Length; i++)
                {
                    if (firstResponse[i] == '\"')
                    {
                        pFrom = i + 1; break;
                    }
                }
                for (i = pFrom + 10; i < firstResponse.Length; i++)
                {
                    if (firstResponse[i] == '\"')
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

          
            if (firstResult != "")
            {
                request = (HttpWebRequest)WebRequest.Create(firstResult);
                //request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.AllowAutoRedirect = false;
                response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect)
                {
                    string redirection_link = response.Headers["Location"].ToString();
                    return redirection_link+"**";
                }
                else
                {
                    return firstResult +"**";
                }
                
            }
            return "";
        }
    }
}
