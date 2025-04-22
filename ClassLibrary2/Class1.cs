using System;
using System.IO;
using System.Reflection;
using Resto.Front.Api.V7;
using Resto.Front.Api.V7.Attributes;
using Resto.Front.Api.V7.Data.Assortment;
using Resto.Front.Api.V7.Data.Orders;
using Resto.Front.Api.V7.Data.Organization;
using System.Reactive.Disposables;

namespace ClassLibrary2
{
    public sealed class Logger
    {
        private static readonly string LogPath;
        private static readonly object LogLock = new object();
        
        static Logger()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            LogPath = Path.Combine(dir, "iiko_events.log");
        }

        public static void Log(string message)
        {
            try
            {
                lock (LogLock)
                {
                    File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch { } // Игнорируем ошибки записи в лог
        }
    }

    [PluginLicenseModuleId(21005)]
    public sealed class EventLoggingPlugin : IFrontPlugin
    {
        private readonly CompositeDisposable _subscriptions;

        public EventLoggingPlugin()
        {
            _subscriptions = new CompositeDisposable();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            Logger.Log("Плагин выгружен");
        }

        public void Init(IPluginContext context)
        {
            Logger.Log("Инициализация плагина");

            // Подписываемся на события заказов
            _subscriptions.Add(
                context.Operations.OrderChanged.Subscribe(e => 
                    Logger.Log($"Изменение заказа #{e.Entity.Number}")
                )
            );

            _subscriptions.Add(
                context.Operations.OrderClosed.Subscribe(e =>
                    Logger.Log($"Закрыт заказ #{e.Number}, сумма: {e.ResultSum}")
                )
            );

            // События по продуктам
            _subscriptions.Add(
                context.Operations.ProductAdded.Subscribe(e =>
                    Logger.Log($"Добавлен продукт: {e.Entity.Name}")
                )
            );

            _subscriptions.Add(
                context.Operations.ProductDeleted.Subscribe(e =>
                    Logger.Log($"Удален продукт: {e.Entity.Name}")  
                )
            );

            // События по оплатам
            _subscriptions.Add(
                context.Operations.PaymentAdded.Subscribe(e =>
                    Logger.Log($"Добавлена оплата на сумму {e.Entity.Sum} (тип: {e.Entity.Type.Name})")
                )
            );

            // События по столам
            _subscriptions.Add(
                context.Operations.TableOccupied.Subscribe(e =>
                    Logger.Log($"Стол {e.Entity.Number} занят")
                )
            );

            _subscriptions.Add(
                context.Operations.TableFree.Subscribe(e =>
                    Logger.Log($"Стол {e.Entity.Number} освобожден")
                )
            );

            Logger.Log("Плагин инициализирован успешно");
        }
    }
}