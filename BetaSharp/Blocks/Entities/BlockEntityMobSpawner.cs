using BetaSharp.Entities;
using BetaSharp.NBT;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks.Entities;

public class BlockEntityMobSpawner : BlockEntity
{
    public override BlockEntityType Type => BlockEntity.MobSpawner;

    public int SpawnDelay { get; set; } = 20;
    public int MinSpawnDelay { get; set; } = 200;
    public int MaxSpawnDelay { get; set; } = 800;
    public int SpawnCount { get; set; } = 4;
    public int MaxNearbyEntities { get; set; } = 6;
    public int RequiredPlayerRange { get; set; } = 16;
    public int SpawnRange { get; set; } = 4;

    public double Rotation { get; set; }
    public double LastRotation { get; set; }

    private string _spawnedEntityId = "Pig";
    private readonly List<WeightedSpawnEntry> _spawnPotentials = [];
    private WeightedSpawnEntry? _spawnData;
    private Entity? _cachedDisplayEntity;

    public string GetSpawnedEntityId()
    {
        string? entityId = _spawnData?.EntityId;
        return string.IsNullOrEmpty(entityId) ? _spawnedEntityId : entityId;
    }

    public void SetSpawnedEntityId(string spawnedEntityId)
    {
        _spawnedEntityId = spawnedEntityId;
        _spawnData = null;
        _spawnPotentials.Clear();
        InvalidateCachedEntity();
    }

    public bool IsPlayerInRange()
    {
        return World.Entities.GetClosestPlayer(X + 0.5D, Y + 0.5D, Z + 0.5D, RequiredPlayerRange) != null;
    }

    public Entity? GetDisplayEntity()
    {
        string entityId = GetSpawnedEntityId();
        if (_cachedDisplayEntity == null || EntityRegistry.GetId(_cachedDisplayEntity) != entityId.ToLowerInvariant())
        {
            _cachedDisplayEntity = EntityRegistry.Create(entityId, World);
            if (_cachedDisplayEntity == null)
            {
                return null;
            }

            ApplySpawnData(_cachedDisplayEntity);
        }

        _cachedDisplayEntity.setWorld(World);
        _cachedDisplayEntity.setPositionAndAnglesKeepPrevAngles(X + 0.5D, Y, Z + 0.5D, 0.0F, 0.0F);
        return _cachedDisplayEntity;
    }

    public override void tick(EntityManager entities)
    {
        LastRotation = Rotation;
        if (!IsPlayerInRange())
        {
            base.tick(entities);
            return;
        }

        double particleX = X + World.Random.NextFloat();
        double particleY = Y + World.Random.NextFloat();
        double particleZ = Z + World.Random.NextFloat();
        World.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
        World.Broadcaster.AddParticle("flame", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);

        if (World.IsRemote)
        {
            if (SpawnDelay > 0)
            {
                --SpawnDelay;
            }

            Rotation = (Rotation + 1000.0F / (SpawnDelay + 200.0F)) % 360.0D;
            base.tick(entities);
            return;
        }

        if (SpawnDelay == -1)
        {
            ResetDelay();
        }

        if (SpawnDelay > 0)
        {
            --SpawnDelay;
            base.tick(entities);
            return;
        }

        bool spawnedEntity = false;

        for (int spawnAttempt = 0; spawnAttempt < SpawnCount; ++spawnAttempt)
        {
            Entity? entity = EntityRegistry.Create(GetSpawnedEntityId(), World);
            if (entity == null)
            {
                return;
            }

            int nearbyCount = World.Entities
                .CollectEntitiesOfType<Entity>(new Box(X, Y, Z, X + 1, Y + 1, Z + 1).Expand(SpawnRange, SpawnRange, SpawnRange))
                .Count(e => e.GetType() == entity.GetType());
            if (nearbyCount >= MaxNearbyEntities)
            {
                ResetDelay();
                base.tick(entities);
                return;
            }

            double spawnX = X + (World.Random.NextDouble() - World.Random.NextDouble()) * SpawnRange + 0.5D;
            double spawnY = Y + World.Random.NextInt(3) - 1;
            double spawnZ = Z + (World.Random.NextDouble() - World.Random.NextDouble()) * SpawnRange + 0.5D;
            entity.setPositionAndAnglesKeepPrevAngles(spawnX, spawnY, spawnZ, World.Random.NextFloat() * 360.0F, 0.0F);
            ApplySpawnData(entity);

            if (entity is EntityLiving livingToCheck && !livingToCheck.canSpawn())
            {
                continue;
            }

            if (!SpawnEntityAndRiders(entity))
            {
                continue;
            }

            for (int particleIndex = 0; particleIndex < 20; ++particleIndex)
            {
                particleX = X + 0.5D + (World.Random.NextFloat() - 0.5D) * 2.0D;
                particleY = Y + 0.5D + (World.Random.NextFloat() - 0.5D) * 2.0D;
                particleZ = Z + 0.5D + (World.Random.NextFloat() - 0.5D) * 2.0D;
                World.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
                World.Broadcaster.AddParticle("flame", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
            }

            if (entity is EntityLiving living)
            {
                if (_spawnData == null)
                {
                    living.PostSpawn();
                }

                living.animateSpawn();
            }

            spawnedEntity = true;
        }

        if (spawnedEntity)
        {
            ResetDelay();
        }

        base.tick(entities);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        _spawnedEntityId = nbt.GetString("EntityId");
        if (string.IsNullOrEmpty(_spawnedEntityId))
        {
            _spawnedEntityId = "Pig";
        }

        SpawnDelay = nbt.GetShort("Delay");

        _spawnPotentials.Clear();
        if (nbt.HasKey("SpawnPotentials"))
        {
            NBTTagList spawnPotentials = nbt.GetTagList("SpawnPotentials");
            for (int i = 0; i < spawnPotentials.TagCount(); ++i)
            {
                if (spawnPotentials.TagAt(i) is NBTTagCompound compound)
                {
                    _spawnPotentials.Add(new WeightedSpawnEntry(compound));
                }
            }
        }

        _spawnData = nbt.HasKey("SpawnData")
            ? new WeightedSpawnEntry(nbt.GetCompoundTag("SpawnData"), _spawnedEntityId, 1)
            : null;

        if (nbt.HasKey("MinSpawnDelay"))
        {
            MinSpawnDelay = nbt.GetShort("MinSpawnDelay");
            MaxSpawnDelay = nbt.GetShort("MaxSpawnDelay");
            SpawnCount = nbt.GetShort("SpawnCount");
        }

        if (nbt.HasKey("MaxNearbyEntities"))
        {
            MaxNearbyEntities = nbt.GetShort("MaxNearbyEntities");
            RequiredPlayerRange = nbt.GetShort("RequiredPlayerRange");
        }

        if (nbt.HasKey("SpawnRange"))
        {
            SpawnRange = nbt.GetShort("SpawnRange");
        }

        InvalidateCachedEntity();
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetString("EntityId", GetSpawnedEntityId());
        nbt.SetShort("Delay", (short)SpawnDelay);
        nbt.SetShort("MinSpawnDelay", (short)MinSpawnDelay);
        nbt.SetShort("MaxSpawnDelay", (short)MaxSpawnDelay);
        nbt.SetShort("SpawnCount", (short)SpawnCount);
        nbt.SetShort("MaxNearbyEntities", (short)MaxNearbyEntities);
        nbt.SetShort("RequiredPlayerRange", (short)RequiredPlayerRange);
        nbt.SetShort("SpawnRange", (short)SpawnRange);

        if (_spawnData != null)
        {
            nbt.SetTag("SpawnData", NbtUtils.DeepCopy(_spawnData.Properties));
        }

        if (_spawnData != null || _spawnPotentials.Count > 0)
        {
            NBTTagList spawnPotentials = new();
            if (_spawnPotentials.Count > 0)
            {
                for (int i = 0; i < _spawnPotentials.Count; ++i)
                {
                    spawnPotentials.SetTag(_spawnPotentials[i].ToNbt());
                }
            }
            else if (_spawnData != null)
            {
                spawnPotentials.SetTag(_spawnData.ToNbt());
            }

            nbt.SetTag("SpawnPotentials", spawnPotentials);
        }
    }

    private void ResetDelay()
    {
        if (MaxSpawnDelay <= MinSpawnDelay)
        {
            SpawnDelay = MinSpawnDelay;
        }
        else
        {
            SpawnDelay = MinSpawnDelay + World.Random.NextInt(MaxSpawnDelay - MinSpawnDelay);
        }

        if (_spawnPotentials.Count > 0)
        {
            _spawnData = ChooseWeightedSpawn(World.Random);
            InvalidateCachedEntity();
        }
    }

    private void InvalidateCachedEntity()
    {
        _cachedDisplayEntity = null;
    }

    private void ApplySpawnData(Entity entity)
    {
        if (_spawnData == null || _spawnData.Properties.Dictionary.Count == 0)
        {
            return;
        }

        NBTTagCompound entityNbt = new();
        entity.write(entityNbt);
        NbtUtils.MergeInto(entityNbt, _spawnData.Properties);
        entity.read(entityNbt);
    }

    private bool SpawnEntityAndRiders(Entity entity)
    {
        if (!World.SpawnEntity(entity))
        {
            return false;
        }

        if (_spawnData != null)
        {
            SpawnRidingChain(entity, _spawnData.Properties);
        }

        return true;
    }

    private void SpawnRidingChain(Entity root, NBTTagCompound data)
    {
        Entity current = root;
        NBTTagCompound currentData = data;

        while (currentData.HasKey("Riding"))
        {
            NBTTagCompound ridingData = currentData.GetCompoundTag("Riding");
            string entityId = ridingData.GetString("id");
            if (string.IsNullOrEmpty(entityId) || EntityRegistry.Create(entityId, World) is not { } rider)
            {
                break;
            }

            NBTTagCompound riderNbt = new();
            rider.write(riderNbt);
            NbtUtils.MergeInto(riderNbt, ridingData);
            rider.read(riderNbt);
            rider.setPositionAndAnglesKeepPrevAngles(current.x, current.y, current.z, current.yaw, current.pitch);
            if (!World.SpawnEntity(rider))
            {
                break;
            }

            current.setVehicle(rider);
            current = rider;
            currentData = ridingData;
        }
    }

    private WeightedSpawnEntry ChooseWeightedSpawn(JavaRandom random)
    {
        int totalWeight = 0;
        for (int i = 0; i < _spawnPotentials.Count; ++i)
        {
            totalWeight += _spawnPotentials[i].Weight;
        }

        if (totalWeight <= 0)
        {
            return _spawnPotentials[0];
        }

        int choice = random.NextInt(totalWeight);
        for (int i = 0; i < _spawnPotentials.Count; ++i)
        {
            choice -= _spawnPotentials[i].Weight;
            if (choice < 0)
            {
                return _spawnPotentials[i];
            }
        }

        return _spawnPotentials[0];
    }

    private sealed class WeightedSpawnEntry
    {
        public string EntityId { get; }
        public int Weight { get; }
        public NBTTagCompound Properties { get; }

        public WeightedSpawnEntry(NBTTagCompound nbt)
        {
            Properties = NbtUtils.DeepCopy(nbt.GetCompoundTag("Properties"));
            EntityId = nbt.GetString("Type");
            Weight = nbt.GetInteger("Weight");
        }

        public WeightedSpawnEntry(NBTTagCompound properties, string entityId, int weight)
        {
            Properties = NbtUtils.DeepCopy(properties);
            EntityId = entityId;
            Weight = weight;
        }

        public NBTTagCompound ToNbt()
        {
            NBTTagCompound nbt = new();
            nbt.SetTag("Properties", NbtUtils.DeepCopy(Properties));
            nbt.SetString("Type", EntityId);
            nbt.SetInteger("Weight", Weight);
            return nbt;
        }
    }
}
