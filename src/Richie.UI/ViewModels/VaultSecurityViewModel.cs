using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Richie.Application.Authentication;
using Richie.Application.Vault;

namespace Richie.UI.ViewModels;

/// <summary>Vault security settings: change the master password and set up security-question recovery.</summary>
public partial class VaultSecurityViewModel : ObservableObject
{
    private readonly IVaultGate _gate;

    // Change master password
    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string? _changeMessage;
    [ObservableProperty] private bool _changeOk;

    // Recovery
    [ObservableProperty] private bool _recoveryEnabled;
    [ObservableProperty] private ObservableCollection<VaultAnswerEntry> _answers = [];
    [ObservableProperty] private string? _recoveryMessage;
    [ObservableProperty] private bool _recoveryOk;

    public VaultSecurityViewModel(IVaultGate gate) => _gate = gate;

    public void Load()
    {
        RecoveryEnabled = _gate.IsRecoveryEnabled();
        Answers = new ObservableCollection<VaultAnswerEntry>(
            _gate.GetRecoveryQuestions().Select(q => new VaultAnswerEntry(q)));
    }

    [RelayCommand]
    private void ChangePassword()
    {
        ChangeMessage = null;
        ChangeOk = false;

        if (NewPassword != ConfirmPassword)
        {
            ChangeMessage = "New password and confirmation don't match.";
            return;
        }

        VaultUnlockResult result = _gate.ChangeMasterPassword(CurrentPassword, NewPassword);
        if (!result.IsSuccess)
        {
            ChangeMessage = result.Message ?? "Could not change the master password.";
            return;
        }

        CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
        ChangeOk = true;
        ChangeMessage = "Master password changed.";
    }

    [RelayCommand]
    private void EnableRecovery()
    {
        RecoveryMessage = null;
        RecoveryOk = false;

        IReadOnlyList<SecurityAnswerInput> answers =
            Answers.Select(a => new SecurityAnswerInput(a.Question, a.Answer)).ToList();

        VaultUnlockResult result = _gate.EnableRecovery(answers);
        if (!result.IsSuccess)
        {
            RecoveryMessage = result.Message ?? "Could not enable recovery.";
            return;
        }

        foreach (VaultAnswerEntry a in Answers)
            a.Answer = string.Empty;
        RecoveryEnabled = true;
        RecoveryOk = true;
        RecoveryMessage = "Security-question recovery is now enabled.";
    }
}
