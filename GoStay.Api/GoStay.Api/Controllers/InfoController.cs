using GoStay.Data.Base;
using GoStay.DataDto.Info;
using GoStay.Services.Info;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

        [HttpGet]
        public async Task<ActionResult<ResponseBase>> GetByDomain(
            [FromQuery, Required, StringLength(255)] string domain, CancellationToken cancellationToken)
        {
            var items = await _infoService.GetByDomainAsync(domain, cancellationToken);
            return Ok(new ResponseBase
            {
                Count = items.Count,
                Data = items
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseBase>> Create(
            [FromBody] CreateInfoRequest request, CancellationToken cancellationToken)
        {
            await _infoService.AddEmailAsync(request.Email, request.Domain, cancellationToken);

            return Ok(new ResponseBase
            {
                Message = "Lưu email thành công.",
                Count = 1,
                Data = new { Email = request.Email.Trim(), Domain = request.Domain?.Trim() }
            });
        }
    }
}
