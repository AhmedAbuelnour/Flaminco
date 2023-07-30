using Flaminco.ManualMapper.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace FlamincoWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IManualMapper _mapper;
        public PersonController(IManualMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        public PersonResponse Get()
        {
            return _mapper.Map<Person, PersonResponse>(new Person
            {
                Age = 1,
                FirstName = "Ahmed",
                LastName = "Abuelnour"
            });
        }
    }
}