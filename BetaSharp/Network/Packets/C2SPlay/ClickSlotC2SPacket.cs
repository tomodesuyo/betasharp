using System.Net.Sockets;
using BetaSharp.Items;

namespace BetaSharp.Network.Packets.C2SPlay;

public class ClickSlotC2SPacket() : Packet(PacketId.ClickSlotC2S)
{
    public int syncId;
    public int slot;
    public int button;
    public int mode;
    public short actionType;
    public ItemStack stack;

    public static ClickSlotC2SPacket Get(int syncId, int slot, int button, int mode, ItemStack stack, short actionType)
    {
        var p = Get<ClickSlotC2SPacket>(PacketId.ClickSlotC2S);
        p.syncId = syncId;
        p.slot = slot;
        p.button = button;
        p.mode = mode;
        p.stack = stack;
        p.actionType = actionType;
        return p;
    }

    public override void Apply(NetHandler handler)
    {
        handler.onClickSlot(this);
    }

    public override void Read(NetworkStream stream)
    {
        syncId = (sbyte)stream.ReadByte();
        slot = stream.ReadShort();
        button = (sbyte)stream.ReadByte();
        mode = (sbyte)stream.ReadByte();
        actionType = stream.ReadShort();
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
        stream.WriteByte((byte)syncId);
        stream.WriteShort((short)slot);
        stream.WriteByte((byte)button);
        stream.WriteByte((byte)mode);
        stream.WriteShort((short)actionType);
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

    public override int Size()
    {
        return 12;
    }
}
