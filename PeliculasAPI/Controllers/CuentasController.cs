using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PeliculasAPI.DTO;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
    [Route("api/cuentas")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userMananger;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IMapper mapper;

        public CuentasController(UserManager<IdentityUser> userMananger , IConfiguration configuration, SignInManager<IdentityUser> signInManager, IMapper mapper)
        {
            this.userMananger = userMananger;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.mapper = mapper;
        }

        [HttpPost("crear")]
        public async Task<ActionResult<RespuestaAutenticacion>> Post([FromBody] CredencialesUsuario credenciales)
        {
            var usuario = new IdentityUser
            {
                UserName = credenciales.email,
                Email = credenciales.email
            };

            var resultado = await userMananger.CreateAsync(usuario, credenciales.password);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credenciales);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login([FromBody] CredencialesUsuario credenciales)
        {
            var resultado = await signInManager.PasswordSignInAsync(credenciales.email, credenciales.password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                //var dto = mapper.Map<CredencialesUsuario>(credenciales);
                //return await ConstruirToken(dto);
                return await ConstruirToken(credenciales);
            }
            else
            {
                return BadRequest("Login Incorrecto");
            }
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credenciales)
        {
            //Creacion de la lista de Claims
            var claims = new List<Claim>()
            {
                new Claim("email", credenciales.email)
            };
            var usuario = await userMananger.FindByEmailAsync(credenciales.email);
            var claimsDB = await userMananger.GetClaimsAsync(usuario);
            claims.AddRange(claimsDB);

            //Creacion del JWT
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(185);
            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracion = expiracion
            };
        }

    }
}
