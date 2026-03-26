using System.Net.Sockets;

namespace BetaSharp.Network.Packets.Play;

public class PlayerRespawnPacket() : Packet(PacketId.PlayerRespawn)
{
    public sbyte dimensionId;
    public sbyte gameMode;

    public static PlayerRespawnPacket Get(sbyte dimensionId, sbyte gameMode = 0)
    {
        var p = Get<PlayerRespawnPacket>(PacketId.PlayerRespawn);
        p.dimensionId = dimensionId;
        p.gameMode = gameMode;
        return p;
    }

    public override void Apply(NetHandler handler)
    {
        handler.onPlayerRespawn(this);
    }

    public override void Read(NetworkStream stream)
    {
        dimensionId = (sbyte)stream.ReadByte();
        if (stream.DataAvailable)
        {
            gameMode = (sbyte)stream.ReadByte();
        }
    }

    public override void Write(NetworkStream stream)
    {
        stream.WriteByte((byte)dimensionId);
        stream.WriteByte((byte)gameMode);
    }

    public override int Size()
    {
        return 2;
    }
}
