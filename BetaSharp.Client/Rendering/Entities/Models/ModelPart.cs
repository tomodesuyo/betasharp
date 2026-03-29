using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Core.Textures;

namespace BetaSharp.Client.Rendering.Entities.Models;

public class ModelPart
{
    private readonly List<Quad[]> _boxes = [];
    private readonly List<ModelPart> _children = [];
    private int textureOffsetX;
    private int textureOffsetY;
    private int textureWidth = 64;
    private int textureHeight = 32;
    public float rotationPointX;
    public float rotationPointY;
    public float rotationPointZ;
    public float rotateAngleX;
    public float rotateAngleY;
    public float rotateAngleZ;
    private bool compiled;
    private uint displayList;
    public bool mirror = false;
    public bool visible = true;
    public bool hidden = false;

    public ModelPart(int var1, int var2)
    {
        textureOffsetX = var1;
        textureOffsetY = var2;
    }

    public void addBox(float var1, float var2, float var3, int var4, int var5, int var6)
    {
        addBox(var1, var2, var3, var4, var5, var6, 0.0F);
    }

    public void addBox(float var1, float var2, float var3, int var4, int var5, int var6, float var7)
    {
        PositionTextureVertex[] corners = new PositionTextureVertex[8];
        Quad[] faces = new Quad[6];
        float var8 = var1 + var4;
        float var9 = var2 + var5;
        float var10 = var3 + var6;
        var1 -= var7;
        var2 -= var7;
        var3 -= var7;
        var8 += var7;
        var9 += var7;
        var10 += var7;
        if (mirror)
        {
            (var8, var1) = (var1, var8);
        }

        PositionTextureVertex var20 = new(var1, var2, var3, 0.0F, 0.0F);
        PositionTextureVertex var12 = new(var8, var2, var3, 0.0F, 8.0F);
        PositionTextureVertex var13 = new(var8, var9, var3, 8.0F, 8.0F);
        PositionTextureVertex var14 = new(var1, var9, var3, 8.0F, 0.0F);
        PositionTextureVertex var15 = new(var1, var2, var10, 0.0F, 0.0F);
        PositionTextureVertex var16 = new(var8, var2, var10, 0.0F, 8.0F);
        PositionTextureVertex var17 = new(var8, var9, var10, 8.0F, 8.0F);
        PositionTextureVertex var18 = new(var1, var9, var10, 8.0F, 0.0F);
        corners[0] = var20;
        corners[1] = var12;
        corners[2] = var13;
        corners[3] = var14;
        corners[4] = var15;
        corners[5] = var16;
        corners[6] = var17;
        corners[7] = var18;
        faces[0] = new Quad([var16, var12, var13, var17], textureOffsetX + var6 + var4, textureOffsetY + var6, textureOffsetX + var6 + var4 + var6, textureOffsetY + var6 + var5, textureWidth, textureHeight);
        faces[1] = new Quad([var20, var15, var18, var14], textureOffsetX + 0, textureOffsetY + var6, textureOffsetX + var6, textureOffsetY + var6 + var5, textureWidth, textureHeight);
        faces[2] = new Quad([var16, var15, var20, var12], textureOffsetX + var6, textureOffsetY + 0, textureOffsetX + var6 + var4, textureOffsetY + var6, textureWidth, textureHeight);
        faces[3] = new Quad([var13, var14, var18, var17], textureOffsetX + var6 + var4, textureOffsetY + 0, textureOffsetX + var6 + var4 + var4, textureOffsetY + var6, textureWidth, textureHeight);
        faces[4] = new Quad([var12, var20, var14, var13], textureOffsetX + var6, textureOffsetY + var6, textureOffsetX + var6 + var4, textureOffsetY + var6 + var5, textureWidth, textureHeight);
        faces[5] = new Quad([var15, var16, var17, var18], textureOffsetX + var6 + var4 + var6, textureOffsetY + var6, textureOffsetX + var6 + var4 + var6 + var4, textureOffsetY + var6 + var5, textureWidth, textureHeight);
        if (mirror)
        {
            for (int var19 = 0; var19 < faces.Length; ++var19)
            {
                faces[var19].flipFace();
            }
        }
        
        _boxes.Add(faces);
        compiled = false;
    }

    public void setRotationPoint(float var1, float var2, float var3)
    {
        rotationPointX = var1;
        rotationPointY = var2;
        rotationPointZ = var3;
    }

    public void AddChild(ModelPart child)
    {
        _children.Add(child);
    }

    public ModelPart setTextureSize(int width, int height)
    {
        textureWidth = width;
        textureHeight = height;
        return this;
    }

    public ModelPart setTextureOffset(int x, int y)
    {
        textureOffsetX = x;
        textureOffsetY = y;
        return this;
    }

    public void render(float var1)
    {
        if (hidden || !visible)
        {
            return;
        }

        if (!compiled)
        {
            compileDisplayList(var1);
        }

        bool hasTransform = rotationPointX != 0.0F || rotationPointY != 0.0F || rotationPointZ != 0.0F || rotateAngleX != 0.0F || rotateAngleY != 0.0F || rotateAngleZ != 0.0F;
        if (hasTransform)
        {
            GLManager.GL.PushMatrix();
            ApplyTransforms(var1);
        }

        GLManager.GL.CallList(displayList);
        for (int i = 0; i < _children.Count; ++i)
        {
            _children[i].render(var1);
        }

        if (hasTransform)
        {
            GLManager.GL.PopMatrix();
        }
    }

    public void renderWithRotation(float var1)
    {
        if (hidden || !visible)
        {
            return;
        }

        if (!compiled)
        {
            compileDisplayList(var1);
        }

        GLManager.GL.PushMatrix();
        ApplyTransforms(var1);
        GLManager.GL.CallList(displayList);
        for (int i = 0; i < _children.Count; ++i)
        {
            _children[i].render(var1);
        }

        GLManager.GL.PopMatrix();
    }

    public void transform(float var1)
    {
        if (hidden || !visible)
        {
            return;
        }

        if (!compiled)
        {
            compileDisplayList(var1);
        }

        ApplyTransforms(var1);
    }

    private void ApplyTransforms(float scale)
    {
        GLManager.GL.Translate(rotationPointX * scale, rotationPointY * scale, rotationPointZ * scale);
        if (rotateAngleZ != 0.0F)
        {
            GLManager.GL.Rotate(rotateAngleZ * (180.0F / (float)Math.PI), 0.0F, 0.0F, 1.0F);
        }

        if (rotateAngleY != 0.0F)
        {
            GLManager.GL.Rotate(rotateAngleY * (180.0F / (float)Math.PI), 0.0F, 1.0F, 0.0F);
        }

        if (rotateAngleX != 0.0F)
        {
            GLManager.GL.Rotate(rotateAngleX * (180.0F / (float)Math.PI), 1.0F, 0.0F, 0.0F);
        }
    }

    private void compileDisplayList(float var1)
    {
        displayList = (uint)GLAllocation.generateDisplayLists(1);
        GLManager.GL.NewList(displayList, GLEnum.Compile);
        Tessellator var2 = Tessellator.instance;

        for (int boxIndex = 0; boxIndex < _boxes.Count; ++boxIndex)
        {
            Quad[] faces = _boxes[boxIndex];
            for (int var3 = 0; var3 < faces.Length; ++var3)
            {
                faces[var3].draw(var2, var1);
            }
        }

        GLManager.GL.EndList();
        compiled = true;
    }
}
