using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace YouTube_MP3_Downloader.Services;

public sealed record IpcSubmitResult(bool Success, string Message, int Added, int Skipped);

public sealed class IpcServerService : IDisposable
{
    public const string PipeName = "NisarYouTubeMp3Downloader";

    private const int MaxPayloadBytes = 64 * 1024;
    private const int MaxServerInstances = 16;
    private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(15);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly LoggerService _logger;
    private readonly Func<string, bool, Task<IpcSubmitResult>> _handler;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _listenTask;
    private bool _disposed;

    public IpcServerService(LoggerService logger, Func<string, bool, Task<IpcSubmitResult>> handler)
    {
        _logger = logger;
        _handler = handler;
        _listenTask = Task.Run(ListenLoopAsync);
    }

    private async Task ListenLoopAsync()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            var pipe = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                MaxServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await pipe.WaitForConnectionAsync(_shutdown.Token).ConfigureAwait(false);
                _ = Task.Run(() => ServeConnectionAsync(pipe));
            }
            catch (OperationCanceledException)
            {
                pipe.Dispose();
                break;
            }
            catch (Exception ex)
            {
                _logger.Error("Named pipe listener error", ex);
                pipe.Dispose();
                try
                {
                    await Task.Delay(500, _shutdown.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.Info("Named pipe listener stopped");
    }

    private async Task ServeConnectionAsync(NamedPipeServerStream pipe)
    {
        try
        {
            using (pipe)
            {
                using (var readCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token))
                {
                    readCts.CancelAfter(IoTimeout);
                    var payload = await ReadFrameAsync(pipe, readCts.Token).ConfigureAwait(false);
                    if (payload is null)
                    {
                        return;
                    }

                    var response = await ProcessPayloadAsync(payload).ConfigureAwait(false);

                    using (var writeCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token))
                    {
                        writeCts.CancelAfter(IoTimeout);
                        await WriteFrameAsync(pipe, response, writeCts.Token).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error("Named pipe client handling error", ex);
        }
    }

    private async Task<Response> ProcessPayloadAsync(string json)
    {
        Request? request;
        try
        {
            request = JsonSerializer.Deserialize<Request>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.Warn($"IPC invalid payload: {ex.Message}");
            return Error("Invalid request payload.");
        }

        if (request is null)
        {
            return Error("Invalid request payload.");
        }

        var action = request.Action?.Trim() ?? "";
        var url = request.Url?.Trim() ?? "";
        if (url.Length == 0)
        {
            return Error("Missing URL.");
        }

        bool autoStart;
        if (action.Equals("add", StringComparison.OrdinalIgnoreCase))
        {
            autoStart = false;
        }
        else if (action.Equals("addAndStart", StringComparison.OrdinalIgnoreCase))
        {
            autoStart = true;
        }
        else
        {
            _logger.Warn($"Unknown IPC action rejected: {action}");
            return Error("Unknown action.");
        }

        var result = await DispatchOnUiThreadAsync(url, autoStart).ConfigureAwait(false);
        _logger.Info($"IPC {action} -> success={result.Success} added={result.Added} skipped={result.Skipped} url={url}");
        return new Response(result.Success, result.Message, result.Added, result.Skipped);
    }

    private async Task<IpcSubmitResult> DispatchOnUiThreadAsync(string url, bool autoStart)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return new IpcSubmitResult(false, "Downloader is shutting down.", 0, 0);
        }

        if (dispatcher.CheckAccess())
        {
            return await _handler(url, autoStart).ConfigureAwait(false);
        }

        try
        {
            return await dispatcher.InvokeAsync(() => _handler(url, autoStart))
                .Task
                .Unwrap()
                .ConfigureAwait(false);
        }
        catch
        {
            return new IpcSubmitResult(false, "Downloader is not available.", 0, 0);
        }
    }

    private static Response Error(string message)
    {
        return new Response(false, message, 0, 0);
    }

    private static byte[] EncodeFrame(string json)
    {
        var body = Encoding.UTF8.GetBytes(json);
        var length = BitConverter.GetBytes(body.Length);
        var frame = new byte[4 + body.Length];
        Buffer.BlockCopy(length, 0, frame, 0, 4);
        Buffer.BlockCopy(body, 0, frame, 4, body.Length);
        return frame;
    }

    private static async Task WriteFrameAsync(Stream stream, Response response, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(response, JsonOptions);
        var frame = EncodeFrame(json);
        await stream.WriteAsync(frame, ct).ConfigureAwait(false);
        await stream.FlushAsync(ct).ConfigureAwait(false);
    }

    private static async Task<string?> ReadFrameAsync(Stream stream, CancellationToken ct)
    {
        var lengthBytes = new byte[4];
        var received = 0;
        while (received < 4)
        {
            var n = await stream.ReadAsync(lengthBytes.AsMemory(received, 4 - received), ct).ConfigureAwait(false);
            if (n == 0)
            {
                return null;
            }

            received += n;
        }

        var length = BitConverter.ToInt32(lengthBytes, 0);
        if (length <= 0 || length > MaxPayloadBytes)
        {
            return "";
        }

        var body = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var n = await stream.ReadAsync(body.AsMemory(offset, length - offset), ct).ConfigureAwait(false);
            if (n == 0)
            {
                throw new EndOfStreamException("Pipe closed while reading payload.");
            }

            offset += n;
        }

        return Encoding.UTF8.GetString(body);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _shutdown.Cancel();
        try
        {
            _listenTask.GetAwaiter().GetResult();
        }
        catch
        {
        }

        _shutdown.Dispose();
    }

    public sealed class Request
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public sealed class Response
    {
        [JsonPropertyName("success")]
        public bool Success { get; }

        [JsonPropertyName("message")]
        public string Message { get; }

        [JsonPropertyName("added")]
        public int Added { get; }

        [JsonPropertyName("skipped")]
        public int Skipped { get; }

        public Response(bool success, string message, int added, int skipped)
        {
            Success = success;
            Message = message;
            Added = added;
            Skipped = skipped;
        }
    }
}