using Flaminco.MinimalEndpoints.Models;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class EndpointResultExtensions
    {
        public static IResult ToBusinessFailure<T>(this T endpointResult, int statusCode = 601) where T : EndpointResult
        {
            endpointResult.Status = statusCode;

            return TypedResults.Json(endpointResult, statusCode: statusCode, contentType: "application/json");
        }

        public static IResult ToOk<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status200OK;

            return TypedResults.Ok(endpointResult);
        }

        public static IResult ToBadRequest<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status400BadRequest;

            return TypedResults.BadRequest(endpointResult);
        }

        public static IResult ToUnprocessableEntity<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status422UnprocessableEntity;

            return TypedResults.UnprocessableEntity(endpointResult);
        }

        public static IResult ToInternalServerError<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status500InternalServerError;

            return TypedResults.InternalServerError(endpointResult);
        }

        public static IResult ToNotFound<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status404NotFound;

            return TypedResults.NotFound(endpointResult);
        }

        public static IResult ToCreated<T>(this T endpointResult, string location) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status201Created;

            return TypedResults.Created(location, endpointResult);
        }

        public static IResult ToAccepted<T>(this T endpointResult, string location) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status202Accepted;

            return TypedResults.Accepted(location, endpointResult);
        }

        public static IResult ToConflict<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status409Conflict;

            return TypedResults.Conflict(endpointResult);
        }

        public static IResult ToUnauthorized<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status401Unauthorized;

            return TypedResults.Json(endpointResult, statusCode: StatusCodes.Status401Unauthorized, contentType: "application/json");
        }

        public static IResult ToLocked<T>(this T endpointResult) where T : EndpointResult
        {
            endpointResult.Status = StatusCodes.Status423Locked;

            return TypedResults.Json(endpointResult, statusCode: StatusCodes.Status423Locked, contentType: "application/json");
        }
    }
}