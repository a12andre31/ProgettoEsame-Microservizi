namespace Magazzino.Business.Abstraction;

public interface IUniprMagazzinoObservable
{
    IObservable<int> NuovoOrdine { get; }
}