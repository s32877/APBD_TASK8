namespace APBD_TASK8.Models;

public partial class Assignment
{
    public bool IsOverdue(DateTime now) => DueDate < now;
}
