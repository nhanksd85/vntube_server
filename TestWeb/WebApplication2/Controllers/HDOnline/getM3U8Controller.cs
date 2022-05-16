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
using WebApplication2.Controllers.HDOnline;
using System.Collections.Specialized;
using System.Text;

namespace WebApplication2.Controllers
{
    public class getM3U8Controller : ApiController
    {
        public string Get(string url, string ep)
        {
            string m3u8Links = "";
            string responseInString = "";
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["url"] = url;
                data["ep"] = ep;
                data["submit_url"] = "";

                var response = wb.UploadValues("http://getlink-lamnnt13936370.codeanyapp.com", "POST", data);
                responseInString = Encoding.UTF8.GetString(response);
            }
            int end_index = responseInString.IndexOf("m3u8");
            int start_index = responseInString.IndexOf("file: 'http:");

            if (start_index > 0 && start_index < end_index)
            {
                m3u8Links = responseInString.Substring(start_index + "file: '".Length, end_index - start_index - "file: '".Length) + "m3u8";
                //get the details
                string firstRequest = m3u8Links;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firstRequest);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream st = response.GetResponseStream();
                StreamReader sr1 = new StreamReader(st);
                string firstResponse = sr1.ReadToEnd();
                //split the details for resolutions
                string[] resolution = firstResponse.Split('\n');
                List<M3U8Detail> m3u8ResolutionDetails = new List<M3U8Detail>();
                for (int i = 0; i < resolution.Length; i++)
                {
                    if (resolution[i].IndexOf("BANDWIDTH=") > 0 && resolution[i].IndexOf("RESOLUTION=") > 0)
                    {
                        M3U8Detail aDetail = new M3U8Detail();
                        getBandwidthAndResolution(resolution[i], ref aDetail);
                        aDetail.link = resolution[i + 1];
                        m3u8ResolutionDetails.Add(aDetail);
                    }
                }
                List<M3U8Detail> newList = m3u8ResolutionDetails.OrderBy(o => o.resolution_width).ToList();
                int index_resolution = -1;
                for (int i = 0; i < newList.Count; i++)
                {
                    if (newList[i].resolution_width > 1000)
                    {
                        index_resolution = i;
                        break;
                    }
                }

                if (index_resolution > 0)
                {
                    string[] splitReplace = m3u8Links.Split('/');
                    string strPlace = splitReplace[splitReplace.Length - 1];
                    m3u8Links = m3u8Links.Replace(strPlace, newList[index_resolution].link);

                    return m3u8Links;
                }
                if (newList.Count > 0)
                {
                    string[] splitReplace = m3u8Links.Split('/');
                    string strPlace = splitReplace[splitReplace.Length - 1];
                    m3u8Links = m3u8Links.Replace(strPlace, newList[index_resolution].link);
                    return newList[newList.Count - 1].link;
                }


            }

            return "";
        }

        private void getBandwidthAndResolution(string data, ref M3U8Detail aDetail)
        {
            int indexStartBandwidth = data.IndexOf("BANDWIDTH=", 1);
            int indexEndBandwidth = data.IndexOf(",", indexStartBandwidth);
            string strBandwidth = data.Substring(indexStartBandwidth + "BANDWIDTH=".Length, indexEndBandwidth - indexStartBandwidth - "BANDWIDTH=".Length);
            aDetail.bandwidth = Convert.ToInt32(strBandwidth);

            int indexStartResolution = data.IndexOf("RESOLUTION=", 1);
            int indexEndResolution = data.Length - 1;
            string strResolution = data.Substring(indexStartResolution + "RESOLUTION=".Length, indexEndResolution - indexStartResolution - "RESOLUTION=".Length);
            string[] splitXY = strResolution.Split('x');
            aDetail.resolution_width = Convert.ToInt32(splitXY[0]);
            aDetail.resolution_height = Convert.ToInt32(splitXY[1]);
        }

        public class M3U8Detail
        {
            public int bandwidth;
            public int resolution_width;
            public int resolution_height;
            public string link;
        }
    }
}
