using dotenv.net;
using HF.Composition.Extensions;

DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 5));

var builder = WebApplication.CreateBuilder(args);
await builder
    .BuildHF(builder.Environment)
    .Build()
    .UseHF<Program>()
    .RunAsync();
