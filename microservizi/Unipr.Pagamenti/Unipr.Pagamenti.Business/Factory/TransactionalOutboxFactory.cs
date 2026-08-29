using System.Text.Json;
using Pagamenti.Repository.Model;
using Pagamenti.Shared;
using Utility.Kafka.Constants;
using Utility.Kafka.Messages;

namespace Pagamenti.Business.Factory;

public static class TransactionalOutboxFactory
{
    public static TransactionalOutbox CreateInsert(RispostaPagamentoDto dto) => Create("Pagamento", dto, Operations.Insert);

    private static TransactionalOutbox Create<TDTO>(string table, TDTO dto, string operation) where TDTO : class
    {
        var opMsg = new OperationMessage<TDTO> { Dto = dto, Operation = operation };
        opMsg.CheckMessage();
        return new TransactionalOutbox { Tabella = table, Messaggio = JsonSerializer.Serialize(opMsg) };
    }
}