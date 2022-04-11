using System;
using System.Numerics;
using Cubic.Primitives;
using Cubic.Render;
using Cubic.Render.Lighting;
using Cubic.Scenes;
using Cubic.Utilities;
using OpenTK.Graphics.OpenGL4;

namespace Cubic.Entities.Components;

public class Model : Component
{
    public readonly VertexPositionTextureNormal[] Vertices;
    public readonly uint[] Indices;

    private int _vao;
    private int _vbo;
    private int _ebo;

    private static Shader _shader;
    private static bool _shaderDisposed;

    private Material _material;

    public const string VertexShader = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormals;

out vec2 frag_texCoords;
out vec3 frag_normal;
out vec3 frag_position;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    frag_texCoords = aTexCoords;
    gl_Position = vec4(aPosition, 1.0) * uModel * uView * uProjection;
    frag_position = vec3(vec4(aPosition, 1.0) * uModel);
    frag_normal = aNormals * mat3(transpose(inverse(uModel)));
}";

    public const string FragmentShader = @"
#version 330 core

struct Material
{
    sampler2D albedo;
    sampler2D specular;
    vec4 color;
    int shininess;
};

struct DirectionalLight
{
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec2 frag_texCoords;
in vec3 frag_normal;
in vec3 frag_position;

out vec4 out_color;

uniform DirectionalLight uSun;
uniform Material uMaterial;
uniform vec3 uCameraPos;

vec3 CalculateDirectional(DirectionalLight light, vec3 normal, vec3 viewDir);

void main()
{
    vec3 norm = normalize(frag_normal);
    vec3 viewDir = normalize(uCameraPos - frag_position);
    
    vec3 result = CalculateDirectional(uSun, norm, viewDir);
    out_color = vec4(result, 1.0) * uMaterial.color;
}

vec3 CalculateDirectional(DirectionalLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    
    float diff = max(dot(normal, lightDir), 0.0);
    
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);

    vec3 ambient = light.ambient * vec3(texture(uMaterial.albedo, frag_texCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(uMaterial.albedo, frag_texCoords));
    vec3 specular = light.specular * spec * vec3(texture(uMaterial.specular, frag_texCoords));
    return (ambient + diffuse + specular);
}";

    static Model()
    {
        _shaderDisposed = true;
    }

    public Model(VertexPositionTextureNormal[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }

    public Model(IPrimitive primitive, Material material)
    {
        Vertices = primitive.Vertices;
        Indices = primitive.Indices;
        _material = material;
    }

    protected internal override unsafe void Initialize()
    {
        base.Initialize();

        if (_shaderDisposed)
        {
            _shader = new Shader(VertexShader, FragmentShader);
            _shaderDisposed = false;
        }

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(VertexPositionTextureNormal), Vertices,
            BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices,
            BufferUsageHint.StaticDraw);
        
        GL.UseProgram(_shader.Handle);
        
        RenderUtils.VertexAttribs(typeof(VertexPositionTextureNormal));

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    protected internal override void Draw()
    {
        base.Draw();
        
        GL.BindVertexArray(_vao);
        GL.UseProgram(_shader.Handle);
        _shader.Set("uProjection", Camera.Main.ProjectionMatrix);
        _shader.Set("uView", Camera.Main.ViewMatrix);
        _shader.Set("uModel", Matrix4x4.CreateFromQuaternion(Transform.Rotation) * Matrix4x4.CreateTranslation(Transform.Position));
        
        _shader.Set("uCameraPos", Camera.Main.Transform.Position);
        _shader.Set("uMaterial.albedo", 0);
        _shader.Set("uMaterial.specular", 1);
        _shader.Set("uMaterial.color", _material.Color);
        _shader.Set("uMaterial.shininess", _material.Shininess);
        DirectionalLight sun = SceneManager.Active.World.Sun;
        Vector3 sunColor = sun.Color.Normalize().ToVector3();
        float sunDegX = CubicMath.ToRadians(sun.Direction.X);
        float sunDegY = CubicMath.ToRadians(-sun.Direction.Y);
        _shader.Set("uSun.direction",
            new Vector3(MathF.Cos(sunDegX) * MathF.Cos(sunDegY), MathF.Cos(sunDegX) * MathF.Sin(sunDegY),
                MathF.Sin(sunDegX)));
        _shader.Set("uSun.ambient", sunColor * sun.AmbientMultiplier);
        _shader.Set("uSun.diffuse", sunColor * sun.DiffuseMultiplier);
        _shader.Set("uSun.specular", sunColor * sun.SpecularMultiplier);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _material.Albedo.Handle);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _material.Specular.Handle);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        
        GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        
        GL.ActiveTexture(TextureUnit.Texture0);
    }

    protected internal override void Unload()
    {
        base.Unload();

        if (!_shaderDisposed)
        {
            _shaderDisposed = true;
            _shader.Dispose();
        }
    }
}