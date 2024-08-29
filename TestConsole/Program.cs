using ApiBlockchair;

Api api = new Api(ECoin.LitecoinTestNet, "A___API_KEY");

var  x = await api.GetUtoxos("myUyoRc9MdStKqbAfmDUwUHtknzeyTZen4");

Console.WriteLine(x);