namespace CargoSim.Infrastructure;

public record RabbitMqSettings(string Host, string Username = "guest", string Password = "guest");