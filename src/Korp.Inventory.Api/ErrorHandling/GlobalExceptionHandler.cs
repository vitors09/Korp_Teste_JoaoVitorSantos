using Korp.Inventory.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Inventory.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ProductNotFoundException =>
                (StatusCodes.Status404NotFound, "Produto não encontrado"),
            DuplicateProductCodeException =>
                (StatusCodes.Status409Conflict, "Código de produto duplicado"),
            InsufficientStockException =>
                (StatusCodes.Status409Conflict, "Saldo insuficiente"),
            DomainRuleException =>
                (StatusCodes.Status422UnprocessableEntity, "Regra de negócio inválida"),
            SimulatedInventoryFailureException =>
                (StatusCodes.Status503ServiceUnavailable, "Serviço de estoque indisponível"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado ao processar {Path}", httpContext.Request.Path);
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
                Detail = status >= StatusCodes.Status500InternalServerError
                    ? "Ocorreu um erro inesperado. Tente novamente."
                    : exception.Message,
                Instance = httpContext.Request.Path
            }
        });
    }
}
