namespace iNdex.Todo.Mobile.Services;

/// <summary>
/// Schedules local notifications for task reminders.
/// On real devices wire up Plugin.LocalNotification (NuGet) per platform.
/// This stub provides the interface used by the app layer.
/// </summary>
public class NotificationService
{
    public Task ScheduleReminderAsync(Guid taskId, string title, DateTime remindAt)
    {
        // TODO: replace with Plugin.LocalNotification.LocalNotificationCenter.Current.Show(...)
        Console.WriteLine($"[NotificationService] Scheduled reminder for '{title}' at {remindAt:u}");
        return Task.CompletedTask;
    }

    public Task CancelReminderAsync(Guid taskId)
    {
        Console.WriteLine($"[NotificationService] Cancelled reminder for task {taskId}");
        return Task.CompletedTask;
    }
}
