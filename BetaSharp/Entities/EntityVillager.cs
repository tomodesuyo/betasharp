using BetaSharp.NBT;
using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Trading;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Villages;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityVillager : EntityAnimal, IMerchant
{
    private int _randomTickDivider;
    private MerchantRecipeList? _buyingList;
    private EntityPlayer? _customer;

    public override EntityType Type => EntityRegistry.Villager;
    public readonly SyncedProperty<int> Profession;
    public Village? Village { get; private set; }

    public EntityVillager(IWorldContext world) : this(world, world.Random.NextInt(5))
    {
    }

    public EntityVillager(IWorldContext world, int profession) : base(world)
    {
        texture = "/mob/villager/villager.png";
        setBoundingBoxSpacing(0.6F, 1.8F);
        movementSpeed = 0.5F;
        health = 20;
        _randomTickDivider = 0;
        Profession = DataSynchronizer.MakeProperty(16, profession);
    }

    public override void tickLiving()
    {
        if (--_randomTickDivider <= 0)
        {
            if (world is BetaSharp.Worlds.Core.World runtimeWorld)
            {
                int blockX = MathHelper.Floor(x);
                int blockY = MathHelper.Floor(y);
                int blockZ = MathHelper.Floor(z);
                runtimeWorld.Villages.AddVillagerPosition(blockX, blockY, blockZ);
                Village = runtimeWorld.Villages.FindNearestVillage(blockX, blockY, blockZ, 32);
            }

            _randomTickDivider = 70 + random.NextInt(50);
        }

        base.tickLiving();
    }

    protected override bool canDespawn() => false;

    public override bool interact(EntityPlayer player)
    {
        if (world.IsRemote)
        {
            return true;
        }

        if (player is ServerPlayerEntity serverPlayer)
        {
            SetCustomer(serverPlayer);
            serverPlayer.openMerchantScreen(this, "Villager");
            return true;
        }

        return false;
    }

    public override bool damage(Entity entity, int amount)
    {
        bool damaged = base.damage(entity, amount);
        if (damaged && Village != null && entity is EntityLiving aggressor)
        {
            Village.AddOrRenewAggressor(aggressor);
        }

        return damaged;
    }

    public override string getTexture()
    {
        return Profession.Value switch
        {
            0 => "/mob/villager/farmer.png",
            1 => "/mob/villager/librarian.png",
            2 => "/mob/villager/priest.png",
            3 => "/mob/villager/smith.png",
            4 => "/mob/villager/butcher.png",
            _ => "/mob/villager/villager.png",
        };
    }

    protected override string getLivingSound() => "mob.villager.default";
    protected override string getHurtSound() => "mob.villager.defaulthurt";
    protected override string getDeathSound() => "mob.villager.defaultdeath";

    public EntityPlayer? GetCustomer()
    {
        return _customer;
    }

    public void SetCustomer(EntityPlayer? player)
    {
        _customer = player;
    }

    public MerchantRecipeList GetRecipes(EntityPlayer player)
    {
        _buyingList ??= GenerateTradeList();
        return _buyingList;
    }

    public void SetRecipes(MerchantRecipeList recipes)
    {
        _buyingList = recipes;
    }

    public void UseRecipe(MerchantRecipe recipe)
    {
        recipe.IncrementToolUses();
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetInteger("Profession", Profession.Value);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        Profession.Value = nbt.GetInteger("Profession");
    }

    private MerchantRecipeList GenerateTradeList()
    {
        MerchantRecipeList trades = new();

        switch (Profession.Value)
        {
            case 0:
                AddBuyTrade(trades, new ItemStack(Item.Wheat, 18), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Block.Wool, 14, 0), new ItemStack(Item.Emerald, 1));
                AddSellTrade(trades, new ItemStack(Item.Bread, 3), 1);
                AddSellTrade(trades, new ItemStack(Item.Apple, 4), 1);
                AddSellTrade(trades, new ItemStack(Item.Cookie, 8), 1);
                AddSellTrade(trades, new ItemStack(Item.Carrot, 3), 1);
                AddSellTrade(trades, new ItemStack(Item.Potato, 4), 1);
                AddSellTrade(trades, new ItemStack(Item.GoldenCarrot, 1), 3);
                break;
            case 1:
                AddBuyTrade(trades, new ItemStack(Item.Paper, 24), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.Book, 9), new ItemStack(Item.Emerald, 1));
                AddSellTrade(trades, new ItemStack(Block.Bookshelf, 1), 3);
                AddSellTrade(trades, new ItemStack(Block.Glass, 4), 1);
                AddSellTrade(trades, new ItemStack(Item.Compass, 1), 10);
                AddSellTrade(trades, new ItemStack(Item.Clock, 1), 10);
                break;
            case 2:
                AddSellTrade(trades, new ItemStack(Item.Redstone, 4), 1);
                AddSellTrade(trades, new ItemStack(Block.Glowstone, 1), 2);
                AddSellTrade(trades, new ItemStack(Item.ExpBottle, 1), 3);
                AddSellTrade(trades, new ItemStack(Item.EyeOfEnder, 1), 7);
                break;
            case 3:
                AddBuyTrade(trades, new ItemStack(Item.Coal, 16, 0), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.IronIngot, 8), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.GoldIngot, 8), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.Diamond, 4), new ItemStack(Item.Emerald, 1));
                AddSellTrade(trades, new ItemStack(Item.IronSword, 1), 7);
                AddSellTrade(trades, new ItemStack(Item.IronPickaxe, 1), 8);
                AddSellTrade(trades, new ItemStack(Item.DiamondSword, 1), 12);
                AddSellTrade(trades, new ItemStack(Item.DiamondPickaxe, 1), 14);
                break;
            case 4:
                AddBuyTrade(trades, new ItemStack(Item.Coal, 16, 0), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.RawPorkchop, 14), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.RawBeef, 14), new ItemStack(Item.Emerald, 1));
                AddBuyTrade(trades, new ItemStack(Item.RawChicken, 14), new ItemStack(Item.Emerald, 1));
                AddSellTrade(trades, new ItemStack(Item.Saddle, 1), 6);
                AddSellTrade(trades, new ItemStack(Item.LeatherChestplate, 1), 7);
                AddSellTrade(trades, new ItemStack(Item.CookedPorkchop, 5), 1);
                AddSellTrade(trades, new ItemStack(Item.CookedBeef, 5), 1);
                AddSellTrade(trades, new ItemStack(Item.CookedChicken, 5), 1);
                break;
            default:
                AddBuyTrade(trades, new ItemStack(Item.Wheat, 18), new ItemStack(Item.Emerald, 1));
                AddSellTrade(trades, new ItemStack(Item.Bread, 3), 1);
                break;
        }

        return trades;
    }

    private static void AddBuyTrade(MerchantRecipeList trades, ItemStack payment, ItemStack output)
    {
        trades.Add(new MerchantRecipe(payment, output));
    }

    private static void AddSellTrade(MerchantRecipeList trades, ItemStack soldItem, int emeraldCost)
    {
        trades.Add(new MerchantRecipe(new ItemStack(Item.Emerald, emeraldCost), soldItem));
    }
}
