namespace Events;

public record UserCreatedEvent(string UserId, string Email) : Event;
