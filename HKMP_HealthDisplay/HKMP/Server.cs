namespace HKMP_HealthDisplay.HKMP;

public class HealthDisplayServer : ServerAddon 
{
        public IServerApi serverApi;

        public override void Initialize(IServerApi _serverApi)
        {
            this.serverApi = _serverApi;
            
            var netReceiver = _serverApi.NetServer.GetNetworkReceiver<ServerPackets>(this,InstantiatePacket);
            var netSender = _serverApi.NetServer.GetNetworkSender<ClientPackets>(this);

            netReceiver.RegisterPacketHandler<ToServerPacketData>
            (
                ServerPackets.SendHealth,
                (id, packetData) => 
                {
                    netSender.BroadcastSingleData(ClientPackets.SendHealth, new ToClientPacketData()
                    {
                        playerId = id,
                        health = packetData.health,
                        soul = packetData.soul, 
                    });

                }
            );
        }

        private static IPacketData InstantiatePacket(ServerPackets Serverpackets) {
            switch (Serverpackets) 
            {
                case ServerPackets.SendHealth:
                    return new ToServerPacketData();
            }
            return null;
        }

        protected override string Name => "Health Display";
        protected override string Version => "0.0.1";
        public override bool NeedsNetwork => true;
    }