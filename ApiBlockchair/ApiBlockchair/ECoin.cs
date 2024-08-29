namespace ApiBlockchair;

public enum ECoin
{
    Bitcoin,
    BitcoinTestNet,
    BitcoinCash,
    BitcoinCashTestNet,
    Litecoin,
    LitecoinTestNet,
    Dogecoin,
    DogecoinTestNet,
    Dash,
    DashTestNet,
}

public class EUtilites
{
    public static string GetPath(ECoin coin)
    {
        switch (coin)
        {
          
            case ECoin.Bitcoin:
                return "bitcoin";
            case ECoin.BitcoinTestNet:
                return "bitcoin/testnet";
            case ECoin.BitcoinCash:
                return "bitcoin-cash";
            case ECoin.BitcoinCashTestNet:
                return "bitcoin-cash/testnet";
            case ECoin.Litecoin:
                return "litecoin";
            case ECoin.LitecoinTestNet:
                return "litecoin/testnet";
            case ECoin.Dogecoin:
                return "dogecoin";
            case ECoin.DogecoinTestNet:
                return "dogecoin/testnet";
            case ECoin.Dash:
                return "dash";
            case ECoin.DashTestNet:
                return "dash/testnet";
            default:
                return "";
        }
    }
    
   
}