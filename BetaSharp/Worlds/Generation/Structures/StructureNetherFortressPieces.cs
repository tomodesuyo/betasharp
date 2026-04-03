using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal static class StructureNetherFortressPieces
{
    private const int South = 0;
    private const int West = 1;
    private const int North = 2;
    private const int East = 3;

    private static readonly PieceWeight[] s_primaryComponents =
    [
        new(typeof(Straight), 30, 0, true),
        new(typeof(Crossing3), 10, 4),
        new(typeof(Crossing), 10, 4),
        new(typeof(Stairs), 10, 3),
        new(typeof(Throne), 5, 2),
        new(typeof(Entrance), 5, 1)
    ];

    private static readonly PieceWeight[] s_secondaryComponents =
    [
        new(typeof(Corridor5), 25, 0, true),
        new(typeof(Crossing2), 15, 5),
        new(typeof(Corridor2), 5, 10),
        new(typeof(Corridor), 5, 10),
        new(typeof(Corridor3), 10, 3, true),
        new(typeof(Corridor4), 7, 2),
        new(typeof(NetherStalkRoom), 5, 2)
    ];

    private static readonly StructurePieceTreasure[] s_loot =
    [
        new(Item.Diamond.id, 0, 1, 3, 5),
        new(Item.IronIngot.id, 0, 1, 5, 5),
        new(Item.GoldIngot.id, 0, 1, 3, 15),
        new(Item.GoldenSword.id, 0, 1, 1, 5),
        new(Item.GoldenChestplate.id, 0, 1, 1, 5),
        new(Item.FlintAndSteel.id, 0, 1, 1, 5),
        new(Item.NetherWart.id, 0, 3, 7, 5),
        new(Item.Saddle.id, 0, 1, 1, 10),
        new(Item.IronHorseArmor.id, 0, 1, 1, 5),
        new(Item.GoldenHorseArmor.id, 0, 1, 1, 8),
        new(Item.DiamondHorseArmor.id, 0, 1, 1, 3),
        new(Block.Obsidian.id, 0, 2, 4, 2)
    ];

    public static StructurePieceTreasure[] GetTreasurePieces() => s_loot;

    private static Piece? CreatePiece(PieceWeight weight, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        Type pieceType = weight.PieceType;
        if (pieceType == typeof(Straight))
        {
            return Straight.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Crossing3))
        {
            return Crossing3.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Crossing))
        {
            return Crossing.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Stairs))
        {
            return Stairs.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Throne))
        {
            return Throne.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Entrance))
        {
            return Entrance.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Corridor5))
        {
            return Corridor5.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Corridor2))
        {
            return Corridor2.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Corridor))
        {
            return Corridor.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Corridor3))
        {
            return Corridor3.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Corridor4))
        {
            return Corridor4.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(Crossing2))
        {
            return Crossing2.Create(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(NetherStalkRoom))
        {
            return NetherStalkRoom.Create(components, random, x, y, z, facing, componentType);
        }

        return null;
    }

    internal sealed class PieceWeight
    {
        public PieceWeight(Type pieceType, int weight, int maxPlaceCount, bool allowConsecutive = false)
        {
            PieceType = pieceType;
            Weight = weight;
            MaxPlaceCount = maxPlaceCount;
            AllowConsecutive = allowConsecutive;
        }

        public Type PieceType { get; }
        public int Weight { get; }
        public int PlaceCount { get; set; }
        public int MaxPlaceCount { get; }
        public bool AllowConsecutive { get; }

        public bool CanSpawn(int depth)
        {
            return MaxPlaceCount == 0 || PlaceCount < MaxPlaceCount;
        }

        public bool CanSpawnMore()
        {
            return MaxPlaceCount == 0 || PlaceCount < MaxPlaceCount;
        }
    }

    internal abstract class Piece : StructureComponent
    {
        protected Piece(int componentType) : base(componentType)
        {
        }

        private int GetTotalWeight(List<PieceWeight> weights)
        {
            bool hasAvailablePieces = false;
            int totalWeight = 0;

            for (int i = 0; i < weights.Count; ++i)
            {
                PieceWeight weight = weights[i];
                if (weight.MaxPlaceCount > 0 && weight.PlaceCount < weight.MaxPlaceCount)
                {
                    hasAvailablePieces = true;
                }

                totalWeight += weight.Weight;
            }

            return hasAvailablePieces ? totalWeight : -1;
        }

        private Piece? GetNextPiece(Start start, List<PieceWeight> weights, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int depth)
        {
            int totalWeight = GetTotalWeight(weights);
            bool canSelectPiece = totalWeight > 0 && depth <= 30;

            for (int attempt = 0; attempt < 5 && canSelectPiece; ++attempt)
            {
                int remaining = random.NextInt(totalWeight);
                for (int i = 0; i < weights.Count; ++i)
                {
                    PieceWeight weight = weights[i];
                    remaining -= weight.Weight;
                    if (remaining >= 0)
                    {
                        continue;
                    }

                    if (!weight.CanSpawn(depth) || (weight == start.PreviousPieceWeight && !weight.AllowConsecutive))
                    {
                        break;
                    }

                    Piece? piece = CreatePiece(weight, components, random, x, y, z, facing, depth);
                    if (piece != null)
                    {
                        ++weight.PlaceCount;
                        start.PreviousPieceWeight = weight;
                        if (!weight.CanSpawnMore())
                        {
                            weights.RemoveAt(i);
                        }

                        return piece;
                    }
                }
            }

            return End.Create(components, random, x, y, z, facing, depth);
        }

        private StructureComponent? GenerateAndAddPiece(Start start, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int depth, bool useSecondaryPieces)
        {
            if (Math.Abs(x - start.GetBoundingBox().MinX) > 112 || Math.Abs(z - start.GetBoundingBox().MinZ) > 112)
            {
                return End.Create(components, random, x, y, z, facing, depth);
            }

            List<PieceWeight> weights = useSecondaryPieces ? start.SecondaryWeights : start.PrimaryWeights;
            StructureComponent? component = GetNextPiece(start, weights, components, random, x, y, z, facing, depth + 1);
            if (component != null)
            {
                components.Add(component);
                start.PendingComponents.Add(component);
            }

            return component;
        }

        protected StructureComponent? GetNextComponentNormal(Start start, List<StructureComponent> components, JavaRandom random, int offsetX, int offsetY, bool useSecondaryPieces)
        {
            return Facing switch
            {
                North => GenerateAndAddPiece(start, components, random, BoundingBox.MinX + offsetX, BoundingBox.MinY + offsetY, BoundingBox.MinZ - 1, North, GetComponentType(), useSecondaryPieces),
                South => GenerateAndAddPiece(start, components, random, BoundingBox.MinX + offsetX, BoundingBox.MinY + offsetY, BoundingBox.MaxZ + 1, South, GetComponentType(), useSecondaryPieces),
                West => GenerateAndAddPiece(start, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + offsetY, BoundingBox.MinZ + offsetX, West, GetComponentType(), useSecondaryPieces),
                East => GenerateAndAddPiece(start, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + offsetY, BoundingBox.MinZ + offsetX, East, GetComponentType(), useSecondaryPieces),
                _ => null
            };
        }

        protected StructureComponent? GetNextComponentX(Start start, List<StructureComponent> components, JavaRandom random, int offsetY, int offsetZ, bool useSecondaryPieces)
        {
            return Facing switch
            {
                North or South => GenerateAndAddPiece(start, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + offsetY, BoundingBox.MinZ + offsetZ, West, GetComponentType(), useSecondaryPieces),
                West or East => GenerateAndAddPiece(start, components, random, BoundingBox.MinX + offsetZ, BoundingBox.MinY + offsetY, BoundingBox.MinZ - 1, North, GetComponentType(), useSecondaryPieces),
                _ => null
            };
        }

        protected StructureComponent? GetNextComponentZ(Start start, List<StructureComponent> components, JavaRandom random, int offsetY, int offsetZ, bool useSecondaryPieces)
        {
            return Facing switch
            {
                North or South => GenerateAndAddPiece(start, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + offsetY, BoundingBox.MinZ + offsetZ, East, GetComponentType(), useSecondaryPieces),
                West or East => GenerateAndAddPiece(start, components, random, BoundingBox.MinX + offsetZ, BoundingBox.MinY + offsetY, BoundingBox.MaxZ + 1, South, GetComponentType(), useSecondaryPieces),
                _ => null
            };
        }

        protected static bool IsAboveGround(StructureBoundingBox? box)
        {
            return box != null && box.MinY > 10;
        }
    }

    internal sealed class Corridor : Piece
    {
        private bool _hasChest;

        public Corridor(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
            _hasChest = random.NextInt(3) == 0;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentX((Start)component, components, random, 0, 1, true);
        }

        public static Corridor? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, 0, 0, 5, 7, 5, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Corridor(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 4, 1, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 4, 5, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 4, 2, 0, 4, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 3, 1, 4, 4, 1, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 3, 3, 4, 4, 3, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 0, 5, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 4, 3, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 3, 4, 1, 4, 4, Block.NetherFence.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 3, 3, 4, 3, 4, 4, Block.NetherFence.id, Block.NetherBrick.id, false);

            if (_hasChest)
            {
                int chestX = GetXWithOffset(3, 3);
                int chestY = GetYWithOffset(2);
                int chestZ = GetZWithOffset(3, 3);
                if (bounds.Contains(chestX, chestY, chestZ))
                {
                    _hasChest = false;
                    CreateTreasureChestAtCurrentPosition(world, bounds, random, 3, 2, 3, GetTreasurePieces(), 2 + random.NextInt(4));
                }
            }

            FillWithBlocks(world, bounds, 0, 6, 0, 4, 6, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            for (int x = 0; x <= 4; ++x)
            {
                for (int z = 0; z <= 4; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Corridor2 : Piece
    {
        private bool _hasChest;

        public Corridor2(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
            _hasChest = random.NextInt(3) == 0;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentZ((Start)component, components, random, 0, 1, true);
        }

        public static Corridor2? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, 0, 0, 5, 7, 5, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Corridor2(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 4, 1, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 4, 5, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 0, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 3, 1, 0, 4, 1, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 3, 3, 0, 4, 3, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 2, 0, 4, 5, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 2, 4, 4, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 3, 4, 1, 4, 4, Block.NetherFence.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 3, 3, 4, 3, 4, 4, Block.NetherFence.id, Block.NetherBrick.id, false);

            if (_hasChest)
            {
                int chestX = GetXWithOffset(1, 3);
                int chestY = GetYWithOffset(2);
                int chestZ = GetZWithOffset(1, 3);
                if (bounds.Contains(chestX, chestY, chestZ))
                {
                    _hasChest = false;
                    CreateTreasureChestAtCurrentPosition(world, bounds, random, 1, 2, 3, GetTreasurePieces(), 2 + random.NextInt(4));
                }
            }

            FillWithBlocks(world, bounds, 0, 6, 0, 4, 6, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            for (int x = 0; x <= 4; ++x)
            {
                for (int z = 0; z <= 4; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Corridor3 : Piece
    {
        public Corridor3(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 1, 0, true);
        }

        public static Corridor3? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, -7, 0, 5, 14, 10, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Corridor3(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            int stairMeta = GetMetadataWithOffset(Block.NetherBrickStairs.id, 2);

            for (int z = 0; z <= 9; ++z)
            {
                int floorHeight = Math.Max(1, 7 - z);
                int roofHeight = Math.Min(Math.Max(floorHeight + 5, 14 - z), 13);
                FillWithBlocks(world, bounds, 0, 0, z, 4, floorHeight, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                FillWithBlocks(world, bounds, 1, floorHeight + 1, z, 3, roofHeight - 1, z, 0, 0, false);

                if (z <= 6)
                {
                    PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairMeta, 1, floorHeight + 1, z, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairMeta, 2, floorHeight + 1, z, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairMeta, 3, floorHeight + 1, z, bounds);
                }

                FillWithBlocks(world, bounds, 0, roofHeight, z, 4, roofHeight, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                FillWithBlocks(world, bounds, 0, floorHeight + 1, z, 0, roofHeight - 1, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                FillWithBlocks(world, bounds, 4, floorHeight + 1, z, 4, roofHeight - 1, z, Block.NetherBrick.id, Block.NetherBrick.id, false);

                if ((z & 1) == 0)
                {
                    FillWithBlocks(world, bounds, 0, floorHeight + 2, z, 0, floorHeight + 3, z, Block.NetherFence.id, Block.NetherFence.id, false);
                    FillWithBlocks(world, bounds, 4, floorHeight + 2, z, 4, floorHeight + 3, z, Block.NetherFence.id, Block.NetherFence.id, false);
                }

                for (int x = 0; x <= 4; ++x)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Corridor4 : Piece
    {
        public Corridor4(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            int offset = Facing is West or North ? 5 : 1;
            GetNextComponentX((Start)component, components, random, 0, offset, random.NextInt(8) > 0);
            GetNextComponentZ((Start)component, components, random, 0, offset, random.NextInt(8) > 0);
        }

        public static Corridor4? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -3, 0, 0, 9, 7, 9, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Corridor4(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 8, 1, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 8, 5, 8, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 6, 0, 8, 6, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 2, 5, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 2, 0, 8, 5, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 3, 0, 1, 4, 0, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 7, 3, 0, 7, 4, 0, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 2, 4, 8, 2, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 1, 4, 2, 2, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 6, 1, 4, 7, 2, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 3, 8, 8, 3, 8, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 3, 6, 0, 3, 7, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 8, 3, 6, 8, 3, 7, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 3, 4, 0, 5, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 8, 3, 4, 8, 5, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 3, 5, 2, 5, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 3, 5, 7, 5, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 4, 5, 1, 5, 5, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 7, 4, 5, 7, 5, 5, Block.NetherFence.id, Block.NetherFence.id, false);

            for (int z = 0; z <= 5; ++z)
            {
                for (int x = 0; x <= 8; ++x)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Corridor5 : Piece
    {
        public Corridor5(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 1, 0, true);
        }

        public static Corridor5? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, 0, 0, 5, 7, 5, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Corridor5(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 4, 1, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 4, 5, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 0, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 2, 0, 4, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 3, 1, 0, 4, 1, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 3, 3, 0, 4, 3, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 3, 1, 4, 4, 1, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 3, 3, 4, 4, 3, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 6, 0, 4, 6, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 0; x <= 4; ++x)
            {
                for (int z = 0; z <= 4; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Crossing : Piece
    {
        public Crossing(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 2, 0, false);
            GetNextComponentX((Start)component, components, random, 0, 2, false);
            GetNextComponentZ((Start)component, components, random, 0, 2, false);
        }

        public static Crossing? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -2, 0, 0, 7, 9, 7, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Crossing(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 6, 1, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 6, 7, 6, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 1, 6, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 6, 1, 6, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 2, 0, 6, 6, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 2, 6, 6, 6, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 0, 6, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 5, 0, 6, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 2, 0, 6, 6, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 2, 5, 6, 6, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 6, 0, 4, 6, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 0, 4, 5, 0, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 2, 6, 6, 4, 6, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 6, 4, 5, 6, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 6, 2, 0, 6, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 5, 2, 0, 5, 4, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 6, 6, 2, 6, 6, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 5, 2, 6, 5, 4, Block.NetherFence.id, Block.NetherFence.id, false);

            for (int x = 0; x <= 6; ++x)
            {
                for (int z = 0; z <= 6; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Crossing2 : Piece
    {
        public Crossing2(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 1, 0, true);
            GetNextComponentX((Start)component, components, random, 0, 1, true);
            GetNextComponentZ((Start)component, components, random, 0, 1, true);
        }

        public static Crossing2? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, 0, 0, 5, 7, 5, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Crossing2(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 4, 1, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 4, 5, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 0, 5, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 2, 0, 4, 5, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 4, 0, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 2, 4, 4, 5, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 6, 0, 4, 6, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 0; x <= 4; ++x)
            {
                for (int z = 0; z <= 4; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal class Crossing3 : Piece
    {
        public Crossing3(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        protected Crossing3(JavaRandom random, int x, int z) : base(0)
        {
            Facing = random.NextInt(4);
            BoundingBox = new StructureBoundingBox(x, 64, z, x + 18, 73, z + 18);
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 8, 3, false);
            GetNextComponentX((Start)component, components, random, 3, 8, false);
            GetNextComponentZ((Start)component, components, random, 3, 8, false);
        }

        public static Crossing3? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -8, -3, 0, 19, 10, 19, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Crossing3(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 7, 3, 0, 11, 4, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 3, 7, 18, 4, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 8, 5, 0, 10, 7, 18, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 5, 8, 18, 7, 10, 0, 0, false);
            FillWithBlocks(world, bounds, 7, 5, 0, 7, 5, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 7, 5, 11, 7, 5, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 11, 5, 0, 11, 5, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 11, 5, 11, 11, 5, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 5, 7, 7, 5, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 11, 5, 7, 18, 5, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 5, 11, 7, 5, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 11, 5, 11, 18, 5, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 7, 2, 0, 11, 2, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 7, 2, 13, 11, 2, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 7, 0, 0, 11, 1, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 7, 0, 15, 11, 1, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 7; x <= 11; ++x)
            {
                for (int z = 0; z <= 2; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, 18 - z, bounds);
                }
            }

            FillWithBlocks(world, bounds, 0, 2, 7, 5, 2, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 13, 2, 7, 18, 2, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 0, 7, 3, 1, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 15, 0, 7, 18, 1, 11, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 0; x <= 2; ++x)
            {
                for (int z = 7; z <= 11; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, 18 - x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class End : Piece
    {
        private readonly int _fillSeed;

        public End(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
            _fillSeed = random.NextInt();
        }

        public static End? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, -3, 0, 5, 10, 8, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new End(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            JavaRandom fillRandom = new(_fillSeed);

            for (int x = 0; x <= 4; ++x)
            {
                for (int y = 3; y <= 4; ++y)
                {
                    int z = fillRandom.NextInt(8);
                    FillWithBlocks(world, bounds, x, y, 0, x, y, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                }
            }

            int length = fillRandom.NextInt(8);
            FillWithBlocks(world, bounds, 0, 5, 0, 0, 5, length, Block.NetherBrick.id, Block.NetherBrick.id, false);
            length = fillRandom.NextInt(8);
            FillWithBlocks(world, bounds, 4, 5, 0, 4, 5, length, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 0; x <= 4; ++x)
            {
                int z = fillRandom.NextInt(5);
                FillWithBlocks(world, bounds, x, 2, 0, x, 2, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
            }

            for (int x = 0; x <= 4; ++x)
            {
                for (int y = 0; y <= 1; ++y)
                {
                    int z = fillRandom.NextInt(3);
                    FillWithBlocks(world, bounds, x, y, 0, x, y, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                }
            }

            return true;
        }
    }

    internal sealed class Entrance : Piece
    {
        public Entrance(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 5, 3, true);
        }

        public static Entrance? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -5, -3, 0, 13, 14, 13, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Entrance(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 3, 0, 12, 4, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 5, 0, 12, 13, 12, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 5, 0, 1, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 11, 5, 0, 12, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 11, 4, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 8, 5, 11, 10, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 9, 11, 7, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 0, 4, 12, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 8, 5, 0, 10, 12, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 9, 0, 7, 12, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 11, 2, 10, 12, 10, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 8, 0, 7, 8, 0, Block.NetherFence.id, Block.NetherFence.id, false);

            for (int i = 1; i <= 11; i += 2)
            {
                FillWithBlocks(world, bounds, i, 10, 0, i, 11, 0, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, i, 10, 12, i, 11, 12, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, 0, 10, i, 0, 11, i, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, 12, 10, i, 12, 11, i, Block.NetherFence.id, Block.NetherFence.id, false);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, i, 13, 0, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, i, 13, 12, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, 0, 13, i, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, 12, 13, i, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, i + 1, 13, 0, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, i + 1, 13, 12, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 0, 13, i + 1, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 12, 13, i + 1, bounds);
            }

            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 0, 13, 0, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 0, 13, 12, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 12, 13, 0, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 12, 13, 12, bounds);

            for (int z = 3; z <= 9; z += 2)
            {
                FillWithBlocks(world, bounds, 1, 7, z, 1, 8, z, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, 11, 7, z, 11, 8, z, Block.NetherFence.id, Block.NetherFence.id, false);
            }

            FillWithBlocks(world, bounds, 4, 2, 0, 8, 2, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 4, 12, 2, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 0, 0, 8, 1, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 0, 9, 8, 1, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 0, 4, 3, 1, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 9, 0, 4, 12, 1, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 4; x <= 8; ++x)
            {
                for (int z = 0; z <= 2; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, 12 - z, bounds);
                }
            }

            for (int x = 0; x <= 2; ++x)
            {
                for (int z = 4; z <= 8; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, 12 - x, -1, z, bounds);
                }
            }

            FillWithBlocks(world, bounds, 5, 5, 5, 7, 5, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 1, 6, 6, 4, 6, 0, 0, false);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, 6, 0, 6, bounds);
            PlaceBlockAtCurrentPosition(world, Block.FlowingLava.id, 0, 6, 5, 6, bounds);
            int lavaX = GetXWithOffset(6, 6);
            int lavaY = GetYWithOffset(5);
            int lavaZ = GetZWithOffset(6, 6);
            if (bounds.Contains(lavaX, lavaY, lavaZ))
            {
                world.TickScheduler.ScheduleBlockUpdate(lavaX, lavaY, lavaZ, Block.FlowingLava.id, Block.FlowingLava.getTickRate());
            }

            return true;
        }
    }

    internal sealed class NetherStalkRoom : Piece
    {
        public NetherStalkRoom(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 5, 3, true);
            GetNextComponentNormal((Start)component, components, random, 5, 11, true);
        }

        public static NetherStalkRoom? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -5, -3, 0, 13, 14, 13, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new NetherStalkRoom(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 3, 0, 12, 4, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 5, 0, 12, 13, 12, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 5, 0, 1, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 11, 5, 0, 12, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 11, 4, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 8, 5, 11, 10, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 9, 11, 7, 12, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 0, 4, 12, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 8, 5, 0, 10, 12, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 9, 0, 7, 12, 1, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 11, 2, 10, 12, 10, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int i = 1; i <= 11; i += 2)
            {
                FillWithBlocks(world, bounds, i, 10, 0, i, 11, 0, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, i, 10, 12, i, 11, 12, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, 0, 10, i, 0, 11, i, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, 12, 10, i, 12, 11, i, Block.NetherFence.id, Block.NetherFence.id, false);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, i, 13, 0, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, i, 13, 12, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, 0, 13, i, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, 12, 13, i, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, i + 1, 13, 0, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, i + 1, 13, 12, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 0, 13, i + 1, bounds);
                PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 12, 13, i + 1, bounds);
            }

            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 0, 13, 0, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 0, 13, 12, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 12, 13, 0, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 12, 13, 12, bounds);

            for (int z = 3; z <= 9; z += 2)
            {
                FillWithBlocks(world, bounds, 1, 7, z, 1, 8, z, Block.NetherFence.id, Block.NetherFence.id, false);
                FillWithBlocks(world, bounds, 11, 7, z, 11, 8, z, Block.NetherFence.id, Block.NetherFence.id, false);
            }

            int stairSouthMeta = GetMetadataWithOffset(Block.NetherBrickStairs.id, 3);
            for (int step = 0; step <= 6; ++step)
            {
                int z = step + 4;
                for (int x = 5; x <= 7; ++x)
                {
                    PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairSouthMeta, x, 5 + step, z, bounds);
                }

                if (z is >= 5 and <= 8)
                {
                    FillWithBlocks(world, bounds, 5, 5, z, 7, step + 4, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                }
                else if (z is 9 or 10)
                {
                    FillWithBlocks(world, bounds, 5, 8, z, 7, step + 4, z, Block.NetherBrick.id, Block.NetherBrick.id, false);
                }

                if (step >= 1)
                {
                    FillWithBlocks(world, bounds, 5, 6 + step, z, 7, 9 + step, z, 0, 0, false);
                }
            }

            for (int x = 5; x <= 7; ++x)
            {
                PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairSouthMeta, x, 12, 11, bounds);
            }

            FillWithBlocks(world, bounds, 5, 6, 7, 5, 7, 7, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 7, 6, 7, 7, 7, 7, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 5, 13, 12, 7, 13, 12, 0, 0, false);
            FillWithBlocks(world, bounds, 2, 5, 2, 3, 5, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 9, 3, 5, 10, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 4, 2, 5, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 9, 5, 2, 10, 5, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 9, 5, 9, 10, 5, 10, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 10, 5, 4, 10, 5, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);

            int stairEastMeta = GetMetadataWithOffset(Block.NetherBrickStairs.id, 0);
            int stairWestMeta = GetMetadataWithOffset(Block.NetherBrickStairs.id, 1);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairWestMeta, 4, 5, 2, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairWestMeta, 4, 5, 3, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairWestMeta, 4, 5, 9, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairWestMeta, 4, 5, 10, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairEastMeta, 8, 5, 2, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairEastMeta, 8, 5, 3, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairEastMeta, 8, 5, 9, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrickStairs.id, stairEastMeta, 8, 5, 10, bounds);
            FillWithBlocks(world, bounds, 3, 4, 4, 4, 4, 8, Block.Soulsand.id, Block.Soulsand.id, false);
            FillWithBlocks(world, bounds, 8, 4, 4, 9, 4, 8, Block.Soulsand.id, Block.Soulsand.id, false);
            FillWithBlocks(world, bounds, 3, 5, 4, 4, 5, 8, Block.NetherWart.id, Block.NetherWart.id, false);
            FillWithBlocks(world, bounds, 8, 5, 4, 9, 5, 8, Block.NetherWart.id, Block.NetherWart.id, false);
            FillWithBlocks(world, bounds, 4, 2, 0, 8, 2, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 4, 12, 2, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 0, 0, 8, 1, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 0, 9, 8, 1, 12, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 0, 4, 3, 1, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 9, 0, 4, 12, 1, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 4; x <= 8; ++x)
            {
                for (int z = 0; z <= 2; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, 12 - z, bounds);
                }
            }

            for (int x = 0; x <= 2; ++x)
            {
                for (int z = 4; z <= 8; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, 12 - x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Stairs : Piece
    {
        public Stairs(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentZ((Start)component, components, random, 6, 2, false);
        }

        public static Stairs? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -2, 0, 0, 7, 11, 7, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Stairs(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 0, 0, 6, 1, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 6, 10, 6, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 1, 8, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 2, 0, 6, 8, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 1, 0, 8, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 2, 1, 6, 8, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 2, 6, 5, 8, 6, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 3, 2, 0, 5, 4, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 6, 3, 2, 6, 5, 2, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 6, 3, 4, 6, 5, 4, Block.NetherFence.id, Block.NetherFence.id, false);
            PlaceBlockAtCurrentPosition(world, Block.NetherBrick.id, 0, 5, 2, 5, bounds);
            FillWithBlocks(world, bounds, 4, 2, 5, 4, 3, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 3, 2, 5, 3, 4, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 2, 5, 2, 5, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 2, 5, 1, 6, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 7, 1, 5, 7, 4, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 8, 2, 6, 8, 4, 0, 0, false);
            FillWithBlocks(world, bounds, 2, 6, 0, 4, 8, 0, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 2, 5, 0, 4, 5, 0, Block.NetherFence.id, Block.NetherFence.id, false);

            for (int x = 0; x <= 6; ++x)
            {
                for (int z = 0; z <= 6; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }

    internal sealed class Start : Crossing3
    {
        public Start(JavaRandom random, int x, int z) : base(random, x, z)
        {
            for (int i = 0; i < s_primaryComponents.Length; ++i)
            {
                PieceWeight prototype = s_primaryComponents[i];
                PrimaryWeights.Add(new PieceWeight(prototype.PieceType, prototype.Weight, prototype.MaxPlaceCount, prototype.AllowConsecutive));
            }

            for (int i = 0; i < s_secondaryComponents.Length; ++i)
            {
                PieceWeight prototype = s_secondaryComponents[i];
                SecondaryWeights.Add(new PieceWeight(prototype.PieceType, prototype.Weight, prototype.MaxPlaceCount, prototype.AllowConsecutive));
            }
        }

        public PieceWeight? PreviousPieceWeight { get; set; }
        public List<PieceWeight> PrimaryWeights { get; } = [];
        public List<PieceWeight> SecondaryWeights { get; } = [];
        public List<StructureComponent> PendingComponents { get; } = [];
    }

    internal sealed class Straight : Piece
    {
        public Straight(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
        {
            GetNextComponentNormal((Start)component, components, random, 1, 3, false);
        }

        public static Straight? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -1, -3, 0, 5, 10, 19, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Straight(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 3, 0, 4, 4, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 5, 0, 3, 7, 18, 0, 0, false);
            FillWithBlocks(world, bounds, 0, 5, 0, 0, 5, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 4, 5, 0, 4, 5, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 0, 4, 2, 5, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 2, 13, 4, 2, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 0, 0, 4, 1, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 0, 15, 4, 1, 18, Block.NetherBrick.id, Block.NetherBrick.id, false);

            for (int x = 0; x <= 4; ++x)
            {
                for (int z = 0; z <= 2; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, 18 - z, bounds);
                }
            }

            FillWithBlocks(world, bounds, 0, 1, 1, 0, 4, 1, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 3, 4, 0, 4, 4, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 3, 14, 0, 4, 14, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 0, 1, 17, 0, 4, 17, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 1, 1, 4, 4, 1, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 3, 4, 4, 4, 4, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 3, 14, 4, 4, 14, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 4, 1, 17, 4, 4, 17, Block.NetherFence.id, Block.NetherFence.id, false);
            return true;
        }
    }

    internal sealed class Throne : Piece
    {
        private bool _hasSpawner;

        public Throne(int componentType, JavaRandom random, StructureBoundingBox boundingBox, int facing) : base(componentType)
        {
            BoundingBox = boundingBox;
            Facing = facing;
        }

        public static Throne? Create(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        {
            StructureBoundingBox box = StructureBoundingBox.Create(x, y, z, -2, 0, 0, 7, 8, 9, facing);
            return IsAboveGround(box) && FindIntersecting(components, box) == null ? new Throne(componentType, random, box, facing) : null;
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
        {
            FillWithBlocks(world, bounds, 0, 2, 0, 6, 7, 7, 0, 0, false);
            FillWithBlocks(world, bounds, 1, 0, 0, 5, 1, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 2, 1, 5, 2, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 3, 2, 5, 3, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 4, 3, 5, 4, 7, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 2, 0, 1, 4, 2, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 2, 0, 5, 4, 2, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 5, 2, 1, 5, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 5, 5, 2, 5, 5, 3, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 0, 5, 3, 0, 5, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 6, 5, 3, 6, 5, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            FillWithBlocks(world, bounds, 1, 5, 8, 5, 5, 8, Block.NetherBrick.id, Block.NetherBrick.id, false);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 1, 6, 3, bounds);
            PlaceBlockAtCurrentPosition(world, Block.NetherFence.id, 0, 5, 6, 3, bounds);
            FillWithBlocks(world, bounds, 0, 6, 3, 0, 6, 8, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 6, 6, 3, 6, 6, 8, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 1, 6, 8, 5, 7, 8, Block.NetherFence.id, Block.NetherFence.id, false);
            FillWithBlocks(world, bounds, 2, 8, 8, 4, 8, 8, Block.NetherFence.id, Block.NetherFence.id, false);

            if (!_hasSpawner)
            {
                int spawnerX = GetXWithOffset(3, 5);
                int spawnerY = GetYWithOffset(5);
                int spawnerZ = GetZWithOffset(3, 5);
                if (bounds.Contains(spawnerX, spawnerY, spawnerZ))
                {
                    _hasSpawner = true;
                    world.Writer.SetBlockWithoutNotifyingNeighbors(spawnerX, spawnerY, spawnerZ, Block.Spawner.id, 0, false);
                    if (world.Entities.GetBlockEntity<BlockEntityMobSpawner>(spawnerX, spawnerY, spawnerZ) is { } spawner)
                    {
                        spawner.SetSpawnedEntityId("Blaze");
                    }
                }
            }

            for (int x = 0; x <= 6; ++x)
            {
                for (int z = 0; z <= 6; ++z)
                {
                    FillCurrentPositionBlocksDownwards(world, Block.NetherBrick.id, 0, x, -1, z, bounds);
                }
            }

            return true;
        }
    }
}
