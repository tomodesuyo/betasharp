using BetaSharp;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Villages;

public sealed class VillageSiege(IWorldContext world)
{
    private bool _siegeSpawned;
    private int _siegeState = -1;
    private int _remainingZombies;
    private int _spawnDelay;
    private Village? _activeVillage;
    private int _spawnX;
    private int _spawnY;
    private int _spawnZ;

    public void Tick()
    {
        if (world.GetTime() % 24000L < 12000L)
        {
            _siegeState = 0;
            return;
        }

        if (_siegeState == 2)
        {
            return;
        }

        if (_siegeState == 0)
        {
            float timeOfDay = world.Environment.GetTime(0.0F);
            if (timeOfDay < 0.5D || timeOfDay > 0.501D)
            {
                return;
            }

            _siegeState = world.Random.NextInt(10) == 0 ? 1 : 2;
            _siegeSpawned = false;
            if (_siegeState == 2)
            {
                return;
            }
        }

        if (!_siegeSpawned)
        {
            if (!TryInitializeSiege())
            {
                return;
            }

            _siegeSpawned = true;
        }

        if (_spawnDelay > 0)
        {
            --_spawnDelay;
            return;
        }

        _spawnDelay = 2;
        if (_remainingZombies > 0)
        {
            SpawnSiegeZombie();
            --_remainingZombies;
        }
        else
        {
            _siegeState = 2;
        }
    }

    private bool TryInitializeSiege()
    {
        for (int i = 0; i < world.Entities.Players.Count; ++i)
        {
            EntityPlayer player = world.Entities.Players[i];
            _activeVillage = (world as BetaSharp.Worlds.Core.World)?.Villages.FindNearestVillage((int)player.x, (int)player.y, (int)player.z, 1);
            if (_activeVillage == null ||
                _activeVillage.GetNumVillageDoors() < 10 ||
                _activeVillage.GetTicksSinceLastDoorAdding() < 20 ||
                _activeVillage.NumVillagers < 20)
            {
                continue;
            }

            Vec3i center = _activeVillage.Center;
            float villageRadius = _activeVillage.VillageRadius;
            bool intersectsOtherVillage = false;

            for (int attempt = 0; attempt < 10; ++attempt)
            {
                _spawnX = center.X + (int)(MathHelper.Cos(world.Random.NextFloat() * (float)Math.PI * 2.0F) * villageRadius * 0.9D);
                _spawnY = center.Y;
                _spawnZ = center.Z + (int)(MathHelper.Sin(world.Random.NextFloat() * (float)Math.PI * 2.0F) * villageRadius * 0.9D);
                intersectsOtherVillage = false;

                BetaSharp.Worlds.Core.World? runtimeWorld = world as BetaSharp.Worlds.Core.World;
                if (runtimeWorld != null)
                {
                    IReadOnlyList<Village> villages = runtimeWorld.Villages.Villages;
                    for (int villageIndex = 0; villageIndex < villages.Count; ++villageIndex)
                    {
                        Village village = villages[villageIndex];
                        if (village != _activeVillage && village.IsInRange(_spawnX, _spawnY, _spawnZ))
                        {
                            intersectsOtherVillage = true;
                            break;
                        }
                    }
                }

                if (!intersectsOtherVillage)
                {
                    break;
                }
            }

            if (intersectsOtherVillage || FindSpawnLocation(_spawnX, _spawnY, _spawnZ) == null)
            {
                continue;
            }

            _spawnDelay = 0;
            _remainingZombies = 20;
            return true;
        }

        return false;
    }

    private bool SpawnSiegeZombie()
    {
        Vec3D? spawnPos = FindSpawnLocation(_spawnX, _spawnY, _spawnZ);
        if (spawnPos == null)
        {
            return false;
        }

        EntityZombie zombie = new(world);
        zombie.setPositionAndAnglesKeepPrevAngles(spawnPos.Value.x, spawnPos.Value.y, spawnPos.Value.z, world.Random.NextFloat() * 360.0F, 0.0F);
        return world.SpawnEntity(zombie);
    }

    private Vec3D? FindSpawnLocation(int x, int y, int z)
    {
        if (_activeVillage == null)
        {
            return null;
        }

        for (int i = 0; i < 10; ++i)
        {
            int candidateX = x + world.Random.NextInt(16) - 8;
            int candidateY = y + world.Random.NextInt(6) - 3;
            int candidateZ = z + world.Random.NextInt(16) - 8;
            if (_activeVillage.IsInRange(candidateX, candidateY, candidateZ) && CreatureKind.Monster.CanSpawnAtLocation(world.Reader, candidateX, candidateY, candidateZ))
            {
                return new Vec3D(candidateX + 0.5D, candidateY, candidateZ + 0.5D);
            }
        }

        return null;
    }
}
