using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logger : MonoBehaviour
{
    public static Logger Instance { get; private set; }

    private class LogEntry
    {
        public string Message { get; set; }
        public float Timestamp { get; set; }
    }

    private List<LogEntry> logMessages = new List<LogEntry>();
    private Vector2 scrollPosition;
    private float logDuration = 5f; // Duration in seconds to keep the log messages
    private GUIStyle logStyle;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize the GUIStyle for log messages
        logStyle = new GUIStyle();
        logStyle.fontSize = 20; // Set the font size
        logStyle.normal.textColor = Color.white; // Set the text color
    }

    public void Log(string message)
    {
        logMessages.Add(new LogEntry { Message = message, Timestamp = Time.time });
        Debug.Log(message);
        StartCoroutine(RemoveLogAfterDuration());
    }

    private IEnumerator RemoveLogAfterDuration()
    {
        yield return new WaitForSeconds(logDuration);
        logMessages.RemoveAll(log => Time.time - log.Timestamp >= logDuration);
    }

    private void OnGUI()
    {
        float width = Screen.width * 0.5f; // 50% of the screen width
        float height = Screen.height * 0.5f; // 50% of the screen height
        float x = 10; // 10 pixels from the left
        float y = 10; // 10 pixels from the top

        GUILayout.BeginArea(new Rect(x, y, width, height));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(width), GUILayout.Height(height));
        foreach (LogEntry log in logMessages)
        {
            GUILayout.Label(log.Message, logStyle); // Apply the custom GUIStyle
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}