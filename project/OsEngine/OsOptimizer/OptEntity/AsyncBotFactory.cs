/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.Robots;
using System;
using OsEngine.Logging;

namespace OsEngine.OsOptimizer.OptimizerEntity
{
    public class AsyncBotFactory
    {
        public AsyncBotFactory()
        {
            for (int i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(
                    WorkerArea,
                    _stopFactory.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        private readonly CancellationTokenSource _stopFactory = new();

        private readonly ConcurrentQueue<BotCreateRequest> _botQueue = new();

        private readonly SemaphoreSlim _queueSignal = new(0);

        private readonly ConcurrentDictionary<string, TaskCompletionSource<BotPanel>> _botWaiters = new();

        public BotPanel GetBot(string botType, string botName, CancellationToken cancellationToken = default)
        {
            string key = GetKey(botType, botName);
            TaskCompletionSource<BotPanel> waiter = _botWaiters.GetOrAdd(key, _ =>
                new TaskCompletionSource<BotPanel>(TaskCreationOptions.RunContinuationsAsynchronously));

            try
            {
                waiter.Task.Wait(cancellationToken);
                return waiter.Task.Result;
            }
            finally
            {
                _botWaiters.TryRemove(key, out _);
            }
        }

        public void CreateNewBots(List<string> botsName, string botType, bool isScript, StartProgram startProgram)
        {
            for (int i = 0; i < botsName.Count; i++)
            {
                string botName = botsName[i];
                string key = GetKey(botType, botName);

                TaskCompletionSource<BotPanel> freshWaiter =
                    new TaskCompletionSource<BotPanel>(TaskCreationOptions.RunContinuationsAsynchronously);

                _botWaiters.AddOrUpdate(key,
                    _ => freshWaiter,
                    (_, old) =>
                    {
                        old.TrySetCanceled();
                        return freshWaiter;
                    });

                _botQueue.Enqueue(new BotCreateRequest
                {
                    BotType = botType,
                    BotName = botName,
                    IsScript = isScript,
                    StartProgram = startProgram,
                    Key = key
                });

                _queueSignal.Release();
            }
        }

        private void WorkerArea()
        {
            while (!_stopFactory.Token.IsCancellationRequested)
            {
                try
                {
                    if (!_queueSignal.Wait(100, _stopFactory.Token))
                    {
                        if (MainWindow.ProccesIsWorked == false)
                        {
                            return;
                        }

                        continue;
                    }

                    if (!_botQueue.TryDequeue(out BotCreateRequest request))
                    {
                        continue;
                    }

                    BotPanel bot = BotFactory.GetStrategyForName(
                        request.BotType,
                        request.BotName,
                        request.StartProgram,
                        request.IsScript);

                    if (_botWaiters.TryGetValue(request.Key, out TaskCompletionSource<BotPanel> waiter))
                    {
                        waiter.TrySetResult(bot);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    SendLogMessage("Optimizer critical error. \n Can`t create bot. Error: " + e.ToString(), LogMessageType.Error);
                }
            }
        }

        private string GetKey(string botType, string botName)
        {
            return botType + "||" + botName;
        }

        public void SendLogMessage(string message, LogMessageType type)
        {
            if (LogMessageEvent != null)
            {
                LogMessageEvent(message, type);
            }
        }

        public event Action<string, LogMessageType> LogMessageEvent;

        private class BotCreateRequest
        {
            public string BotType;
            public string BotName;
            public bool IsScript;
            public StartProgram StartProgram;
            public string Key;
        }
    }
}
