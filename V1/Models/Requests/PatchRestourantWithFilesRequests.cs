using Microsoft.AspNetCore.JsonPatch;

namespace MiniApp1Api.V1.Models.Requests;

public class PatchRestourantWithFilesRequests
{
    public PatchRestourantRequest PatchData { get; set; }

    public List<IFormFile> Files { get; set; }
}