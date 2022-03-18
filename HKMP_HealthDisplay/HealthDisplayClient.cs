using Hkmp.Api.Client;
using Hkmp.Networking.Packet;
using HKMP_HealthDisplay.Settings;
using ServerPacketId = HKMP_HealthDisplay.Settings.ServerPacketId;

namespace HKMP_HealthDisplay;

public class HealthDisplayClient : ClientAddon
{
    public ClientAddon _clientAddon;
    public IClientApi _clientApi;
    
    protected override string Name => "Health Display";
    protected override string Version => "0.0.1";
    public override bool NeedsNetwork => true;

    private static Dictionary<IClientPlayer, HealthOnTopOfPlayer> Cache =
        new Dictionary<IClientPlayer, HealthOnTopOfPlayer>();

    public override void Initialize(IClientApi clientApi)
    {
        _clientApi = clientApi;
        _clientAddon = this;

        Logger.Info(_clientAddon, "Woah now you can peak at peoples healths");

        var netReceiver = clientApi.NetClient.GetNetworkReceiver<ClientPacketId>(_clientAddon, InstantiatePacket);
        var netSender = clientApi.NetClient.GetNetworkSender<ServerPacketId>(_clientAddon);
        
        netReceiver.RegisterPacketHandler<ToClientPacketData>(
            ClientPacketId.SendHealth,
            packetData =>
            {
                var player = _clientApi.ClientManager.GetPlayer(packetData.playerId);
                if (player is { IsInLocalScene: true }) //for local player
                {
                    if (Cache.TryGetValue(player, out var htop))
                    {
                        if (htop == null)
                        {
                            htop = getAddHealthOnTopOfPlayer(player);
                        }
                        htop.UpdateText(packetData.health, packetData.soul);
                    }
                    else
                    {
                        htop = AddPlayerToCache(player);
                        htop.UpdateText(packetData.health, packetData.soul);
                    }
                }
            });
    }

    private HealthOnTopOfPlayer AddPlayerToCache(IClientPlayer player)
    {
        HealthOnTopOfPlayer htop = getAddHealthOnTopOfPlayer(player);
        
        Cache[player] = htop;
        return htop;
    }

    private HealthOnTopOfPlayer getAddHealthOnTopOfPlayer(IClientPlayer player)
    {
        HealthOnTopOfPlayer htop;
        if (player.PlayerContainer.GetComponent<HealthOnTopOfPlayer>() == null)
        {
            htop = player.PlayerContainer.AddComponent<HealthOnTopOfPlayer>();
            htop.Host = player.PlayerContainer;
        }
        else
        {
            htop = player.PlayerContainer.GetComponent<HealthOnTopOfPlayer>();
        }

        return htop;
    }
    
    public void SendUpdate(int newHealth, int newsoul)
    {
        var netSender = _clientApi.NetClient.GetNetworkSender<ServerPacketId>(_clientAddon);
        netSender.SendSingleData(ServerPacketId.SendHealth, new ToServerPacketData
        {
            health = (byte) newHealth,
            soul = (ushort) newsoul,
        });
    }
    
    private static IPacketData InstantiatePacket(ClientPacketId packetId) {
        switch (packetId) 
        {
            case ClientPacketId.SendHealth:
                return new ToClientPacketData();
        }
        return null;
    }
}


