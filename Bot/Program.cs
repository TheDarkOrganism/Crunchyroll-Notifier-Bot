using AppRunner runner = new();

await runner.Run("./Config.json");

await Task.Delay(-1);