
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;

namespace WebApplication2.Controllers.HDOnline
{
    public class MovieHDItem
    {
        private string _movieTitle; 
        private string _movieUrl;
        private string _movieLogo;
        private string _movieRating;
        private string _movieDescription;
        private string _movieTotalEp;
        private string _movieCurrentEp;
        private string _movieYear;
        private string _movieM3U8;

        public async System.Threading.Tasks.Task<string> getM3U8()
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
                        {
                            { "url", _movieUrl },
                            { "ep", MovieCurrentEp},
                            { "submit_url",""}
                        };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("http://getlink-lamnnt13936370.codeanyapp.com", content);

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString.ToString();
        }

        public void getM3U82()
        {
            string responseInString = "";
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["url"] = _movieUrl;
                data["ep"] = _movieCurrentEp;
                data["submit_url"] = "";

                var response = wb.UploadValues("http://getlink-lamnnt13936370.codeanyapp.com", "POST", data);
                responseInString = Encoding.UTF8.GetString(response);
            }
            int end_index = responseInString.IndexOf("m3u8");
            int start_index = responseInString.IndexOf("file: 'http:");

            if (start_index > 0 && start_index < end_index)
            {
                _movieM3U8 = responseInString.Substring(start_index + "file: '".Length, end_index - start_index - "file: '".Length) + "m3u8";
            }
        }

        public string MovieTitle
        {
            get
            {
                return _movieTitle;
            }

            set
            {
                _movieTitle = value;
            }
        }


        public string MovieUrl
        {
            get
            {
                return _movieUrl;
            }

            set
            {
                _movieUrl = value;


            }
        }

        public string MovieLogo
        {
            get
            {
                return _movieLogo;
            }

            set
            {
                _movieLogo = value;
            }
        }

        public string MovieRating
        {
            get
            {
                return _movieRating;
            }

            set
            {
                _movieRating = value;
            }
        }

        public string MovieDescription
        {
            get
            {
                return _movieDescription;
            }

            set
            {
                _movieDescription = value;
            }
        }

        public string MovieTotalEp
        {
            get
            {
                return _movieTotalEp;
            }

            set
            {
                _movieTotalEp = value;
            }
        }

        public string MovieCurrentEp
        {
            get
            {
                return _movieCurrentEp;
            }

            set
            {
                _movieCurrentEp = value;
            }
        }

        public string MovieYear
        {
            get
            {
                return _movieYear;
            }

            set
            {
                _movieYear = value;
            }
        }

        public string MovieM3U8
        {
            get
            {
                return _movieM3U8;
            }

            set
            {
                _movieM3U8 = value;
            }
        }
    }
}