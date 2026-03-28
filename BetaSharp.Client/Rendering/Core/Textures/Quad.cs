using Silk.NET.Maths;

namespace BetaSharp.Client.Rendering.Core.Textures;

internal struct Quad
{
    private PositionTextureVertex[] _vertexPositions;
    public int NVertices;
    private readonly bool _invertNormal;

    private Quad(PositionTextureVertex[] vertices)
    {
        NVertices = 0;
        _invertNormal = false;
        _vertexPositions = vertices;
        NVertices = vertices.Length;
    }

    public Quad(PositionTextureVertex[] vertices, int texMinU, int texMinV, int texMaxU, int texMaxV) : this(vertices, texMinU, texMinV, texMaxU, texMaxV, 64, 32)
    {
    }

    public Quad(PositionTextureVertex[] vertices, int texMinU, int texMinV, int texMaxU, int texMaxV, int textureWidth, int textureHeight) : this(vertices)
    {
        float uMargin = 0.0F / textureWidth;
        float vMargin = 0.0F / textureHeight;
        vertices[0] = vertices[0].setTexturePosition(texMaxU / (float)textureWidth - uMargin, texMinV / (float)textureHeight + vMargin);
        vertices[1] = vertices[1].setTexturePosition(texMinU / (float)textureWidth + uMargin, texMinV / (float)textureHeight + vMargin);
        vertices[2] = vertices[2].setTexturePosition(texMinU / (float)textureWidth + uMargin, texMaxV / (float)textureHeight - vMargin);
        vertices[3] = vertices[3].setTexturePosition(texMaxU / (float)textureWidth - uMargin, texMaxV / (float)textureHeight - vMargin);
    }

    public void flipFace()
    {
        PositionTextureVertex[] reversed = new PositionTextureVertex[_vertexPositions.Length];

        for (int i = 0; i < _vertexPositions.Length; ++i)
        {
            reversed[i] = _vertexPositions[_vertexPositions.Length - i - 1];
        }

        _vertexPositions = reversed;
    }

    public void draw(Tessellator tessellator, float scale)
    {
        Vector3D<double> edge1 = _vertexPositions[1].vector3D - _vertexPositions[0].vector3D;
        Vector3D<double> edge2 = _vertexPositions[1].vector3D - _vertexPositions[2].vector3D;
        Vector3D<double> normal = Vector3D.Normalize(Vector3D.Cross(edge2, edge1));

        tessellator.startDrawingQuads();

        if (_invertNormal)
        {
            tessellator.setNormal(-(float)normal.X, -(float)normal.Y, -(float)normal.Z);
        }
        else
        {
            tessellator.setNormal((float)normal.X, (float)normal.Y, (float)normal.Z);
        }

        for (int i = 0; i < 4; ++i)
        {
            PositionTextureVertex vertex = _vertexPositions[i];
            tessellator.addVertexWithUV(((float)vertex.vector3D.X * scale), ((float)vertex.vector3D.Y * scale), ((float)vertex.vector3D.Z * scale), vertex.texturePositionX, vertex.texturePositionY);
        }

        tessellator.draw();
    }
}
