using System.Net.Sockets;
using BetaSharp.Items;
using BetaSharp.Trading;

namespace BetaSharp.Network.Packets.S2CPlay;

public class MerchantOffersS2CPacket() : Packet(PacketId.MerchantOffersS2C)
{
    public int syncId;
    public int selectedRecipeIndex;
    public MerchantRecipeList offers = new();

    public static MerchantOffersS2CPacket Get(int syncId, MerchantRecipeList offers, int selectedRecipeIndex)
    {
        var packet = Get<MerchantOffersS2CPacket>(PacketId.MerchantOffersS2C);
        packet.syncId = syncId;
        packet.selectedRecipeIndex = selectedRecipeIndex;
        packet.offers = new MerchantRecipeList();

        foreach (MerchantRecipe recipe in offers)
        {
            packet.offers.Add(new MerchantRecipe(recipe.BuyItem1, recipe.BuyItem2, recipe.SellItem, recipe.ToolUses, recipe.MaxToolUses));
        }

        return packet;
    }

    public override void Apply(NetHandler handler)
    {
        handler.onMerchantOffers(this);
    }

    public override void Read(NetworkStream stream)
    {
        syncId = (sbyte)stream.ReadByte();
        selectedRecipeIndex = stream.ReadShort();
        int count = stream.ReadByte();
        offers = new MerchantRecipeList();

        for (int i = 0; i < count; ++i)
        {
            ItemStack buyItem1 = ReadItemStack(stream)!;
            ItemStack? buyItem2 = stream.ReadBoolean() ? ReadItemStack(stream) : null;
            ItemStack sellItem = ReadItemStack(stream)!;
            int toolUses = stream.ReadShort();
            int maxToolUses = stream.ReadShort();
            offers.Add(new MerchantRecipe(buyItem1, buyItem2, sellItem, toolUses, maxToolUses));
        }
    }

    public override void Write(NetworkStream stream)
    {
        stream.WriteByte((byte)syncId);
        stream.WriteShort((short)selectedRecipeIndex);
        stream.WriteByte((byte)offers.Count);

        foreach (MerchantRecipe recipe in offers)
        {
            WriteItemStack(stream, recipe.BuyItem1);
            stream.WriteBoolean(recipe.BuyItem2 != null);
            if (recipe.BuyItem2 != null)
            {
                WriteItemStack(stream, recipe.BuyItem2);
            }

            WriteItemStack(stream, recipe.SellItem);
            stream.WriteShort((short)recipe.ToolUses);
            stream.WriteShort((short)recipe.MaxToolUses);
        }
    }

    public override int Size()
    {
        return 4 + offers.Count * 20;
    }

    private static void WriteItemStack(NetworkStream stream, ItemStack? stack)
    {
        if (stack == null)
        {
            stream.WriteShort(-1);
            return;
        }

        stream.WriteShort((short)stack.itemId);
        stream.WriteByte((byte)stack.count);
        stream.WriteShort((short)stack.getDamage());
    }

    private static ItemStack? ReadItemStack(NetworkStream stream)
    {
        short itemId = stream.ReadShort();
        if (itemId < 0)
        {
            return null;
        }

        sbyte count = (sbyte)stream.ReadByte();
        short damage = stream.ReadShort();
        return new ItemStack(itemId, count, damage);
    }
}
