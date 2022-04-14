using Hkmp.Networking.Packet;
using Vector2 = Hkmp.Math.Vector2;

namespace HKMP_HealthDisplay.HKMP;

public class ToClientPacketData : IPacketData
{
    public bool IsReliable => false;
    public bool DropReliableDataIfNewerExists => false;

    public byte health { get; set; }
    public ushort soul { get; set; }
    public ushort playerId { get; set; }

    public void WriteData(IPacket packet)
    {
        packet.Write(playerId);
        packet.Write(health);
        packet.Write(soul);
    }

    public void ReadData(IPacket packet)
    {
        playerId = packet.ReadUShort();
        health = packet.ReadByte();
        soul = packet.ReadUShort();
    }
}

public class ToServerPacketData : IPacketData
{
    public bool IsReliable => false;
    public bool DropReliableDataIfNewerExists => false;

    public byte health { get; set; }
    public ushort soul { get; set; }

    public void WriteData(IPacket packet)
    {
        packet.Write(health);
        packet.Write(soul);
    }

    public void ReadData(IPacket packet)
    {
        health = packet.ReadByte();
        soul = packet.ReadUShort();
    }
}

public enum ClientPackets
{
    SendHealth
}
public enum ServerPackets
{
    SendHealth
}

