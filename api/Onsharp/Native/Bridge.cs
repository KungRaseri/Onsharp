﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Nett;
using Onsharp.Events;
using Onsharp.IO;
using Onsharp.Plugins;

namespace Onsharp.Native
{
    /// <summary>
    /// The bridge managed the direct contact from the c++ runtime
    /// and the base runtime functionality and data.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal class Bridge : IRuntime
    {
        /// <summary>
        /// The name of the c++ runtime dll.
        /// </summary>
        internal const string DllName = "onsharp-runtime";
        
        /// <summary>
        /// A list containing all the events which needs as first argument a player, so called player events.
        /// </summary>
        private static readonly List<EventType> PlayerEvents = new List<EventType>
        {
            EventType.PlayerChat, EventType.PlayerChatCommand, EventType.PlayerJoin, EventType.PlayerQuit, EventType.PlayerPickupHit,
            EventType.NPCStreamIn, EventType.NPCStreamOut, EventType.PlayerEnterVehicle, EventType.PlayerLeaveVehicle, EventType.PlayerStateChange,
            EventType.VehicleStreamIn, EventType.VehicleStreamOut, EventType.PlayerDamage, EventType.PlayerDeath, EventType.PlayerInteractDoor,
            EventType.PlayerStreamIn, EventType.PlayerStreamOut, EventType.PlayerServerAuth, EventType.PlayerSteamAuth, EventType.PlayerDownloadFile,
            EventType.PlayerWeaponShot, EventType.PlayerSpawn
        };

        /// <summary>
        /// The path of the server software running this runtime.
        /// </summary>
        internal static string ServerPath { get; private set; }
        
        /// <summary>
        /// The path to the runtime folder.
        /// </summary>
        internal static string AppPath { get; private set; }
        
        /// <summary>
        /// The path to the third party libraries folder.
        /// </summary>
        internal static string LibsPath { get; private set; }
        
        /// <summary>
        /// The path to the folder containing all plugin files.
        /// </summary>
        internal static string PluginsPath { get; private set; }
        
        /// <summary>
        /// The path to the folder containing data to the plugins or the runtime itself.
        /// </summary>
        internal static string DataPath { get; private set; }
        
        /// <summary>
        /// The path to the folder containing logs to the plugins or the runtime itself.
        /// </summary>
        internal static string LogPath { get; private set; }
        
        /// <summary>
        /// The config of the current Onsharp runtime.
        /// </summary>
        internal static RuntimeConfig Config { get; private set; }
        
        /// <summary>
        /// The logger of the Onsharp runtime.
        /// </summary>
        internal static ILogger Logger { get; private set; }
        
        /// <summary>
        /// The current plugin manager instance managing all the plugins
        /// </summary>
        internal static PluginManager PluginManager { get; private set; }
        
        /// <summary>
        /// The current wrapped runtime instance for the bridge.
        /// </summary>
        internal static Bridge Runtime { get; private set; }

        /// <summary>
        /// The flag defining if the entity refreshing of the pools is enabled.
        /// If true, the pool gets refreshed if its getting accessed for retrieving all elements.
        /// Turning it off is recommended if Onsharp is the only scripting environment running in Onset,
        /// because than the management of every entity is managed by Onsharp.
        /// </summary>
        internal static bool IsEntityRefreshingEnabled => Runtime._isEntityRefreshingEnabled;

        private bool _isEntityRefreshingEnabled = true;
        
        /// <summary>
        /// Gets called by the native runtime when Onsharp should load itself.
        /// <param name="appPath">The path to the server given from the coreclr host</param>
        /// </summary>
        internal static void Load(string appPath)
        {
            try
            {
                ServerPath = appPath;
                AppPath = Path.Combine(ServerPath, "onsharp");
                Directory.CreateDirectory(AppPath);
                LibsPath = Path.Combine(AppPath, "libs");
                Directory.CreateDirectory(LibsPath);
                PluginsPath = Path.Combine(AppPath, "plugins");
                Directory.CreateDirectory(PluginsPath);
                LogPath = Path.Combine(AppPath, "logs");
                Directory.CreateDirectory(LogPath);
                DataPath = Path.Combine(AppPath, "data");
                Directory.CreateDirectory(DataPath);
                string configPath = Path.Combine(DataPath, "global.toml");
                if (File.Exists(configPath))
                {
                    Config = Toml.ReadFile<RuntimeConfig>(configPath);
                }
                else
                {
                    Config = new RuntimeConfig();
                    Toml.WriteFile(Config, configPath);
                }
            
                Logger = new Logger("Onsharp", Config.IsDebug, "_global");
                if(Config.IsDebug) Logger.Warn("{DEBUG}-Mode is currently active!", "DEBUG");
                Runtime = new Bridge();
                PluginManager = new PluginManager();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "The loading of the runtime ran into an error!");
            }
        }

        /// <summary>
        /// Gets called by the native runtime when Onsharp should unload itself.
        /// </summary>
        internal static void Unload()
        {
            Logger.Warn("Stopping bridge...");
            for (int i = PluginManager.Plugins.Count - 1; i >= 0; i--)
            {
                Plugin plugin = PluginManager.Plugins[i];
                PluginManager.ForceStop(plugin);
            }
            
            Logger.Info("Onsharp successfully stopped!");
        }

        /// <summary>
        /// Just a placeholder: Maybe this will be replaced by another technique.
        /// </summary>
        internal static bool ExecuteEvent(int typeId, string json)
        {
            ReturnData data = new ReturnData(json);
            EventType type = (EventType) typeId;
            bool flag = true;
            PluginManager.IteratePlugins(plugin =>
            {
                PluginDomain domain = PluginManager.GetDomain(plugin);
                if (domain == null)
                {
                    Logger.Fatal("Could not get plugin domain for loaded plugin {PLUGIN}!", plugin.Display);
                    return;
                }

                object[] args = ParseEventArgs(domain, type, data);
                if (!domain.Server.CallEvent(type, args))
                    flag = false;
            });
            
            return flag;
        }

        /// <summary>
        /// Converts a pointer to a string.
        /// </summary>
        /// <param name="ptr">The pointer to be converted</param>
        /// <returns>The converted string</returns>
        internal static string PtrToString(IntPtr ptr)
        {
            return Marshal.PtrToStringUTF8(ptr);
        }
        
        /// <summary>
        /// Returns a boolean whether the given event is a player event - so needs a player as the first argument - or not.
        /// </summary>
        /// <param name="type">The event type</param>
        /// <returns>True if it is a player event</returns>
        internal static bool IsPlayerEvent(EventType type)
        {
            return PlayerEvents.Contains(type);
        }

        private static object[] ParseEventArgs(PluginDomain owner, EventType type, ReturnData data)
        {
            try
            {
                object[] args = null;
                switch (type)
                {
                    case EventType.PlayerQuit:
                        break;
                    case EventType.PlayerChat:
                        break;
                    case EventType.PlayerChatCommand:
                        break;
                    case EventType.PlayerJoin:
                        break;
                    case EventType.PlayerPickupHit:
                        break;
                    case EventType.PackageStart:
                        break;
                    case EventType.PackageStop:
                        break;
                    case EventType.GameTick:
                        break;
                    case EventType.ClientConnectionRequest:
                        break;
                    case EventType.NPCReachTarget:
                        break;
                    case EventType.NPCDamage:
                        break;
                    case EventType.NPCSpawn:
                        break;
                    case EventType.NPCDeath:
                        break;
                    case EventType.NPCStreamIn:
                        break;
                    case EventType.NPCStreamOut:
                        break;
                    case EventType.PlayerEnterVehicle:
                        break;
                    case EventType.PlayerLeaveVehicle:
                        break;
                    case EventType.PlayerStateChange:
                        break;
                    case EventType.VehicleRespawn:
                        break;
                    case EventType.VehicleStreamIn:
                        break;
                    case EventType.VehicleStreamOut:
                        break;
                    case EventType.PlayerServerAuth:
                        break;
                    case EventType.PlayerSteamAuth:
                        break;
                    case EventType.PlayerDownloadFile:
                        break;
                    case EventType.PlayerStreamIn:
                        break;
                    case EventType.PlayerStreamOut:
                        break;
                    case EventType.PlayerSpawn:
                        break;
                    case EventType.PlayerDeath:
                        break;
                    case EventType.PlayerWeaponShot:
                        break;
                    case EventType.PlayerDamage:
                        break;
                    case EventType.PlayerInteractDoor:
                        break;
                    case EventType.PlayerCommandFailed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                return args;
            }
            catch (Exception ex)
            {
                Logger.Error(ex,
                    "An error occurred while parsing event args for the Event {TYPE} owned by the Plugin {PLUGIN}!",
                    Enum.GetName(typeof(EventType), type), owner.Plugin.Display);
                return null;
            }
        }

        public bool CallEvent(string name, params object[] args)
        {
            bool flag = true;
            PluginManager.IteratePlugins(plugin =>
            {
                PluginDomain domain = PluginManager.GetDomain(plugin);
                if (domain == null)
                {
                    Logger.Fatal("Could not get plugin domain for loaded plugin {PLUGIN}!", plugin.Display);
                    return;
                }

                if (!domain.Server.CallEvent(name, args))
                    flag = false;
            });
            return flag;
        }

        public void DisableEntityPoolRefreshing()
        {
            _isEntityRefreshingEnabled = false;
        }
    }
}