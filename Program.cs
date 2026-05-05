
using GenericRepositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VCFFileImport.Contracts;
using VCFFileImport.Exceptions;
using VCFFileImport.Data;
using VCFFileImport.Models.Database;
using VCFFileImport.Repositories;
using VCFFileImport.Services;
using MagellanFileServices.Services;
using MagellanFileServices.Contracts;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

ILogger logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

builder.Services.AddSingleton(builder.Configuration);

builder.Services.AddDbContextFactory<VcfContext>(opt =>
    opt.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddScoped<IGenericRepository<ImportFile, VcfContext,int>, ImportFileRepository> ();
builder.Services.AddScoped<IFileServices, FileServices>();

builder.Services.AddScoped<IVcfServices, VcfServices>();

using IHost host = builder.Build();

IVcfServices? services = host.Services.GetRequiredService<IVcfServices>();

if(services != null)
{
    if(!await services.ProcessFileAsync())
    {
        throw new VcfProcessingException("File processing failed. See errors folder.");
    }
}
else
{
    logger.LogCritical("VcfServices failed to start");
    throw new VcfConfigurationException("VcfServices failed to resolve from the service container");
}
