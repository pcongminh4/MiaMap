using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Behaviors;

public sealed class LogginDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<LogginDecorator<TRequest, TResponse>> _logger;

	public LogginDecorator(ILogger<LogginDecorator<TRequest, TResponse>> logger)
	{
		_logger = logger;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var requestName = typeof(TRequest).Name;
		var stopwatch = Stopwatch.StartNew();

		_logger.LogInformation("Handling request {RequestName}", requestName);

		try
		{
			var response = await next();

			stopwatch.Stop();
			_logger.LogInformation(
				"Handled request {RequestName} in {ElapsedMilliseconds}ms",
				requestName,
				stopwatch.ElapsedMilliseconds);

			return response;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(
				ex,
				"Request {RequestName} failed after {ElapsedMilliseconds}ms",
				requestName,
				stopwatch.ElapsedMilliseconds);

			throw;
		}
	}
}

