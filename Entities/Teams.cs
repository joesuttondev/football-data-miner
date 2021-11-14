using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FootballDataMiner
{

    public partial class Temperatures
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("filters")]
        public Filters Filters { get; set; }

        [JsonProperty("competition")]
        public Competition Competition { get; set; }

        [JsonProperty("season")]
        public Season Season { get; set; }

        [JsonProperty("teams")]
        public List<Team> Teams { get; set; }
    }

    public partial class Competition
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("area")]
        public Area Area { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }
    }

    public partial class Area
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Filters
    {
    }

    public partial class Season
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("currentMatchday")]
        public object CurrentMatchday { get; set; }

        [JsonProperty("availableStages")]
        public List<string> AvailableStages { get; set; }
    }

    public partial class Team
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("area")]
        public Area Area { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("tla")]
        public string Tla { get; set; }

        [JsonProperty("crestUrl")]
        public string CrestUrl { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("website")]
        public Uri Website { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("founded")]
        public long Founded { get; set; }

        [JsonProperty("clubColors")]
        public string ClubColors { get; set; }

        [JsonProperty("venue")]
        public string Venue { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }
    }
}
