using System.Globalization;
using System.Linq.Expressions;

namespace Flaminco.QueryableExtensions.Extensions
{
    public static class OrderExtensions
    {
        public static IOrderedQueryable<TSource> OrderByColumn<TSource>(this IQueryable<TSource> source, string columnPath)
            => source.OrderByColumnUsing(columnPath, "OrderBy");

        public static IOrderedQueryable<TSource> OrderByColumnDescending<TSource>(this IQueryable<TSource> source, string columnPath)
            => source.OrderByColumnUsing(columnPath, "OrderByDescending");

        public static IOrderedQueryable<TSource> ThenByColumn<TSource>(this IOrderedQueryable<TSource> source, string columnPath)
            => source.OrderByColumnUsing(columnPath, "ThenBy");

        public static IOrderedQueryable<TSource> ThenByColumnDescending<TSource>(this IOrderedQueryable<TSource> source, string columnPath)
            => source.OrderByColumnUsing(columnPath, "ThenByDescending");


        private static IOrderedQueryable<TSource> OrderByColumnUsing<TSource>(this IQueryable<TSource> source, string columnPath, string method)
        {
            if (string.IsNullOrEmpty(columnPath))
                throw new ArgumentNullException(nameof(columnPath));

            ParameterExpression parameter = Expression.Parameter(typeof(TSource), "item");

            // Convert each part of the column path to Pascal case
            List<string> splittedPascalCaseItems = columnPath.Split('.')
                .Select(ConvertToPascalCase)
                .ToList();

            // Build the member expression for the property path
            Expression member = splittedPascalCaseItems.Aggregate((Expression)parameter, Expression.PropertyOrField);

            // Create lambda expression
            var keySelector = Expression.Lambda(member, parameter);

            // params argument
            var methodCall = Expression.Call(
                typeof(Queryable),
                method,
                [typeof(TSource), member.Type],
                source.Expression,
                Expression.Quote(keySelector)
            );

            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery(methodCall);
        }

        private static string ConvertToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var textInfo = new CultureInfo("en-US", false).TextInfo;

            return textInfo.ToTitleCase(input).Replace(" ", string.Empty);
        }
    }
}
