using AppRunner runner = new();

await runner.Run("./Config.json", new Channels(), new Data());

await Task.Delay(-1);