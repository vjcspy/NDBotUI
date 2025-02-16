using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.AutoCore.Store;

public abstract class EffectBase
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected virtual bool GetForceEligible()
    {
        return false;
    }

    // Dictionary để lưu trạng thái xử lý của từng EmulatorId
    private readonly ConcurrentDictionary<string, bool> _isProcessingByEmulator = new();

    protected virtual bool IsParallel()
    {
        return true;
    }

    protected abstract IEventActionFactory[] GetAllowEventActions();
    protected abstract Task<EventAction> Process(EventAction action);

    protected async Task<EventAction> ProcessWrapper(EventAction action)
    {
        if (IsParallel())
        {
            // Nếu có thể xử lý đồng thời thì không cần kiểm tra trạng thái
            return await Process(action);
        }

        // Lấy EmulatorId từ action
        string emulatorId = GetEmulatorIdFromAction(action);

        // Kiểm tra xem EmulatorId này có đang được xử lý hay không
        if (_isProcessingByEmulator.TryGetValue(emulatorId, out var isProcessing) && isProcessing)
        {
            // Nếu đang xử lý thì trả về Empty Action
            Logger.Info("EmulatorId {0} is processing, skip this action", emulatorId);
            return CoreAction.Empty;
        }

        try
        {
            // Đánh dấu EmulatorId là đang được xử lý
            _isProcessingByEmulator[emulatorId] = true;

            // Xử lý action
            return await Process(action);
        }
        finally
        {
            // Sau khi xử lý xong, đánh dấu EmulatorId là không còn xử lý
            _isProcessingByEmulator[emulatorId] = false;
        }
    }

    private string GetEmulatorIdFromAction(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            return baseActionPayload.EmulatorId;
        }

        return Guid.Empty.ToString(); // Trường hợp không có Id, đảm bảo không lỗi
    }

    protected virtual int GetThrottleTime()
    {
        return 0;
    }

    protected virtual bool Filter(EventAction action)
    {
        return true;
    }

    [Effect]
    public virtual RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(GetAllowEventActions())
            .GroupBy(
                action =>
                {
                    if (action.Payload is BaseActionPayload baseActionPayload)
                    {
                        return baseActionPayload.EmulatorId;
                    }

                    return Guid.Empty.ToString(); // Trường hợp không có Id, đảm bảo không lỗi
                }
            )
            .SelectMany(
                groupedStream =>
                {
                    // Lấy throttle time từ phương thức
                    var throttleTime = GetThrottleTime();

                    // Kiểm tra throttle time, chỉ thêm .Throttle nếu throttleTime > 0
                    var stream = groupedStream
                        .FilterBaseEligibility(GetForceEligible())
                        .Where(Filter);

                    if (throttleTime > 0)
                    {
                        stream = stream.Throttle(TimeSpan.FromSeconds(throttleTime)); // Throttle nếu cần
                    }

                    return stream;
                }
            )
            .SelectMany(ProcessWrapper);
    }
}