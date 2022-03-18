using Hkmp.Networking.Packet;
using Vector2 = Hkmp.Math.Vector2;

namespace HKMP_HealthDisplay.Settings;

public class ToClientPacketData : IPacketData
{
    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => true;

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
    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => true;

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

public enum ClientPacketId
{
    SendHealth
}
public enum ServerPacketId
{
    SendHealth
}

