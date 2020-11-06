using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace CTIClient
{
    public partial class CTIClient: UserControl
    {
        public CTIClient()
        {
            InitializeComponent();
        }

        private void CTIClient_Load(object sender, EventArgs e)
        {

        }

        public string telno { get; set; }               //  電話番号が設定される場所
        public int port { get; set; }                   //  通信使用ポート

        public delegate void EventHandler(EventArgs e);
        //  イベントデリゲートの宣言
        public event EventHandler CustomEvent;

        /**
               * 
               *  クライアント受信モード
               * 
               */
        public async void start()
        {
            while (true)
            {
                //  非同期にループ
                await Task.Delay(1000).ConfigureAwait(false);

                //IPv4とIPv6の全てのIPアドレスをListenする


                System.Net.Sockets.TcpListener listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.IPv6Any, port);

                //IPv6Onlyを0にする
              
                listener.Server.SetSocketOption(
                    System.Net.Sockets.SocketOptionLevel.IPv6,
                    System.Net.Sockets.SocketOptionName.IPv6Only,
                    0);
              
                listener.Server.SetSocketOption(
                    SocketOptionLevel.Socket, 
                    SocketOptionName.ReuseAddress, 
                true);


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

    }
}
