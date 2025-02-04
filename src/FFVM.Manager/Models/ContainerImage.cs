namespace FFVM.Manager.Models;

public class ContainerImage
{
    public DateTime ImagePushedAt { get; set; }
    public List<string> ImageTags { get; set; } = [];
}
