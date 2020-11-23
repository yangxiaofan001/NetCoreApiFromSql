
using System.Linq;
using System.Threading.Tasks;
using BookStoreApi.Data;
using BookStoreApi.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        public TestController(BookStoreDBContext dbContext, IBookStoreApiRepository repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<ActionResult<Book[]>> GetAction()
        {
            var books = await _repository.GetAllBooksAsync(2);

            return Ok(books);
        }
    }
}