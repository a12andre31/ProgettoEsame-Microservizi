using Azure;
using Magazzino.Repository.Model;
using Magazzino.Shared;
using System.Text.Json;
using Utility.Kafka.Constants;
using Utility.Kafka.Messages;

namespace Magazzino.Business.Factory;

public static class TransactionalOutboxFactory
{
    public static TransactionalOutbox CreateInsert(RispostaMagazzinoDto dto) => Create("Articolo", dto, Operations.Insert);
    public static TransactionalOutbox CreateUpdate(RispostaMagazzinoDto dto) => Create("Articolo", dto, Operations.Update);

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