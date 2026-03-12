namespace Events;

public record PasswordResetRequestedEvent(string Email, string ResetToken) : Event;
