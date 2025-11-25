using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Access;
using StayOps.Domain.Abstractions;
using System.Text.Json;

namespace StayOps.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflict exception.");
            await WriteProblemDetailsAsync(context, StatusCodes.Status409Conflict, ex.Message, "Conflict");
        }
        catch (AccessManagementException ex)
        {
            _logger.LogWarning(ex, "Access management exception.");
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, ex.Message, "Bad Request");
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception.");
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, ex.Message, "Bad Request");
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found exception.");
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, ex.Message, "Not Found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "Internal Server Error");
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string detail, string title)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.TraceIdentifier
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, SerializerOptions));
    }
}
