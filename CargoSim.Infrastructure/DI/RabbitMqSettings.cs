namespace CargoSim.Infrastructure.DI;

public record RabbitMqSettings(string Host, string Username = "guest", string Password = "guest");