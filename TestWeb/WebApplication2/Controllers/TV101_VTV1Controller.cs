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
    public class TV101_VTV1Controller : ApiController
    {
        public ResResult Get(string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string link = getLinkVTV1FromTV101();// "KSD Test get=link";// //getlinkVTV1();
            
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

        private string getLinkVTV1FromTV101()
        {
            int pFrom = -1; 
            int pTo = -1;
            int i = 0;
            string firstRequest = "http://tv.101vn.com/ok/vtv/vtv11.php";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstResponse = sr.ReadToEnd();
            pFrom = firstResponse.IndexOf("src=");
            pTo = -1;
            if(pFrom > 0)
            {
                pFrom += "src=".Length + 1;
                for(i = pFrom + 5; i< firstResponse.Length ; i++)
                {
                    if(firstResponse[i] == '"')
                    {
                        pTo = i; break;
                    }
                }
            }
            string firstResult = "";
            if(pFrom > -1&& pTo > -1)
            {
                firstResult = firstResponse.Substring(pFrom, pTo - pFrom);

            }

            String secondResponse = "";
            if(firstResult != "")
            {
                request = (HttpWebRequest)WebRequest.Create(firstResult);
                response = (HttpWebResponse)request.GetResponse();
                st = response.GetResponseStream();
                sr = new StreamReader(st);
                secondResponse = sr.ReadToEnd();

                pFrom = -1; pTo = -1;

                pFrom = secondResponse.IndexOf("var sv =");
                if(pFrom > 0)
                {
                    for(i = pFrom; i< secondResponse.Length;i++)
                    {
                        if (secondResponse[i] == '"')
                        {
                            pFrom = i + 1; break;
                        }
                    }
                    for(i=pFrom + 5; i < secondResponse.Length; i++)
                    {
                        if(secondResponse[i] == '"')
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

        private string getlinkVTV1()
        {
            string url = "http://vtv.vn/truyen-hinh-truc-tuyen/vtv1.htm";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Referer = "http://vtv.vn/";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();
            StreamReader sr = new StreamReader(st);
            String firstStep = sr.ReadToEnd();
            //richVTVGo.Text = firstStep;

            int pFrom = firstStep.IndexOf("<iframe src=") + "<iframe src=".Length + 1;
            int pTo = firstStep.IndexOf("</iframe>");
            if (pFrom > 0 && pTo > 0)
            {
                String frameLink = firstStep.Substring(pFrom, pTo - pFrom);
                int nextPoint = frameLink.IndexOf("\"");
                frameLink = frameLink.Substring(0, nextPoint);
                //   txtVTV_VN.Text = frameLink;

                request = (HttpWebRequest)WebRequest.Create(frameLink);
                request.Referer = "http://vtv.vn/";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                response = (HttpWebResponse)request.GetResponse();
                st = response.GetResponseStream();
                sr = new StreamReader(st);
                String secondStep = sr.ReadToEnd();


                pFrom = secondStep.IndexOf("<iframe");
                pTo = secondStep.IndexOf("</iframe>");

                String m3u8 = secondStep.Substring(pFrom, pTo - pFrom);
                pFrom = m3u8.IndexOf("live=") + "live=".Length;
                pTo = m3u8.IndexOf("m3u8") + "m3u8".Length;
                String linkM3U8 = m3u8.Substring(pFrom, pTo - pFrom);
                linkM3U8 = linkM3U8.Replace("%2F", "/");
                linkM3U8 = linkM3U8.Replace("%3A", ":");
                linkM3U8 = linkM3U8.Replace("%3D", "=");

                return linkM3U8;
                //   txtVTV_VN.Text = linkM3U8;
            }
            return "";
        }
    }
}