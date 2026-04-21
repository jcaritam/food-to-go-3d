using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using McpBridge;
using UnityEditor;
using UnityEngine;

public class McpBridgeWindow : EditorWindow
{
    private const int DefaultPort = 6400;

    private TcpListener _listener;
    private Thread _listenerThread;
    private bool _isRunning;
    private readonly ConcurrentQueue<(NetworkStream stream, string json)> _queue
        = new ConcurrentQueue<(NetworkStream, string)>();
    private readonly System.Collections.Generic.List<string> _log
        = new System.Collections.Generic.List<string>();

    [MenuItem("Tools/MCP Kitchen Bridge")]
    public static void ShowWindow()
    {
        GetWindow<McpBridgeWindow>("MCP Kitchen Bridge");
    }

    void OnEnable()
    {
        EditorApplication.update += ProcessQueue;
    }

    void OnDisable()
    {
        EditorApplication.update -= ProcessQueue;
        StopListener();
    }

    void OnGUI()
    {
        GUILayout.Label("MCP Kitchen Bridge", EditorStyles.boldLabel);
        GUILayout.Label($"Port: {DefaultPort}  |  Status: {(_isRunning ? "Running" : "Stopped")}");
        GUILayout.Space(4);

        if (_isRunning)
        {
            if (GUILayout.Button("Stop Bridge"))
                StopListener();
        }
        else
        {
            if (GUILayout.Button("Start Bridge"))
                StartListener();
        }

        GUILayout.Space(8);
        GUILayout.Label("Log (last 50 lines):", EditorStyles.boldLabel);

        var scrollPos = Vector2.zero;
        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Height(300)))
        {
            int start = Mathf.Max(0, _log.Count - 50);
            for (int i = start; i < _log.Count; i++)
                GUILayout.Label(_log[i], EditorStyles.miniLabel);
        }

        if (GUILayout.Button("Clear Log"))
            _log.Clear();
    }

    private void StartListener()
    {
        _isRunning = true;
        _listenerThread = new Thread(ListenLoop) { IsBackground = true };
        _listenerThread.Start();
        Log($"Bridge started on port {DefaultPort}");
    }

    private void StopListener()
    {
        _isRunning = false;
        try { _listener?.Stop(); } catch { }
        _listener = null;
        Log("Bridge stopped");
    }

    private void ListenLoop()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Loopback, DefaultPort);
            _listener.Start();
            while (_isRunning)
            {
                if (!_listener.Pending()) { Thread.Sleep(50); continue; }
                var client = _listener.AcceptTcpClient();
                var t = new Thread(() => ReadClient(client)) { IsBackground = true };
                t.Start();
            }
        }
        catch (Exception e)
        {
            if (_isRunning) Log($"Listener error: {e.Message}");
        }
    }

    private void ReadClient(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        _queue.Enqueue((stream, line));
                }
            }
        }
        catch (Exception e)
        {
            Log($"Client read error: {e.Message}");
        }
    }

    private void ProcessQueue()
    {
        while (_queue.TryDequeue(out var item))
        {
            try
            {
                var cmd = UnityCommand.FromJson(item.json);
                var resp = McpCommandDispatcher.Dispatch(cmd);
                var bytes = Encoding.UTF8.GetBytes(resp.ToJson());
                item.stream.Write(bytes, 0, bytes.Length);
                item.stream.Flush();
                Log($"[{cmd.Type}] id={cmd.Id.Substring(0, 8)} ok={resp.Ok}");
            }
            catch (Exception e)
            {
                Log($"ProcessQueue error: {e.Message}");
            }
        }

        if (_isRunning) Repaint();
    }

    private void Log(string msg)
    {
        _log.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
        if (_log.Count > 200) _log.RemoveAt(0);
    }
}
