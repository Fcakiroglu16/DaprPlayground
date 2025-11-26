namespace DaprPlayground.Events;

public record UserCreatedEvent(
    Guid UserId,
    string UserName,
    string Email,
    DateTime CreatedAt
);
