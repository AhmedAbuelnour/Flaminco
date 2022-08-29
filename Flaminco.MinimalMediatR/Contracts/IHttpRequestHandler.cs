using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Flaminco.MinimalMediatR.Contracts;

public interface IHttpRequestHandler<THttpRequest> : IRequestHandler<THttpRequest, IResult> where THttpRequest : IHttpRequest
{
}

public interface ITypedHttpRequestHandler<THttpRequest, TResult1, TResult2> : IRequestHandler<THttpRequest, Results<TResult1, TResult2>>
                                                                                                            where THttpRequest : IResultHttpRequest<TResult1, TResult2>
                                                                                                            where TResult1 : IResult
                                                                                                            where TResult2 : IResult
{
}

public interface ITypedHttpRequestHandler<THttpRequest, TResult1, TResult2, TResult3> : IRequestHandler<THttpRequest, Results<TResult1, TResult2, TResult3>>
                                                                                                            where THttpRequest : IResultHttpRequest<TResult1, TResult2, TResult3>
                                                                                                            where TResult1 : IResult
                                                                                                            where TResult2 : IResult
                                                                                                            where TResult3 : IResult
{
}

public interface ITypedHttpRequestHandler<THttpRequest, TResult1, TResult2, TResult3, TResult4> : IRequestHandler<THttpRequest, Results<TResult1, TResult2, TResult3, TResult4>>
                                                                                                            where THttpRequest : IResultHttpRequest<TResult1, TResult2, TResult3, TResult4>
                                                                                                            where TResult1 : IResult
                                                                                                            where TResult2 : IResult
                                                                                                            where TResult3 : IResult
                                                                                                            where TResult4 : IResult
{
}

public interface ITypedHttpRequestHandler<THttpRequest, TResult1, TResult2, TResult3, TResult4, TResult5> : IRequestHandler<THttpRequest, Results<TResult1, TResult2, TResult3, TResult4, TResult5>>
                                                                                                            where THttpRequest : IResultHttpRequest<TResult1, TResult2, TResult3, TResult4, TResult5>
                                                                                                            where TResult1 : IResult
                                                                                                            where TResult2 : IResult
                                                                                                            where TResult3 : IResult
                                                                                                            where TResult4 : IResult
                                                                                                            where TResult5 : IResult
{
}

public interface ITypedHttpRequestHandler<THttpRequest, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> : IRequestHandler<THttpRequest, Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>>
                                                                                                            where THttpRequest : IResultHttpRequest<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>
                                                                                                            where TResult1 : IResult
                                                                                                            where TResult2 : IResult
                                                                                                            where TResult3 : IResult
                                                                                                            where TResult4 : IResult
                                                                                                            where TResult5 : IResult
                                                                                                            where TResult6 : IResult
{
}