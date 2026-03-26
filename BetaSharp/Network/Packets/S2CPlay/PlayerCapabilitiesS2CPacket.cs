using System.Net.Sockets;
using BetaSharp.Entities;

namespace BetaSharp.Network.Packets.S2CPlay;

public class PlayerCapabilitiesS2CPacket() : ExtendedProtocolPacket(PacketId.PlayerCapabilitiesS2C)
{
    public bool disableDamage;
    public bool isFlying;
    public bool allowFlying;
    public bool isCreativeMode;
    public sbyte gameMode;
    public float flySpeed;
    public float walkSpeed;

    public static PlayerCapabilitiesS2CPacket Get(PlayerCapabilities capabilities)
    {
        var p = Get<PlayerCapabilitiesS2CPacket>(PacketId.PlayerCapabilitiesS2C);
        p.disableDamage = capabilities.disableDamage;
        p.isFlying = capabilities.isFlying;
        p.allowFlying = capabilities.allowFlying;
        p.isCreativeMode = capabilities.isCreativeMode;
        p.gameMode = (sbyte)capabilities.gameMode;
        p.flySpeed = capabilities.GetFlySpeed();
        p.walkSpeed = capabilities.GetWalkSpeed();
        return p;
    }

    public override void Read(NetworkStream stream)
    {
        byte flags = (byte)stream.ReadByte();
        disableDamage = (flags & 1) != 0;
        isFlying = (flags & 2) != 0;
        allowFlying = (flags & 4) != 0;
        isCreativeMode = (flags & 8) != 0;
        if (stream.DataAvailable)
        {
            gameMode = (sbyte)stream.ReadByte();
        }
        else
        {
            gameMode = (sbyte)(isCreativeMode ? Worlds.Core.Systems.GameMode.Creative : Worlds.Core.Systems.GameMode.Survival);
        }

        if (stream.DataAvailable)
        {
            flySpeed = stream.ReadFloat();
            walkSpeed = stream.ReadFloat();
        }
        else
        {
            flySpeed = 0.05F;
            walkSpeed = 0.1F;
        }
    }

    public override void Write(NetworkStream stream)
    {
        byte flags = 0;
        if (disableDamage) flags |= 1;
        if (isFlying) flags |= 2;
        if (allowFlying) flags |= 4;
        if (isCreativeMode) flags |= 8;
        stream.WriteByte(flags);
        stream.WriteByte((byte)gameMode);
        stream.WriteFloat(flySpeed);
        stream.WriteFloat(walkSpeed);
    }

    public override void Apply(NetHandler handler)
    {
        handler.onPlayerCapabilities(this);
    }

    public override int Size()
    {
        return 10;
    }
}
