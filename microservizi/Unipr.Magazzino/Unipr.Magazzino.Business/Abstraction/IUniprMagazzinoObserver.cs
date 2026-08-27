namespace Magazzino.Business.Abstraction;

public interface IUniprMagazzinoObserver
{
    IObserver<int> NuovoOrdine { get; }
}