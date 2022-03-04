using System;
using Newtonsoft.Json;

public struct ConfigReader{

    [JsonProperty("token")]
    public string token {get; private set;}
    [JsonProperty("prefix")]
    public string prefix {get; private set;}

}