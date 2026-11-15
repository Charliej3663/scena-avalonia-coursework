namespace ScenaAvalonia.Models;

public sealed record Course(int Id, string Title, string Description, int Category, int Price, string Teacher, string Image, string[] Schedule);
