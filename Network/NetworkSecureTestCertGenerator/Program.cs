using Network.Secure;
using System;
using System.IO;
using System.Reflection;

namespace NetworkSecureTestCertGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var certificate = CertificateGenerator.Create(Environment.MachineName, Environment.MachineName);
            var content = certificate.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx, "psw");
            File.WriteAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + "certificate.pfx", content);
        }
    }
}