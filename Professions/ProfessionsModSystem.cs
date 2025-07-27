using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Professions;

public class ProfessionsModSystem : ModSystem
{
    public ProfessionsConfig config;
    private CharacterSystem characterSystem;
    private ICoreAPI api;

    public override void Start(ICoreAPI api)
    {
        this.api = api;
        characterSystem = api.ModLoader.GetModSystem<CharacterSystem>();
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Event.IsPlayerReady += OnPlayerReady;
    }

    private bool OnPlayerReady(ref EnumHandling handling)
    {
        loadCharacterClasses();

        return true;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Event.ServerRunPhase(EnumServerRunPhase.WorldReady, loadCharacterClasses);

        config = api.LoadModConfig<ProfessionsConfig>("ProfessionsConfig.json") ?? ProfessionsConfig.GetDefault(api);

        api.Event.PlayerJoin += Event_PlayerJoin;
    }

    private void Event_PlayerJoin(IServerPlayer byPlayer)
    {
        // Set all players to have fast walk speed the same as hunter profession
        byPlayer.Entity.Stats.Set("walkspeed", "universalfastwalk", config.WalkSpeedBuff, false);
    }

    private void loadCharacterClasses()
    {
        characterSystem.traits.Clear();
        characterSystem.TraitsByCode.Clear();
        characterSystem.characterClasses.Clear();
        characterSystem.characterClassesByCode.Clear();

        characterSystem.traits = api.Assets.Get("professions:config/traits.json").ToObject<List<Trait>>();
        characterSystem.characterClasses = api.Assets.Get("professions:config/characterclasses.json").ToObject<List<CharacterClass>>();

        foreach (Trait trait in characterSystem.traits)
        {
            characterSystem.TraitsByCode[trait.Code] = trait;
        }

        foreach (CharacterClass characterClass in characterSystem.characterClasses)
        {
            characterSystem.characterClassesByCode[characterClass.Code] = characterClass;
            JsonItemStack[] gear = characterClass.Gear;
            foreach (JsonItemStack jsonItemStack in gear)
            {
                if (!jsonItemStack.Resolve(api.World, "character class gear", printWarningOnError: false))
                {
                    api.World.Logger.Warning(string.Concat("Unable to resolve character class gear ", jsonItemStack.Type.ToString(), " with code ", jsonItemStack.Code, " item/block does not seem to exist. Will ignore."));
                }
            }
        }
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
