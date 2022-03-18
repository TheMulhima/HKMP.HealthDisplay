using System.Drawing.Printing;
using Hkmp.Api.Server;
using Hkmp.Networking.Packet;
using HKMP_HealthDisplay.Settings;
using ServerPacketId = HKMP_HealthDisplay.Settings.ServerPacketId;

namespace HKMP_HealthDisplay;

public class HealthDisplayServer : ServerAddon 
{
        internal IServerApi _serverApi;

        public override void Initialize(IServerApi serverApi)
        {
            _serverApi = serverApi;
            
            var netReceiver = serverApi.NetServer.GetNetworkReceiver<ServerPacketId>(this,InstantiatePacket);
            var netSender = serverApi.NetServer.GetNetworkSender<ClientPacketId>(this);

            netReceiver.RegisterPacketHandler<ToServerPacketData>
            (
                ServerPacketId.SendHealth,
                (id, packetData) => 
                {
                    var player = serverApi.ServerManager.GetPlayer(id);

                    netSender.BroadcastSingleData(ClientPacketId.SendHealth, new ToClientPacketData()
                    {
                        playerId = id,
                        health = packetData.health,
                        soul = packetData.soul, 
                    });

                }
            );
        }

        private static IPacketData InstantiatePacket(ServerPacketId packetId) {
            switch (packetId) 
            {
                case ServerPacketId.SendHealth:
                    return new ToServerPacketData();
            }
            return null;
        }

        protected override string Name => "Health Display";
        protected override string Version => "0.0.1";
        public override bool NeedsNetwork => true;
    }