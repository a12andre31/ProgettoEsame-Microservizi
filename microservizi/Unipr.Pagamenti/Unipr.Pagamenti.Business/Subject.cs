using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Subjects;
using Pagamenti.Business.Abstraction;

namespace Pagamenti.Business;

[method: ActivatorUtilitiesConstructor]
public class Subject() : ISubject
{
    private Subject<int> nuovoPagamento { get; } = new Subject<int>();
    IObservable<int> IUniprPagamentiObservable.NuovoPagamento => nuovoPagamento;
    IObserver<int> IUniprPagamentiObserver.NuovoPagamento => nuovoPagamento;
}