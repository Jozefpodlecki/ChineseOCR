using Gui.Services;
using Moq;
using Moq.Protected;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class HttpClientServiceTests
    {
        [Fact]
        public async Task It_Should_Return_Stream()
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            var httpClientService = new HttpClientService(httpClient);
            var expectedStream = new MemoryStream(new byte [] { 1, 2, 3 });
            
            var uri = new Uri("http://image.com");

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(pr => pr.RequestUri == uri),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
               })
               .Verifiable();

            using var actualStream = await httpClientService.GetAsync(uri);

            httpMessageHandlerMock.Verify();

            Assert.Equal(expectedStream.Length, actualStream.Length);
        }
    }
}
