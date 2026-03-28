using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using HeroesApi.Models;
using HeroesApi.Data;

namespace HeroesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeroesController : ControllerBase
{

    [HttpGet]
    public ActionResult<List<Hero>> GetAll([FromQuery] string? universe = null)
    {
        if (string.IsNullOrEmpty(universe))
        {
            return Ok(HeroesStore.Heroes);
        }
        
        var filtered = HeroesStore.Heroes
            .Where(h => h.Universe.ToString() == universe)
            .ToList();
        
        return Ok(filtered);
    }

    [HttpGet("{id}")]
    public ActionResult<Hero> GetById(int id)
    {
        var hero = HeroesStore.Heroes.FirstOrDefault(h => h.Id == id);
        
        if (hero == null)
        {
            return NotFound(new { message = $"Герой с ID {id} не найден" });
        }
        
        return Ok(hero);
    }

    [HttpGet("demo")]
    public ActionResult GetDemo()
    {
        var hero = HeroesStore.Heroes.First();
        
        var defaultOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        var ourOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        
        return Ok(new
        {
            withDefaultSettings = JsonSerializer.Deserialize<object>(
                JsonSerializer.Serialize(hero, defaultOptions), defaultOptions),
            withOurSettings = JsonSerializer.Deserialize<object>(
                JsonSerializer.Serialize(hero, ourOptions), ourOptions),
            note = "Сравните имена полей и значение universe в двух вариантах"
        });
    }
    [HttpGet("serialize")]
    public ActionResult GetSerialize()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var hero = new Hero
        {
            Id = 100,
            Name = "Тестовый герой",
            RealName = "Тест Тестович",
            Universe = Universe.Marvel,
            PowerLevel = 50,
            Powers = new() { "тестирование", "отладка" },
            Weapon = new() { Name = "Отладочный пистолет", IsRanged = true },
            InternalNotes = "Это поле не должно попасть в JSON"
        };

        string serialized = JsonSerializer.Serialize(hero, options);

        Hero? deserialized = JsonSerializer.Deserialize<Hero>(serialized, options);

        return Ok(new
        {
            serializedJson = serialized,
            deserializedObject = deserialized,
            internalNotesAfterDeserialize = deserialized?.InternalNotes
        });
    }
    [HttpGet("search")]
    public ActionResult<List<Hero>> Search([FromQuery] string? name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest(new { message = "Параметр name обязателен" });
        }
        
        var found = HeroesStore.Heroes
            .Where(h => h.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        return Ok(found);
    }
}