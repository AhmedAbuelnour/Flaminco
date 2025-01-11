using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Flaminco.Minimal.Filters.Transaction
{
    internal sealed class TransactionEndpointFilter(DbContext _dbContext) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // Start a database transaction
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(context.HttpContext.RequestAborted);

            try
            {
                // Execute the next middleware or endpoint in the pipeline
                var result = await next(context);

                // Commit the transaction if successful
                await transaction.CommitAsync(context.HttpContext.RequestAborted);

                return result;
            }
            catch
            {
                // Rollback the transaction in case of an exception
                await transaction.RollbackAsync(context.HttpContext.RequestAborted);

                throw;
            }

        }
    }

}
