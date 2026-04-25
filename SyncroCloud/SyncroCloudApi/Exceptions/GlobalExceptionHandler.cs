using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SyncroCloudApi.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (status, message) = exception switch
        {
            DbUpdateException dbEx when IsUniqueViolation(dbEx) =>
                (StatusCodes.Status409Conflict, ExtractUniqueViolationMessage(dbEx)),

            DbUpdateException =>
                (StatusCodes.Status500InternalServerError, "A database error occurred. Please try again."),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, exception.Message),

            ArgumentException =>
                (StatusCodes.Status400BadRequest, exception.Message),

            InvalidOperationException =>
                (StatusCodes.Status400BadRequest, exception.Message),

            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title  = ReasonPhrase(status),
            Detail = message
        }, ct);

        return true;
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: "23505" };

    private static string ExtractUniqueViolationMessage(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pg)
        {
            var detail = pg.Detail ?? string.Empty;
            if (detail.Contains("Name"))       return "A record with this name already exists.";
            if (detail.Contains("Email"))      return "This email address is already registered.";
            if (detail.Contains("PhoneNumber")) return "This phone number is already registered.";
            if (detail.Contains("SensorId"))   return "This sensor is already installed on the device.";
        }
        return "A record with the same unique key already exists.";
    }

    private static string ReasonPhrase(int status) => status switch
    {
        400 => "Bad Request",
        404 => "Not Found",
        409 => "Conflict",
        _   => "Internal Server Error"
    };
}
