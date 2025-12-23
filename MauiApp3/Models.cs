using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace MauiApp3;

public class ThingItem : INotifyPropertyChanged
{
    public string Id { get; set; }
    
    private string name;
    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged();
        }
    }

    private bool isChecked;
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            isChecked = value;
            OnPropertyChanged();
        }
    }

    private TextDecorations textDecoration;
    public TextDecorations TextDecoration
    {
        get => textDecoration;
        set
        {
            textDecoration = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ThingsList
{
    public string Id { get; set; }
    public string Name { get; set; }
}

// Новые модели для БД
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("username")]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационное свойство
    public virtual ICollection<SharedList> OwnedLists { get; set; }
    public virtual ICollection<SharedListAccess> SharedAccesses { get; set; }
}

[Table("shared_lists")]
public class SharedList
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("list_id")]
    [MaxLength(50)]
    public string ListId { get; set; }
    
    [Required]
    [Column("list_name")]
    [MaxLength(200)]
    public string ListName { get; set; }
    
    [Required]
    [Column("owner_id")]
    public int OwnerId { get; set; }
    
    [Column("list_data")]
    public string ListData { get; set; } // JSON с items
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    [ForeignKey("OwnerId")]
    public virtual User Owner { get; set; }
    
    public virtual ICollection<SharedListAccess> SharedAccesses { get; set; }
}

[Table("shared_list_access")]
public class SharedListAccess
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("list_id")]
    public int ListId { get; set; }
    
    [Required]
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("access_level")]
    [MaxLength(20)]
    public string AccessLevel { get; set; } = "view"; // view, edit
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    [ForeignKey("ListId")]
    public virtual SharedList SharedList { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}