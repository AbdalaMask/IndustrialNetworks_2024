using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NetStudio.Common.Security;

public class RSACryptoUtility
{
	public static RSAKey GenerateKey()
	{
		RSAKey rSAKey = new RSAKey();
		using RSA rSA = RSA.Create();
		rSAKey.PublicKey = rSA.ToXmlString(includePrivateParameters: false);
		rSAKey.PrivateKey = rSA.ToXmlString(includePrivateParameters: true);
		return rSAKey;
	}

	public static string Encrypt(RSAObject rsaobj)
	{
		using RSA rSA = RSA.Create();
		rSA.FromXmlString(rsaobj.RSAKey.PublicKey);
		byte[] bytes = Encoding.ASCII.GetBytes(rsaobj.LicenseKey.SerialNumber);
		return Convert.ToBase64String(rSA.Encrypt(bytes, RSAEncryptionPadding.Pkcs1));
	}

	public static string Decrypt(RSAObject rsaobj)
	{
		using RSA rSA = RSA.Create();
		rSA.FromXmlString(rsaobj.RSAKey.PrivateKey);
		byte[] data = Convert.FromBase64String(rsaobj.LicenseKey.Code);
		byte[] bytes = rSA.Decrypt(data, RSAEncryptionPadding.Pkcs1);
		return Encoding.ASCII.GetString(bytes);
	}

	public static byte[] Encrypt(string publicKey, string dataToEncrypt)
	{
		using RSA rSA = RSA.Create();
		rSA.FromXmlString(publicKey);
		byte[] bytes = Encoding.UTF8.GetBytes(dataToEncrypt);
		return rSA.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
	}

	public static string Decrypt(string privateKey, byte[] encryptedBytes)
	{
		using RSA rSA = RSA.Create();
		rSA.FromXmlString(privateKey);
		byte[] bytes = rSA.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
		return Encoding.UTF8.GetString(bytes);
	}

	public static byte[] Sign(string privateKey, string dataToSign)
	{
		using RSA rSA = RSA.Create();
		rSA.FromXmlString(privateKey);
		byte[] bytes = Encoding.UTF8.GetBytes(dataToSign);
		return rSA.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	public static bool Verify(string publicKey, string dataToValidate, byte[] signature)
	{
		using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		rSACryptoServiceProvider.FromXmlString(publicKey);
		byte[] bytes = Encoding.UTF8.GetBytes(dataToValidate);
		return rSACryptoServiceProvider.VerifyData(bytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	public static string EncryptString(string IP, string plainText)
	{
		byte[] iV = new byte[16];
		byte[] inArray;
		using (Aes aes = Aes.Create())
		{
			aes.Key = Encoding.UTF8.GetBytes(IP);
			aes.IV = iV;
			ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
			using MemoryStream memoryStream = new MemoryStream();
			using CryptoStream stream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			using (StreamWriter streamWriter = new StreamWriter(stream))
			{
				streamWriter.Write(plainText);
			}
			inArray = memoryStream.ToArray();
		}
		return Convert.ToBase64String(inArray);
	}

	public static string DecryptString(string IP, string cipherText)
	{
		byte[] iV = new byte[16];
		byte[] buffer = Convert.FromBase64String(cipherText);
		using Aes aes = Aes.Create();
		aes.Key = Encoding.UTF8.GetBytes(IP);
		aes.IV = iV;
		ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		using MemoryStream stream = new MemoryStream(buffer);
		using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}

	public static string GenerateKey(string[] values)
	{
		char[] array = values[0].ToCharArray();
		char[] array2 = values[1].ToCharArray();
		string text = string.Empty;
		for (int i = 0; i < array2.Length; i++)
		{
			text = ((i <= 2 || i >= array.Length) ? (text + (array2[i] + 102).ToString("X2")) : (text + (array2[i] + array[i]).ToString("X2").Substring(1)));
		}
		return text;
	}
}
