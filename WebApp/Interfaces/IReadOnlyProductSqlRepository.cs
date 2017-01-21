using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Models;

namespace WebApp.Interfaces
{
    //ne koristi se
    public interface IReadOnlyProductSqlRepository
    {
        Task<List<Product>> GetAllAsync();

        Task<int> GetProductQuantityAsync(Guid productId);

        List<Product> GetFiltered(Func<Product, bool> filterFunction);
    }
}
