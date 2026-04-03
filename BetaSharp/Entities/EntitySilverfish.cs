using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntitySilverfish : EntityMonster
{
    private static readonly int[] s_offsetX = [0, 0, 0, 0, -1, 1];
    private static readonly int[] s_offsetY = [-1, 1, 0, 0, 0, 0];
    private static readonly int[] s_offsetZ = [0, 0, -1, 1, 0, 0];
    private int _summonSilverfishDelay;

    public override EntityType Type => EntityRegistry.Silverfish;

    public EntitySilverfish(IWorldContext world) : base(world)
    {
        texture = "/mob/silverfish.png";
        setBoundingBoxSpacing(0.3F, 0.7F);
        movementSpeed = 0.6F;
        attackStrength = 1;
        health = 8;
    }

    protected override Entity? findPlayerToAttack()
    {
        EntityPlayer? player = world.Entities.GetClosestPlayerTarget(x, y, z, 8.0D);
        return player != null && canSee(player) ? player : null;
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (attackTime <= 0 && distance < 1.2F && entity.boundingBox.MaxY > boundingBox.MinY && entity.boundingBox.MinY < boundingBox.MaxY)
        {
            attackTime = 20;
            entity.damage(this, attackStrength);
        }
    }

    protected override float getBlockPathWeight(int x, int y, int z)
    {
        int belowId = world.Reader.GetBlockId(x, y - 1, z);
        int belowMeta = world.Reader.GetBlockMeta(x, y - 1, z);
        return BlockSilverfish.CanContainSilverfish(belowId, belowMeta) ? 10.0F : base.getBlockPathWeight(x, y, z);
    }

    public override bool canSpawn()
    {
        return base.canSpawn() && world.Entities.GetClosestPlayer(x, y, z, 5.0D) == null;
    }

    public override bool damage(Entity entity, int amount)
    {
        if (!base.damage(entity, amount))
        {
            return false;
        }

        if (!world.IsRemote && amount > 0)
        {
            _summonSilverfishDelay = 20;
        }

        return true;
    }

    public override void tickLiving()
    {
        base.tickLiving();
        if (world.IsRemote)
        {
            return;
        }

        bool releasedNearbySilverfish = false;
        if (_summonSilverfishDelay > 0 && --_summonSilverfishDelay == 0)
        {
            SummonSilverfishFromNearbyBlocks();
            releasedNearbySilverfish = true;
        }

        if (!releasedNearbySilverfish && playerToAttack == null && random.NextInt(10) == 0)
        {
            TryHideInNearbyBlock();
        }
    }

    private void TryHideInNearbyBlock()
    {
        int baseX = MathHelper.Floor(x);
        int baseY = MathHelper.Floor(y + 0.5D);
        int baseZ = MathHelper.Floor(z);
        int startOffset = random.NextInt(s_offsetX.Length);

        for (int i = 0; i < s_offsetX.Length; ++i)
        {
            int offsetIndex = (startOffset + i) % s_offsetX.Length;
            int targetX = baseX + s_offsetX[offsetIndex];
            int targetY = baseY + s_offsetY[offsetIndex];
            int targetZ = baseZ + s_offsetZ[offsetIndex];
            int blockId = world.Reader.GetBlockId(targetX, targetY, targetZ);
            int blockMeta = world.Reader.GetBlockMeta(targetX, targetY, targetZ);
            if (!BlockSilverfish.CanContainSilverfish(blockId, blockMeta))
            {
                continue;
            }

            int monsterEggMeta = BlockSilverfish.GetMonsterEggMetaForModel(blockId, blockMeta);
            world.Writer.SetBlock(targetX, targetY, targetZ, Block.Silverfish.id, monsterEggMeta, true);
            world.Broadcaster.AddParticle("explode", x, y + height * 0.5D, z, 0.0D, 0.0D, 0.0D);
            markDead();
            return;
        }
    }

    private void SummonSilverfishFromNearbyBlocks()
    {
        int originX = MathHelper.Floor(x);
        int originY = MathHelper.Floor(y);
        int originZ = MathHelper.Floor(z);

        for (int dy = 0; dy <= 5; dy = dy <= 0 ? 1 - dy : -dy)
        {
            for (int dx = 0; dx <= 10; dx = dx <= 0 ? 1 - dx : -dx)
            {
                for (int dz = 0; dz <= 10; dz = dz <= 0 ? 1 - dz : -dz)
                {
                    int blockX = originX + dx;
                    int blockY = originY + dy;
                    int blockZ = originZ + dz;
                    if (world.Reader.GetBlockId(blockX, blockY, blockZ) != Block.Silverfish.id)
                    {
                        continue;
                    }

                    int blockMeta = world.Reader.GetBlockMeta(blockX, blockY, blockZ);
                    world.Writer.SetBlock(blockX, blockY, blockZ, BlockSilverfish.GetModelBlockId(blockMeta), BlockSilverfish.GetModelBlockMeta(blockMeta), true);

                    EntitySilverfish silverfish = new(world);
                    silverfish.setPositionAndAngles(blockX + 0.5D, blockY, blockZ + 0.5D, random.NextFloat() * 360.0F, 0.0F);
                    world.SpawnEntity(silverfish);

                    if (random.NextBoolean())
                    {
                        return;
                    }
                }
            }
        }
    }
}
