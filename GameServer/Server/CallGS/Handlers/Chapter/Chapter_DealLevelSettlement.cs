using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MikuSB.GameServer.Server.CallGS.Handlers.Chapter;

[CallGSApi("Chapter_DealLevelSettlement")]
public class Chapter_DealLevelSettlement : ICallGSHandler
{
    public async Task Handle(Connection connection, string param, ushort seqNo)
    {
        var req = JsonSerializer.Deserialize<DealLevelSettlementParam>(param);
        var response = new JsonObject
        {
            ["sCmd"] = req?.SCmd ?? "Chapter_LevelSettlement",
            ["tbParam"] = BuildSettlementPayload(req?.SCmd, req?.TbParam)
        };

        await CallGSRouter.SendScript(connection, "Chapter_DealLevelSettlement", response.ToJsonString());
    }

    private static JsonNode BuildSettlementPayload(string? sCmd, JsonNode? tbParam)
    {
        if (string.Equals(sCmd, "Chapter_LevelSettlement", StringComparison.Ordinal))
        {
            return new JsonArray();
        }

        if (string.Equals(sCmd, "Chapter_NewPrologueSettlement", StringComparison.Ordinal))
        {
            var result = new JsonObject();
            if (tbParam is JsonObject source && source.TryGetPropertyValue("bWaitServer", out var bWaitServer))
            {
                result["bWaitServer"] = bWaitServer?.DeepClone();
            }
            result["tbShowAward"] = new JsonArray();
            return result;
        }

        return tbParam?.DeepClone() ?? new JsonObject();
    }
}

internal sealed class DealLevelSettlementParam
{
    [JsonPropertyName("sCmd")]
    public string? SCmd { get; set; }

    [JsonPropertyName("tbParam")]
    public JsonNode? TbParam { get; set; }
}
