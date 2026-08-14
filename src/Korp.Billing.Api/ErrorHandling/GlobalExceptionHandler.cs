using Korp.Billing.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Billing.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            InvoiceNotFoundException =>
                (StatusCodes.Status404NotFound, "Nota fiscal não encontrada", exception.Message),
            BillingRuleException =>
                (StatusCodes.Status422UnprocessableEntity, "Regra de negócio inválida", exception.Message),
            InventoryUnavailableException =>
                (StatusCodes.Status503ServiceUnavailable, "Estoque indisponível", exception.Message),
            _ =>
                (StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Ocorreu um erro inesperado. Tente novamente.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Falha ao processar {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Requisição rejeitada em {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            }
        });
    }
}
