using Gui.Configuration;
using Gui.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gui.Services
{
    public class OCRSpaceService
    {
        private readonly HttpClient _httpClient;
        private readonly OCRSpaceServiceOptions _options;

        public OCRSpaceService(HttpClient httpClient,
            IOptions<OCRSpaceServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<OCRSpaceServiceResponse> GetTextAsync(MemoryStream stream)
        {
            var requestContent = new MultipartFormDataContent();
            var imageData = stream.ToArray();
            var imageContent = new ByteArrayContent(imageData, 0, imageData.Length);
            
            requestContent.Add(new StringContent(_options.ApiKey), "apikey");
            requestContent.Add(new StringContent("chs"), "language");
            //requestContent.Add(new StringContent("2"), "ocrengine");
            
            requestContent.Add(imageContent, "image", "image.jpg");
            
            try
            {
                var httpResponseMessage = await _httpClient.PostAsync("/parse/image", requestContent);
                httpResponseMessage.EnsureSuccessStatusCode();
                var httpContent = httpResponseMessage.Content;

                var stringd = await httpContent.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                using var streamReader = new StreamReader(await httpContent.ReadAsStreamAsync());
                using var jsonTextReader = new JsonTextReader(streamReader);
                var result = serializer.Deserialize<OCRSpaceServiceResponse>(jsonTextReader);

                return result;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
