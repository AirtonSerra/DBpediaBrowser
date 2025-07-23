using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DBPediaNetwork.Helpers.Communication
{
    public class Comunication
    {
        private static HttpClient _client;
        private static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    int timeout = 2;
                    _client.Timeout = timeout == 0 ? TimeSpan.FromSeconds(300) : new TimeSpan(0, timeout, 0);
                }

                return _client;
            }
        }
        public static T doPostRequest<T>(string url, object objBody, string auth_token = null, bool forceReturn = false)
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(objBody, Formatting.None);
                Client.DefaultRequestHeaders.Clear();

                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                if (!string.IsNullOrWhiteSpace(auth_token))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", auth_token);
                }

                HttpResponseMessage response = Client.PostAsync(url, content).Result;


                if (response.IsSuccessStatusCode || forceReturn)
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrWhiteSpace(strContentInfo))
                    {
                        return JsonConvert.DeserializeObject<T>(strContentInfo);
                    }
                }
                else
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"[HttpCode: {response.StatusCode} - Url: {url}] Content: {strContentInfo}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return default;
        }
        public static T doPutRequest<T>(string url, object objBody, string auth_token = null, bool forceReturn = false)
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(objBody, Formatting.None);
                Client.DefaultRequestHeaders.Clear();

                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                if (!string.IsNullOrWhiteSpace(auth_token))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", auth_token);
                }

                HttpResponseMessage response = Client.PutAsync(url, content).Result;


                if (response.IsSuccessStatusCode || forceReturn)
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrWhiteSpace(strContentInfo))
                    {
                        return JsonConvert.DeserializeObject<T>(strContentInfo);
                    }
                }
                else
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"[HttpCode: {response.StatusCode} - Url: {url}] Content: {strContentInfo}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return default;
        }
        public static T doGetRequest<T>(string url, List<HttpParams> lstParams = null, string auth_token = null, bool forceReturn = false)
        {
            try
            {
                Client.DefaultRequestHeaders.Clear();
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(string.Concat(url, lstParams?.buildQueryParams())),
                };
                if (!string.IsNullOrWhiteSpace(auth_token))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", auth_token);
                }
                HttpResponseMessage response = Client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode || forceReturn)
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<T>(strContentInfo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return default;
        }
        public static T doPatchRequest<T>(string url, object objBody, string auth_token = null, bool forceReturn = false)
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(objBody, Formatting.None);
                Client.DefaultRequestHeaders.Clear();

                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, new Uri(url))
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrWhiteSpace(auth_token))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", auth_token);
                }

                HttpResponseMessage response = Client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode || forceReturn)
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrWhiteSpace(strContentInfo))
                    {
                        return JsonConvert.DeserializeObject<T>(strContentInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return default;
        }
        public static T doDelRequest<T>(string url, string auth_token = null, bool forceReturn = false)
        {
            try
            {
                Client.DefaultRequestHeaders.Clear();

                if (!string.IsNullOrWhiteSpace(auth_token))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", auth_token);
                }

                HttpResponseMessage response = Client.DeleteAsync(url).Result;


                if (response.IsSuccessStatusCode || forceReturn)
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrWhiteSpace(strContentInfo))
                    {
                        return JsonConvert.DeserializeObject<T>(strContentInfo);
                    }
                }
                else
                {
                    string strContentInfo = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"[HttpCode: {response.StatusCode} - Url: {url}] Content: {strContentInfo}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return default;
        }
    }
}