namespace Gui.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class HttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Stream> GetAsync(Uri uri)
        {
            var httpResponseMessage = await _httpClient.GetAsync(uri);
            httpResponseMessage.EnsureSuccessStatusCode();
            var httpContent = httpResponseMessage.Content;
            var stream = await httpContent.ReadAsStreamAsync();
            return stream;
        }
    }
}
