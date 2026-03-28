using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public abstract class StructureStart
{
    protected readonly List<StructureComponent> Components = [];
    protected StructureBoundingBox BoundingBox = null!;

    public StructureBoundingBox GetBoundingBox() => BoundingBox;
    public List<StructureComponent> GetComponents() => Components;

    public void GenerateStructure(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        for (int i = 0; i < Components.Count; ++i)
        {
            StructureComponent component = Components[i];
            if (component.GetBoundingBox().Intersects(bounds) && !component.AddComponentParts(world, random, bounds))
            {
                Components.RemoveAt(i--);
            }
        }
    }

    protected void UpdateBoundingBox()
    {
        BoundingBox = StructureBoundingBox.CreateUnknownBox();
        for (int i = 0; i < Components.Count; ++i)
        {
            BoundingBox.ExpandTo(Components[i].GetBoundingBox());
        }
    }

    protected void MarkAvailableHeight(IWorldContext world, JavaRandom random, int padding)
    {
        int maxY = 63 - padding;
        int sizeY = BoundingBox.GetYSize() + 1;
        if (sizeY < maxY)
        {
            sizeY += random.NextInt(maxY - sizeY);
        }

        int offsetY = sizeY - BoundingBox.MaxY;
        BoundingBox.Offset(0, offsetY, 0);
        for (int i = 0; i < Components.Count; ++i)
        {
            Components[i].GetBoundingBox().Offset(0, offsetY, 0);
        }
    }

    public virtual bool IsSizeableStructure() => true;
}
