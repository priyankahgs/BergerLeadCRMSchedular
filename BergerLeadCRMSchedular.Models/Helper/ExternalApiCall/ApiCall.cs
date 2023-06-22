using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;

namespace BergerLeadCRMSchedular.Models.Helper.ExternalApiCall
{
    public abstract class ApiCall
    {        
        protected HttpClient client = new HttpClient();

        public ApiCall(string baseUrlConfigKeyName, string tokenConfigKeyName, string tokenHeaderName = "Token", string contentType = "application/json")
        {
            client.BaseAddress = GetBaseAddress(baseUrlConfigKeyName);
            client.DefaultRequestHeaders.Add(tokenHeaderName, ConfigurationManager.AppSettings[tokenConfigKeyName]);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

        }

        private Uri GetBaseAddress(string tokenConfigKeyName)
        {
            string baseAddress = ConfigurationManager.AppSettings[tokenConfigKeyName];
            if (!baseAddress.EndsWith("/"))
            {
                baseAddress = string.Format("{0}/", baseAddress);
            }

            return new Uri(baseAddress);
        }

        public TOut Get<TOut>(string url)
        {
            string result = client
                            .GetStringAsync(url)
                            .Result;


            var data = JsonConvert.DeserializeObject<TOut>(result);

            return data;
        }

        public TOut Post<TIn, TOut>(string url, TIn param)
        {
            var result = client
                             .PostAsJsonAsync(url, param)
                             .Result
                             .Content.ReadAsAsync<object>()
                             .Result;

            var data = JsonConvert.DeserializeObject<TOut>(Convert.ToString(result));

            return data;
        }

        public List<TOut> PostReturnList<TIn, TOut>(string url, TIn param)
        {
            var result = client
                             .PostAsJsonAsync(url, param)
                             .Result
                             .Content.ReadAsAsync<object>()
                             .Result;

            var data = JsonConvert.DeserializeObject<List<TOut>>(Convert.ToString(result));

            return data;
        }

        public TOut Post<TOut>(string url)
        {
            var result = client
                            .PostAsJsonAsync(url, new { })
                            .Result
                            .Content.ReadAsAsync<object>()
                            .Result;

            var data = JsonConvert.DeserializeObject<TOut>(Convert.ToString(result));

            return data;
        }

        public TOut Put<TIn, TOut>(string url, TIn param)
        {
            var result = client
                            .PutAsJsonAsync(url, param)
                            .Result
                            .Content.ReadAsAsync<object>()
                            .Result;


            var data = JsonConvert.DeserializeObject<TOut>(Convert.ToString(result));

            return data;
        }

        public TOut Put<TOut>(string url)
        {
            var result = client
                            .PutAsJsonAsync(url, new { })
                            .Result
                            .Content.ReadAsAsync<object>()
                            .Result;


            var data = JsonConvert.DeserializeObject<TOut>(Convert.ToString(result));

            return data;
        }        
    }
}
