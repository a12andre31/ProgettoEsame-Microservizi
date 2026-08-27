namespace Ordini.Business.Abstraction;

public interface IUniprOrdiniObservable
{
    IObservable<int> NuovoOrdine { get; }
}