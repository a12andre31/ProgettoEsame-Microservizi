using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Subjects;
using Ordini.Business.Abstraction;

namespace Ordini.Business;

[method: ActivatorUtilitiesConstructor]
public class Subject() : ISubject
{
    private Subject<int> nuovoOrdine { get; } = new Subject<int>();

    IObservable<int> IUniprOrdiniObservable.NuovoOrdine => nuovoOrdine;
    IObserver<int> IUniprOrdiniObserver.NuovoOrdine => nuovoOrdine;
}