namespace CruiseShip.UpdateAgent.Models;

public class ContainerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Ports { get; set; } = new();
    public Dictionary<string, string> Labels { get; set; } = new();
    public DateTime Created { get; set; }
    public string State { get; set; } = string.Empty;
}
