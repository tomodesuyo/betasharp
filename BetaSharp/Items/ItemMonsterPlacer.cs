using BetaSharp.Entities;
using BetaSharp.Util.Hit;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemMonsterPlacer : Item
{
    private readonly Dictionary<int, EggInfo> _eggs = new()
    {
        [50] = new("Creeper", 0x0DA70B, 0x000000),
        [51] = new("Skeleton", 0xC1C1C1, 0x494949),
        [52] = new("Spider", 0x342D27, 0xA80E0E),
        [53] = new("Giant", 0x799C65, 0x799C65),
        [54] = new("Zombie", 0x00AF00, 0x799C65),
        [55] = new("Slime", 0x51A03E, 0x7EBF6E),
        [56] = new("Ghast", 0xF9F9F9, 0xBCBCBC),
        [57] = new("PigZombie", 0xEA9393, 0x4C7129),
        [58] = new("Enderman", 0x161616, 0x000000),
        [59] = new("CaveSpider", 0x0C424E, 0xA80E0E),
        [60] = new("Silverfish", 0x6E6E6E, 0x303030),
        [90] = new("Pig", 0xF0A5A2, 0xDB635F),
        [91] = new("Sheep", 0xE7E7E7, 0xFFB5B5),
        [92] = new("Cow", 0x443626, 0xA1A1A1),
        [93] = new("Chicken", 0xA1A1A1, 0xFF0000),
        [94] = new("Squid", 0x223B4D, 0x708899),
        [95] = new("Wolf", 0xD7D3D3, 0xCEAF96),
        [96] = new("MushroomCow", 0xA00F10, 0xB7B7B7),
        [120] = new("Villager", 0x563C33, 0xBC9862)
    };

    public ItemMonsterPlacer(int id) : base(id)
    {
        setHasSubtypes(true);
        setTexturePosition(12, 0);
        setItemName("monsterPlacer");
    }

    public IEnumerable<int> GetSupportedEntityIds()
    {
        return _eggs.Keys;
    }

    public override bool requiresMultipleRenderPasses() => true;

    public override int getColorMultiplier(int entityId)
    {
        return _eggs.TryGetValue(entityId, out EggInfo? eggInfo) ? eggInfo.PrimaryColor : 0xFFFFFF;
    }

    public override int getColorMultiplier(int entityId, int pass)
    {
        return _eggs.TryGetValue(entityId, out EggInfo? eggInfo)
            ? (pass == 0 ? eggInfo.PrimaryColor : eggInfo.SecondaryColor)
            : 0xFFFFFF;
    }

    public override int getTextureId(int damage, int pass) => pass > 0 ? base.getTextureId(damage) + 16 : base.getTextureId(damage);

    public override string getItemNameIS(ItemStack itemStack)
    {
        return base.getItemNameIS(itemStack);
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        if (!TryGetPlacementPosition(world, x, y, z, side, out double spawnX, out double spawnY, out double spawnZ))
        {
            return false;
        }

        return TrySpawn(itemStack, entityPlayer, world, spawnX, spawnY, spawnZ);
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        HitResult hitResult = entityPlayer.rayTrace(5.0D, 1.0F);
        if (hitResult.Type != HitResultType.TILE)
        {
            return itemStack;
        }

        if (!world.CanInteract(entityPlayer, hitResult.BlockX, hitResult.BlockY, hitResult.BlockZ))
        {
            return itemStack;
        }

        if (!world.Reader.GetMaterial(hitResult.BlockX, hitResult.BlockY, hitResult.BlockZ).IsFluid)
        {
            return itemStack;
        }

        TrySpawn(itemStack, entityPlayer, world, hitResult.BlockX + 0.5D, hitResult.BlockY + 0.5D, hitResult.BlockZ + 0.5D);
        return itemStack;
    }

    private bool TrySpawn(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, double x, double y, double z)
    {
        int entityId = itemStack.getDamage();
        if (!_eggs.ContainsKey(entityId) || !EntityRegistry.TryCreate(entityId, world, out Entity? entity))
        {
            return false;
        }

        if (entity is not EntityLiving living)
        {
            return false;
        }

        float yaw = world.Random.NextFloat() * 360.0F;
        living.setPositionAndAngles(x, y, z, yaw, 0.0F);
        living.bodyYaw = living.yaw;

        if (!world.SpawnEntity(living))
        {
            return false;
        }

        living.playLivingSound();

        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            --itemStack.count;
        }

        return true;
    }

    private static bool TryGetPlacementPosition(IWorldContext world, int x, int y, int z, int side, out double spawnX, out double spawnY, out double spawnZ)
    {
        switch (side)
        {
            case 0:
                --y;
                break;
            case 1:
                ++y;
                break;
            case 2:
                --z;
                break;
            case 3:
                ++z;
                break;
            case 4:
                --x;
                break;
            case 5:
                ++x;
                break;
        }

        if (!world.Reader.IsAir(x, y, z) && world.Reader.GetMaterial(x, y, z).IsSolid)
        {
            spawnX = spawnY = spawnZ = 0.0D;
            return false;
        }

        double yOffset = side == 1 && world.Reader.GetBlockId(x, y - 1, z) != 0 ? 0.5D : 0.0D;
        spawnX = x + 0.5D;
        spawnY = y + yOffset;
        spawnZ = z + 0.5D;
        return true;
    }

    private sealed record EggInfo(string EntityName, int PrimaryColor, int SecondaryColor);
}
