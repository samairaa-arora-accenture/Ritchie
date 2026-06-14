using Richie.Application.Authentication;
using Richie.Domain.Authentication;

namespace Richie.Application.Vault;

/// <summary>
/// Owns the vault's encryption key lifecycle and enforces the re-authentication gate (PRD §8.1):
/// the data-encryption key is held in memory only while the vault is unlocked, and is dropped on
/// <see cref="Lock"/> (called on every navigation away / logout / auto-lock). All field
/// encryption/decryption flows through here so a credential can never be read while locked.
/// Scoped to the currently signed-in user.
/// </summary>
public interface IVaultGate
{
    /// <summary>True once the current user has established a vault master password.</summary>
    bool IsConfigured();

    /// <summary>True while the vault is unlocked for the current user (key held in memory).</summary>
    bool IsUnlocked { get; }

    /// <summary>First-time setup: choose the vault master password. Auto-unlocks on success.</summary>
    VaultUnlockResult SetupMasterPassword(string masterPassword);

    /// <summary>Verify the master password and unlock the vault (holds the key for this access).</summary>
    VaultUnlockResult Unlock(string masterPassword);

    /// <summary>Verify the master password without changing lock state (for reveal/export re-auth).</summary>
    bool Verify(string masterPassword);

    /// <summary>Drop the in-memory key — the next access must re-authenticate.</summary>
    void Lock();

    /// <summary>Change the master password: verifies the current one, then re-wraps the data key
    /// under a key derived from the new password. Cheap (credentials are untouched). Stays unlocked.</summary>
    VaultUnlockResult ChangeMasterPassword(string currentPassword, string newPassword);

    /// <summary>Set a new master password using the already-unlocked key (recovery flow — the user
    /// doesn't know the old password). Requires the vault to be unlocked.</summary>
    VaultUnlockResult SetMasterPassword(string newPassword);

    /// <summary>True once security-question recovery has been enabled for the current user.</summary>
    bool IsRecoveryEnabled();

    /// <summary>The current user's chosen security questions (to present for recovery setup/unlock).</summary>
    IReadOnlyList<SecurityQuestion> GetRecoveryQuestions();

    /// <summary>Enable recovery: verifies the security answers, then wraps the data key under a key
    /// derived from them so the vault can later be unlocked with the answers. Requires unlocked.</summary>
    VaultUnlockResult EnableRecovery(IReadOnlyList<SecurityAnswerInput> answers);

    /// <summary>Unlock the vault by answering the security questions (recovery path).</summary>
    VaultUnlockResult UnlockWithAnswers(IReadOnlyList<SecurityAnswerInput> answers);

    /// <summary>Encrypt a field value with the unlocked vault key. Throws if locked.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypt a field value with the unlocked vault key. Throws if locked.</summary>
    string Decrypt(string cipher);
}
