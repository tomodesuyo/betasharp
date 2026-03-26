using System.Net.Sockets;
using BetaSharp.Items;

namespace BetaSharp.Network.Packets.C2SPlay;

public class CreativeInventoryActionC2SPacket() : ExtendedProtocolPacket(PacketId.CreativeInventoryActionC2S)
{
    public int slot;
    public ItemStack? stack;

    public static CreativeInventoryActionC2SPacket Get(int slot, ItemStack? stack)
    {
        var p = Get<CreativeInventoryActionC2SPacket>(PacketId.CreativeInventoryActionC2S);
        p.slot = slot;
        p.stack = stack?.copy();
        return p;
    }

    public override void Read(NetworkStream stream)
    {
        slot = stream.ReadShort();
        short itemId = stream.ReadShort();
        if (itemId >= 0)
        {
            sbyte count = (sbyte)stream.ReadByte();
            short damage = stream.ReadShort();
            stack = new ItemStack(itemId, count, damage);
        }
        else
        {
            stack = null;
        }
    }

    public override void Write(NetworkStream stream)
    {
        stream.WriteShort((short)slot);
        if (stack == null)
        {
            stream.WriteShort((short)-1);
        }
        else
        {
            stream.WriteShort((short)stack.itemId);
            stream.WriteByte((byte)stack.count);
            stream.WriteShort((short)stack.getDamage());
        }
    }

    public override void Apply(NetHandler handler)
    {
        handler.onCreativeInventoryAction(this);
    }

    public override int Size()
    {
        return 8;
    }
}
