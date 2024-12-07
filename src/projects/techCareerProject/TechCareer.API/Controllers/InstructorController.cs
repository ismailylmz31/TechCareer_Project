using Microsoft.AspNetCore.Mvc;
using TechCareer.Models.Dtos.Instructors;
using TechCareer.Models.Entities;
using TechCareer.Service.Abstracts;
using TechCareer.Service.Concretes;

namespace TechCareer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorController(IInstructorService _instructorService) : ControllerBase
    {

        // Tüm Instructorları Listeleme
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instructors = await _instructorService.GetListAsync();
            return Ok(instructors);
        }

        // Sayfalama ile Listeleme
        [HttpGet("paginate")]
        public async Task<IActionResult> GetPaginated()
        {
            var paginatedInstructors = await _instructorService.GetPaginateAsync();
            return Ok(paginatedInstructors);
        }

        // Belirli Bir Instructor Detayı
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var instructor = await _instructorService.GetByIdAsync(id);
            return Ok(instructor);
        }

        // Yeni Instructor Ekleme
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateInstructorRequestDto dto)
        {
            var instructor = await _instructorService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
        }

        // Instructor Güncelleme
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInstructorRequestDto dto)
        {
            var instructor = await _instructorService.UpdateAsync(id, dto);
            return Ok(instructor);
        }

        // Instructor Silme
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _instructorService.DeleteAsync(id);
            return Ok(result);

        }
    }
}