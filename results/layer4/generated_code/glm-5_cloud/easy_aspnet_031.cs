using System.ComponentModel.DataAnnotations;

public class FileUploadRequest
{
    [Required]
    public string FileName { get; set; }

    [Range(1, 10485760)]
    public long FileSizeBytes { get; set; }

    [Required]
    public string ContentType { get; set; }
}