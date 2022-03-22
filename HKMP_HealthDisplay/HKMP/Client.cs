namespace HKMP_HealthDisplay.HKMP;

public class HealthDisplayClient : ClientAddon
{
    public ClientAddon clientAddon;
    public IClientApi clientApi;
    
    protected override string Name => "Health Display";
    protected override string Version => "0.0.1";
    public override bool NeedsNetwork => true;

    internal static Dictionary<IClientPlayer, HealthBarController> HealthBarComponentCache =
        new Dictionary<IClientPlayer, HealthBarController>();

    public override void Initialize(IClientApi _clientApi)
    {
        this.clientApi = _clientApi;
        clientAddon = this;

        Logger.Info(clientAddon, "Woah now you can peak at peoples healths");

        var netReceiver = _clientApi.NetClient.GetNetworkReceiver<ClientPackets>(clientAddon, InstantiatePacket);
        var netSender = _clientApi.NetClient.GetNetworkSender<ServerPackets>(clientAddon);
        
        netReceiver.RegisterPacketHandler<ToClientPacketData>(
            ClientPackets.SendHealth,
            packetData =>
            {
                var player = this.clientApi.ClientManager.GetPlayer(packetData.playerId);
                if (player is { IsInLocalScene: true })
                {
                    if (HealthBarComponentCache.TryGetValue(player, out var htop))
                    {
                        if (htop == null)
                        {
                            htop = getAddHealthOnTopOfPlayer(player);
                        }

                        htop.Host = player.PlayerContainer;
                        htop.UpdateText(packetData.health, packetData.soul);
                    }
                    else
                    {
                        htop = AddPlayerToCache(player);
                        htop.Host = player.PlayerContainer;
                        htop.UpdateText(packetData.health, packetData.soul);
                    }
                }
            });
    }

    private HealthBarController AddPlayerToCache(IClientPlayer player)
    {
        HealthBarController htop = getAddHealthOnTopOfPlayer(player);
        
        HealthBarComponentCache[player] = htop;
        return htop;
    }

    private HealthBarController getAddHealthOnTopOfPlayer(IClientPlayer player)
    {
        HealthBarController htop;
        if (player.PlayerContainer.GetComponent<HealthBarController>() == null)
        {
            htop = player.PlayerContainer.AddComponent<HealthBarController>();
            htop.Host = player.PlayerContainer;
        }
        else
        {
            htop = player.PlayerContainer.GetComponent<HealthBarController>();
        }

        return htop;
    }
    
    public void SendUpdate(int newHealth, int newsoul)
    {
        var netSender = clientApi.NetClient.GetNetworkSender<ServerPackets>(clientAddon);
        netSender.SendSingleData(ServerPackets.SendHealth, new ToServerPacketData
        {
            health = (byte) newHealth,
            soul = (ushort) newsoul,
        });
    }
    
    private static IPacketData InstantiatePacket(ClientPackets Clientpackets) {
        switch (Clientpackets) 
        {
            case ClientPackets.SendHealth:
                return new ToClientPacketData();
        }
        return null;
    }
}


