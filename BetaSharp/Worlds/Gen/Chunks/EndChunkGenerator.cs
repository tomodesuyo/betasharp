using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Biomes.Source;

namespace BetaSharp.Worlds.Gen.Chunks;

internal sealed class EndChunkGenerator : IChunkSource
{
    private readonly SkyChunkGenerator _delegate;

    public EndChunkGenerator(IWorldContext world, long seed)
    {
        _delegate = new SkyChunkGenerator(world, seed);
    }

    public bool IsChunkLoaded(int x, int z) => false;

    public Chunk LoadChunk(int x, int z) => GetChunk(x, z);

    public Chunk GetChunk(int x, int z)
    {
        Chunk chunk = _delegate.GetChunk(x, z);
        byte[] blocks = chunk.Blocks;
        for (int i = 0; i < blocks.Length; ++i)
        {
            byte id = blocks[i];
            if (id == Block.Stone.id || id == Block.Dirt.id || id == Block.GrassBlock.id)
            {
                blocks[i] = (byte)Block.EndStone.id;
            }
        }

        chunk.PopulateHeightMap();
        return chunk;
    }

    public void DecorateTerrain(IChunkSource source, int x, int z)
    {
    }

    public bool Save(bool saveEntities, LoadingDisplay display) => true;

    public bool Tick() => false;

    public bool CanSave() => true;

    public string GetDebugInfo() => "EndSource";

    public IChunkSource CreateParallelInstance() => this;
}
