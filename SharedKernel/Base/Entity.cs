namespace SharedKernel.Base;

public abstract class Entity
{
    public Guid Id { get; set; }

    public DateTime CreatedUtc { get; set; }

    protected Entity()
    {
        Id = Guid.NewGuid();

        CreatedUtc = DateTime.UtcNow;
    }

 
}