using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using FootballDataMiner;

namespace Company.Function
{
    public static class HttpTrigger1
    {
        private static string _endpointURI;
        private static string _primaryKey;
        private static string _authToken;
        private static string _baseURL = "https://api.football-data.org/v2/";
        private static DateTime _lastRunDate;

        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            _endpointURI = System.Environment.GetEnvironmentVariable("CosmosEndpointUri", EnvironmentVariableTarget.Process);
            _primaryKey = System.Environment.GetEnvironmentVariable("CosmosPrimaryKey", EnvironmentVariableTarget.Process);
            _authToken = System.Environment.GetEnvironmentVariable("FootballDataToken", EnvironmentVariableTarget.Process);

            var cosmosClient = new CosmosClient(_endpointURI, _primaryKey);
            var database = await CreateDatabaseAsync(cosmosClient, "Football");
            var lastUpdatedContainer = await CreateContainerAsync(database, "LastRun", "/id");
            _lastRunDate = await GetAndUpdateLastRunDate(lastUpdatedContainer);

            // For testing only
            _lastRunDate = DateTime.MinValue;

            Console.WriteLine($"Last run date: {_lastRunDate:dd/MM/yyyy}");
            var teamsContainer = await CreateContainerAsync(database, "Teams", "/id");
            if (await CountContainerItems(teamsContainer) == 0)
            {
                Console.WriteLine("No teams in container. Populating now...");
                await PopulateTeams(teamsContainer);
            }
            var fixturesContainer = await CreateContainerAsync(database, "Fixtures", "/id");
            await UpdateFixtures(fixturesContainer);
            return new OkObjectResult("Finished");
        }

        private static async Task<Database> CreateDatabaseAsync(CosmosClient client, string databaseID)
        {
            // Create a new database
            Database database = await client.CreateDatabaseIfNotExistsAsync(databaseID);
            Console.WriteLine("Created Database: {0}\n", database.Id);
            return database;
        }

        private static async Task<Container> CreateContainerAsync(Database database, string containerId, string partitionKey)
        {
            // Create a new container
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, partitionKey);
            Console.WriteLine("Created Container: {0}\n", container.Id);
            return container;
        }

        private static async Task<int> CountContainerItems(Container container)
        {
            var sqlQueryText = "SELECT VALUE COUNT(1) FROM c";
            var count = 0;
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            FeedIterator<int> queryResultSetIterator = container.GetItemQueryIterator<int>(queryDefinition);
            FeedResponse<int> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (int i in currentResultSet)
            {
                count = i;
            }
            return count;
        }

        private static async Task<DateTime> GetAndUpdateLastRunDate(Container container)
        {
            var sqlQueryText = "SELECT * FROM c";
            var lastUpdated = DateTime.MinValue;
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<LastUpdated> queryResultSet = container.GetItemQueryIterator<LastUpdated>(queryDefinition);
            FeedResponse<LastUpdated> currentResultSet = await queryResultSet.ReadNextAsync();
            foreach (var lu in currentResultSet)
            {
                lastUpdated = lu.LastRunDate;
            }
            try
            {
                // Update to now
                ItemResponse<LastUpdated> lastUpdatedResponse = await container.CreateItemAsync<LastUpdated>(new LastUpdated() { id = "1", LastRunDate = DateTime.UtcNow }, new PartitionKey("1"));
            }
            catch
            {
                ItemResponse<LastUpdated> lastUpdatedResponse = await container.ReplaceItemAsync<LastUpdated>(new LastUpdated() { id = "1", LastRunDate = DateTime.UtcNow }, "1", new PartitionKey("1"));
            }

            return lastUpdated;
        }

        private static async Task<string> MakeRequest(string endpoint)
        {
            WebRequest webRequest = WebRequest.Create(_baseURL + endpoint);
            webRequest.Headers.Add("X-Auth-Token", _authToken);
            var responseStr = "";
            try
            {
                var response = await webRequest.GetResponseAsync();
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    responseStr = reader.ReadToEnd();
                    // Close the response.
                    response.Close();
                }
            }
            catch
            { }

            return responseStr;
        }

        private static async Task<bool> PopulateTeams(Container container)
        {
            var success = true;

            var teamsStr = await MakeRequest($"competitions/PL/teams");
            if (!string.IsNullOrEmpty(teamsStr))
            {
                //Console.Write(teamsStr);
                var teams = JsonConvert.DeserializeObject<Temperatures>(teamsStr);

                foreach (var team in teams.Teams)
                {
                    ItemResponse<Team> teamResponse = await container.CreateItemAsync<Team>(team, new PartitionKey(team.Id));
                }
            }
            return success;
        }

        private static async Task<bool> UpdateFixtures(Container container)
        {
            var success = true;

            var fixtureStr = await MakeRequest($"competitions/PL/matches");
            if (!string.IsNullOrEmpty(fixtureStr))
            {
                var fixtures = JsonConvert.DeserializeObject<Fixtures>(fixtureStr);

                foreach (var fixture in fixtures.Matches)
                {
                    if (fixture.LastUpdated > _lastRunDate)
                    {
                        // Check if the utcDate field has changed
                        var existingFixture = await GetFixtureByID(container, fixture.Id);
                        if (existingFixture != null)
                        {
                            if (existingFixture.UtcDate != fixture.UtcDate)
                            {
                                // Update the fixture, and notify subscribers of fixure change
                                Console.WriteLine($"Updating fixture ID {fixture.Id} ({fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name})");
                                var replaceResponse = await container.ReplaceItemAsync<Match>(fixture, fixture.Id, new PartitionKey(fixture.Id));
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Adding fixture ID {fixture.Id} ({fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name})");
                            ItemResponse<Match> teamResponse = await container.CreateItemAsync<Match>(fixture, new PartitionKey(fixture.Id));
                        }
                    }
                }
            }

            return success;
        }

        private static async Task<Match> GetFixtureByID(Container container, string id)
        {
            try
            {
                ItemResponse<Match> fixtureResponse = await container.ReadItemAsync<Match>(id, new PartitionKey(id));
                return fixtureResponse.Resource;
            }
            catch
            {
                return null;
            }
        }
    }
}
