using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;
using System.Text.RegularExpressions;

namespace CTIForm
{
    public partial class CTI : UserControl
    {
        //  プロパティ
        public string COMport { get; set; }             //  ctiのポート番号(COM3など)
        public string telno { get; set; }               //  電話番号が設定される場所
        public int port { get; set; }                   //  通信使用ポート

        public int oct1 { get; set; }                   //  第1オクテッド
        public int oct2 { get; set; }                   //  第2オクテッド
        public int oct3 { get; set; }                   //  第3オクテッド
        public int oct4From { get; set; }               //  第4オクテッドFROM
        public int oct4To { get; set; }                 //  第4オクテッドTO

        //  デリゲートの宣言
        public delegate void Aloha_Callback_t(int devnum, int event_id, string message, int msg_length, IntPtr userdata);
        public delegate int Callback(string port, Aloha_Callback_t Aloha_Callback_t, IntPtr userdata);
        public delegate void EventHandler(EventArgs e);
        //  イベントデリゲートの宣言
        public event EventHandler CustomEvent;


        //  接続 
        [DllImport("AlohaDLL.dll", EntryPoint = "Aloha_SetupPort")]
        public static extern int Aloha_SetupPort(string com, Aloha_Callback_t Aloha_Callback_t, IntPtr zero);

        //  切断
        [DllImport("AlohaDLL.dll", EntryPoint = "Aloha_TearDown")]
        public static extern int Aloha_TearDown(int kikino);



 






        //  コールバック変数
        public static Aloha_Callback_t Aloha_Callback;

        public CTI()
        {
            InitializeComponent();
        }


        public static int CallbackMethod(string port, Aloha_Callback_t Aloha_Callback_t, IntPtr zero)
        {
            return Aloha_SetupPort(port, Aloha_Callback_t, IntPtr.Zero);
        }

        public void sendCTI(int devnum, int event_id, string message, int msg_length, IntPtr userdata)
        {
            //  電話番号プロパティにメッセージを挿入
            this.telno = Mid(message,10,19);
            //Console.WriteLine(message + "AAAA" + msg_length + "BBBB" + devnum + "CCCC" + event_id);

            /*

            if (CustomEvent != null)
            {
                CustomEvent(new EventArgs());
            }
            */
            // クライアントに電話番号を送る
            
            //Task.Run(() =>
            //{                 

            if (System.Text.RegularExpressions.Regex.IsMatch(this.telno.Trim(), @"^[0-9]+$"))
            {
                Server.Main(this.telno.Trim(), port, oct1, oct2, oct3, oct4From, oct4To);//, clientIP,port);
            }

               
            //});

        }

        // 起動時のメソッド
        public void start()
        {



            string[] ports = GetDeviceNames();
            if (ports != null)
            {
                foreach (string port in ports)
                {
                    COMport = "COM" + port;
                    Console.WriteLine(COMport);
                }
            }







            // パソコンのIPアドレスがsendIPプロパティと同じならばサーバーモードとして動作する
            //if (Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString() == serverIP)

            /**
             * 
             *  COMが設定されていればサーバーモードとしても起動する
             * 
             **/
            if (COMport != ""  && ports != null)
            {
                
                Callback cb = CallbackMethod;
                Aloha_Callback = sendCTI;
                //cb(this.COMport, Aloha_Callback, IntPtr.Zero);

                // 受信側
                //Main();

                foreach (string port in ports)
                {
                    COMport = "COM" + port;
                    //Task.Run(() =>
                    //{
                        cb(COMport, Aloha_Callback, IntPtr.Zero);
                    //});
                }







            }

                //Console.WriteLine(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                // 受信側
                Main();


        }
        
        // 終了時のメソッド
        public void close()
        {

            Aloha_TearDown(0);
        }


        public static string[] GetDeviceNames()
        {
            var deviceNameList = new System.Collections.ArrayList();
            var check = new System.Text.RegularExpressions.Regex("Prolific USB-to-Serial Comm Port");// ("(COM[1-9][0-9]?[0-9]?)");

            ManagementClass mcPnPEntity = new ManagementClass("Win32_PnPEntity");
            ManagementObjectCollection manageObjCol = mcPnPEntity.GetInstances();

            //全てのPnPデバイスを探索しシリアル通信が行われるデバイスを随時追加する
            foreach (ManagementObject manageObj in manageObjCol)
            {
                //Nameプロパティを取得
                var namePropertyValue = manageObj.GetPropertyValue("Name");
                if (namePropertyValue == null)
                {
                    continue;
                }

                //Nameプロパティ文字列の一部が"(COM1)～(COM999)"と一致するときリストに追加"
                string name = namePropertyValue.ToString();
                if (check.IsMatch(name))
                {
                    //string str = Regex.Replace (gameObject.name, @"[^0-9]", "");
                    deviceNameList.Add(Regex.Replace(name, @"[^0-9]", ""));
                }
            }

            //戻り値作成
            if (deviceNameList.Count > 0)
            {
                string[] deviceNames = new string[deviceNameList.Count];
                int index = 0;
                foreach (var name in deviceNameList)
                {
                    deviceNames[index++] = name.ToString();
                }
                return deviceNames;
            }
            else
            {
                return null;
            }
        }






















        /**
         * 
         *  クライアント受信モード
         * 
         */
        public async void Main()
        {
            while (true)
            {
                //  非同期にループ
                await Task.Delay(1000).ConfigureAwait(false);

                //IPv4とIPv6の全てのIPアドレスをListenする
                System.Net.Sockets.TcpListener listener =
                new System.Net.Sockets.TcpListener(System.Net.IPAddress.IPv6Any, port);
                //IPv6Onlyを0にする
                
                listener.Server.SetSocketOption(
                    System.Net.Sockets.SocketOptionLevel.IPv6,
                    System.Net.Sockets.SocketOptionName.IPv6Only,
                    0);



                //listener.Server.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, true);
                //Listenを開始する

                listener.Start();

                //接続要求があったら受け入れる
                System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();

                    Console.WriteLine("IPアドレス:{0} ポート番号:{1})。",
                        ((System.Net.IPEndPoint)client.Client.LocalEndPoint).Address,
                        ((System.Net.IPEndPoint)client.Client.LocalEndPoint).Port);

                    Console.WriteLine("Listenを開始しました({0}:{1})。",
                        ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
                        ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);
                    //NetworkStreamを取得
                    System.Net.Sockets.NetworkStream ns = client.GetStream();



                    //読み取り、書き込みのタイムアウトを10秒にする
                    //デフォルトはInfiniteで、タイムアウトしない
                    //(.NET Framework 2.0以上が必要)
                    ns.ReadTimeout = 10000;
                    ns.WriteTimeout = 10000;

                    //サーバーから送られたデータを受信する
                    System.Text.Encoding enc = System.Text.Encoding.UTF8;
                    //bool disconnected = false;
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    byte[] resBytes = new byte[12];
                    int resSize = 0;
                    do
                    {
                        //データの一部を受信する
                        resSize = ns.Read(resBytes, 0, resBytes.Length);
                        //Readが0を返した時はクライアントが切断したと判断
                        if (resSize == 0)
                        {
                            //disconnected = true;
                            Console.WriteLine("クライアントが切断しました。");
                            break;
                        }
                        //受信したデータを蓄積する
                        ms.Write(resBytes, 0, resSize);
                        //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                        // 受信を続ける
                    } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');

                    //受信したデータを文字列に変換
                    string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                    ms.Close();
                    //末尾の\nを削除
                    resMsg = resMsg.TrimEnd('\n');

                    Console.WriteLine(resMsg + "受信");

                    //閉じる
                    ns.Close();
                    client.Close();
                    client.Dispose();
                    Console.WriteLine("クライアントとの接続を閉じました。");



                    //  カスタムイベントを着火する
                    this.telno = resMsg;
                    Console.WriteLine("着火■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
                    if (CustomEvent != null)
                    {
                    
                        CustomEvent(new EventArgs());
                    }

                
                //リスナを閉じる
                listener.Stop();
                Console.WriteLine("Listenerを閉じました。");

            }
        }

        private void CTI_Load(object sender, EventArgs e)
        {



        }

        /**
         * 
         * MID関数
         * 
         */
        public static string Mid(string str, int start, int len)
        {
            if (start <= 0)
            {
                throw new ArgumentException("引数'start'は1以上でなければなりません。");
            }
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null || str.Length < start)
            {
                return "";
            }
            if (str.Length < (start + len))
            {
                return str.Substring(start - 1);
            }
            return str.Substring(start - 1, len);
        }
    }



    /**
     * 
     *  データ送信
     * 
     */
    public class Server
    {
        public static void Main(string telNo, int port, int oct1, int oct2, int oct3, int oct4From, int oct4To)//,string clientIP,int port)
        {
            //System.Net.NetworkInformation.Ping mainPing = null;
            string sendMsg = telNo;
            //何も入力されなかった時は終了
            if (sendMsg == null || sendMsg.Length == 0)
            {
                return;
            }

            /*
            int oct1 = 192;     // IPアドレスの第1オクテット
            int oct2 = 168;     // IPアドレスの第2オクテット
            int oct3 = 1;       // IPアドレスの第3オクテット

            string address = string.Empty; // 送信するIPアドレス
                                           // 第4オクテットを1から255までループします
            for (int oct4 = 1; oct4 <= 255; oct4++)
            {
                address = string.Format("{0}.{1}.{2}.{3}", oct1, oct2, oct3, oct4);
                System.Net.NetworkInformation.Ping sender = new System.Net.NetworkInformation.Ping();
                // タイムアウト時間50msecで送信します

                //Pingのオプションを設定
                //TTLを64、フラグメンテーションを無効にする
                System.Net.NetworkInformation.PingOptions opts =
                    new System.Net.NetworkInformation.PingOptions(64, true);
                //Pingで送信する32バイトのデータを作成
                byte[] bs = System.Text.Encoding.ASCII.GetBytes(new string('A', 32));


                System.Net.NetworkInformation.PingReply reply = sender.Send(address, 1,bs,opts);
                 //   mainPing.SendAsync("www.yahoo.com", 10000, bs, opts, null);
                
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    Console.WriteLine("Reply from {0}: bytes={1} time={2}ms TTL={3}",
                        reply.Address, reply.Buffer.Length,
                        reply.RoundtripTime, reply.Options.Ttl);
                }
                else
                {
                    //Console.WriteLine("No Reply from {0} Status={1}", address, reply.Status);
                }
                
            }

            Console.WriteLine("\nPing.Send Ended.");

            */












            //CTI cti = new CTIForm.CTI();

            /*
            int oct1 = cti.oct1;     // IPアドレスの第1オクテット
            int oct2 = cti.oct2;     // IPアドレスの第2オクテット
            int oct3 = cti.oct3;       // IPアドレスの第3オクテット
            */
            string address = string.Empty; // 送信するIPアドレス
                                           // 第4オクテットを1から255までループします

            for (int oct4 = oct4From; oct4 <= oct4To; oct4++)
            {
            // 並列だと同時着火しなくなる
            //Parallel.For(oct4From, oct4To + 1, oct4 => {


                address = string.Format("{0}.{1}.{2}.{3}", oct1, oct2, oct3, oct4);
                //Console.WriteLine(address);
                //クライアントのIPアドレス（または、ホスト名）とポート番号
                string ipOrHost = address;//clientIP;
                                          //string ipOrHost = "localhost";
                                          //int port = 2001;

                //TcpClientを作成し、サーバーと接続する
                //System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient(ipOrHost, port);
                /*Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address,
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port,
                    ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address,
                    ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port);
                    */


                var tcp = new System.Net.Sockets.TcpClient();
                var result = tcp.BeginConnect(ipOrHost, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.1));//0.01);//);// 01));

                try
                {
                    if (result.IsCompleted)
                    {
                        if (!success)
                        {
                            throw new Exception("Failed to connect.");
                        }
                        else
                        {
                            Console.WriteLine("CATCH★★★★★★★★★★★★★★★★★★★★★★★★★★");
                            Console.WriteLine(address);
                            Console.WriteLine("CATCH★★★★★★★★★★★★★★★★★★★★★★★★★★");
                            //Console.WriteLine(address);

                            //NetworkStreamを取得する
                            System.Net.Sockets.NetworkStream ns = tcp.GetStream();


                            //読み取り、書き込みのタイムアウトを10秒にする
                            //デフォルトはInfiniteで、タイムアウトしない
                            //(.NET Framework 2.0以上が必要)
                            //ns.ReadTimeout = 1000;
                            //ns.WriteTimeout = 1000;

                            //サーバーにデータを送信する
                            //文字列をByte型配列に変換
                            System.Text.Encoding enc = System.Text.Encoding.UTF8;
                            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
                            //データを送信する
                            ns.Write(sendBytes, 0, sendBytes.Length);
                            Console.WriteLine(sendMsg + "送信");
                            /*
                            //サーバーから送られたデータを受信する
                            System.IO.MemoryStream ms = new System.IO.MemoryStream();
                            byte[] resBytes = new byte[256];
                            int resSize = 0;
                            do
                            {
                                //データの一部を受信する
                                resSize = ns.Read(resBytes, 0, resBytes.Length);
                                //Readが0を返した時はサーバーが切断したと判断
                                if (resSize == 0)
                                {
                                    Console.WriteLine("サーバーが切断しました。");
                                    break;
                                }
                                //受信したデータを蓄積する
                                ms.Write(resBytes, 0, resSize);
                                //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                                // 受信を続ける
                            } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                            //受信したデータを文字列に変換
                            string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                            ms.Close();
                            //末尾の\nを削除
                            resMsg = resMsg.TrimEnd('\n');
                            Console.WriteLine(resMsg);
                            */
                            //閉じる
                            ns.Close();
                            // we have connected
                            tcp.EndConnect(result);

                            //tcp.Close();
                            Console.WriteLine("切断しました。");

                            Console.ReadLine();

                        }
                    }

                }
                catch (Exception e)
                {

                }

               
            tcp.Dispose();
            }// using
            //}); //Parallel.For


        }
    }


}