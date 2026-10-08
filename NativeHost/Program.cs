using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeMp3NativeHost;

internal static class Program
{
    public const string PipeName = "NisarYouTubeMp3Downloader";

    private const int MaxPayloadBytes = 64 * 1024;
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan PipeIoTimeout = TimeSpan.FromSeconds(10);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class Request
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private sealed class Response
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    private static async Task<int> Main()
    {
        try
        {
            var stdin = Console.OpenStandardInput();
            var payload = await ReadFrameAsync(stdin, CancellationToken.None).ConfigureAwait(false);
            if (payload is null)
            {
                return 0;
            }

            var response = await ProcessRequestAsync(payload).ConfigureAwait(false);

            var stdout = Console.OpenStandardOutput();
            await WriteFrameAsync(stdout, response, CancellationToken.None).ConfigureAwait(false);
            await stdout.FlushAsync(CancellationToken.None).ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex)
        {
            Log($"Fatal host error: {ex.Message}");
            try
            {
                var stdout = Console.OpenStandardOutput();
                await WriteFrameAsync(stdout, Fail("Host failure."), CancellationToken.None).ConfigureAwait(false);
                await stdout.FlushAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
            }

            return 1;
        }
    }

    private static async Task<string> ProcessRequestAsync(string payload)
    {
        Request? request;
        try
        {
            request = JsonSerializer.Deserialize<Request>(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            Log($"Invalid payload: {ex.Message}");
            return Fail("Invalid request payload.");
        }

        if (request is null)
        {
            return Fail("Invalid request payload.");
        }

        var action = request.Action?.Trim() ?? "";
        var url = request.Url?.Trim() ?? "";

        if (!action.Equals("add", StringComparison.OrdinalIgnoreCase)
            && !action.Equals("addAndStart", StringComparison.OrdinalIgnoreCase))
        {
            Log($"Unknown action rejected: {action}");
            return Fail("Unknown action.");
        }

        if (url.Length == 0)
        {
            return Fail("Missing URL.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Fail("Unsupported URL.");
        }

        var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            using (pipe)
            {
                try
                {
                    using var connectCts = new CancellationTokenSource(ConnectTimeout);
                    await pipe.ConnectAsync(connectCts.Token).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Log($"Could not connect to {PipeName}");
                    return Fail("Downloader is not running.");
                }

                var outbound = JsonSerializer.Serialize(new { action, url }, JsonOptions);

                using (var writeCts = new CancellationTokenSource(PipeIoTimeout))
                {
                    await WriteFrameAsync(pipe, outbound, writeCts.Token).ConfigureAwait(false);
                    await pipe.FlushAsync(writeCts.Token).ConfigureAwait(false);
                }

                using var readCts = new CancellationTokenSource(PipeIoTimeout);
                var reply = await ReadFrameAsync(pipe, readCts.Token).ConfigureAwait(false);
                return reply ?? Fail("Downloader returned no response.");
            }
        }
        catch (OperationCanceledException)
        {
            Log("Timed out waiting for the desktop downloader.");
            return Fail("Downloader did not respond.");
        }
        catch (Exception ex)
        {
            Log($"Pipe error: {ex.Message}");
            return Fail("Downloader is not running.");
        }
    }

    private static string Fail(string message)
    {
        return JsonSerializer.Serialize(new Response { Success = false, Message = message }, JsonOptions);
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
            return null;
        }

        var body = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var n = await stream.ReadAsync(body.AsMemory(offset, length - offset), ct).ConfigureAwait(false);
            if (n == 0)
            {
                return null;
            }

            offset += n;
        }

        return Encoding.UTF8.GetString(body);
    }

    private static async Task WriteFrameAsync(Stream stream, string json, CancellationToken ct)
    {
        var body = Encoding.UTF8.GetBytes(json);
        var frame = new byte[4 + body.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(body.Length), 0, frame, 0, 4);
        Buffer.BlockCopy(body, 0, frame, 4, body.Length);
        await stream.WriteAsync(frame, ct).ConfigureAwait(false);
    }

    private static void Log(string message)
    {
        try
        {
            Console.Error.WriteLine($"[YouTubeMp3NativeHost] {message}");
        }
        catch
        {
        }
    }
}