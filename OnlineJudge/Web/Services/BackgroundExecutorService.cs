using Microsoft.EntityFrameworkCore;
using OnlineJudge.Database;
using OnlineJudge.Miscs;
using OnlineJudge.Models.Domain;
using OnlineJudge.Parsing;
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
                            .ThenInclude(x => x.TestCases)
                            .Include(x => x.Result)
                            .Include(x => x.Results)
                            .Include(x => x.Libraries)
                            .Where(x => x.Result == null || x.Result.ExecutionStatus == ExecutionStatusEnum.NetworkError)
                            .OrderByDescending(x => x.Submitted)
                            .ToList();

                        TestCase currentTestCase = new();
                        foreach (var execution in executions)
                        {
                            if (execution.Result != null && execution.Result.AttemptedExecutionsCount > 1)
                            {                               
                                Log.Logger.Information("skipping execution");
                                continue;
                            }
                            List<SubmissionResult> results = new List<SubmissionResult>();
                            try
                            {
                                bool allResultsOk = true;
                                foreach (var testCase in execution.Assignment.TestCases)
                                {
                                    currentTestCase = testCase;
                                    var result = cs.ExecuteCode(execution, testCase.Input, testCase.Lp).Result;

                                    if (result.Success)
                                    {
                                        results.Add(result.Value);
                                        
                                    }
                                    else
                                    {
                                        var newStatus = new SubmissionResult
                                        {
                                            ExecutionStatus = ExecutionStatusEnum.Failed,
                                            Output = result.Error,
                                            Time = -1
                                        };
                                        results.Add(newStatus);
                                        allResultsOk = false;
                                        cs.UpdateSubmissionResult(execution, results, currentTestCase).Wait();
                                        break;
                                    }
                                }
                                if (allResultsOk)
                                {
                                    cs.UpdateSubmissionResult(execution, results, currentTestCase).Wait();
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
                                results.Add(newStatus);
                                cs.UpdateSubmissionResult(execution, results, currentTestCase).Wait();
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
