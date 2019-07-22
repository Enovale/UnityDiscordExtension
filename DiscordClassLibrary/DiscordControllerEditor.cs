using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEditor.SceneManagement;

[System.Serializable]
public class DiscordJoinEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordSpectateEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordJoinRequestEvent : UnityEngine.Events.UnityEvent<DiscordRpc.DiscordUser> { }

[InitializeOnLoad]
public class DiscordControllerEditor : Editor
{
    public static bool started;
    public static DiscordControllerEditor controller;
    public static DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
    public static string applicationId;
    public static string editorAppId = "481126411986534410";
    public static bool inEditor;
    public static string optionalSteamId;
    public static int callbackCalls;
    public static int levelNumber;
    public static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static DateTime now;
    public static int startEpoch;
    public static DiscordRpc.DiscordUser joinRequest;
    public static UnityEngine.Events.UnityEvent onConnect;
    public static UnityEngine.Events.UnityEvent onDisconnect;
    public static UnityEngine.Events.UnityEvent hasResponded;
    public static DiscordJoinEvent onJoin;
    public static DiscordJoinEvent onSpectate;
    public static DiscordJoinRequestEvent onJoinRequest;

    static DiscordRpc.EventHandlers handlers;

    static DiscordControllerEditor()
    {
        RunOnce();
    }

    static void RunOnce()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.activeSceneChangedInEditMode += HierarchyChanged;
            EditorApplication.playModeStateChanged += StateChanged;
            EditorApplication.quitting += EditorOnQuit;
            Debug.Log("RunOnce!");
            started = true;
            now = DateTime.UtcNow;
            startEpoch = (int)(now - epochStart).TotalSeconds;
            EditorPrefs.SetInt("discordEpoch", startEpoch);
            Debug.Log(EditorPrefs.GetInt("discordEpoch", startEpoch));
            Start();
        }
    }

    public static void EditorOnQuit()
    {
        EditorPrefs.SetInt("discordEpoch", 0);
    }

    public static void RequestRespondYes()
    {
        Debug.Log("Discord: responding yes to Ask to Join request");
        DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
        hasResponded.Invoke();
    }

    public static void RequestRespondNo()
    {
        Debug.Log("Discord: responding no to Ask to Join request");
        DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
        hasResponded.Invoke();
    }

    public static void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
        onConnect.Invoke();
    }

    public static void DisconnectedCallback(int errorCode, string message)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
        onDisconnect.Invoke();
    }

    public static void ErrorCallback(int errorCode, string message)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
    }

    public static void JoinCallback(string secret)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: join ({0})", secret));
        onJoin.Invoke(secret);
    }

    public static void SpectateCallback(string secret)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: spectate ({0})", secret));
        onSpectate.Invoke(secret);
    }

    public static void RequestCallback(ref DiscordRpc.DiscordUser request)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
        joinRequest = request;
        onJoinRequest.Invoke(request);
    }

    public static void UpdateRPCDetails(string details)
    {
        presence.details = details;
        DiscordRpc.UpdatePresence(presence);
    }

    public static void UpdateRPCLargeIconKey(string key)
    {
        presence.largeImageKey = key;
        DiscordRpc.UpdatePresence(presence);
    }

    public static void UpdateRPCSmallIconKey(string key)
    {
        presence.smallImageKey = key;
        DiscordRpc.UpdatePresence(presence);
    }

    static void StateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("State is exiting Edit");
            OnDisable();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            Debug.Log("State is entering Edit");
            Resume();
        }
    }

    static void Update()
    {
        DiscordRpc.RunCallbacks();
    }

    static void HierarchyChanged(Scene scenefrom, Scene sceneto)
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }
        else
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            Debug.Log("project = " + projectName);
            presence.details = string.Format("Working on {0}, in scene {1}", projectName, sceneto.name);
            DiscordRpc.UpdatePresence(presence);
        }
    }

    static void Start()
    {
        Debug.ClearDeveloperConsole();
        Debug.Log("Discord: init");
        applicationId = editorAppId;
        callbackCalls = 0;

        handlers = new DiscordRpc.EventHandlers();
        handlers.readyCallback = ReadyCallback;
        handlers.disconnectedCallback += DisconnectedCallback;
        handlers.errorCallback += ErrorCallback;
        handlers.joinCallback += JoinCallback;
        handlers.spectateCallback += SpectateCallback;
        handlers.requestCallback += RequestCallback;
        DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
        started = true;
        string[] s = Application.dataPath.Split('/');
        string projectName = s[s.Length - 2];
        Debug.Log("project = " + projectName);
        presence.details = string.Format("Working on {0}, in scene {1}", projectName, SceneManager.GetActiveScene().name);
        presence.largeImageKey = string.Format("unityicon");
        presence.startTimestamp = EditorPrefs.GetInt("discordEpoch");
        DiscordRpc.UpdatePresence(presence);
        DiscordRpc.RunCallbacks();
    }

    static void Resume()
    {
        applicationId = editorAppId;
        callbackCalls = 0;

        handlers = new DiscordRpc.EventHandlers();
        handlers.readyCallback = ReadyCallback;
        handlers.disconnectedCallback += DisconnectedCallback;
        handlers.errorCallback += ErrorCallback;
        handlers.joinCallback += JoinCallback;
        handlers.spectateCallback += SpectateCallback;
        handlers.requestCallback += RequestCallback;
        DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
        string[] s = Application.dataPath.Split('/');
        string projectName = s[s.Length - 2];
        Debug.Log("project = " + projectName);
        presence.details = string.Format("Working on {0}, in scene {1}", projectName, SceneManager.GetActiveScene().name);
        presence.largeImageKey = string.Format("unityicon");
        presence.startTimestamp = EditorPrefs.GetInt("discordEpoch");
        DiscordRpc.UpdatePresence(presence);
        DiscordRpc.RunCallbacks();
    }

    static void OnDisable()
    {
        Debug.Log("Discord: shutdown");
        DiscordRpc.Shutdown();
    }

    static void OnDestroy()
    {

    }
}
