using System.Reflection;
using System.Windows.Forms;

namespace TapeAdhesionApp.UI.Utils;

public static class ControlExtensions
{
    public static void EnableDoubleBuffered(this DataGridView dgv)
    {
        typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(dgv, true, null);
    }
}
