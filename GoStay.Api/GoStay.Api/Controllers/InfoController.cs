using GoStay.Data.Base;
using GoStay.DataDto.Info;
using GoStay.Services.Info;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

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

        // Pass domain=0 to retrieve all records.
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

        [HttpDelete("{id:int}")]
        public async Task<ResponseBase> Delete(
            [FromRoute] int id, CancellationToken cancellationToken)
        {
            var response = new ResponseBase();
            if (id < 1)
            {
                response.Code = 400;
                response.Message = "Id phải lớn hơn 0.";
                return response;
            }

            try
            {
                var deleted = await _infoService.DeleteAsync(id, cancellationToken);
                if (!deleted)
                {
                    response.Code = 400;
                    response.Message = "Không tìm thấy bản ghi Info với id này.";
                    return response;
                }

                response.Code = 200;
                response.Data = "Success";
                return response;
            }
            catch (SqlException)
            {
                response.Code = 400;
                response.Message = "Không thể xóa bản ghi Info.";
                return response;
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseBase>> Create(
            [FromBody] CreateInfoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _infoService.AddEmailAsync(request.Email, request.Domain, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 2601 || exception.Number == 2627)
            {
                return Conflict(new ResponseBase
                {
                    Ok = false,
                    Code = StatusCodes.Status409Conflict,
                    Message = "Email đã tồn tại trong domain này.",
                    Count = 0,
                    Data = new { Email = request.Email.Trim(), Domain = request.Domain?.Trim() }
                });
            }

            return Ok(new ResponseBase
            {
                Message = "Lưu email thành công.",
                Count = 1,
                Data = new { Email = request.Email.Trim(), Domain = request.Domain?.Trim() }
            });
        }
    }
}
