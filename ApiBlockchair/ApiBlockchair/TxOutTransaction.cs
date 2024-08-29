using System.Numerics;

namespace ApiBlockchair;

public class TxOutTransaction 
{
    public string TransactionHash { get; private set; }
    public int OutputIndex { get; private set; }
    public BigInteger OutputValue { get; private set; }

    public TxOutTransaction(string transactionHash, int outputIndex, long outputValue)
    {
        TransactionHash = transactionHash;
        OutputIndex = outputIndex;
        OutputValue = new BigInteger(outputValue);
    }
}
