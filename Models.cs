using System;

namespace TestePIM
{
    public enum Priority { Invalid = 0, High = 1, Medium = 2, Low = 3 }

    public record AdminSendRequest(string AdminSessionId, string TargetSessionId, string Content);
    public record LoginRequest(string Email, string Password);
    public record MessageRequest(string SessionId, string Content);

    public class SessionState
    {
        public Conversa Conversa { get; set; } = new Conversa();
        public string LastContext { get; set; } = "root";
        public bool IsAdmin { get; set; } = false;
        public string? Email { get; set; }
        public bool IsWaitingHuman { get; set; } = false;
        public Priority Priority { get; set; } = Priority.Invalid;
        public string? TempDetail { get; set; }
        public string? AssignedAgent { get; set; }
        public string? ChamadoTitulo { get; set; }
    }

    public static class Utils
    {
        public static Priority ParsePriority(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Priority.Invalid;
            input = input.Trim().ToLower();
            if (input == "1" || input.Contains("alta") || input.Contains("high")) return Priority.High;
            if (input == "2" || input.Contains("media") || input.Contains("média") || input.Contains("medium")) return Priority.Medium;
            if (input == "3" || input.Contains("baixa") || input.Contains("low")) return Priority.Low;
            return Priority.Invalid;
        }

        public static string PriorityToPortuguese(Priority priority)
        {
            return priority switch
            {
                Priority.High => "Alta",
                Priority.Medium => "Média",
                Priority.Low => "Baixa",
                _ => "Não definida"
            };
        }
    }
}
