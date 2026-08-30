using Microsoft.Extensions.DependencyInjection;

namespace Pagamenti.Business.Kafka;

public class KafkaTopicsOutput : AbstractKafkaTopics
{
    public string Pagamenti { get; set; } = "Pagamenti";
    public string Ordini { get; set; } = "Ordini";

    public override IEnumerable<string> GetTopics() => [Ordini];
}