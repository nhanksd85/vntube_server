using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using WebApplication2.Models;
using System.Web.Mvc;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace WebApplication2.Controllers
{
    public class THVLI1Controller : ApiController
    {
        public ResResult Get(string id0 = "", string id1 = "", string id2 = "", string id3 = "", string id4 = "")
        {
            string link = getLinkTHVLI();

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
            string[] splitLinks = link.Split('*');
            if (splitLinks.Length == 3)
            {
                obj.low_res = splitLinks[0];
                obj.mid_res = splitLinks[1];
                obj.high_res = splitLinks[2];
            }
            else
            {
                obj.low_res = link;
                obj.mid_res = "";
                obj.high_res = "";
            }
            obj.parameters = new List<string>();

            return obj;

        }
        
        //https://www.thvli.vn/detail/thvl1-hd

        private string getLinkTHVLI()
        {

            try
            {
                string firstRequest = "http://api.thvli.vn/backend/cm/detail/thvl1-hd/";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream st = response.GetResponseStream();
                StreamReader sr = new StreamReader(st);
                String firstResponse = sr.ReadToEnd();
                var details = JObject.Parse(firstResponse);
                string link = details["link_play"].ToString();

                request = (HttpWebRequest)WebRequest.Create(link);
                response = (HttpWebResponse)request.GetResponse();
                st = response.GetResponseStream();
                sr = new StreamReader(st);
                string secondResponse = sr.ReadToEnd();
                string[] exp_resolution = secondResponse.Split('\n');

                if(exp_resolution.Length > 8)
                {
                    int max_index = exp_resolution.Length - 2;
                    string[] split_link = link.Split('/');
                    string prefix = link.Replace(split_link[split_link.Length - 1], "");
                    string low_res = prefix + "/" + exp_resolution[7];//[max_index];
                    string mid_res = prefix + "/" + exp_resolution[5];//[max_index - 2];
                    string high_res = prefix + "/" + exp_resolution[3];//[max_index - 4];
                    return low_res + "*" + mid_res + "*" + high_res;
                }

                return link;
            }catch
            {
                return "";
            }
        }
    }
}
