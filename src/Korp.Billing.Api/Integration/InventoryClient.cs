using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Korp.Billing.Api.Domain.Exceptions;

namespace Korp.Billing.Api.Integration;

public sealed class InventoryClient(
    HttpClient httpClient,
    ILogger<InventoryClient> logger) : IInventoryClient
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(150),
        TimeSpan.FromMilliseconds(300),
        TimeSpan.FromMilliseconds(600)
    ];

    public async Task<IReadOnlyList<InventoryProduct>> ListProductsAsync(
        CancellationToken cancellationToken)
    {
        using var response = await SendWithRetryAsync(
            () => new HttpRequestMessage(HttpMethod.Get, "api/products"),
            cancellationToken);

        await EnsureBusinessSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<InventoryProduct>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<StockDebitResult> DebitStockAsync(
        Guid idempotencyKey,
        IReadOnlyCollection<StockDebitItem> items,
        CancellationToken cancellationToken)
    {
        using var response = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/stock/debits")
            {
                Content = JsonContent.Create(new { items })
            };
            request.Headers.Add("Idempotency-Key", idempotencyKey.ToString());
            return request;
        }, cancellationToken);

        await EnsureBusinessSuccessAsync(response, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<DebitResponse>(
            cancellationToken: cancellationToken);

        return new StockDebitResult(body?.AlreadyProcessed ?? false);
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            try
            {
                using var request = requestFactory();
                var response = await httpClient.SendAsync(request, cancellationToken);

                if ((int)response.StatusCode < 500)
                {
                    return response;
                }

                lastException = new HttpRequestException(
                    $"Estoque respondeu com status {(int)response.StatusCode}.");
                response.Dispose();
            }
            catch (HttpRequestException exception)
            {
                lastException = exception;
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                lastException = exception;
            }

            if (attempt < RetryDelays.Length)
            {
                logger.LogWarning(
                    lastException,
                    "Falha ao acessar Estoque. Nova tentativa {Attempt}/{TotalAttempts}",
                    attempt + 2,
                    RetryDelays.Length + 1);
                await Task.Delay(RetryDelays[attempt], cancellationToken);
            }
        }

        throw new InventoryUnavailableException(
            "Não foi possível acessar o serviço de estoque após várias tentativas. " +
            "A nota permaneceu aberta e pode ser processada novamente.",
            lastException);
    }

    private static async Task EnsureBusinessSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await ReadProblemDetailAsync(response, cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest
            or HttpStatusCode.NotFound
            or HttpStatusCode.Conflict
            or HttpStatusCode.UnprocessableEntity)
        {
            throw new BillingRuleException(detail);
        }

        throw new InventoryUnavailableException(detail);
    }

    private static async Task<string> ReadProblemDetailAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? "Falha ao processar a operação no estoque.";
            }
        }
        catch (JsonException)
        {
        }

        return "Falha ao processar a operação no estoque.";
    }

    private sealed record DebitResponse(bool AlreadyProcessed);
}
