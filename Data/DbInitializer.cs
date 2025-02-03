using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Models;
using MatchType = TennisClubRanking.Models.MatchType;

namespace TennisClubRanking.Data
{
    public static class DbInitializer
    {
        private static readonly string[] MaleFirstNames = new[]
        {
            "João", "Pedro", "Lucas", "Miguel", "Arthur", "Davi", "Gabriel", "Bernardo", "Rafael",
            "Guilherme", "Enzo", "Felipe", "Gustavo", "Nicolas", "Henrique", "Samuel", "Theo",
            "Eduardo", "Vitor", "Daniel", "Leonardo", "Bruno", "Thiago", "André", "Carlos",
            "Fernando", "Ricardo", "Marcelo", "Roberto", "Diego"
        };

        private static readonly string[] FemaleFirstNames = new[]
        {
            "Maria", "Ana", "Julia", "Sofia", "Isabella", "Helena", "Valentina", "Laura",
            "Alice", "Manuela", "Beatriz", "Luiza", "Mariana", "Rafaela", "Gabriela",
            "Carolina", "Vitória", "Letícia", "Clara", "Marina", "Fernanda", "Patricia",
            "Camila", "Daniela", "Amanda", "Bruna", "Larissa", "Paula", "Renata", "Vanessa"
        };

        private static readonly string[] LastNames = new[]
        {
            "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira",
            "Lima", "Gomes", "Costa", "Ribeiro", "Martins", "Carvalho", "Almeida", "Lopes",
            "Soares", "Fernandes", "Vieira", "Barbosa", "Rocha", "Dias", "Nascimento",
            "Andrade", "Moreira", "Nunes", "Marques", "Machado", "Mendes", "Freitas"
        };

        public static void Initialize(TennisContext context)
        {
            try
            {
                // Check if we already have data
                if (context.Users.Any() && context.Players.Any() && context.Matches.Any())
                {
                    return; // Database has been seeded
                }

                // Create test user if it doesn't exist
                if (!context.Users.Any(u => u.Email == "test@example.com"))
                {
                    context.Users.Add(new User
                    {
                        Email = "test@example.com",
                        Username = "testuser",
                        PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", // "password123"
                        FirstName = "Test",
                        LastName = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    context.SaveChanges();
                }

                var random = new Random();

                // Generate 30 male players
                var malePlayers = new List<Player>();
                for (int i = 0; i < 30; i++)
                {
                    var firstName = MaleFirstNames[i];
                    var lastName = LastNames[random.Next(LastNames.Length)];
                    var player = new Player
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                        RankingPoints = random.Next(1000, 5000),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    malePlayers.Add(player);
                }
                context.Players.AddRange(malePlayers);

                // Generate 30 female players
                var femalePlayers = new List<Player>();
                for (int i = 0; i < 30; i++)
                {
                    var firstName = FemaleFirstNames[i];
                    var lastName = LastNames[random.Next(LastNames.Length)];
                    var player = new Player
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                        RankingPoints = random.Next(1000, 5000),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    femalePlayers.Add(player);
                }
                context.Players.AddRange(femalePlayers);
                context.SaveChanges();

                // Create some tournaments
                var tournaments = new List<Tournament>();
                var tournamentNames = new[] { "Spring Championship", "Summer Open", "Fall Classic", "Winter Cup" };
                var now = DateTime.UtcNow;

                foreach (var name in tournamentNames)
                {
                    var tournament = new Tournament
                    {
                        Name = name + " " + now.Year,
                        Description = $"Annual {name} tournament",
                        StartDate = now.AddDays(random.Next(1, 60)),
                        Status = TournamentStatus.RegistrationOpen,
                        CreatedAt = now
                    };
                    tournaments.Add(tournament);
                }
                context.Tournaments.AddRange(tournaments);
                context.SaveChanges();

                // Create some matches
                var allPlayers = malePlayers.Concat(femalePlayers).ToList();
                var matches = new List<Match>();

                for (int i = 0; i < 20; i++)
                {
                    var homePlayer = allPlayers[random.Next(allPlayers.Count)];
                    var awayPlayer = allPlayers[random.Next(allPlayers.Count)];
                    while (awayPlayer == homePlayer)
                    {
                        awayPlayer = allPlayers[random.Next(allPlayers.Count)];
                    }

                    var match = new Match
                    {
                        HomePlayerId = homePlayer.Id,
                        AwayPlayerId = awayPlayer.Id,
                        ScheduledDateTime = now.AddDays(random.Next(-30, 30)),
                        Status = MatchStatus.Scheduled,
                        Type = MatchType.Singles,
                        CreatedAt = now
                    };
                    matches.Add(match);
                }
                context.Matches.AddRange(matches);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }
    }
}
