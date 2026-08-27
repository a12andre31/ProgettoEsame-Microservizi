namespace Ordini.Business.Abstraction;

public interface IUniprOrdiniObserver
{
    IObserver<int> NuovoOrdine { get; }
}