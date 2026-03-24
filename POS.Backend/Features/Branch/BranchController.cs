using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Backend.Features.Branch;

namespace POS.Backend.Features.Branch
{
    [Authorize(Roles = "Admin,MerchantAdmin")]
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
            return result.IsSuccess ? Ok(new { Message = "Branch Created", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBranches()
        {
            var result = await _branchServices.GetAllBranchesAsync();
            return result.IsSuccess ? Ok(new { Message = "Branches retrieved successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranchById(Guid id)
        {
            var result = await _branchServices.GetBranchByIdAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Branch retrieved successfully", Data = result.Value }) : NotFound(new { Message = result.Error });
        }

        [HttpGet("merchant/{merchantId}")]
        public async Task<IActionResult> GetBranchesByMerchantId(Guid merchantId)
        {
            var result = await _branchServices.GetBranchesByMerchantIdAsync(merchantId);
            return result.IsSuccess ? Ok(new { Message = "Branches retrieved successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(Guid id, UpdateBranchRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { Message = "ID mismatch" });
            }
            var result = await _branchServices.UpdateBranchAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Branch Updated" }) : BadRequest(new { Message = result.Error });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(Guid id)
        {
            var result = await _branchServices.DeleteBranchAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Branch Deleted" }) : BadRequest(new { Message = result.Error });
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreBranch(Guid id)
        {
            var result = await _branchServices.RestoreBranchAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Branch Restored" }) : BadRequest(new { Message = result.Error });
        }
    }
}
