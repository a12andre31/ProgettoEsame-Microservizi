namespace Pagamenti.Business.Abstraction;
public interface IUniprPagamentiObserver { 
    IObserver<int> NuovoPagamento { get; } 
}