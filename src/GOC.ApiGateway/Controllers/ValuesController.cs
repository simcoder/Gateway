using System;
using System.Net.Http;
using System.Threading.Tasks;
using EasyNetQ;
using GOC.ApiGateway.Interfaces;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOC.ApiGateway.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    public class ValuesController : Controller
    {
        private readonly IInventoryService _service;

        public ValuesController(IInventoryService service, IBus bus, IGocHttpBasicClient http)
        {
            _service = service;
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            //var token = Request.Headers["authorization"].ToString();
            //token = token.Replace("Bearer",string.Empty);
            //token = token.Replace(" ",string.Empty);
            //var delegateToke = await DelegateAsync(token);
            //var client = new HttpClient();
            //client.SetBearerToken(delegateToke.AccessToken);
            //var response = new HttpResponseMessage();
            //response = await client.GetAsync(new Uri("http://vagrant:5002/api/values"));
            //if(response.IsSuccessStatusCode)
            //{
            //   return Ok(response.Content.ReadAsStringAsync());
            // }
            //return Ok(new string[] { response.StatusCode.ToString(), "value4" });
            return Ok("hahax");
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
