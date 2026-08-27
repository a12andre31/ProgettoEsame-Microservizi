using Microsoft.Extensions.DependencyInjection;

namespace Magazzino.Business.Kafka;

public class KafkaTopicsOutput : AbstractKafkaTopics
{
    public string Magazzino { get; set; } = "Magazzino";

    public override IEnumerable<string> GetTopics() => [Magazzino];
}