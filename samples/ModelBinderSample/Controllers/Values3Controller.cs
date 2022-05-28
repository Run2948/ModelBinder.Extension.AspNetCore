using Microsoft.AspNetCore.Mvc;
using ModelBinder.Extension.AspNetCore.FromJsonBody;
using ModelBinder.Extension.AspNetCore.FromSmartBody;
using System.Collections.Generic;

namespace ModelBinderSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Values3Controller : ControllerBase
    {
        [HttpGet]
        public string Get([FromSmartBody("dadIds")] int[] ids, [FromSmartBody] IEnumerable<string> hobbies)
        {
            System.Diagnostics.Debug.WriteLine(ids);
            System.Diagnostics.Debug.WriteLine(hobbies);
            return $"{string.Join(",", ids)}，爱好：{string.Join("、",hobbies)}";
        }
    }
}
