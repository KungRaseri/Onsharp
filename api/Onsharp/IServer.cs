﻿using System.Collections.Generic;
using Onsharp.Entities;
using Onsharp.Entities.Factory;

namespace Onsharp
{
    /// <summary>
    /// The interface represents the server in all its functionality.
    /// The interaction and manipulation of the system can be called from here.
    /// Every plugin has its own instance for the server. 
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// A list containing all players currently on the server.
        /// </summary>
        IReadOnlyList<Player> Players { get; }
        
        /// <summary>
        /// A list containing all doors currently on the server.
        /// </summary>
        IReadOnlyList<Door> Doors { get; }
        
        /// <summary>
        /// A list containing all NPCs currently on the server.
        /// </summary>
        IReadOnlyList<NPC> NPCs { get; }
        
        /// <summary>
        /// A list containing all objects currently on the server.
        /// </summary>
        IReadOnlyList<Object> Objects { get; }
        
        /// <summary>
        /// A list containing all pickups currently on the server.
        /// </summary>
        IReadOnlyList<Pickup> Pickups { get; }
        
        /// <summary>
        /// A list containing all 3d texts currently on the server.
        /// </summary>
        IReadOnlyList<Text3D> Text3Ds { get; }
        
        /// <summary>
        /// A list containing all vehicles currently on the server.
        /// </summary>
        IReadOnlyList<Vehicle> Vehicles { get; }
        
        /// <summary>
        /// Overrides the build in version of the existing entity factory.
        /// </summary>
        /// <param name="factory">The new factory which overrides the old one</param>
        /// <typeparam name="T"></typeparam>
        void OverrideEntityFactory<T>(IEntityFactory<T> factory) where T : Entity;

        /// <summary>
        /// Searches through the class of the given owner objects for <see cref="Events.ServerEvent"/> marked methods and registers them.
        /// <see cref="IEntryPoint"/> classes will be registered automatically.
        /// </summary>
        /// <param name="owner">The owner object owning the marked methods</param>
        void RegisterServerEvents(object owner);

        /// <summary>
        /// Searches through the class of the given owner objects for <see cref="Events.RemoteEvent"/> marked methods and registers them.
        /// <see cref="IEntryPoint"/> classes will be registered automatically.
        /// The difference to the other method is that in this method no owner object is created,
        /// instead only the static methods are registered as handlers. 
        /// </summary>
        /// <typeparam name="T">The type which will be searched through</typeparam>
        void RegisterRemoteEvents<T>();

        /// <summary>
        /// Searches through the class of the given owner objects for <see cref="Events.RemoteEvent"/> marked methods and registers them.
        /// <see cref="IEntryPoint"/> classes will be registered automatically.
        /// </summary>
        /// <param name="owner">The owner object owning the marked methods</param>
        void RegisterRemoteEvents(object owner);

        /// <summary>
        /// Searches through the class of the given owner objects for <see cref="Events.ServerEvent"/> marked methods and registers them.
        /// <see cref="IEntryPoint"/> classes will be registered automatically.
        /// The difference to the other method is that in this method no owner object is created,
        /// instead only the static methods are registered as handlers. 
        /// </summary>
        /// <typeparam name="T">The type which will be searched through</typeparam>
        void RegisterServerEvents<T>();

        /// <summary>
        /// Calls a custom event on this server with the given arguments. If the event gets cancelled, this event is returning false.
        /// </summary>
        /// <param name="name">The name of the custom event</param>
        /// <param name="args">The arguments of the custom event. Onsharp Entities are valid but only in the single form. Lists or something like that are not allowed in combination</param>
        /// <returns>False, if the event gets cancelled</returns>
        bool CallEvent(string name, params object[] args);
    }
}