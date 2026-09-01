namespace SharedKernel.Entities;

public class Team : Base.Entity
{
    public string Name { get; set; } = "";

    public string Country { get; set; } = "";

    public string League { get; set; } = "";
}