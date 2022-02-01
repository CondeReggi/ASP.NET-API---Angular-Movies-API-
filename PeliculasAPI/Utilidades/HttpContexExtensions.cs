using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Utilidades
{
    public static class HttpContexExtensions
    {
        public async static Task InsertarParametrosPaginacionEnCabecera<T>(this HttpContext httpContext , IQueryable<T> queryable)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));  
            }

            double cantidad = await queryable.CountAsync(); //Cuenta la cantidad de registros de la pagina
            httpContext.Response.Headers.Add("cantidadTotalRegistros" ,  cantidad.ToString());

        }
    }
}
