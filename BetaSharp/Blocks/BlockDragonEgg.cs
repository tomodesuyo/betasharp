using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockDragonEgg : Block
{
    public BlockDragonEgg(int id, int textureId) : base(id, textureId, Material.Stone)
    {
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.DragonEgg;

    public override void onPlaced(OnPlacedEvent @event)
    {
        @event.World.TickScheduler.ScheduleBlockUpdate(@event.X, @event.Y, @event.Z, id, getTickRate());
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        @event.World.TickScheduler.ScheduleBlockUpdate(@event.X, @event.Y, @event.Z, id, getTickRate());
    }

    public override void onTick(OnTickEvent @event)
    {
        FallIfPossible(@event.World, @event.X, @event.Y, @event.Z);
    }

    public override bool onUse(OnUseEvent @event)
    {
        TeleportNearby(@event.World, @event.X, @event.Y, @event.Z);
        return true;
    }

    public override void onBlockBreakStart(OnBlockBreakStartEvent @event)
    {
        TeleportNearby(@event.World, @event.X, @event.Y, @event.Z);
    }

    public override int getTickRate() => 3;

    private void FallIfPossible(IWorldContext world, int x, int y, int z)
    {
        if (y <= 0 || !BlockSand.canFallThrough(new OnTickEvent(world, x, y - 1, z, 0, id)))
        {
            return;
        }

        const sbyte checkRadius = 32;
        if (!BlockSand.fallInstantly && world.ChunkHost.IsRegionLoaded(x - checkRadius, y - checkRadius, z - checkRadius, x + checkRadius, y + checkRadius, z + checkRadius))
        {
            EntityFallingSand fallingEgg = new(world, x + 0.5F, y + 0.5F, z + 0.5F, id);
            world.Entities.SpawnEntity(fallingEgg);
            return;
        }

        world.Writer.SetBlock(x, y, z, 0);
        while (y > 0 && BlockSand.canFallThrough(new OnTickEvent(world, x, y - 1, z, 0, id)))
        {
            --y;
        }

        if (y > 0)
        {
            world.Writer.SetBlock(x, y, z, id);
        }
    }

    private void TeleportNearby(IWorldContext world, int x, int y, int z)
    {
        if (world.Reader.GetBlockId(x, y, z) != id)
        {
            return;
        }

        if (!world.IsRemote)
        {
            int meta = world.Reader.GetBlockMeta(x, y, z);
            for (int attempt = 0; attempt < 1000; ++attempt)
            {
                int targetX = x + world.Random.NextInt(16) - world.Random.NextInt(16);
                int targetY = y + world.Random.NextInt(8) - world.Random.NextInt(8);
                int targetZ = z + world.Random.NextInt(16) - world.Random.NextInt(16);
                if (!world.Reader.IsAir(targetX, targetY, targetZ))
                {
                    continue;
                }

                world.Writer.SetBlock(targetX, targetY, targetZ, id, meta);
                world.Writer.SetBlock(x, y, z, 0);

                const int particleCount = 128;
                for (int particle = 0; particle < particleCount; ++particle)
                {
                    double progress = world.Random.NextDouble();
                    float velocityX = (world.Random.NextFloat() - 0.5F) * 0.2F;
                    float velocityY = (world.Random.NextFloat() - 0.5F) * 0.2F;
                    float velocityZ = (world.Random.NextFloat() - 0.5F) * 0.2F;
                    double particleX = targetX + (x - targetX) * progress + (world.Random.NextDouble() - 0.5D) + 0.5D;
                    double particleY = targetY + (y - targetY) * progress + world.Random.NextDouble() - 0.5D;
                    double particleZ = targetZ + (z - targetZ) * progress + (world.Random.NextDouble() - 0.5D) + 0.5D;
                    world.Broadcaster.AddParticle("portal", particleX, particleY, particleZ, velocityX, velocityY, velocityZ);
                }

                return;
            }

            return;
        }

        const int clientParticleCount = 128;
        for (int particle = 0; particle < clientParticleCount; ++particle)
        {
            double particleX = x + 0.5D + (world.Random.NextDouble() - 0.5D);
            double particleY = y + world.Random.NextDouble();
            double particleZ = z + 0.5D + (world.Random.NextDouble() - 0.5D);
            double velocityX = (world.Random.NextFloat() - 0.5F) * 0.2F;
            double velocityY = (world.Random.NextFloat() - 0.5F) * 0.2F;
            double velocityZ = (world.Random.NextFloat() - 0.5F) * 0.2F;
            world.Broadcaster.AddParticle("portal", particleX, particleY, particleZ, velocityX, velocityY, velocityZ);
        }
    }
}
