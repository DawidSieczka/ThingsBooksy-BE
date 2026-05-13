using System;
using System.Threading.Tasks;

namespace ThingsBooksy.Shared.Infrastructure.Postgres;

public interface IUnitOfWork
{
    Task ExecuteAsync(Func<Task> action);
}