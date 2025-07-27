using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Professions;

public class ProfessionsModSystem : ModSystem
{
    public ProfessionsConfig config;

    public override void Start(ICoreAPI api) { }

    public override void StartClientSide(ICoreClientAPI api) { }

    public override void StartServerSide(ICoreServerAPI api)
    {
        config = api.LoadModConfig<ProfessionsConfig>("ProfessionsConfig.json") ?? ProfessionsConfig.GetDefault(api);

        //api.Event.ServerRunPhase(EnumServerRunPhase.WorldReady, loadCharacterClasses);
        api.Event.PlayerJoin += Event_PlayerJoin;
    }

    private void Event_PlayerJoin(IServerPlayer byPlayer)
    {
        // Set all players to have fast walk speed the same as hunter profession
        byPlayer.Entity.Stats.Set("walkspeed", "universalfastwalk", config.WalkSpeedBuff, false);
    }
}

public class ProfessionsConfig
{
    public float WalkSpeedBuff = 0.1f;

    public static ProfessionsConfig GetDefault(ICoreAPI api)
    {
        var cfg = new ProfessionsConfig();
        api.StoreModConfig(cfg, "ProfessionsConfig.json");
        return cfg;
    }
}
