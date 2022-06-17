using System;
using System.Numerics;
using Cubic.Render;

namespace Cubic.Primitives;

public struct Sphere : IPrimitive
{
    private VertexPositionTextureNormal[] _vertices;
    private uint[] _indices;

    public VertexPositionTextureNormal[] Vertices => _vertices;

    public uint[] Indices => _indices;

    public Sphere(int rings, int segments, float radius = 0.5f)
    {
        // Make sure the rings and segments count is in range
        if (rings < 1) 
            rings = 1;
        if (segments < 3)
            segments = 3;

        int vertexCount = 2 * segments + rings * (segments + 1);
        _vertices = new VertexPositionTextureNormal[vertexCount];

        int indexCount = 6 * segments * rings;
        _indices = new uint[indexCount];

        GenerateVertices(rings, segments, radius);
        GenerateIndices(rings, segments, vertexCount);
    }

    public Sphere()
    {
        _vertices = new VertexPositionTextureNormal[10];
        _indices = new uint[18];

        GenerateVertices(1, 3, 0.5f);
        GenerateIndices(1, 3, 10);
    }

    void GenerateVertices(int rings, int segments, float radius)
    {
        float latitudeSpacing = 1.0f / (rings + 1);
        float ongitudeSpacing = 1.0f / segments;

        int currentVertex = 0;

        // Generate the north pole's vertices
        for (int i = 0; i < segments; i++)
        {
            Vector2 texCoords = new Vector2((i + 0.5f) * ongitudeSpacing, 0.0f);
            _vertices[currentVertex++] = new VertexPositionTextureNormal(Vector3.UnitY * radius, texCoords, Vector3.UnitY);
        }

        // Generate a ring of vertices at every latitude
        for (int ring = 1; ring <= rings; ring++)
        {
            float v = ring * latitudeSpacing;
            float theta = MathF.PI * v;
            for (int segment = 0; segment <= segments; segment++)
            {
                float u = segment * ongitudeSpacing;
                float phi = 2.0f * MathF.PI * u;
                Vector2 texCoords = new Vector2(u, v);
                Vector3 vertexPosition = new Vector3(radius * MathF.Sin(theta) * MathF.Sin(phi), radius * MathF.Cos(theta), radius * MathF.Sin(theta) * MathF.Cos(phi));
                _vertices[currentVertex++] = new VertexPositionTextureNormal(vertexPosition, texCoords, Vector3.Normalize(vertexPosition));
            }
        }

        // Generate the south pole's vertices
        for (int i = 0; i < segments; i++)
        {
            Vector2 texCoords = new Vector2((i + 0.5f) * ongitudeSpacing, 1.0f);
            _vertices[currentVertex++] = new VertexPositionTextureNormal(-Vector3.UnitY * radius, texCoords, -Vector3.UnitY);
        }
    }

    void GenerateIndices(int rings, int segments, int vertexCount)
    {
        int currentIndex = 0;

        // Generate the indices for the top cap
        for (int i = 0; i < segments; i++)
        {
            _indices[currentIndex++] = (uint)i;
            _indices[currentIndex++] = (uint)(i + segments + 1);
            _indices[currentIndex++] = (uint)(i + segments);
        }

        // Generate the indices for the middle section
        for (int i = segments; i < (rings - 1) * (segments + 1) + segments; i++)
        {
            // If we're at the last vertex of the ring then jump to the next ring
            if ((i - 2 * segments) % (segments + 1) == 0)
                continue;
            _indices[currentIndex++] = (uint)i;
            _indices[currentIndex++] = (uint)(i + segments + 2);
            _indices[currentIndex++] = (uint)(i + segments + 1);

            _indices[currentIndex++] = (uint)i;
            _indices[currentIndex++] = (uint)(i + 1);
            _indices[currentIndex++] = (uint)(i + segments + 2);
        }

        // Generate the indices for the bottom cap
        for (int i = 0; i < segments; i++)
        {
            _indices[currentIndex++] = (uint)(vertexCount - segments + i);
            _indices[currentIndex++] = (uint)(vertexCount - 2 * segments - 1 + i);
            _indices[currentIndex++] = (uint)(vertexCount - 2 * segments + i);
        }
    }
}