using Microsoft.AspNetCore.Mvc;
using POS.Core.Features.Branch;

namespace POS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly IBranchServices _branchServices;
        public BranchController(IBranchServices branchServices)
        {
            _branchServices = branchServices;
        }
        [HttpPost]
        public async Task<IActionResult> CreateBranch(CreateBranchRequest request)
        {
            var result = await _branchServices.CreateBranchAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Branch Created", Id = result.Value }) : BadRequest(result.Error);
        }

        [HttpGet("merchant/{merchantId}")]
        public async Task<IActionResult> GetBranchesByMerchantId(Guid merchantId)
        {
            var result = await _branchServices.GetBranchesByMerchantIdAsync(merchantId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(Guid id, UpdateBranchRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }
            var result = await _branchServices.UpdateBranchAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Branch Updated" }) : BadRequest(result.Error);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(Guid id)
        {
            var result = await _branchServices.DeleteBranchAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Branch Deleted" }) : BadRequest(result.Error);
        }
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreBranch(Guid id)
        {
            var result = await _branchServices.RestoreBranchAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Branch Restored" }) : BadRequest(result.Error);
        }
    }
}
