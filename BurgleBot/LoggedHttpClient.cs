using Microsoft.Extensions.Logging;

namespace BurgleBot;
public class HttpLoggingHandler(ILogger<HttpLoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogInformation("➡️ HTTP Request: {Method} {Uri}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentString = System.Text.Encoding.UTF8.GetString(buffer);
            logger.LogDebug("Request Content:\n{Content}", contentString);

            var newContent = new ByteArrayContent(buffer);

            foreach (var header in request.Content.Headers)
            {
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            request.Content = newContent;
        }

        var response = await base.SendAsync(request, cancellationToken);

        logger.LogInformation("⬅️ HTTP Response: {StatusCode}", response.StatusCode);

        if (response.Content != null)
        {
            var buffer = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentString = System.Text.Encoding.UTF8.GetString(buffer);
            logger.LogDebug("Response Content:\n{Content}", contentString);

            var newContent = new ByteArrayContent(buffer);
            foreach (var header in response.Content.Headers)
            {
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            response.Content = newContent;
        }

        return response;
    }
}
