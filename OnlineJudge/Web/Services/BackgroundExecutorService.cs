using Microsoft.EntityFrameworkCore;
using OnlineJudge.Database;
using OnlineJudge.Miscs;
using OnlineJudge.Models.Domain;
using Serilog;

namespace OnlineJudge.Services
{
    public class BackgroundExecutorService : IHostedService, IDisposable
    {
        private Timer? _timer = null;
        private readonly IServiceProvider _services;
        private readonly object lockObj = new object();

        public BackgroundExecutorService(IServiceProvider services)
        {
            this._services = services;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(35));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            Log.Logger.Information("challenging lock");
            if (Monitor.TryEnter(lockObj))
            {
                try
                {
                    Log.Logger.Information("entering critical section");
                    using (var scope = _services.CreateScope())
                    {
                        var ctx = scope.ServiceProvider.GetRequiredService<Context>();
                        var cs = scope.ServiceProvider.GetRequiredService<CodeService>();

                        var executions = ctx.Submissions
                            .Include(x => x.User)
                            .Include(x => x.Assignment)
                            .Include(x => x.Result)
                            .Include(x => x.Libraries)
                            .Where(x => x.Result == null || x.Result.ExecutionStatus == ExecutionStatusEnum.NetworkError)
                            .OrderByDescending(x => x.Submitted)
                            .ToList();

                        foreach (var execution in executions)
                        {
                            if (execution.Result != null && execution.Result.AttemptedExecutionsCount > 1)
                            {
                                Log.Logger.Information("skipping execution");
                                continue;
                            }

                            try
                            {
                                var result = cs.ExecuteCode(execution).Result;

                                if (result.Success)
                                {
                                    cs.UpdateSubmissionResult(execution, result.Value).Wait();
                                }
                                else
                                {
                                    var newStatus = new SubmissionResult
                                    {
                                        ExecutionStatus = ExecutionStatusEnum.Failed,
                                        Output = result.Error,
                                        Time = -1
                                    };

                                    cs.UpdateSubmissionResult(execution, newStatus).Wait();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Logger.Error(ex.ToString());

                                var newStatus = new SubmissionResult
                                {
                                    ExecutionStatus = ExecutionStatusEnum.NetworkError,
                                    Output = "Network Error",
                                    Time = -1
                                };

                                cs.UpdateSubmissionResult(execution, newStatus).Wait();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.ToString());
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            else
            {
                Log.Logger.Information("skipping lock");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
