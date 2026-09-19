using BackEnd.Modules.File.Dto;
using BackEnd.Utils.Dto;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Utils.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace BackEnd.Modules.File
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(FileService fileService, IMapper mapper) : ControllerBase
    {
        private readonly FileService _fileService = fileService;
        private readonly IMapper _mapper = mapper;

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload(
             IFormFile fileUpload,
             [FromHeader(Name = "folder_type")] UploadFolder folder,
             [FromQuery] FileType fileType = FileType.Image
        )
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var fileName = await _fileService.UploadAsync(fileUpload, folder, fileType);

                return Ok(new ApiResponse<FileUploadResponse>
                {
                    StatusCode = 200,
                    Message = "Upload successfully",
                    Result = new FileUploadResponse { FileName = fileName }
                });
            }, _mapper);
        }
    }
}
