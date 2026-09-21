using Terraria;
using Terraria.ID;

namespace VariousModPatches
{
    public static class NetmodeHelper
    {
        public static bool IsServer => Main.netMode == NetmodeID.Server;
        public static bool IsSingleplayer => Main.netMode == NetmodeID.SinglePlayer;
        public static bool IsMultiplayerClient => Main.netMode == NetmodeID.MultiplayerClient;
    }
}