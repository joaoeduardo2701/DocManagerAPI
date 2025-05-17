using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DriveController : ControllerBase
{
    private readonly GoogleDriveService _googleDrive;

    public DriveController(GoogleDriveService googleDrive)
    {
        _googleDrive = googleDrive;
    }

    [HttpGet("listar")]
    public async Task<IActionResult> ListarArquivos([FromQuery] string? nome)
    {
        var arquivos = await _googleDrive.ListarArquivosAsync(nome);
        return Ok(arquivos);
    }
}