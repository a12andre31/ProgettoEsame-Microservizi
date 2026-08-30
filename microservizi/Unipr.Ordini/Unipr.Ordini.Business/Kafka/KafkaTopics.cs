using Microsoft.Extensions.DependencyInjection;

namespace Ordini.Business.Kafka;

public class KafkaTopicsOutput : AbstractKafkaTopics
{
    public string Ordini { get; set; } = "Ordini";
    public string Magazzino { get; set; } = "Magazzino";

    public string Pagamenti { get; set; } = "Pagamenti";

    public override IEnumerable<string> GetTopics() => [Ordini, Magazzino, Pagamenti];
}