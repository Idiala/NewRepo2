using Microsoft.AspNetCore.Mvc;

public class UserDto
{
    public string Name { get; set; }
    public string NickName { get; set; }
}

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] UserDto user)
    {
        if (user == null)
            return BadRequest("Request body is null");

        return Ok(new { Message = $"Hello {user.Name} aka {user.NickName} from v1" });
    }
}
