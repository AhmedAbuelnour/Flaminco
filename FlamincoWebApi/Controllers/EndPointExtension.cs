using System.Reflection;

namespace FlamincoWebApi.Controllers
{
    public static class EndPointExtension
    {
        public static void AddEndPoints(this IEndpointRouteBuilder app)
        {
            IEnumerable<Type> endpoints = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(EndPointAttribute)));

            foreach (var endpoint in endpoints)
            {
                if (Attribute.GetCustomAttribute(endpoint, typeof(EndPointAttribute)) is EndPointAttribute endPoint)
                {
                    if (endPoint.HttpVerb == HttpVerb.Get)
                    {
                        MethodInfo? mediatGet = typeof(Flaminco.MinimalMediatR.MinimalMediatRExtensions).GetMethod("MediateGet");

                        mediatGet.MakeGenericMethod(endpoint).Invoke(null, new object[] { app, endPoint.Template });
                    }
                }
            }

        }
    }
}
