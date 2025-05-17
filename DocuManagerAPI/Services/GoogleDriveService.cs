using DocManagerAPI.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;

public class GoogleDriveService
{
    private readonly DriveService _driveService;

    public GoogleDriveService(IConfiguration config)
    {
        var credentialPath = Path.Combine(AppContext.BaseDirectory, "credentials", "google-drive-key.json");

        GoogleCredential credential;
        using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.DriveReadonly);
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "DocManagerAPI"
        });
    }

    public async Task<IList<FileDto>> ListarArquivosAsync(string? nomeContem = null, string? folderId = null, string? driveId = null)
    {
        var request = _driveService.Files.List();
        request.Fields = "files(id, name, mimeType, webViewLink)";
        request.Spaces = "drive";
        request.SupportsAllDrives = true;
        request.IncludeItemsFromAllDrives = true;

        // Construção da query
        var queryParts = new List<string>();

        if (!string.IsNullOrEmpty(folderId))
        {
            queryParts.Add($"'{folderId}' in parents");
        }

        if (!string.IsNullOrEmpty(nomeContem))
        {
            queryParts.Add($"name contains '{nomeContem}'");
        }

        // Filtro básico para evitar arquivos na lixeira
        queryParts.Add("trashed = false");

        request.Q = string.Join(" and ", queryParts);

        // Configuração de Drive/Corpora
        if (!string.IsNullOrEmpty(driveId))
        {
            request.Corpora = "drive";
            request.DriveId = driveId;
        }
        else
        {
            request.Corpora = "user";
        }

        var result = await request.ExecuteAsync();

        return result.Files.Select(f => new FileDto
        {
            Id = f.Id,
            Name = f.Name,
            Link = f.WebViewLink ?? $"https://drive.google.com/file/d/{f.Id}/view"
        }).ToList();
    }
}
