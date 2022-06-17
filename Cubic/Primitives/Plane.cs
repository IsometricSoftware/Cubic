using System.Numerics;
using Cubic.Render;

namespace Cubic.Primitives;

public struct Plane : IPrimitive
{
    private VertexPositionTextureNormal[] _vertices;
    private uint[] _indices;

    public VertexPositionTextureNormal[] Vertices => _vertices;

    public uint[] Indices => _indices;

    public Plane(int horizontalSubdivisions, int depthSubdivisions, Vector2 size)
    {
        int horizontalVertexCount = horizontalSubdivisions + 2;
        int depthVertexCount = depthSubdivisions + 2;

        int vertexCount = horizontalVertexCount * depthVertexCount;

        float horizontalSpacing = 1.0f / (horizontalVertexCount - 1.0f);
        float depthSpacing = 1.0f / (depthVertexCount - 1.0f);

        // Generate the vertices
        _vertices = new VertexPositionTextureNormal[vertexCount];
        int currentVertex = 0;
        for (int z = 0; z < depthVertexCount; z++)
        {
            for (int x = 0; x < horizontalVertexCount; x++)
            {
                float u = x * horizontalSpacing;
                float v = z * depthSpacing;
                Vector2 texCoords = new Vector2(u, v);
                Vector3 vertexPosition = new Vector3((-0.5f + u) * size.X, 0, (-0.5f + v) * size.Y);
                _vertices[currentVertex++] = new VertexPositionTextureNormal(vertexPosition, texCoords, new Vector3(0, 1, 0));
            }
        }

        // Generate the indices
        int indexCount = 6 * (horizontalVertexCount - 1) * (depthVertexCount - 1);
        _indices = new uint[indexCount];
        int currentIndex = 0;
        for (uint x = 0; x < (horizontalVertexCount - 1) * (depthVertexCount - 1); x++)
        {
            uint z = (uint)(x / (horizontalVertexCount - 1));

            _indices[currentIndex++] = x + z;
            _indices[currentIndex++] = x + z + 1;
            _indices[currentIndex++] = (uint)(horizontalVertexCount - 1) + x + z + 2;

            _indices[currentIndex++] = x + z;
            _indices[currentIndex++] = (uint)(horizontalVertexCount - 1) + x + z + 2;
            _indices[currentIndex++] = (uint)(horizontalVertexCount - 1) + x + z + 1;
        }
    }

    public Plane()
    {
        _vertices = new VertexPositionTextureNormal[]
        {
            new VertexPositionTextureNormal(new Vector3(-0.5f, 0.0f, -0.5f), new Vector2(0, 0), new Vector3(0, 1, 0)),
            new VertexPositionTextureNormal(new Vector3(0.5f, 0.0f, -0.5f), new Vector2(1, 0), new Vector3(0, 1, 0)),
            new VertexPositionTextureNormal(new Vector3(0.5f, 0.0f, 0.5f), new Vector2(1, 1), new Vector3(0, 1, 0)),
            new VertexPositionTextureNormal(new Vector3(-0.5f, 0.0f, 0.5f), new Vector2(0, 1), new Vector3(0, 1, 0))
        };
        _indices = new uint[]
        {
            0, 1, 2, 0, 2, 3
        };
    }
}