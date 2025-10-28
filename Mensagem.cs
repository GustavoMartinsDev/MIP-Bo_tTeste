using System;

namespace TestePIM
{
    public record Mensagem
    {
        public required string Remetente { get; init; }
        public required string Conteudo { get; init; }
        public string? PrioridadeMsg { get; init; }
        public DateOnly Data { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}