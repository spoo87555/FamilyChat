using System;
using System.Threading.Tasks;

namespace FamilyChat.Domain.Interfaces;

public interface IEncryptionService
{
    // Generate a new RSA key pair for a user
    Task<(string publicKey, string privateKey)> GenerateKeyPairAsync();
    
    // Encrypt a message with a symmetric key (AES)
    Task<(string encryptedContent, string encryptedKey)> EncryptMessageAsync(string content, string recipientPublicKey);
    
    // Decrypt a message using the user's private key
    Task<string> DecryptMessageAsync(string encryptedContent, string encryptedKey, string recipientPrivateKey);
    
    // Encrypt a symmetric key with a recipient's public key
    Task<string> EncryptSymmetricKeyAsync(string symmetricKey, string recipientPublicKey);
    
    // Decrypt a symmetric key with the recipient's private key
    Task<string> DecryptSymmetricKeyAsync(string encryptedSymmetricKey, string recipientPrivateKey);
} 