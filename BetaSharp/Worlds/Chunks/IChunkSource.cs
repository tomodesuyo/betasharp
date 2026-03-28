using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Chunks;

public interface IChunkSource
{
    bool IsChunkLoaded(int x, int z);

    Chunk GetChunk(int x, int z);

    Chunk LoadChunk(int x, int z);

    void DecorateTerrain(IChunkSource source, int x, int z);

    bool Save(bool saveEntities, LoadingDisplay display);

    bool Tick();

    bool CanSave();

    string GetDebugInfo();

    Vec3i? FindNearestStructure(string structureId, int x, int y, int z) => null;

    IChunkSource CreateParallelInstance() => this;
}
