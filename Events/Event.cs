namespace Events;

public abstract record Event
{
	public Guid Id { get; set; }

	public Event()
	{
		Id = Guid.NewGuid();
	}
}
