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
    public class TV101_SimplestController : ApiController
    {
        public ResResult Get(string original = "", string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string[] splitData = original.Split('?');
            string link = getLinkFromTV101(splitData[0]);// "KSD Test get=link";// //getlinkVTV1();

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
            obj.low_res = link;
            obj.mid_res = "";
            obj.high_res = "";
            obj.parameters = new List<string>();

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
            pFrom = firstResponse.IndexOf("src=");
            pTo = -1;
            if (pFrom > 0)
            {
                pFrom += "src=".Length + 1;
                for (i = pFrom + 5; i < firstResponse.Length; i++)
                {
                    if (firstResponse[i] == '"')
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

            String secondResponse = "";
            if (firstResult != "")
            {
                request = (HttpWebRequest)WebRequest.Create(firstResult);
                response = (HttpWebResponse)request.GetResponse();
                st = response.GetResponseStream();
                sr = new StreamReader(st);
                secondResponse = sr.ReadToEnd();

                pFrom = -1; pTo = -1;

                pFrom = secondResponse.IndexOf("var sv =");
                if (pFrom > 0)
                {
                    for (i = pFrom; i < secondResponse.Length; i++)
                    {
                        if (secondResponse[i] == '"')
                        {
                            pFrom = i + 1; break;
                        }
                    }
                    for (i = pFrom + 5; i < secondResponse.Length; i++)
                    {
                        if (secondResponse[i] == '"')
                        {
                            pTo = i; break;
                        }
                    }
                }
                string secondResult = "";
                if (pFrom > 0 && pTo > 0)
                {
                    secondResult = secondResponse.Substring(pFrom, pTo - pFrom);
                    return secondResult;
                }

            }
            return "";
        }
    }
}
