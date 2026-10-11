using MassTransit;
using IntegrationContracts.Events;

namespace NotificationService.Consumers;

public sealed class TicketAssignedConsumer : IConsumer<TicketAssignedEvent>
{
    public async Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var message = context.Message;

        Console.WriteLine($"[إشعار]: تم إنشاء تذكرة جديدة بنجاح!");
        Console.WriteLine($"رقم التذكرة: {message.TicketId} | العنوان: {message.TicketTitle}");
        Console.WriteLine($"إرسال بريد تأكيدي إلى العميل: {message.CreatorId}");
        Console.WriteLine($"إرسال بريد تأكيدي إلى العميل: {message.AssigneeId}");


        await Task.CompletedTask;
    }
}