using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlokChein.Models
{
    public class Wallet
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public byte[] PublicKey { get; set; }

        private byte[] PrivateKey { get; set; }

        // Конструктор за замовчуванням: автоматично генерує нову пару ключів та адресу
        // Конструктор за замовчуванням: автоматично генерує нову пару ключів та адресу
        public Wallet()
        {
            using (var ecdsa = ECDsa.Create())
            {
                Name = "DefaultWallet";
                PrivateKey = ecdsa.ExportECPrivateKey();
                PublicKey = ecdsa.ExportSubjectPublicKeyInfo();
                Address = Convert.ToHexString(PublicKey);
            }
        }

        // Існуючий конструктор з параметрами
        public Wallet(string name, string address, byte[] publicKey, byte[] privateKey)
        {
            Name = name;
            Address = address;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public byte[] SignTransaction(Transaction transaction)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportECPrivateKey(PrivateKey, out _);
                return ecdsa.SignData(transaction.GetDataToSign(), HashAlgorithmName.SHA256);
            }
        }
    }
}