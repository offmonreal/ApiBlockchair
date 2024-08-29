using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog;
using JsonException = System.Text.Json.JsonException;

namespace ApiBlockchair;

public class FeeFromTransaction
{
    [JsonProperty("block_id")]
    public int BlockId { get; set; }

    [JsonProperty("fee")]
    public int Fee { get; set; }

    [JsonProperty("fee_per_kb")]
    public decimal FeePerKb { get; set; }

    [JsonProperty("fee_per_kwu")]
    public decimal FeePerKwu { get; set; }
}

public class FeeRoot
{
    [JsonProperty("data")]
    public List<FeeFromTransaction> Data { get; set; }
}

public class TemporaryBlockchairResponse
{
    [JsonProperty("data")]
    public JToken Data { get; set; } // Using JToken for More Flexible JSON Handling
}


public class BlockchairResponseFromPost
{
    [JsonProperty("data")]
    public Dictionary<string, long> Data { get; set; }
}

// Describe the data structure for deserializing the response from the Blockchair API
public class BlockchairResponse
{
    public Dictionary<string, AddressDataWrapper> Data { get; set; }
}

public class AddressDataWrapper
{
    public AddressData Address { get; set; }
}

public class AddressData
{
    public long? Balance { get; set; }
    public long? Received { get; set; }
    public long? Send { get; set; }
}

public class Api
{
     private readonly HttpClient httpClient = new HttpClient();

     private string _url_coin;

     private string _apiKey;

    public Api(ECoin coin, string apiKey)
    {
        _url_coin = EUtilites.GetPath(coin);
        _apiKey = apiKey;
    }

    public async Task<IEnumerable<TxOutTransaction>?> GetUtoxosLight(string address)
    {
        try
        {
            var response = await GetUtoxos(address);  // We use the first version of the method to obtain data

            if (response == null || !response.Data.Values.Any())
            {
                return null;
            }

            var transactions = new List<TxOutTransaction>();

            // Преобразуем UTXOs в список ITxOutTransaction
            foreach (var item in response.Data.Values)
            {
                foreach (var utxo in item.Utxos)
                {
                    transactions.Add(new TxOutTransaction(
                        utxo.TransactionHash,
                        utxo.Index,
                        utxo.Value
                    ));
                }
            }

            return transactions;
        }
        catch (Exception e)
        {
            Log.Error($"ApiBlockchair::GetUtoxos Error retrieving or processing UTXOs: {e.Message}");
            return null;
        }
    }


    public async Task<UtoxResponse?> GetUtoxos(string address)
    {
        try
        {
            
            string url = $"https://api.blockchair.com/{_url_coin}/dashboards/address/{address}?limit=9999&transaction_details=true&key={_apiKey}";
            var response = await httpClient.GetStringAsync(url);
            var tempResponse = JsonConvert.DeserializeObject<UtoxResponse>(response);

            if (tempResponse != null && tempResponse.Data.Values.Any(a => a.Address.UnspentOutputCount > 0))
            {
                return tempResponse;
            }
            else
            {
                Log.Warning("No UTXOs found for this address.");
                return null;
            }
        }
        catch (HttpRequestException e)
        {
            Log.Error($"ApiBlockchair::GetUtoxos Request error: {e.Message}");
            return null;
        }
        catch (JsonException e)
        {
            Log.Error($"ApiBlockchair::GetUtoxos Serialization error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Log.Error($"ApiBlockchair::GetUtoxos An error occurred: {e.Message}");
            return null;
        }
    }


    //Получить информацию о транзакции
    public async Task<CoinTransactionInfo> FetchTransactionInfo(string transaction)
    {
        try
        {
            
            string url = $"https://api.blockchair.com/{_url_coin}/dashboards/transaction/{transaction}";  

            var response = await httpClient.GetStringAsync(url);

            dynamic data = JObject.Parse(response);

            var transactionData = data?.data?[transaction]?.transaction;

            if (transactionData == null)
                return null;

            var transactionInfo = new CoinTransactionInfo
            {
                Transaction = transactionData?.hash,
                TransactionTime = transactionData?.time,
                Confirmations = data?.context?.state - transactionData?.block_id,
                TotalReceived = data?.data?[transaction]?.transaction?.input_total ?? "0",
                TotalSent = data?.data?[transaction]?.transaction?.output_total ?? "0",
            };

            var inputsData = data?.data?[transaction]?.inputs;
            if (inputsData != null)
                foreach (var input in inputsData)
                {
                    var walletInfo = new CoinTransactionWalletInfo
                    {
                        Wallet = input?.recipient,
                        Balance = input?.value  
                    };
                    transactionInfo.IncomingWallets.Add(walletInfo);
                }

            var outputsData = data?.data?[transaction]?.outputs;

            if (outputsData != null)
                foreach (var output in outputsData)
                {
                    var walletInfo = new CoinTransactionWalletInfo
                    {
                        Wallet = output?.recipient,
                        Balance = output?.value  
                    };
                    transactionInfo.OutgoingWallets.Add(walletInfo);
                }

            return transactionInfo;
        }
        catch (HttpRequestException e)
        {
            Log.Error($"ApiBlockchairCom::FetchTransactionInfo Request error: {e.Message}");
            return null;
        }
        catch (JsonException e)
        {
            Log.Error($"ApiBlockchairCom::FetchTransactionInfo Serialization error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Log.Error($"ApiBlockchairCom::FetchTransactionInfo An error occurred: {e.Message}");
            return null;
        }
    }

    //Get address information (Do not use for processing payments without confirmation parameter)
    public async Task<Dictionary<string, long>> FetchAdresInfo(string address)
    {
        try
        {
           
            
            string url = $"https://api.blockchair.com/{_url_coin}/addresses/balances?addresses={address}&key={_apiKey}";
            
            var response = await httpClient.GetStringAsync(url);
            
            var tempResponse = JsonConvert.DeserializeObject<TemporaryBlockchairResponse>(response);
            
            if (tempResponse.Data.Type != JTokenType.Array || tempResponse.Data.HasValues)
            {
                var blockchairResponse = JsonConvert.DeserializeObject<BlockchairResponseFromPost>(response);
                
                return blockchairResponse?.Data;

            }
            else
            {
               // Handle the case when 'data' is empty or not an array

                var dictioanry = new Dictionary<string, long>
                {
                    { address, 0 },
                };

                return dictioanry;
            }
            
          
         
        }
        catch (HttpRequestException e)
        {
            Log.Error($"ApiBlockchair::FetchAdresInfo Request error: {e.Message}");
            return null;
        }
        catch (JsonException e)
        {
            Log.Error($"ApiBlockchair::FetchAdresInfo Serialization error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Log.Error($"ApiBlockchair::FetchAdresInfo An error occurred: {e.Message}");
            return null;
        }
    }

    //Only wallet balances is the most economical request (the more wallets, the cheaper the request)
    public async Task<Dictionary<string, long>> FetchAdressBalance(string[] addresses)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("addresses", string.Join(",", addresses))
        });

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://api.blockchair.com/{_url_coin}/addresses/balances"))
            {
                requestMessage.Headers.Add("x-api-key", _apiKey); //Add the API key to the request header
                requestMessage.Content = content; // Adding the request body

                HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                
                var tempResponse = JsonConvert.DeserializeObject<TemporaryBlockchairResponse>(responseBody);

                if (tempResponse.Data.Type != JTokenType.Array || tempResponse.Data.HasValues)
                {
                    var blockchairResponse = JsonConvert.DeserializeObject<BlockchairResponseFromPost>(responseBody);
                    return blockchairResponse?.Data;
                }
                else
                {
                    return new Dictionary<string, long>();
                }
               
            }
        }
        catch (HttpRequestException e)
        {
            Log.Error($"ApiBlockchair::FetchAdressBalance An error occurred: {e.Message}");
            return null;
            return null;
        }
    }


    //The function returns the history of fees for transactions based on the depth of the query.
    public async Task<FeeRoot> getlastFees(int fromLastTransactions = 3)
    {
        try
        {
             string url = $"https://api.blockchair.com/{_url_coin}/transactions?limit={fromLastTransactions}&key={_apiKey}";

            var response = await httpClient.GetStringAsync(url);

            FeeRoot root = JsonConvert.DeserializeObject<FeeRoot>(response);


            return root;
            /*
            var result = new List<object>();

            foreach (var item in root.Data)
            {
                var extractedData = new
                {
                    BlockId = item.BlockId,
                    Fee = item.Fee,             //Commission in satoshi
                    FeePerKb = item.FeePerKb,   //Fees per kilobyte (1000 bytes) of data in satoshi
                    FeePerKwu = item.FeePerKwu //Fees for 1000 weighted data units in satoshi
                };

                result.Add(extractedData);
            }

            return null;*/
        }
        catch (HttpRequestException e)
        {
            Log.Error($"ApiBlockchair::FetchAdressBalance An error occurred: {e.Message}");
            return null;
        }
    }
}