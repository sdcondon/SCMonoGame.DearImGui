using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static ImGuiNET.ImGui;

namespace SCMonoGameUtilities.DearImGui.Demos.GuiElements.MiniApps;

class LogWindow(ExampleLogWindowContentSource contentSource, int maxEntryCount, bool isOpen = false)
{
    public bool IsOpen = isOpen;

    // This window is for demo purposes only, and wouldn't feature in a real app:
    private readonly LogGeneratorWindow logGeneratorWindow = new(isOpen: true);

    private readonly ExampleLogWindowContentSource contentSource = contentSource;
    private readonly RingBuffer<string> content = new(maxEntryCount);
    private readonly unsafe ImGuiTextFilterPtr filter = new(ImGuiNative.ImGuiTextFilter_ImGuiTextFilter(null));

    private bool autoScroll = true;

    ~LogWindow() => filter.Destroy();

    public void Update()
    {
        if (!IsOpen) return;

        // Again, this window is for demo purposes only, and wouldn't exist in a real app:
        logGeneratorWindow.Update();

        // In a real app, perhaps consider a maximum number of messages to consume per update
        // step. Just in case something goes wrong in your app to the extent that lots of messages
        // are constantly generated (on some thread other than the main game one) - probably don't
        // want to compound the issue by having the log window try too hard to keep up.
        while (contentSource.TryDequeueMessage(out var message))
        {
            content.Add(message);
        }

        SetNextWindowSize(new Vector2(500, 400), ImGuiCond.FirstUseEver);
        if (Begin("Example: Log", ref IsOpen))
        {
            UpdateContextMenu(out var copyContentToClipboard);
            UpdateContentPane(copyContentToClipboard);
        }

        End();
    }

    private void UpdateContextMenu(out bool copyContentToClipboard)
    {
        copyContentToClipboard = false;
        if (BeginPopupContextWindow())
        {
            MenuItem("Auto-scroll", null, ref autoScroll);
            if (Selectable("Clear")) content.Clear();
            if (Selectable("Copy to clipboard")) copyContentToClipboard = true;

            Separator();
            filter.Draw();

            // Once again, demo purposes only, and wouldn't feature in a real app:
            Separator();
            MenuItem("Show Log Generator Window", null, ref logGeneratorWindow.IsOpen);

            EndPopup();
        }
    }

    private void UpdateContentPane(bool copyContentToClipboard)
    {
        if (copyContentToClipboard)
        {
            LogToClipboard();
        }

        PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        if (filter.IsActive())
        {
            TextUnformatted($"Entries matching active filter:");

            var filteredContent = content.Where(str => filter.PassFilter(str));

            if (!filteredContent.Any())
            {
                BulletText("No matches!");
            }

            foreach (string str in filteredContent)
            {
                BulletText(str);
            }
        }
        else
        {
            foreach (string str in content)
            {
                TextUnformatted(str);
            }
        }
        PopStyleVar();

        if (autoScroll && GetScrollY() >= GetScrollMaxY())
        {
            SetScrollHereY(1.0f);
        }
    }

    /// <summary>
    /// <para>
    /// A basic circular buffer type used for storing the log window's content -
    /// on the assumption that we don't want it to just grow forever.
    /// </para>
    /// <para>
    /// NB: Of course, in a real app this wouldn't necessarily be an inner type like this.
    /// Just wanted to keep all the examples as self-contained as possible.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of elements to be stored.</typeparam>
    /// <param name="maxSize">The maximum number of elements that the buffer will store. The oldest elements will be dropped once this is size is reached.</param>
    private class RingBuffer<T>(int maxSize) : IEnumerable<T>
    {
        // We *could* use something that automatically resizes itself (e.g. a List<>), so that we require
        // less memory while at less than capacity. However, on the assumption that we will be at capacity
        // most of the time, there isn't much point, and the code is simpler if we just use an array.
        public readonly T[] content = new T[maxSize];
        private int headIndex = 0;
        private int count = 0;

        public void Add(T item)
        {
            content[(headIndex + count) % content.Length] = item;

            if (count < content.Length)
            {
                count++;
            }
            else
            {
                headIndex++;
                headIndex %= content.Length;
            }
        }

        public void Clear()
        {
            Array.Clear(content); // NB: actually clear the array to avoid leaks when T is a reference type
            headIndex = 0;
            count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return content[(headIndex + i) % content.Length];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

// Example source of content for a log window, separate from the window itself for separation of concerns.
// A real app would probably want to do the same thing, though the implementation might look very different.
// This particular implementation just grabs trace (including debug) messages.
// TODO: Should probably demo (structured?) logging too - e.g. MS Logging ILogger, Serilog LogSink
class ExampleLogWindowContentSource
{
    private readonly ConcurrentQueue<string> messageQueue = new();

    public ExampleLogWindowContentSource()
    {
        // In general, its a bad idea to include such a high impact side-effect in a constructor
        // (and at the very least we should make the type disposable so that it can be unhooked),
        // but its fine for this demo app. For now.. Hmm, I'll probably fix this at some point.
        Trace.Listeners.Add(new QueuingTraceListener(messageQueue));
    }

    public bool TryDequeueMessage(out string message) => messageQueue.TryDequeue(out message);

    // This is obviously an absolutely minimal trace listener. By overriding TraceListener's other methods,
    // a more sophisticated implementation could use a queue of complex objects instead of strings - objects
    // that the log window could apply pretty formatting to when displaying them. We could, for example,
    // store the category separate from the message. And/or we could handle the object-accepting methods
    // in a more sophisticated manner than just calling ToString(), as the default method implementations do.
    private class QueuingTraceListener(ConcurrentQueue<string> messageQueue) : TraceListener
    {
        public override void Write(string message) => messageQueue.Enqueue(message);

        public override void WriteLine(string message) => messageQueue.Enqueue(message);
    }
}

// Window that offers buttons to write Debug and Trace messages, for demo purposes. Obviously wouldn't feature
// in a real app.
//
// NB: There's no dependency on the logging window or content source here - messages are going via  dotnet's
// tracing infrastructure.
class LogGeneratorWindow(bool isOpen = false)
{
    public bool IsOpen = isOpen;

    private const string TraceMessageCategory = "Log Source Window - Trace";
    private const string DebugMessageCategory = "Log Source Window - Debug";
    private static readonly string[] randomWords = ["Bumfuzzled", "Cattywampus", "Snickersnee", "Abibliophobia", "Absquatulate"];

    public void Update()
    {
        if (!IsOpen) return;

        if (Begin("Example: Log Generator", ref IsOpen))
        {
            if (Button("Debug line"))
            {
                Debug.WriteLine(MakeMessage());
            }

            if (Button("Debug line with category"))
            {
                Debug.WriteLine(MakeMessage(), DebugMessageCategory);
            }

            if (Button("Trace line"))
            {
                Debug.WriteLine(MakeMessage());
            }

            if (Button("Trace line with category"))
            {
                Trace.WriteLine(MakeMessage(), TraceMessageCategory);
            }

            if (Button("Trace event: info"))
            {
                Trace.TraceInformation(MakeMessage());
            }

            if (Button("Trace event: warning"))
            {
                Trace.TraceWarning(MakeMessage());
            }

            if (Button("Trace event: error"))
            {
                Trace.TraceError(MakeMessage());
            }

            // TODO: Add (structured?) log message
        }

        End();
    }

    private static string MakeMessage()
    {
        return $"Hello, elapsed game time {GetTime():F2}s, here's a word: {randomWords[Random.Shared.Next(randomWords.Length)]}";
    }
}
