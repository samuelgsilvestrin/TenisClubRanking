using System;
using System.Linq;
using TennisClubRanking.Models;

namespace TennisClubRanking.Data
{
    public static class DbSeeder
    {
        public static void Initialize(TennisContext context)
        {
            if (context.Players.Any())
            {
                return;   // DB has been seeded
            }

            // Add players
            var player1 = new Player
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = Gender.Male,
                PhoneNumber = "123-456-7890",
                RegistrationDate = DateTime.Now,
                IsActive = true,
                RankingPoints = 1200
            };

            var player2 = new Player
            {
                FirstName = "Emma",
                LastName = "Johnson",
                DateOfBirth = new DateTime(1992, 8, 22),
                Gender = Gender.Female,
                PhoneNumber = "234-567-8901",
                RegistrationDate = DateTime.Now,
                IsActive = true,
                RankingPoints = 1150
            };

            var player3 = new Player
            {
                FirstName = "Michael",
                LastName = "Brown",
                DateOfBirth = new DateTime(1988, 3, 10),
                Gender = Gender.Male,
                PhoneNumber = "345-678-9012",
                RegistrationDate = DateTime.Now,
                IsActive = true,
                RankingPoints = 1100
            };

            var player4 = new Player
            {
                FirstName = "Sarah",
                LastName = "Davis",
                DateOfBirth = new DateTime(1995, 11, 30),
                Gender = Gender.Female,
                PhoneNumber = "456-789-0123",
                RegistrationDate = DateTime.Now,
                IsActive = true,
                RankingPoints = 1050
            };

            context.Players.AddRange(player1, player2, player3, player4);
            context.SaveChanges();

            // Add matches
            var match1 = new Match
            {
                HomePlayerId = player1.Id,
                AwayPlayerId = player2.Id,
                ScheduledDateTime = DateTime.Now.AddDays(-7),
                Court = Court.Court1,
                Status = MatchStatus.Completed,
                WinnerId = player1.Id
            };

            var match2 = new Match
            {
                HomePlayerId = player3.Id,
                AwayPlayerId = player4.Id,
                ScheduledDateTime = DateTime.Now.AddDays(-3),
                Court = Court.Court2,
                Status = MatchStatus.Completed,
                WinnerId = player4.Id
            };

            var match3 = new Match
            {
                HomePlayerId = player1.Id,
                AwayPlayerId = player3.Id,
                ScheduledDateTime = DateTime.Now.AddDays(2),
                Court = Court.Court1,
                Status = MatchStatus.Scheduled
            };

            context.Matches.AddRange(match1, match2, match3);
            context.SaveChanges();

            // Add ranking points
            var points1 = new RankingPoints
            {
                PlayerId = player1.Id,
                Points = 10,
                DateEarned = DateTime.Now.AddDays(-7),
                MatchId = match1.Id,
                IsWinnerPoints = true
            };

            var points2 = new RankingPoints
            {
                PlayerId = player2.Id,
                Points = 5,
                DateEarned = DateTime.Now.AddDays(-7),
                MatchId = match1.Id,
                IsWinnerPoints = false
            };

            var points3 = new RankingPoints
            {
                PlayerId = player4.Id,
                Points = 10,
                DateEarned = DateTime.Now.AddDays(-3),
                MatchId = match2.Id,
                IsWinnerPoints = true
            };

            var points4 = new RankingPoints
            {
                PlayerId = player3.Id,
                Points = 5,
                DateEarned = DateTime.Now.AddDays(-3),
                MatchId = match2.Id,
                IsWinnerPoints = false
            };

            context.RankingPoints.AddRange(points1, points2, points3, points4);
            context.SaveChanges();
        }
    }
}
