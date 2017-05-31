﻿using System;

namespace nats_simple_client
{
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Linq;
    class Program
    {
        static void Main(string[] args)
        {
            var opt = NatsConnectOption.CreateDefault();
            opt.verbose = true;
            using (var con = NatsConnection.Create("127.0.0.1", 4222, NatsConnectOption.CreateDefault()))
            {
                const string subject = "natscsharp";
                var sid = con.Subscribe(subject, null);
                var dat = new byte[] { 0x32, 0x32 };
                const int loopCount = 100000;
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Task.WhenAll(
                    Task.Run(() =>
                    {
                        Console.WriteLine($"begin subscription");
                        for (int i = 0; i < loopCount; i++)
                        {
                            while (true)
                            {
                                var ret = con.WaitMessage();
                                if (ret.id == NatsServerMessageId.Msg)
                                {
                                    if (!string.IsNullOrEmpty(ret.msg.Reply))
                                    {
                                        con.Publish(ret.msg.Reply, null, ret.msg.Data.ToArray());
                                    }
                                    break;
                                }
                            }
                        }
                    })
                    ,
                    Task.Run(async () =>
                    {
                        Console.WriteLine($"begin publish");
                        for (int i = 0; i < loopCount; i++)
                        {
                            await Task.Yield();
                            var reply = con.Request(subject, "replyto", dat);
                            if (i % (loopCount / 10) == (loopCount / 10 - 1))
                            {
                                Console.WriteLine($"reply:{i},{sw.Elapsed},{Encoding.UTF8.GetString(reply)}");
                            }
                        }
                    })
                ).Wait();
                Console.WriteLine($"finished: {loopCount},{sw.Elapsed},rps={loopCount * 1000 / sw.ElapsedMilliseconds}");
                // con.WaitMessage().Wait();
                // System.Threading.Thread.Sleep(3000);
            }
            // using (var client = new TcpClient())
            // using (var c2 = new TcpClient())
            // {
            //     var buf = new byte[4096];
            //     client.Connect("127.0.0.1", 4222);
            //     c2.Connect("127.0.0.1", 4222);
            //     using (var stm = client.GetStream())
            //     using (var s2 = c2.GetStream())
            //     {
            //         var bytesread = stm.Read(buf, 0, buf.Length);
            //         Console.WriteLine("svr info:{0}", Encoding.UTF8.GetString(buf, 0, bytesread));
            //         bytesread = s2.Read(buf, 0, buf.Length);
            //         Console.WriteLine("svr info2:{0}", Encoding.UTF8.GetString(buf, 0, bytesread));

            //         var wbuf = Encoding.UTF8.GetBytes("SUB hoge 1\r\n");
            //         s2.Write(wbuf, 0, wbuf.Length);
            //         bytesread = s2.Read(buf, 0, buf.Length);
            //         Console.WriteLine("sub res:{0}", Encoding.UTF8.GetString(buf, 0, bytesread));

            //         wbuf = Encoding.UTF8.GetBytes("PUB hoge 1\r\na\r\n");
            //         stm.Write(wbuf, 0, wbuf.Length);
            //         bytesread = stm.Read(buf, 0, buf.Length);
            //         Console.WriteLine("pub res:{0}", Encoding.UTF8.GetString(buf, 0, bytesread));

            //         bytesread = s2.Read(buf, 0, buf.Length);
            //         Console.WriteLine("sub msg:{0}", Encoding.UTF8.GetString(buf, 0, bytesread));
            //     }
            // }
            Console.WriteLine("Hello World!");
        }
    }
}
