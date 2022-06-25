using System;
using Cubic.GUI;
using Cubic.Render;
using Cubic.Scenes;
using Cubic.Windowing;

namespace Cubic.Entities.Components;

public abstract class Component
{
    public bool Enabled = true;
    
    protected internal Entity Entity { get; internal set; }

    protected CubicGame Game => Entity.Game;
    protected GraphicsMachine Graphics => CubicGame.GraphicsInternal;

    protected Scene CurrentScene => SceneManager.Active;

    protected Transform Transform => Entity.Transform;
    
    protected internal virtual void Initialize() { }

    protected internal virtual void Update() { }

    protected internal virtual void Draw() { }

    protected internal virtual void Unload() { }

    protected int EntityCount => CurrentScene.EntityCount;

    protected T GetComponent<T>() where T : Component => Entity.GetComponent<T>();

    protected void AddComponent(Component component) => Entity.AddComponent(component);

    protected void RemoveComponent(Type component) => Entity.RemoveComponent(component);

    protected void RemoveComponent<T>() => Entity.RemoveComponent<T>();

    protected void AddEntity(string name, Entity entity) => CurrentScene.AddEntity(name, entity);

    protected void AddEntity(Entity entity) => CurrentScene.AddEntity(entity);

    protected void RemoveEntity(string name) => CurrentScene.RemoveEntity(name);

    protected void Destroy() => CurrentScene.RemoveEntity(Entity.Name);

    protected Entity GetEntity(string name) => CurrentScene.GetEntity(name);

    protected T GetEntity<T>(string name) where T : Entity => CurrentScene.GetEntity<T>(name);

    protected Entity[] GetEntitiesWithComponent<T>() where T : Component =>
        CurrentScene.GetEntitiesWithComponent<T>();

    protected Entity[] GetEntitiesWithTag(string tag) => CurrentScene.GetEntitiesWithTag(tag);

    protected Entity[] GetAllEntities() => CurrentScene.GetAllEntities();

    protected void AddScreen(Screen screen, string name) => CurrentScene.AddScreen(screen, name);

    protected void OpenScreen(string name) => CurrentScene.OpenScreen(name);

    protected void CloseScreen() => CurrentScene.CloseScreen();
}