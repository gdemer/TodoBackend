using System.ComponentModel.DataAnnotations;

public class Todo
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Text { get; set; } = string.Empty;
    
    public bool Completed { get; set; }
    
    public string DueDate { get; set; } = "Χωρίς προθεσμία";
    
    public string Priority { get; set; } = "Κανονική";
    
    [Required]
    public string Username { get; set; } = string.Empty; // Σύνδεση με τον χρήστη της React
}
