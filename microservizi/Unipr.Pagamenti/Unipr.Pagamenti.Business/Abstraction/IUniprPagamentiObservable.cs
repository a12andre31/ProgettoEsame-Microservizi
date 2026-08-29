namespace Pagamenti.Business.Abstraction;
public interface IUniprPagamentiObservable { 
    IObservable<int> NuovoPagamento { get; } }