using CinemaSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CinemaDbContext context)
        {
            // Удаляем существующую базу данных, если нужно начать с чистого листа
            //context.Database.EnsureDeleted(); // Осторожно! Удалит все данные

            // Создаем базу данных и таблицы, если их нет
            context.Database.EnsureCreated();

            // === Кинозалы ===
            if (!context.Halls.Any())
            {
                context.Halls.AddRange(
                    new Hall { HallNumber = 1, Capacity = 150, Description = "Большой зал, Dolby Atmos" },
                    new Hall { HallNumber = 2, Capacity = 120, Description = "Средний зал" },
                    new Hall { HallNumber = 3, Capacity = 80, Description = "Малый VIP-зал" },
                    new Hall { HallNumber = 4, Capacity = 200, Description = "Главный зал" }
                );
                context.SaveChanges();
            }

            // === Жанры ===
            if (!context.Genres.Any())
            {
                context.Genres.AddRange(
                    new Genre { Name = "Фантастика" },
                    new Genre { Name = "Драма" },
                    new Genre { Name = "Комедия" },
                    new Genre { Name = "Боевик" },
                    new Genre { Name = "Ужасы" }
                );
                context.SaveChanges();
            }

            // === Режиссёры ===
            if (!context.Directors.Any())
            {
                context.Directors.AddRange(
                    new Director { FullName = "Дени Вильнёв" },
                    new Director { FullName = "Кристофер Нолан" }
                );
                context.SaveChanges();
            }

            // === Пример фильма ===
            if (!context.Films.Any())
            {
                var genre = context.Genres.FirstOrDefault(g => g.Name == "Фантастика");
                var director = context.Directors.FirstOrDefault();

                if (genre != null && director != null)
                {
                    context.Films.Add(new Film
                    {
                        Title = "Дюна: Часть вторая",
                        GenreId = genre.Id,
                        DirectorId = director.Id,
                        ReleaseYear = 2024,
                        DurationMin = 166,
                        Announcement = "Продолжение эпической саги."
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}