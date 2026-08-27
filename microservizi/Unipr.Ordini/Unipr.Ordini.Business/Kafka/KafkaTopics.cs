using Microsoft.Extensions.DependencyInjection;

namespace Ordini.Business.Kafka;

public class KafkaTopicsOutput : AbstractKafkaTopics
{
    public string Ordini { get; set; } = "Ordini";

    public override IEnumerable<string> GetTopics() => [Ordini];
}