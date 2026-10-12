using System.Windows;
using FqdnFirewallManager.Models;
using FqdnFirewallManager.Services;

namespace FqdnFirewallManager;

public partial class MainWindow : Window
{
    private readonly FirewallService _service = new();
    private List<DynamicKeywordEntry> _entries = new();

    public MainWindow() => InitializeComponent();

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await RefreshAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshAsync();

    private void Clear_Click(object sender, RoutedEventArgs e) => InputBox.Clear();

    private void LoadCurrent_Click(object sender, RoutedEventArgs e) =>
        InputBox.Text = string.Join(Environment.NewLine, _entries.Select(x => x.Keyword).OrderBy(x => x));

    private async void Apply_Click(object sender, RoutedEventArgs e)
    {
        var desired = FirewallService.ParseInput(InputBox.Text);
        if (desired.Count == 0 && RemoveMissingCheck.IsChecked != true)
        {
            Log("Nothing to apply.");
            return;
        }
        if (RemoveMissingCheck.IsChecked == true &&
            MessageBox.Show(this, "Unused keywords not in the list will be removed. Continue?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        await RunBusy(async () =>
        {
            var r = await _service.ApplyAsync(desired, _entries, RemoveMissingCheck.IsChecked == true);
            Log($"Added: {r.Added.Count}  Removed: {r.Removed.Count}  Unchanged: {r.Unchanged.Count}  Rejected: {r.Rejected.Count}");
            foreach (var s in r.Added) Log("  + " + s);
            foreach (var s in r.Removed) Log("  - " + s);
            foreach (var s in r.Rejected) Log("  ! invalid FQDN: " + s);
            foreach (var s in r.Errors) Log("  ERROR " + s);
            await LoadEntriesAsync();
        });
    }

    private async void RemoveSelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = EntriesGrid.SelectedItems.Cast<DynamicKeywordEntry>().ToList();
        if (selected.Count == 0) { Log("Select one or more entries first."); return; }

        var inUse = selected.Where(s => s.UsedByRules.Count > 0).ToList();
        var msg = $"Remove {selected.Count} keyword(s)?";
        if (inUse.Count > 0)
            msg += $"\n\n{inUse.Count} are still used by firewall rules:\n" +
                   string.Join("\n", inUse.Select(i => $"{i.Keyword} → {i.UsedByText}"));
        if (MessageBox.Show(this, msg, "Confirm removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        await RunBusy(async () =>
        {
            foreach (var s in selected)
            {
                try { await _service.RemoveAsync(s.Id); Log("  - " + s.Keyword); }
                catch (Exception ex) { Log($"  ERROR {s.Keyword}: {ex.Message}"); }
            }
            await LoadEntriesAsync();
        });
    }

    private Task RefreshAsync() => RunBusy(LoadEntriesAsync);

    private async Task LoadEntriesAsync()
    {
        _entries = await _service.GetEntriesAsync();
        EntriesGrid.ItemsSource = _entries.OrderBy(x => x.Keyword).ToList();
        Log($"Loaded {_entries.Count} AutoResolve keyword(s).");
    }

    private async Task RunBusy(Func<Task> action)
    {
        SetEnabled(false);
        try { await action(); }
        catch (Exception ex) { Log("ERROR " + ex.Message); }
        finally { SetEnabled(true); }
    }

    private void SetEnabled(bool on)
    {
        ApplyButton.IsEnabled = LoadButton.IsEnabled = RefreshButton.IsEnabled = RemoveButton.IsEnabled = on;
        Cursor = on ? null : System.Windows.Input.Cursors.Wait;
    }

    private void Log(string line)
    {
        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
        LogBox.ScrollToEnd();
    }
}
