using System.Net.Sockets;
using BetaSharp.Entities;

namespace BetaSharp.Network.Packets.C2SPlay;

public class PlayerCapabilitiesC2SPacket() : ExtendedProtocolPacket(PacketId.PlayerCapabilitiesC2S)
{
    public bool disableDamage;
    public bool isFlying;
    public bool allowFlying;
    public bool isCreativeMode;
    public float flySpeed;
    public float walkSpeed;

    public static PlayerCapabilitiesC2SPacket Get(PlayerCapabilities capabilities)
    {
        var packet = Get<PlayerCapabilitiesC2SPacket>(PacketId.PlayerCapabilitiesC2S);
        packet.disableDamage = capabilities.disableDamage;
        packet.isFlying = capabilities.isFlying;
        packet.allowFlying = capabilities.allowFlying;
        packet.isCreativeMode = capabilities.isCreativeMode;
        packet.flySpeed = capabilities.GetFlySpeed();
        packet.walkSpeed = capabilities.GetWalkSpeed();
        return packet;
    }

    public override void Read(NetworkStream stream)
    {
        byte flags = (byte)stream.ReadByte();
        disableDamage = (flags & 1) != 0;
        isFlying = (flags & 2) != 0;
        allowFlying = (flags & 4) != 0;
        isCreativeMode = (flags & 8) != 0;
        flySpeed = stream.ReadFloat();
        walkSpeed = stream.ReadFloat();
    }

    public override void Write(NetworkStream stream)
    {
        byte flags = 0;
        if (disableDamage) flags |= 1;
        if (isFlying) flags |= 2;
        if (allowFlying) flags |= 4;
        if (isCreativeMode) flags |= 8;
        stream.WriteByte(flags);
        stream.WriteFloat(flySpeed);
        stream.WriteFloat(walkSpeed);
    }

    public override void Apply(NetHandler handler)
    {
        handler.onPlayerCapabilities(this);
    }

    public override int Size()
    {
        return 9;
    }
}
