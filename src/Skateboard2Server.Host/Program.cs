using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Bedrock.Framework;
using Org.BouncyCastle.Crypto.Tls;

namespace Skateboard2Server.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Setup NLog
            LogManager.Setup().LoadConfigurationFromAppSettings();

            try
            {
                CreateHostBuilder(args).Build().Run();

                //var port = 18040;
                //var localAddr = IPAddress.Parse("127.0.0.1");
                //var server = new TcpListener(localAddr, port);
                //server.Start();

                //Console.WriteLine("Server Started");

                //while (true)
                //{
                //    var client = server.AcceptTcpClient();
                //    Console.WriteLine("Client Connected");
                //    var protocol = new TlsServerProtocol(client.GetStream(),
                //        new Org.BouncyCastle.Security.SecureRandom());
                //    protocol.Accept(new FeslTlsServer());
                //    Console.WriteLine("Ssl handshake done");
                //    client.Close();
                //}

            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        //skate2-ps3 (Fesl)
                        serverOptions.ListenAnyIP(18040,
                            options =>
                            {
                                options.UseSslServer()
                                    .UseConnectionLogging(loggingFormatter: HexLoggingFormatter)
                                    .UseConnectionHandler<DummyConnectionHandler>();
                            });
                        ////skate2-ps3 (Theater) 
                        //serverOptions.ListenAnyIP(18045,
                        //    options =>
                        //    {
                        //        options.UseConnectionLogging(loggingFormatter: HexLoggingFormatter)
                        //        .UseConnectionHandler<DummyConnectionHandler>();
                        //    });
                    })
                        .UseStartup<Startup>();
                });
        }

        private static void HexLoggingFormatter(Microsoft.Extensions.Logging.ILogger logger, string method, ReadOnlySpan<byte> buffer)
        {
            if (!logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
                return;

            var builder = new StringBuilder($"{method}[{buffer.Length}] ");

            // Write the hex
            foreach (var b in buffer)
            {
                builder.Append(b.ToString("X2"));
                builder.Append(" ");
            }

            logger.LogTrace(builder.ToString());
        }
    }
}
