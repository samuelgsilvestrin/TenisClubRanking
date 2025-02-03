using Microsoft.AspNetCore.Mvc;

namespace TennisClubRanking.ViewComponents
{
    public class UserMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            var model = new UserMenuViewModel
            {
                IsAuthenticated = userId.HasValue && !string.IsNullOrEmpty(username),
                Username = username
            };

            return View(model);
        }
    }

    public class UserMenuViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? Username { get; set; }
    }
}
