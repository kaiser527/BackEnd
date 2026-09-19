using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.History
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController(HistoryService historyService, IMapper mapper) : ControllerBase
    {
        private readonly HistoryService _historyService = historyService;
        private readonly IMapper _mapper = mapper;
    }
}
