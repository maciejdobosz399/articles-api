namespace Events;

public record UserCreatedEvent(string Email) : Event { }
