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
        BiomeSource = new FixedBiomeSource(Biome.Sky, 0.5D, 0.0D);
    }

    public override bool HasWorldSpawn => false;

    public override IChunkSource CreateChunkGenerator() => new EndChunkGenerator(World, World.Seed);

    public override float GetTimeOfDay(long time, float tickDelta) => 0.0F;

    public override Vector3D<double> GetFogColor(float celestialAngle, float partialTicks) => new(0.09D, 0.0D, 0.09D);

    public override float CloudHeight => 8.0F;
}
