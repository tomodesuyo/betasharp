using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockStairs : Block
{
    private readonly Block _baseBlock;

    public BlockStairs(int id, Block block) : base(id, block.textureId, block.material)
    {
        _baseBlock = block;
        setHardness(block.hardness);
        setResistance(block.resistance / 3.0F);
        setSoundGroup(block.soundGroup);
        setOpacity(255);
    }

    public override void updateBoundingBox(IBlockReader blockReader, EntityManager? entities, int x, int y, int z) => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Stairs;

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        int meta = world.GetBlockMeta(x, y, z);
        int direction = meta & 3;
        float minY = 0.0F;
        float maxY = 0.5F;
        float stepMinY = 0.5F;
        float stepMaxY = 1.0F;

        if ((meta & 4) != 0)
        {
            minY = 0.5F;
            maxY = 1.0F;
            stepMinY = 0.0F;
            stepMaxY = 0.5F;
        }

        setBoundingBox(0.0F, minY, 0.0F, 1.0F, maxY, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);

        if (direction == 0)
        {
            setBoundingBox(0.5F, stepMinY, 0.0F, 1.0F, stepMaxY, 1.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }
        else if (direction == 1)
        {
            setBoundingBox(0.0F, stepMinY, 0.0F, 0.5F, stepMaxY, 1.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }
        else if (direction == 2)
        {
            setBoundingBox(0.0F, stepMinY, 0.5F, 1.0F, stepMaxY, 1.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }
        else if (direction == 3)
        {
            setBoundingBox(0.0F, stepMinY, 0.0F, 1.0F, stepMaxY, 0.5F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }

        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);
    }

    public override void randomDisplayTick(OnTickEvent @event) => _baseBlock.randomDisplayTick(@event);

    public override void onBlockBreakStart(OnBlockBreakStartEvent @event) => _baseBlock.onBlockBreakStart(@event);

    public override void onMetadataChange(OnMetadataChangeEvent @event) => _baseBlock.onMetadataChange(@event);

    public override float getLuminance(ILightProvider lighting, int x, int y, int z) => _baseBlock.getLuminance(lighting, x, y, z);

    public override float getBlastResistance(Entity entity) => _baseBlock.getBlastResistance(entity);

    public override int getRenderLayer() => _baseBlock.getRenderLayer();

    public override int getDroppedItemId(int blockMeta) => _baseBlock.getDroppedItemId(blockMeta);

    public override int getDroppedItemCount() => _baseBlock.getDroppedItemCount();

    public override int getTexture(int side, int meta) => _baseBlock.getTexture(side, meta);

    public override int getTexture(int side) => _baseBlock.getTexture(side);

    public override int getTextureId(IBlockReader iBlockReader, int x, int y, int z, int side) => _baseBlock.getTextureId(iBlockReader, x, y, z, side);

    public override int getTickRate() => _baseBlock.getTickRate();

    public override Box getBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z) => _baseBlock.getBoundingBox(world, entities, x, y, z);

    public override Vec3D applyVelocity(OnApplyVelocityEvent ctx) => _baseBlock.applyVelocity(ctx);

    public override bool hasCollision() => _baseBlock.hasCollision();

    public override bool hasCollision(int meta, bool allowLiquids) => _baseBlock.hasCollision(meta, allowLiquids);

    public override bool canPlaceAt(CanPlaceAtContext evt) => _baseBlock.canPlaceAt(evt);

    public override void onPlaced(OnPlacedEvent evt)
    {
        int meta = evt.World.Reader.GetBlockMeta(evt.X, evt.Y, evt.Z) & 4;
        if (evt.Direction == 0)
        {
            meta |= 4;
        }

        if (evt.Placer != null)
        {
            int facing = MathHelper.Floor(evt.Placer.yaw * 4.0F / 360.0F + 0.5D) & 3;

            if (facing == 0)
            {
                meta = 2;
            }

            if (facing == 1)
            {
                meta = 1;
            }

            if (facing == 2)
            {
                meta = 3;
            }

            if (facing == 3)
            {
                meta = 0;
            }
        }

        evt.World.Writer.SetBlockMeta(evt.X, evt.Y, evt.Z, meta);
        evt.World.Broadcaster.NotifyNeighbors(evt.X, evt.Y, evt.Z, id);
        _baseBlock.onPlaced(evt);
    }

    public override void onBreak(OnBreakEvent ctx) => _baseBlock.onBreak(ctx);

    public override void dropStacks(OnDropEvent ctx) => _baseBlock.dropStacks(ctx);

    public override void onSteppedOn(OnEntityStepEvent ctx) => _baseBlock.onSteppedOn(ctx);

    public override void onTick(OnTickEvent ctx) => _baseBlock.onTick(ctx);

    public override bool onUse(OnUseEvent ctx) => _baseBlock.onUse(ctx);

    public override void onDestroyedByExplosion(OnDestroyedByExplosionEvent ctx) => _baseBlock.onDestroyedByExplosion(ctx);
}
