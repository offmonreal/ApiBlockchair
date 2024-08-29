using System.Text.Json.Serialization;

namespace ApiBlockchair;

public class CoinTransactionInfo
{
    [JsonPropertyName("transaction")]
    public string Transaction { get; set; }
    
    [JsonPropertyName("incoming")]
    public List<CoinTransactionWalletInfo> IncomingWallets { get; set; } = new List<CoinTransactionWalletInfo>();
    
    [JsonPropertyName("outgoing")]
    public List<CoinTransactionWalletInfo> OutgoingWallets { get; set; } = new List<CoinTransactionWalletInfo>();
    
    [JsonPropertyName("confirmations")]
    public int Confirmations { get; set; }
    
    [JsonPropertyName("time")]
    public string TransactionTime { get; set; }
    
    [JsonPropertyName("total_received")]
    public string TotalReceived { get; set; }
    
    [JsonPropertyName("total_sent")]
    public string TotalSent { get; set; }
}