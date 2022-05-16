using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class ResResult
    {
        public string low_res { get; set; }
        public string mid_res { get; set; }

        public string high_res { get; set; }
        //public IEnumerable<ResParam> parameters { get; set; }

        //public object parameters { get; set; }

        public object parameters { get; set; }
        public string jsonFormat()
        {
            string strResult = "";
            strResult = "{\"low_res\":\"" + low_res + "\","
                          + "\"mid_res\":\"" + mid_res + "\","
                          + "\"high_res\":\"" + low_res + "\","
                          + "\"parameters:\"[" 
            
                          
                                        
                          + "]"; 



            return strResult;
        }

    }

    public class ResParam
    {
        public string header;
        public string value;
    }
}