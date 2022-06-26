#region

using Newtonsoft.Json;

#endregion

namespace LProxy.Util
{
    internal class IPResult
    {
        [JsonProperty("as")] public string As;

        [JsonProperty("city")] public string City;

        [JsonProperty("country")] public string Country;

        [JsonProperty("countryCode")] public string CountryCode;

        [JsonProperty("isp")] public string Isp;

        [JsonProperty("lat")] public double Lat;

        [JsonProperty("lon")] public double Lon;

        [JsonProperty("org")] public string Org;

        [JsonProperty("query")] public string Query;

        [JsonProperty("region")] public string Region;

        [JsonProperty("regionName")] public string RegionName;

        [JsonProperty("status")] public string Status;

        [JsonProperty("timezone")] public string Timezone;

        [JsonProperty("zip")] public string Zip;
    }
}