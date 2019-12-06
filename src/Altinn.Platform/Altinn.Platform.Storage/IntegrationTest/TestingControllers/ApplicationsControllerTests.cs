using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;

using Altinn.Platform.Storage.Clients;
using Altinn.Platform.Storage.Controllers;
using Altinn.Platform.Storage.Interface.Models;
using Altinn.Platform.Storage.Repository;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;

using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Altinn.Platform.Storage.IntegrationTest.TestingControllers
{
    /// <summary>
    /// Represents a collection of integration tests of the <see cref="ApplicationsController"/>.
    /// </summary>
    [Collection("Sequential")]
    public class ApplicationsControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string BasePath = "/storage/api/v1";

        private readonly WebApplicationFactory<Startup> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationsControllerTests"/> class with the given <see cref="WebApplicationFactory{TStartup}"/>.
        /// </summary>
        /// <param name="factory">The <see cref="WebApplicationFactory{TStartup}"/> to use when setting up the test server.</param>
        public ApplicationsControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Scenario:
        ///   Post a simple but valid Application instance.
        /// Expected result:
        ///   Returns HttpStatus Created and the Application instance.
        /// Success criteria:
        ///   The response has correct status and the returned application instance has been populated with a data type.
        /// </summary>
        [Fact]
        public async void Post_GivenValidApplication_ReturnsStatusCreatedAndCorrectData()
        {
            // Arrange
            string org = "test";
            string appName = "app20";
            string requestUri = $"{BasePath}/applications?appId={org}/{appName}";
            
            Application appInfo = CreateApplication(org, appName);

            DocumentClientException dex = CreateDocumentClientExceptionForTesting("Not found", HttpStatusCode.NotFound);

            Mock<IApplicationRepository> applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindOne(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(dex);
            applicationRepository.Setup(s => s.Create(It.IsAny<Application>())).ReturnsAsync((Application app) => app);

            HttpClient client = GetTestClient(applicationRepository.Object);

            // Act
            HttpResponseMessage response = await client.PostAsync(requestUri, appInfo.AsJson());

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string content = response.Content.ReadAsStringAsync().Result;
            Application application = JsonConvert.DeserializeObject(content, typeof(Application)) as Application;

            Assert.NotNull(application);
            Assert.NotNull(application.DataTypes);
            Assert.Single(application.DataTypes);
            Assert.Equal("default", application.DataTypes[0].Id);
        }

        /// <summary>
        /// Scenario:
        ///   Post a simple application with an invalid id.
        /// Expected result:
        ///   Returns HttpStatus BadRequest with a reason phrase.
        /// Success criteria:
        ///   The response has correct status and the returned reason phrase has the correct keywords.
        /// </summary>
        [Fact]
        public async void Post_GivenApplicationWithInvalidId_ReturnsStatusBadRequestWithMessage()
        {
            // Arrange
            string org = "TEST";
            string appName = "app";
            string requestUri = $"{BasePath}/applications?appId={org}/{appName}";

            Application appInfo = CreateApplication(org, appName);
            
            Mock<IApplicationRepository> applicationRepository = new Mock<IApplicationRepository>();

            HttpClient client = GetTestClient(applicationRepository.Object);

            // Act
            HttpResponseMessage response = await client.PostAsync(requestUri, appInfo.AsJson());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string content = response.Content.ReadAsStringAsync().Result;

            Assert.Contains("not valid", content);
        }

        /// <summary>
        /// Scenario:
        ///   Soft delete an existing application
        /// Expected result:
        ///   Returns HttpStatus Accepted and an updated application
        /// Success criteria:
        ///   The response has correct status code and the returned application has updated valid to date.
        /// </summary>
        [Fact]
        public async void Delete_GivenExistingApplicationToSoftDelete_ReturnsStatusAcceptedWithUpdatedValidDateOnApplication()
        {
            // Arrange
            string org = "test";
            string appName = "app21";
            string requestUri = $"{BasePath}/applications/{org}/{appName}";
            
            Application appInfo = CreateApplication(org, appName);

            Mock<IApplicationRepository> applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindOne(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(appInfo);
            applicationRepository.Setup(s => s.Update(It.IsAny<Application>())).ReturnsAsync((Application app) => app);

            HttpClient client = GetTestClient(applicationRepository.Object);

            // Act
            HttpResponseMessage response = await client.DeleteAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            string content = response.Content.ReadAsStringAsync().Result;
            Application application = JsonConvert.DeserializeObject(content, typeof(Application)) as Application;

            Assert.NotNull(application);
            Assert.True(application.ValidTo < DateTime.UtcNow);
        }

        /// <summary>
        /// Create an application, read one, update it and get it one more time.
        /// </summary>
        [Fact]
        public async void GetAndUpdateApplication()
        {
            // Arrange
            string org = "test";
            string appName = "app21";
            string requestUri = $"{BasePath}/applications/{org}/{appName}";
           
            Application originalApp = CreateApplication(org, appName);

            Mock<IApplicationRepository> applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindOne(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(originalApp);
            applicationRepository.Setup(s => s.Update(It.IsAny<Application>())).ReturnsAsync((Application app) => app);
            
            HttpClient client = GetTestClient(applicationRepository.Object);
           
            Application updatedApp = CreateApplication(org, appName);
            updatedApp.VersionId = "r34";
            updatedApp.PartyTypesAllowed = new PartyTypesAllowed { BankruptcyEstate = true };

            // Act
            HttpResponseMessage response = await client.PutAsync(requestUri, updatedApp.AsJson());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = response.Content.ReadAsStringAsync().Result;
            Application application = JsonConvert.DeserializeObject(content, typeof(Application)) as Application;

            Assert.NotNull(application);
            Assert.Equal("r34", application.VersionId);
            Assert.True(application.PartyTypesAllowed.BankruptcyEstate);
            Assert.False(application.PartyTypesAllowed.Person);
        }

        private static DocumentClientException CreateDocumentClientExceptionForTesting(string message, HttpStatusCode httpStatusCode)
        {
            Type type = typeof(DocumentClientException);

            string fullName = type.FullName ?? "wtf?";

            object documentClientExceptionInstance = type.Assembly.CreateInstance(
                fullName,
                false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { message, null, null, httpStatusCode, null },
                null,
                null);

            return (DocumentClientException)documentClientExceptionInstance;
        }

        private HttpClient GetTestClient(IApplicationRepository applicationRepository)
        {
            // No setup required for these services. They are not in use by the ApplicationController
            Mock<IDataRepository> dataRepository = new Mock<IDataRepository>();
            Mock<IInstanceRepository> instanceRepository = new Mock<IInstanceRepository>();
            Mock<IInstanceEventRepository> instanceEventRepository = new Mock<IInstanceEventRepository>();

            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(applicationRepository);
                    services.AddSingleton(dataRepository.Object);
                    services.AddSingleton(instanceRepository.Object);
                    services.AddSingleton(instanceEventRepository.Object);
                });
            }).CreateClient();

            return client;
        }

        private Application CreateApplication(string org, string appName)
        {
            Application appInfo = new Application
            {
                Id = $"{org}/{appName}",
                VersionId = "r33",
                Title = new Dictionary<string, string>(),
                Org = org,
            };

            appInfo.Title.Add("nb", "Tittel");

            return appInfo;
        }
    }
}
