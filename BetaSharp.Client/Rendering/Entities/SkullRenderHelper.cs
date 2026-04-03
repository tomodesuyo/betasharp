using System;
using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Rendering.Entities.Models;

namespace BetaSharp.Client.Rendering.Entities;

internal static class SkullRenderHelper
{
    private static readonly ModelSkeletonHead s_skeletonHead = new(0, 35, 64, 64);
    private static readonly ModelHumanoidHead s_humanoidHead = new();

    public static void RenderSkull(Action<string> bindTexture, int skullType, float yawDegrees)
    {
        ModelBase model = s_skeletonHead;
        string texturePath = "/mob/skeleton.png";

        switch (skullType)
        {
            case BlockEntitySkull.WitherSkeleton:
                texturePath = "/mob/skeleton_wither.png";
                break;
            case BlockEntitySkull.Zombie:
                texturePath = "/mob/zombie.png";
                model = s_humanoidHead;
                break;
            case BlockEntitySkull.Player:
                texturePath = "/mob/char.png";
                model = s_humanoidHead;
                break;
            case BlockEntitySkull.Creeper:
                texturePath = "/mob/creeper.png";
                break;
        }

        bindTexture(texturePath);
        model.render(0.0F, 0.0F, 0.0F, yawDegrees, 0.0F, 1.0F / 16.0F);
    }
}
