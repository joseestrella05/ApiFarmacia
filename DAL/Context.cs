using ApiFarmacia.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiFarmacia.DAL;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Productos> Productos { get; set; }

}
