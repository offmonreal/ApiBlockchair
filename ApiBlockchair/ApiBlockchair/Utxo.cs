using Newtonsoft.Json;

namespace ApiBlockchair;

public class UtoxResponse
{
    [JsonProperty("data")]
    public Dictionary<string, AddressDataUtox> Data { get; set; }
}

public class AddressDataUtox
{
    [JsonProperty("address")]
    public AddressDetails Address { get; set; }
    [JsonProperty("utxo")]
    public List<Utxo> Utxos { get; set; }
}

public class AddressDetails
{
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("script_hex")]
    public string ScriptHex { get; set; }
    [JsonProperty("balance")]
    public long Balance { get; set; }
    [JsonProperty("unspent_output_count")]
    public int UnspentOutputCount { get; set; }
}

public class Utxo
{
    [JsonProperty("transaction_hash")]
    public string TransactionHash { get; set; }
    [JsonProperty("index")]
    public int Index { get; set; }
    [JsonProperty("value")]
    public long Value { get; set; }
}
