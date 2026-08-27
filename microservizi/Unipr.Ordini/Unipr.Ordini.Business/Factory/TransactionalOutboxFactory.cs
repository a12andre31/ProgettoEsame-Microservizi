using Azure;
using Ordini.Repository.Model;
using Ordini.Shared;
using System.Text.Json;
using Utility.Kafka.Constants;
using Utility.Kafka.Messages;

namespace Ordini.Business.Factory;

public static class TransactionalOutboxFactory
{
    public static TransactionalOutbox CreateInsert(OrdineReadDto dto) => Create("Ordine", dto, Operations.Insert);
    public static TransactionalOutbox CreateUpdate(OrdineReadDto dto) => Create("Ordine", dto, Operations.Update);

    private static TransactionalOutbox Create<TDTO>(string table, TDTO dto, string operation) where TDTO : class
    {
        OperationMessage<TDTO> opMsg = new OperationMessage<TDTO>()
        {
            Dto = dto,
            Operation = operation
        };
        opMsg.CheckMessage();

        return new TransactionalOutbox()
        {
            Tabella = table,
            Messaggio = JsonSerializer.Serialize(opMsg)
        };
    }
}