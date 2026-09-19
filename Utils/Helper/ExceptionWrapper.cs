using AutoMapper;
using BackEnd.Utils.Dto;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Utils.Helper
{
    public static class ExceptionWrapper
    {
        public static async Task<IActionResult> Execute(Func<Task<IActionResult>> action, IMapper mapper)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                var error = mapper.Map<ErrorResponse>(ex);
                return new ObjectResult(error)
                {
                    StatusCode = error.StatusCode
                };
            }
        }
    }
}