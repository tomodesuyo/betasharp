using System.Net.Sockets;

namespace BetaSharp.Network.Packets.C2SPlay;

public class SelectMerchantTradeC2SPacket() : Packet(PacketId.SelectMerchantTradeC2S)
{
    public int syncId;
    public int recipeIndex;

    public static SelectMerchantTradeC2SPacket Get(int syncId, int recipeIndex)
    {
        var packet = Get<SelectMerchantTradeC2SPacket>(PacketId.SelectMerchantTradeC2S);
        packet.syncId = syncId;
        packet.recipeIndex = recipeIndex;
        return packet;
    }

    public override void Apply(NetHandler handler)
    {
        handler.onSelectMerchantTrade(this);
    }

    public override void Read(NetworkStream stream)
    {
        syncId = (sbyte)stream.ReadByte();
        recipeIndex = stream.ReadShort();
    }

    public override void Write(NetworkStream stream)
    {
        stream.WriteByte((byte)syncId);
        stream.WriteShort((short)recipeIndex);
    }

    public override int Size()
    {
        return 3;
    }
}
