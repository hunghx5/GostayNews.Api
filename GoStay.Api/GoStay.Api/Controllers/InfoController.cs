using GoStay.Data.Base;
using GoStay.DataDto.Info;
using GoStay.Services.Info;
using Microsoft.AspNetCore.Mvc;

namespace GoStay.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IInfoService _infoService;

        public InfoController(IInfoService infoService)
        {
            _infoService = infoService;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseBase>> Create(
            [FromBody] CreateInfoRequest request, CancellationToken cancellationToken)
        {
            await _infoService.AddEmailAsync(request.Email, cancellationToken);

            return Ok(new ResponseBase
            {
                Message = "Lưu email thành công.",
                Count = 1,
                Data = new { Email = request.Email.Trim() }
            });
        }
    }
}
