using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
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

        RegisterCommands(api);
    }

    private void Event_PlayerJoin(IServerPlayer byPlayer)
    {
        // Set all players to have fast walk speed the same as hunter profession
        byPlayer.Entity.Stats.Set("walkspeed", "universalfastwalk", config.WalkSpeedBuff, false);
    }

    private void RegisterCommands(ICoreServerAPI api)
    {
        api.ChatCommands.Create("inventory")
            .WithDescription("Prints your worn clothing by slot")
            .RequiresPrivilege(Privilege.chat)
            .WithArgs(api.ChatCommands.Parsers.OnlinePlayer("target"))
            .HandleWith(args =>
            {
                var player = args[0] as IServerPlayer;
                if (player == null)
                    return TextCommandResult.Error("Target player not found or not online.");

                var invMgr = player.InventoryManager;
                if (invMgr == null)
                    return TextCommandResult.Error("No inventory manager available.");

                var gearInv = invMgr.GetOwnInventory(GlobalConstants.characterInvClassName);
                if (gearInv == null)
                    return TextCommandResult.Error("Could not access worn clothing inventory.");

                foreach (var slot in gearInv)
                {
                    if (slot.Empty)
                        continue;

                    string itemName = slot.Itemstack.Collectible.Code.ToString();
                    player.SendMessage(GlobalConstants.GeneralChatGroup, itemName, EnumChatType.CommandSuccess);
                }

                return TextCommandResult.Success("Listed your worn clothing.");
            });

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
