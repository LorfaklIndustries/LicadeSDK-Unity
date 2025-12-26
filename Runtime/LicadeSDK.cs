using UnityEngine;
using NativeWebSocket;

namespace LorfaklInudustries.LicadeSDK
{
    public class LicadeSDK : MonoBehaviour
{
    private static LicadeSDK _instance;
    private WebSocket _websocket;

    // --- Public API ---

    public static void Initialize()
    {
        if (_instance == null)
        {
            var go = new GameObject("LicadeSDK");
            _instance = go.AddComponent<LicadeSDK>();
            DontDestroyOnLoad(go);
            _instance.Connect();
        }
    }

    public static void GameIsReady()
    {
        _instance.SendMsg("game_is_ready", null);
    }

    public static void SendEvent(string eventType, object payload)
    {
        // Use a simple wrapper object for the payload
        var eventData = new { type = eventType, data = payload };
        _instance.SendMsg("game_event", eventData);
    }

    // --- Internal Logic ---

    private async void Connect()
    {
        _websocket = new WebSocket("ws://127.0.0.1:9001");

        _websocket.OnOpen += () => Debug.Log("LicadeSDK: Connected to Shell.");
        _websocket.OnError += (e) => Debug.LogError("LicadeSDK: Connection Error: " + e);
        _websocket.OnClose += (e) => Debug.Log("LicadeSDK: Disconnected from Shell.");

        // Keep sending heartbeats to let the shell know we are alive
        InvokeRepeating(nameof(SendHeartbeat), 0, 5.0f);

        await _websocket.Connect();
    }
    
    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (_websocket != null)
        {
            _websocket.DispatchMessageQueue();
        }
        #endif
    }

    private void SendHeartbeat()
    {
        SendMsg("heartbeat", null);
    }

    private void SendMsg(string messageType, object payload)
    {
        if (_websocket.State == WebSocketState.Open)
        {
            // We define a standard JSON structure for all messages
            var message = new { type = messageType, payload = payload };
            _websocket.SendText(JsonUtility.ToJson(message));
        }
    }

    private async void OnDestroy()
    {
        if (_websocket != null) await _websocket.Close();
    }
}
}


