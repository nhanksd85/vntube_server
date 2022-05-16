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
    public class vtv9vn2Controller : ApiController
    {
        public ResResult Get(string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string link = getLinkFromVTV_VN_2();// "KSD Test get=link";// //getlinkVTV1();

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
            String[] splitLinks = link.Split('*');
            obj.low_res = splitLinks[0];
            obj.mid_res = splitLinks[1];
            obj.high_res = splitLinks[2];


            obj.parameters = new List<ResParam>();
            return obj;

        }

        private string getLinkFromVTV_VN_2()
        {
            string firstRequest = "http://vtv.vn2.vn/php/1/ok/get-vtvgo1.php?link=http://vtv.vn2.vn/php/1/ok/token-vtvgo1.php?link=http://mytv.vn2.vn/mytv.php?id=vtv9";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();

            try
            {
                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(firstResponse);
                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();
                Stream st2 = response2.GetResponseStream();
                StreamReader sr2 = new StreamReader(st2);
                String secondResponse = sr2.ReadToEnd();
                String[] splitResolution = secondResponse.Split('\n');
                List<String> resolutionName = new List<string>();
                for (int i = 0; i < splitResolution.Length; i++)
                {
                    if (splitResolution[i].IndexOf("#EXT-X-STREAM-INF") >= 0 && (i + 1) < splitResolution.Length)
                    {
                        i++;
                        resolutionName.Add(splitResolution[i]);
                    }
                }
                String[] splitOriginal = firstResponse.Split('/');
                String strReplace = splitOriginal[splitOriginal.Length - 1];
                if (resolutionName.Count == 1)
                {
                    String lowRes = firstResponse.Replace(strReplace, resolutionName[0]);
                    return lowRes + "**";
                }
                else if (resolutionName.Count == 2)
                {
                    String lowRes = firstResponse.Replace(strReplace, resolutionName[0]);
                    String midRes = firstResponse.Replace(strReplace, resolutionName[1]);
                    return lowRes + "*" + midRes + "*";
                }
                else if (resolutionName.Count >= 3)
                {
                    String lowRes = firstResponse.Replace(strReplace, resolutionName[0]);
                    String midRes = firstResponse.Replace(strReplace, resolutionName[1]);
                    String highRes = firstResponse.Replace(strReplace, resolutionName[2]);
                    return lowRes + "*" + midRes + "*" + highRes;
                }
            }
            catch { }

            return firstResponse + "**";
        }

    }
}
