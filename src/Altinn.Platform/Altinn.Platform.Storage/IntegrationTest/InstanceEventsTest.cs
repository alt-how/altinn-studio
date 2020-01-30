using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Platform.Storage.IntegrationTest.Clients;
using Altinn.Platform.Storage.IntegrationTest.Fixtures;
using Altinn.Platform.Storage.Interface.Models;
using Xunit;

namespace Altinn.Platform.Storage.IntegrationTest
{
    /// <summary>
    ///  Tests data service REST api for instance events.
    /// </summary>
    [Collection("Sequential")]
    public class InstanceEventsTest : IClassFixture<PlatformStorageFixture>, IClassFixture<CosmosDBFixture>, IDisposable
    {
        private readonly PlatformStorageFixture _fixture;
        private readonly HttpClient _client;
        private readonly InstanceClient _instanceClient;
        private readonly string testInstanceId = "100/922e412e-0e7d-4af3-968f-10b372ec7fd9";

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceEventsTest"/> class.
        /// </summary>
        /// <param name="fixture">the fixture object which talks to the SUT (System Under Test)</param>
        public InstanceEventsTest(PlatformStorageFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
            _instanceClient = new InstanceClient(_client);
        }

        /// <summary>
        /// Make sure repository is cleaned after the tests is run.
        /// </summary>
        public async void Dispose()
        {
            await CleanDB();
        }

        /// <summary>
        /// Deleting all testdata from Cosmos.
        /// </summary>
        private async Task CleanDB()
        {
            try
            {
                await _instanceClient.DeleteInstanceEvents(testInstanceId);
            }
            catch
            {
            }            
        }

        /// <summary>
        /// Gets all instance events for a given instance id.
        /// Verifies that the number of retrieved events matches what is expected.
        /// </summary>
        [Fact]
        public async void QueryInstanceEventsOnInstanceId()
        {
            // Assign
            int expectedNoEvents = 3;

            // Act
            await CleanDB();
            await PopulateDatabase();
            InstanceEventList instanceEvents = await _instanceClient.GetInstanceEvents(testInstanceId, null, null, null);

            // Assert
            Assert.Equal(expectedNoEvents, instanceEvents.InstanceEvents.Count());
        }

        /// <summary>
        /// Gets all instance events for a given instance id and list of event types.
        /// Verifies that the number of retrieved events matches what is expected.
        /// </summary>
        [Fact]
        public async void QueryInstanceEventsOnEventTypes()
        {
            // Assign
            int expectedNoEvents = 1;

            // Act
            await CleanDB();
            await PopulateDatabase();
            InstanceEventList instanceEvents = await _instanceClient.GetInstanceEvents(testInstanceId, new string[] { "deleted" }, null, null);

            // Assert
            Assert.Equal(expectedNoEvents, instanceEvents.InstanceEvents.Count());
        }

        /// <summary>
        /// Gets all instance events for a given instance id and time frame.
        /// Verifies that the number of retrieved events matches what is expected.
        /// </summary>
        [Fact]
        public async void QueryInstanceEventsOnTimeFrame()
        {
            // Assign
            int expectedNoEvents = 3;

            // Act
            await CleanDB();

            string from = DateTime.UtcNow.AddMinutes(-1.5).ToString("s", CultureInfo.InvariantCulture);

            await PopulateDatabase();

            string to = DateTime.UtcNow.AddMinutes(1.5).ToString("s", CultureInfo.InvariantCulture);
            InstanceEventList instanceEvents = await _instanceClient.GetInstanceEvents(testInstanceId, null, from, to);

            // Assert
            Assert.Equal(expectedNoEvents, instanceEvents.InstanceEvents.Count());
        }

        private async Task<bool> PopulateDatabase()
        {
            InstanceEvent testEvent01 = new InstanceEvent
            {
                InstanceId = testInstanceId,
                EventType = "deleted",
                InstanceOwnerPartyId = "12346",
                User = new PlatformUser
                {
                    UserId = 0,
                    AuthenticationLevel = 4,
                    EndUserSystemId = 1,
                },                
                ProcessInfo = new ProcessState
                {
                    CurrentTask = new ProcessElementInfo
                    {
                        ElementId = "Step123456"
                    }
                }
            };

            InstanceEvent testEvent02 = new InstanceEvent
            {
                InstanceId = testInstanceId,
                EventType = "submited",
                InstanceOwnerPartyId = "12346",
                User = new PlatformUser
                {
                    UserId = 0,
                    AuthenticationLevel = 4,
                    EndUserSystemId = 1,
                },
                ProcessInfo = new ProcessState
                {
                    CurrentTask = new ProcessElementInfo
                    {
                        ElementId = "Step123456"
                    }
                }
            };

            InstanceEvent testEvent03 = new InstanceEvent
            {
                InstanceId = testInstanceId,
                EventType = "created",
                InstanceOwnerPartyId = "12346",
                User = new PlatformUser
                {
                    UserId = 0,
                    AuthenticationLevel = 4,
                    EndUserSystemId = 1,
                },
                ProcessInfo = new ProcessState
                {
                    CurrentTask = new ProcessElementInfo
                    {
                        ElementId = "Step123456"
                    }
                }
            };

            await _instanceClient.PostInstanceEvent(testEvent01);
            await _instanceClient.PostInstanceEvent(testEvent02);
            await _instanceClient.PostInstanceEvent(testEvent03);

            return true;
        }
    }
}
