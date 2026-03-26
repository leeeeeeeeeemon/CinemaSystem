using CinemaSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CinemaDbContext context)
        {
            context.Database.EnsureCreated();   // создаёт базу, если её нет

            // Если жанры уже есть — выходим
            if (context.Genres.Any()) return;

            // Заполняем справочники
            var genres = new[]
            {
                new Genre { Name = "Фантастика" },
                new Genre { Name = "Драма" },
                new Genre { Name = "Комедия" },
                new Genre { Name = "Боевик" },
                new Genre { Name = "Ужасы" },
                new Genre { Name = "Триллер" },
                new Genre { Name = "Приключения" }
            };
            context.Genres.AddRange(genres);

            var directors = new[]
            {
                new Director { FullName = "Дени Вильнёв" },
                new Director { FullName = "Кристофер Нолан" },
                new Director { FullName = "Грета Гервиг" },
                new Director { FullName = "Джеймс Кэмерон" },
                new Director { FullName = "Квентин Тарантино" }
            };
            context.Directors.AddRange(directors);

            context.SaveChanges();

            // Пример одного фильма для теста
            var duneGenre = context.Genres.First(g => g.Name == "Фантастика");
            var villeneuve = context.Directors.First(d => d.FullName == "Дени Вильнёв");

            var testFilm = new Film
            {
                Title = "Дюна: Часть вторая",
                GenreId = duneGenre.Id,
                DirectorId = villeneuve.Id,
                ReleaseYear = 2024,
                DurationMin = 166,
                Announcement = "Продолжение эпической истории о войне за пустынную планету Арракис."
            };

            context.Films.Add(testFilm);
            context.SaveChanges();
        }
    }
}