using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Security.Claims;

namespace LoginAspNet.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(new { Msg = "Usuário Já logado!" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logar(string username, string password)
        {
            SqlConnection SqlConnection = new SqlConnection(@"Data Source=DESKTOP-NO90F78\SQLEXPRESS;Initial Catalog=usuariosdb;Integrated Security=True");
            await SqlConnection.OpenAsync();

            SqlCommand SqlCommand = SqlConnection.CreateCommand();
            SqlCommand.CommandText = $"SELECT * FROM Usuario WHERE username = '{username}' AND password = '{password}'";

            SqlDataReader reader = SqlCommand.ExecuteReader();

            if(await reader.ReadAsync())
                {
                int usuarioId = reader.GetInt32(0);
                string nome = reader.GetString(1);

                List<Claim> direitosAcesso = new List<Claim>
                { 
                 new Claim(ClaimTypes.NameIdentifier,usuarioId.ToString()),
                 new Claim(ClaimTypes.Name,nome)
                };

                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var userPrincipal = new ClaimsPrincipal(new[] { identity });

                await HttpContext.SignInAsync(userPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false, 
                        ExpiresUtc = DateTime.Now.AddHours(1)
                    });

                    return Json(new { Msg = "Usuário Logado com sucesso!" });
                }

            return Json(new {Msg = "Usuário não encontrado! Verifique suas credenciais!" });
        }

        public async Task<IActionResult> Logout()
        {
            if(User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index","Login");
        }

    }
}
