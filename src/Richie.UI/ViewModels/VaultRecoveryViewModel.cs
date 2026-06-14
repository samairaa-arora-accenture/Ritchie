using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Authentication;
using Richie.Application.Vault;

namespace Richie.UI.ViewModels;

/// <summary>Forgot-master-password recovery: answer the security questions to unlock, then set a
/// new master password (PRD §8.1 — unlock via security questions).</summary>
public partial class VaultRecoveryViewModel : ObservableObject
{
    private readonly IVaultGate _gate;

    [ObservableProperty] private ObservableCollection<VaultAnswerEntry> _answers = [];
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string? _error;

    public event Action<bool>? CloseRequested;

    public VaultRecoveryViewModel(IVaultGate gate) => _gate = gate;

    public void Load() =>
        Answers = new ObservableCollection<VaultAnswerEntry>(
            _gate.GetRecoveryQuestions().Select(q => new VaultAnswerEntry(q)));

    [RelayCommand]
    private void Recover()
    {
        Error = null;
        if (NewPassword != ConfirmPassword)
        {
            Error = "New password and confirmation don't match.";
            return;
        }

        IReadOnlyList<SecurityAnswerInput> answers =
            Answers.Select(a => new SecurityAnswerInput(a.Question, a.Answer)).ToList();

        if (!_gate.UnlockWithAnswers(answers).IsSuccess)
        {
            Error = "Those answers don't match your security questions.";
            return;
        }

        VaultUnlockResult set = _gate.SetMasterPassword(NewPassword);
        if (!set.IsSuccess)
        {
            Error = set.Message ?? "Could not set the new master password.";
            return;
        }

        CloseRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
