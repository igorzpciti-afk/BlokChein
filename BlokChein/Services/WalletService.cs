using System;
using System.Security.Cryptography;
using System.Text;
using BlokChein.Models;

namespace BlokChein.Services
{
        public class WalletService
        {
            public Wallet CreateWallet(string name)
            {
                using (var rsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                {
                    var privateKey = rsa.ExportECPrivateKey();
                    var publicKey = rsa.ExportSubjectPublicKeyInfo();
                    var address = Convert.ToBase64String(SHA256.HashData(publicKey)).Substring(0, 10);

                    return new Wallet(name, address, publicKey, privateKey);
                }
            }

            public bool VerifyTransactionSignature(Transaction transaction, byte[] publicKey)
            {
                using (var ecdsa = ECDsa.Create())
                {
                    ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                    return ecdsa.VerifyData(Encoding.UTF8.GetBytes(transaction.ToRawString()), transaction.Signature, HashAlgorithmName.SHA256);
                }
            }
        }
    }
