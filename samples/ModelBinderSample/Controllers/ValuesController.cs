using Microsoft.AspNetCore.Mvc;
using ModelBinder.Extension.AspNetCore.FromJsonBody;

namespace ModelBinderSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [HttpPost]
        public int Post([FromJsonBody("i1")] int i3, [FromJsonBody] int i2, [FromJsonBody("author.age")] int aAge, [FromJsonBody("author.father.name")] string dadName)
        {
            System.Diagnostics.Debug.WriteLine(aAge);
            System.Diagnostics.Debug.WriteLine(dadName);
            return i3 + i2 + aAge;
        }
    }
}
