using System;
using System.IO;
using System.Text;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace Skateboard2Server.Host
{
    public class FeslTlsServer : DefaultTlsServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //TODO: bad
        private string _certificate = @"";

        private string _privateKey = @"";

        protected override TlsEncryptionCredentials GetRsaEncryptionCredentials()
        {
            var privatePemReader = new PemReader(new StringReader(_privateKey));
            //var privateKey = (AsymmetricKeyParameter)privatePemReader.ReadObject();
            var privateKey = (AsymmetricCipherKeyPair)privatePemReader.ReadObject();

            var certPemReader = new PemReader(new StringReader(_certificate));
            var cert = (X509Certificate)certPemReader.ReadObject();

            return new DefaultTlsEncryptionCredentials(mContext, new Certificate(new[] {cert.CertificateStructure}), privateKey.Private);
        }

        protected override int[] GetCipherSuites() => new[] { CipherSuite.TLS_RSA_WITH_RC4_128_SHA, CipherSuite.TLS_RSA_WITH_RC4_128_MD5 }; //Ciphers sent by ps3
        protected override ProtocolVersion MaximumVersion => ProtocolVersion.SSLv3; //Force SSLv3
        protected override ProtocolVersion MinimumVersion => ProtocolVersion.SSLv3; //Force SSLv3
        public override bool ShouldUseGmtUnixTime() => true; //Send correct time in random of server hello

        public override void NotifySecureRenegotiation(bool secureRenegotiation) { } //Prevent AbstractTlsPeer from throwing exception

        public override void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
        {
            base.NotifyAlertRaised(alertLevel, alertDescription, message, cause);
            var sb = new StringBuilder();
            sb.AppendLine($"AlertLevel: {alertLevel}");
            sb.AppendLine($"AlertDescription: {alertDescription}");
            sb.AppendLine($"Message: {message}");
            sb.AppendLine($"Exception: {cause}");
            Logger.Error($"Exception in BlazeTlsServer: {sb}");
        }
    }
}

