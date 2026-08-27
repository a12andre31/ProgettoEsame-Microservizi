using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Subjects;
using Magazzino.Business.Abstraction;

namespace Magazzino.Business;

[method: ActivatorUtilitiesConstructor]
public class Subject() : ISubject
{
    private Subject<int> nuovoOrdine { get; } = new Subject<int>();

    IObservable<int> IUniprMagazzinoObservable.NuovoOrdine => nuovoOrdine;
    IObserver<int> IUniprMagazzinoObserver.NuovoOrdine => nuovoOrdine;
}