using BetaSharp.Blocks.Entities;
using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockBrewingStand : BlockWithEntity
{
    private static readonly ThreadLocal<JavaRandom> s_random = new(() => new JavaRandom());

    public BlockBrewingStand(int id) : base(id, 157, Material.Metal)
    {
        setBoundingBox(0.125F, 0.0F, 0.125F, 0.875F, 0.875F, 0.875F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.BrewingStand;

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        setBoundingBox(7.0F / 16.0F, 0.0F, 7.0F / 16.0F, 9.0F / 16.0F, 14.0F / 16.0F, 9.0F / 16.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 2.0F / 16.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setupRenderBoundingBox();
    }

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 2.0F / 16.0F, 1.0F);

    public override bool onUse(OnUseEvent @event)
    {
        if (@event.World.IsRemote)
        {
            return true;
        }

        BlockEntityBrewingStand? brewingStand = @event.World.Entities.GetBlockEntity<BlockEntityBrewingStand>(@event.X, @event.Y, @event.Z);
        if (brewingStand != null)
        {
            @event.Player.openBrewingStandScreen(brewingStand);
        }

        return true;
    }

    public override void randomDisplayTick(OnTickEvent @event)
    {
        double particleX = @event.X + 0.4F + Random.Shared.NextSingle() * 0.2F;
        double particleY = @event.Y + 0.7F + Random.Shared.NextSingle() * 0.3F;
        double particleZ = @event.Z + 0.4F + Random.Shared.NextSingle() * 0.2F;
        @event.World.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
    }

    public override int getDroppedItemId(int blockMeta) => Items.Item.BrewingStand.id;

    public override BlockEntity getBlockEntity() => new BlockEntityBrewingStand();

    public override void onBreak(OnBreakEvent @event)
    {
        BlockEntityBrewingStand? brewingStand = @event.World.Entities.GetBlockEntity<BlockEntityBrewingStand>(@event.X, @event.Y, @event.Z);
        if (brewingStand != null)
        {
            JavaRandom random = s_random.Value!;

            for (int slotIndex = 0; slotIndex < brewingStand.size(); ++slotIndex)
            {
                ItemStack? stack = brewingStand.getStack(slotIndex);
                if (stack == null)
                {
                    continue;
                }

                float offsetX = random.NextFloat() * 0.8F + 0.1F;
                float offsetY = random.NextFloat() * 0.8F + 0.1F;
                float offsetZ = random.NextFloat() * 0.8F + 0.1F;

                while (stack.count > 0)
                {
                    int amount = random.NextInt(21) + 10;
                    if (amount > stack.count)
                    {
                        amount = stack.count;
                    }

                    stack.count -= amount;
                    EntityItem itemEntity = new(@event.World, @event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ, new Items.ItemStack(stack.itemId, amount, stack.getDamage()));
                    float velocityScale = 0.05F;
                    itemEntity.velocityX = random.NextGaussian() * velocityScale;
                    itemEntity.velocityY = random.NextGaussian() * velocityScale + 0.2F;
                    itemEntity.velocityZ = random.NextGaussian() * velocityScale;
                    @event.World.Entities.SpawnEntity(itemEntity);
                }
            }
        }

        base.onBreak(@event);
    }
}
