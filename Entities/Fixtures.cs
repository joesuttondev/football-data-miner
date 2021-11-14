    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

namespace FootballDataMiner
{
    public partial class Fixtures
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("filters")]
        public Filters Filters { get; set; }

        [JsonProperty("competition")]
        public Competition Competition { get; set; }

        [JsonProperty("matches")]
        public List<Match> Matches { get; set; }
    }

    public partial class Filters
    {
    }

    public partial class Match
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("season")]
        public Season Season { get; set; }

        [JsonProperty("utcDate")]
        public DateTimeOffset UtcDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("matchday")]
        public long Matchday { get; set; }

        [JsonProperty("stage")]
        public string Stage { get; set; }

        [JsonProperty("group")]
        public object Group { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("odds")]
        public Odds Odds { get; set; }

        [JsonProperty("score")]
        public Score Score { get; set; }

        [JsonProperty("homeTeam")]
        public Area HomeTeam { get; set; }

        [JsonProperty("awayTeam")]
        public Area AwayTeam { get; set; }

        [JsonProperty("referees")]
        public List<Referee> Referees { get; set; }
    }

    public partial class Odds
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public partial class Referee
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; }
    }

    public partial class Score
    {
        [JsonProperty("winner")]
        public string Winner { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("fullTime")]
        public ExtraTime FullTime { get; set; }

        [JsonProperty("halfTime")]
        public ExtraTime HalfTime { get; set; }

        [JsonProperty("extraTime")]
        public ExtraTime ExtraTime { get; set; }

        [JsonProperty("penalties")]
        public ExtraTime Penalties { get; set; }
    }

    public partial class ExtraTime
    {
        [JsonProperty("homeTeam")]
        public long? HomeTeam { get; set; }

        [JsonProperty("awayTeam")]
        public long? AwayTeam { get; set; }
    }

}
