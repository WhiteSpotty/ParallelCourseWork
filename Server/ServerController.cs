using Microsoft.AspNetCore.Mvc;

namespace Server;

[Route("api/[controller]")]
[ApiController]
public class ServerController : ControllerBase
{
    private readonly InvertedIndex _invertedIndex;

    public ServerController(InvertedIndex invertedIndex)
    {
        _invertedIndex = invertedIndex;
    }
    
    [HttpGet("get/{word}")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
    public IActionResult GetInvertedIndex([FromRoute]string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return BadRequest();
        }
        
        return Ok(_invertedIndex.GetInvertedIndex(word));
    }
}