using BetaSharp.Blocks;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Gen.Chunks;
using BetaSharp.Worlds.Generation.Biomes;
using BetaSharp.Worlds.Generation.Biomes.Source;
using Silk.NET.Maths;

namespace BetaSharp.Worlds.Dimensions;

internal sealed class EndDimension : Dimension
{
    public override void InitBiomeSource()
    {
        Id = 1;
        HasCeiling = true;
        BiomeSource = new FixedBiomeSource(Biome.End, 0.5D, 0.0D);
    }

    public override bool HasWorldSpawn => false;

    public override IChunkSource CreateChunkGenerator() => new EndChunkGenerator(World, World.Seed);

    public override float GetTimeOfDay(long time, float tickDelta) => 0.0F;

    public override Vector3D<double> GetFogColor(float celestialAngle, float partialTicks)
    {
        const int fogColor = 8421536;
        return new Vector3D<double>(
            ((fogColor >> 16) & 255) / 255.0D * 0.15D,
            ((fogColor >> 8) & 255) / 255.0D * 0.15D,
            (fogColor & 255) / 255.0D * 0.15D);
    }

    public override bool IsValidSpawnPoint(int x, int z)
    {
        int blockId = World.GetSpawnBlockId(x, z);
        return blockId != 0 && Block.Blocks[blockId] != null && Block.Blocks[blockId].material.BlocksMovement;
    }

    public override float CloudHeight => 8.0F;
}
