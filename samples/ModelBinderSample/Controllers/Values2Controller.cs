using Microsoft.AspNetCore.Mvc;
using ModelBinder.Extension.AspNetCore.FromSmartBody;

namespace ModelBinderSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Values2Controller : ControllerBase
    {
        [HttpPost]
        public string Post([FromSmartBody("id")] int dadId, [FromSmartBody] string dadName)
        {
            System.Diagnostics.Debug.WriteLine(dadId);
            System.Diagnostics.Debug.WriteLine(dadName);
            return $"{dadId}:{dadName}";
        }
    }
}
