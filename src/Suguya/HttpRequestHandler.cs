using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Suguya
{
    public class HttpRequestHandler
    {

        private static HttpClient _httpClient = new HttpClient();

        internal static async Task<string> GetJsonStringAsync(string requestUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            var s = await response.Content.ReadAsStringAsync();
            return s.Substring(s.IndexOf('\n'));
        }
    }
}
