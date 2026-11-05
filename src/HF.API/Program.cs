using HF.Composition.Extensions;
dotenv.net.DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
await builder
    .BuildHF(builder.Environment)
    .Build()
    .UseHF<Program>()
    .RunAsync();
