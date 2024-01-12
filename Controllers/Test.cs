using educlient.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace educlient.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class TestController : ControllerBase
  {
    // GET: Test
    readonly IDbLiteContext data;
    readonly IConfiguration configuration;

    public TestController(IDbLiteContext data, IConfiguration configuration)
    {
      this.data = data;
      this.configuration = configuration;
    }
    
    [HttpGet, Route("list")]
    public object GetSampleTable()
    {
      // var tb = data.Table<DataSample>();
      // var ret = tb.FindAll();
      // tb.Insert(new DataSample { name = "zzzzz"});//add
      // var it = tb.FindById("123");
      // if (it != null)
      // {
      //   it.name = "123";
      //   // tb.Update(it); //edit
      //   // tb.Delete(it.id);//delete
      // }
      // return ret;
      return "";
    }
  }
}
