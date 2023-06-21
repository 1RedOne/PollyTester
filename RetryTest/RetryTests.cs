//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Polly.Extensions.Http;
//using System.Net;

//namespace RetryTest
//{
//    [TestClass]
//    public class RetryTests
//    {
//        [TestInitialize]
//        public void Setup()
//        {
//            // Given / Arrange
//            IServiceCollection services = new ServiceCollection();

//            bool retryCalled = false;

//            HttpStatusCode codeHandledByPolicy = HttpStatusCode.InternalServerError;

//            services.AddHttpClient("HttpGetter").AddRetries();
//            //.AddHttpMessageHandler(() => new StubDelegatingHandler(codeHandledByPolicy));

//            HttpClient configuredClient =
//                services
//                    .BuildServiceProvider()
//                    .GetRequiredService<IHttpClientFactory>()
//                    .CreateClient("HttpGetter");

//            // When / Act
//            var result = await configuredClient.GetAsync("https://www.doesnotmatterwhatthisis.com/");

//            // Then / Assert
//            Assert.Equal(codeHandledByPolicy, result.StatusCode);
//            Assert.True(retryCalled);
//        }
//    }
//    }
//}