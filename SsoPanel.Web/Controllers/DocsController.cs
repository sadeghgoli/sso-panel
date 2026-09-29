using Microsoft.AspNetCore.Mvc;
using SsoPanel.Web.Models;

namespace SsoPanel.Web.Controllers;

public class DocsController : Controller
{
    private readonly IConfiguration _config;

    public DocsController(IConfiguration config)
    {
        _config = config;
    }

    public IActionResult Index() => View(new DocsViewModel
    {
        SsoBaseUrl = (_config["Sso:BaseUrl"] ?? "https://auth.sabzevar.ir").TrimEnd('/')
    });
}
