using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TennisClubRanking.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultValuesForExistingPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set default values for existing players
            migrationBuilder.Sql(@"
                UPDATE Players 
                SET FirstName = 'Legacy',
                    LastName = 'Player',
                    DateOfBirth = '1990-01-01',
                    Gender = 0,
                    PhoneNumber = 'Not Available'
                WHERE FirstName IS NULL OR FirstName = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to revert as we don't want to lose data
        }
    }
}
