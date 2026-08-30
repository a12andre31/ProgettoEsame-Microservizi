using Microsoft.Extensions.DependencyInjection;

namespace Magazzino.Business.Kafka;

public class KafkaTopicsOutput : AbstractKafkaTopics
{
    public string Magazzino { get; set; } = "Magazzino";
    public string Ordini { get; set; } = "Ordini";

    public override IEnumerable<string> GetTopics() => [Magazzino, Ordini];
}