using MikuSB.Proto;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MikuSB.GameServer.Server.CallGS.Handlers.House;

[CallGSApi("House_Request")]
public class House_Request : ICallGSHandler
{
    private static readonly Random Random = new();

    // GID for all house/arcade attributes
    private const uint ArcadeGid = 101;
    // HouseStorage.HouseArcadeStart = 18000
    // Attr_GirlEndlessModeState = 5  → SID 18005
    // ConditionType.TeachMode = 8, Attr_ConditionValStart = 36 → SID 18000+36+8 = 18044
    private const uint EndlessModeStateSid = 18000 + 5;
    private const uint TeachModeConditionSid = 18000 + 36 + 8; // = 18044

    public async Task Handle(Connection connection, string param, ushort seqNo)
    {
        var req = JsonSerializer.Deserialize<HouseRequestParam>(param);
        if (req?.FuncName == null) return;

        switch (req.FuncName)
        {
            case "ArcadeGameEnterMainUI":
                await HandleArcadeGameEnterMainUI(connection);
                break;
            case "ArcadeGameEnter":
                await HandleArcadeGameEnter(connection);
                break;
            case "ArcadeGameSettlement":
                await HandleArcadeGameSettlement(connection);
                break;
            case "ArcadeGameLogSettlement":
                await HandleArcadeGameLogSettlement(connection);
                break;
            default:
                var err = new JsonObject { ["FuncName"] = req.FuncName, ["sErr"] = "error.NotImplemented" };
                await CallGSRouter.SendScript(connection, "House_Request", err.ToJsonString());
                break;
        }
    }

    private static async Task HandleArcadeGameEnterMainUI(Connection connection)
    {
        var girlList = new JsonArray();
        for (int i = 1; i <= 25; i++)
            girlList.Add(i);

        var rsp = new JsonObject
        {
            ["FuncName"] = "ArcadeGameEnterMainUI",
            ["tbUnlockGirlList"] = girlList
        };

        var player = connection.Player!;
        var sync = new NtfSyncPlayer();

        sync.Custom[player.ToPackedAttrKey(ArcadeGid, TeachModeConditionSid)] = 1;
        sync.Custom[player.ToShiftedAttrKey(ArcadeGid, TeachModeConditionSid)] = 1;

        // Bits 1-25 set → all girls have endless mode unlocked
        const uint endlessAllUnlocked = 0x3FFFFFE; // bits 1-25
        sync.Custom[player.ToPackedAttrKey(ArcadeGid, EndlessModeStateSid)] = endlessAllUnlocked;
        sync.Custom[player.ToShiftedAttrKey(ArcadeGid, EndlessModeStateSid)] = endlessAllUnlocked;

        await CallGSRouter.SendScript(connection, "House_Request", rsp.ToJsonString(), sync);
    }

    private static async Task HandleArcadeGameEnter(Connection connection)
    {
        var seed = Random.Next(1, 1_000_000_000);
        var rsp = new JsonObject
        {
            ["FuncName"] = "ArcadeGameEnter",
            ["nSeed"] = seed
        };
        await CallGSRouter.SendScript(connection, "House_Request", rsp.ToJsonString());
    }

    private static async Task HandleArcadeGameSettlement(Connection connection)
    {
        var rsp = new JsonObject
        {
            ["FuncName"] = "ArcadeGameSettlement",
            ["nAddExp"] = 0
        };
        await CallGSRouter.SendScript(connection, "House_Request", rsp.ToJsonString());
    }

    private static async Task HandleArcadeGameLogSettlement(Connection connection)
    {
        var rsp = new JsonObject { ["FuncName"] = "ArcadeGameLogSettlement" };
        await CallGSRouter.SendScript(connection, "House_Request", rsp.ToJsonString());
    }
}

internal sealed class HouseRequestParam
{
    [JsonPropertyName("FuncName")]
    public string? FuncName { get; set; }
}
