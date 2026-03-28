using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Carvers;

internal class RavineCarver : Carver
{
    private readonly float[] _widthFactors = new float[1024];

    private void CarveRavine(long seed, int centerChunkX, int centerChunkZ, byte[] blocks, double x, double y, double z, float width, float yaw, float pitch, int step, int maxSteps, double verticalScale)
    {
        JavaRandom random = new(seed);
        double chunkCenterX = centerChunkX * 16 + 8;
        double chunkCenterZ = centerChunkZ * 16 + 8;
        float yawSpeed = 0.0F;
        float pitchSpeed = 0.0F;

        if (maxSteps <= 0)
        {
            int range = Radius * 16 - 16;
            maxSteps = range - random.NextInt(range / 4);
        }

        bool isStartingPoint = false;
        if (step == -1)
        {
            step = maxSteps / 2;
            isStartingPoint = true;
        }

        float widthFactor = 1.0F;
        for (int i = 0; i < 128; ++i)
        {
            if (i == 0 || random.NextInt(3) == 0)
            {
                widthFactor = 1.0F + random.NextFloat() * random.NextFloat();
            }

            _widthFactors[i] = widthFactor * widthFactor;
        }

        for (; step < maxSteps; ++step)
        {
            double horizontalRadius = 1.5D + MathHelper.Sin(step * (float)Math.PI / maxSteps) * width;
            double verticalRadius = horizontalRadius * verticalScale;
            horizontalRadius *= random.NextFloat() * 0.25D + 0.75D;
            verticalRadius *= random.NextFloat() * 0.25D + 0.75D;
            float cosPitch = MathHelper.Cos(pitch);
            float sinPitch = MathHelper.Sin(pitch);
            x += MathHelper.Cos(yaw) * cosPitch;
            y += sinPitch;
            z += MathHelper.Sin(yaw) * cosPitch;
            pitch *= 0.7F;
            pitch += pitchSpeed * 0.05F;
            yaw += yawSpeed * 0.05F;
            pitchSpeed *= 0.8F;
            yawSpeed *= 0.5F;
            pitchSpeed += (random.NextFloat() - random.NextFloat()) * random.NextFloat() * 2.0F;
            yawSpeed += (random.NextFloat() - random.NextFloat()) * random.NextFloat() * 4.0F;

            if (!isStartingPoint && random.NextInt(4) == 0)
            {
                continue;
            }

            double distX = x - chunkCenterX;
            double distZ = z - chunkCenterZ;
            double stepsRemaining = maxSteps - step;
            double boundRadius = width + 2.0F + 16.0F;
            if (distX * distX + distZ * distZ - stepsRemaining * stepsRemaining > boundRadius * boundRadius)
            {
                return;
            }

            if (x < chunkCenterX - 16.0D - horizontalRadius * 2.0D || z < chunkCenterZ - 16.0D - horizontalRadius * 2.0D ||
                x > chunkCenterX + 16.0D + horizontalRadius * 2.0D || z > chunkCenterZ + 16.0D + horizontalRadius * 2.0D)
            {
                continue;
            }

            int xMin = MathHelper.Floor(x - horizontalRadius) - centerChunkX * 16 - 1;
            int xMax = MathHelper.Floor(x + horizontalRadius) - centerChunkX * 16 + 1;
            int yMin = MathHelper.Floor(y - verticalRadius) - 1;
            int yMax = MathHelper.Floor(y + verticalRadius) + 1;
            int zMin = MathHelper.Floor(z - horizontalRadius) - centerChunkZ * 16 - 1;
            int zMax = MathHelper.Floor(z + horizontalRadius) - centerChunkZ * 16 + 1;

            if (xMin < 0) xMin = 0;
            if (xMax > 16) xMax = 16;
            if (yMin < 1) yMin = 1;
            if (yMax > 120) yMax = 120;
            if (zMin < 0) zMin = 0;
            if (zMax > 16) zMax = 16;

            bool waterPresent = false;
            for (int blockX = xMin; !waterPresent && blockX < xMax; ++blockX)
            {
                for (int blockZ = zMin; !waterPresent && blockZ < zMax; ++blockZ)
                {
                    for (int blockY = yMax + 1; !waterPresent && blockY >= yMin - 1; --blockY)
                    {
                        int blockIndex = (blockX * 16 + blockZ) * 128 + blockY;
                        if (blockY < 0 || blockY >= 128)
                        {
                            continue;
                        }

                        byte block = blocks[blockIndex];
                        if (block == Block.FlowingWater.id || block == Block.Water.id)
                        {
                            waterPresent = true;
                        }

                        if (blockY != yMin - 1 && blockX != xMin && blockX != xMax - 1 && blockZ != zMin && blockZ != zMax - 1)
                        {
                            blockY = yMin;
                        }
                    }
                }
            }

            if (waterPresent)
            {
                continue;
            }

            for (int blockX = xMin; blockX < xMax; ++blockX)
            {
                double localX = (blockX + centerChunkX * 16 + 0.5D - x) / horizontalRadius;

                for (int blockZ = zMin; blockZ < zMax; ++blockZ)
                {
                    double localZ = (blockZ + centerChunkZ * 16 + 0.5D - z) / horizontalRadius;
                    int blockIndex = (blockX * 16 + blockZ) * 128 + yMax;
                    bool carvedGrass = false;

                    if (localX * localX + localZ * localZ >= 1.0D)
                    {
                        continue;
                    }

                    for (int blockY = yMax - 1; blockY >= yMin; --blockY)
                    {
                        double localY = (blockY + 0.5D - y) / verticalRadius;
                        if ((localX * localX + localZ * localZ) * _widthFactors[blockY] + localY * localY / 6.0D < 1.0D)
                        {
                            byte block = blocks[blockIndex];
                            if (block == Block.GrassBlock.id)
                            {
                                carvedGrass = true;
                            }

                            if (block == Block.Stone.id || block == Block.Dirt.id || block == Block.GrassBlock.id)
                            {
                                if (blockY < 10)
                                {
                                    blocks[blockIndex] = (byte)Block.FlowingLava.id;
                                }
                                else
                                {
                                    blocks[blockIndex] = 0;
                                    if (carvedGrass && blocks[blockIndex - 1] == Block.Dirt.id)
                                    {
                                        blocks[blockIndex - 1] = (byte)Block.GrassBlock.id;
                                    }
                                }
                            }
                        }

                        --blockIndex;
                    }
                }
            }

            if (isStartingPoint)
            {
                break;
            }
        }
    }

    protected override void CarveCaves(IWorldContext world, int chunkX, int chunkZ, int centerChunkX, int centerChunkZ, byte[] blocks)
    {
        if (Rand.NextInt(50) != 0)
        {
            return;
        }

        double ravineX = chunkX * 16 + Rand.NextInt(16);
        double ravineY = Rand.NextInt(Rand.NextInt(40) + 8) + 20;
        double ravineZ = chunkZ * 16 + Rand.NextInt(16);
        float yaw = Rand.NextFloat() * (float)Math.PI * 2.0F;
        float pitch = (Rand.NextFloat() - 0.5F) * 2.0F / 8.0F;
        float width = (Rand.NextFloat() * 2.0F + Rand.NextFloat()) * 2.0F;
        CarveRavine(Rand.NextLong(), centerChunkX, centerChunkZ, blocks, ravineX, ravineY, ravineZ, width, yaw, pitch, 0, 0, 3.0D);
    }
}
