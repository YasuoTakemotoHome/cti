using System;
using System.Windows;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;

namespace CTIService
{

    public class CTI
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

        public string ip1 { get; set; }                   //  IPアドレス1
        public string ip2 { get; set; }                   //  IPアドレス2
        public string ip3 { get; set; }                   //  IPアドレス3
        public string ip4 { get; set; }                   //  IPアドレス4
        public string ip5 { get; set; }                   //  IPアドレス5

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

        // Win32APIの GetPrivateProfileString を使う宣言
        [DllImport("KERNEL32.DLL")]
        public static extern uint
          GetPrivateProfileString(string lpAppName,
          string lpKeyName, string lpDefault,
          StringBuilder lpReturnedString, uint nSize,
          string lpFileName);


        [DllImport("Rockey2.dll")] static extern int RY2_Find();

        [DllImport("Rockey2.dll")] static extern int RY2_Open(int mode, Int32 uid, ref Int32 hid);

        [DllImport("Rockey2.dll")] static extern void RY2_Close(int handle);

        [DllImport("Rockey2.dll")] static extern int RY2_GenUID(int handle, ref Int32 uid, String seed, int isProtect);

        [DllImport("Rockey2.dll")] static extern int RY2_Read(int handle, int block_index, StringBuilder buffer512);

        [DllImport("Rockey2.dll")] static extern int RY2_Write(int handle, int block_index, String buffer512);






        //  コールバック変数
        public static Aloha_Callback_t Aloha_Callback;

        public static int CallbackMethod(string port, Aloha_Callback_t Aloha_Callback_t, IntPtr zero)
        {
            return Aloha_SetupPort(port, Aloha_Callback_t, IntPtr.Zero);
        }

        public void sendCTI(int devnum, int event_id, string message, int msg_length, IntPtr userdata)
        {
            //  電話番号プロパティにメッセージを挿入
            this.telno = Mid(message, 10, 19);
            // クライアントに電話番号を送る
             

            if (System.Text.RegularExpressions.Regex.IsMatch(this.telno.Trim(), @"^[0-9]+$"))
            {
                Server.Main(this.telno.Trim(), port, oct1, oct2, oct3, oct4From, oct4To,
                            ip1, ip2, ip3, ip4, ip5);//, clientIP,port);
            }
        }

        // 起動時のメソッド
        public void start()
        {

            String strinfo;
            int ret = 0;
            Int32 handle = 0;
            Int32 hid = 0;
            //Find Rockey2
            /*
            ret = RY2_Find();
            if (ret <= 0)
            {
                MessageBox.Show("Not Find Rockey2!");
                //Console.WriteLine("Not Find Rockey2!");
                Application.Exit();
                return;
            }

            
            //Open Rockey2
            ret = RY2_Open(0, 0, ref hid);
            if (ret != 0)
            {
                strinfo = "Err :" + handle;
                MessageBox.Show(strinfo);
                return;
            }
            handle = ret;
            strinfo = "Rockey2 :" + hid;
            //MessageBox.Show(strinfo);
            //Console.WriteLine(strinfo);

            
            if (hid != 753361036)//Take0123
            {
                MessageBox.Show("ドングルがちがいます。");
                //Console.WriteLine("Not Find Rockey2!");
                Application.Exit();
                return;
            }
            */






            // iniファイル名を決める（実行ファイルが置かれたフォルダと同じ場所）
            string iniFileName = AppDomain.CurrentDomain.BaseDirectory + "CTIService.ini";


            // iniファイルから文字列を取得
            StringBuilder port_ini = new StringBuilder(1024);
            StringBuilder oct1_ini = new StringBuilder(1024);
            StringBuilder oct2_ini = new StringBuilder(1024);
            StringBuilder oct3_ini = new StringBuilder(1024);
            StringBuilder oct4From_ini = new StringBuilder(1024);
            StringBuilder oct4To_ini = new StringBuilder(1024);

            StringBuilder ip1_ini = new StringBuilder(1024);
            StringBuilder ip2_ini = new StringBuilder(1024);
            StringBuilder ip3_ini = new StringBuilder(1024);
            StringBuilder ip4_ini = new StringBuilder(1024);
            StringBuilder ip5_ini = new StringBuilder(1024);

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "PORT",          // キー名    
                "2001",   // 値が取得できなかった場合に返される初期値
                port_ini,             // 格納先
                Convert.ToUInt32(port_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "OCT1",          // キー名    
                "192",   // 値が取得できなかった場合に返される初期値
                oct1_ini,             // 格納先
                Convert.ToUInt32(oct1_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "OCT2",          // キー名    
                "168",   // 値が取得できなかった場合に返される初期値
                oct2_ini,             // 格納先
                Convert.ToUInt32(oct2_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名


            GetPrivateProfileString(
                "SECTION",      // セクション名
                "OCT3",          // キー名    
                "100",   // 値が取得できなかった場合に返される初期値
                oct3_ini,             // 格納先
                Convert.ToUInt32(oct3_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "OCT4From",          // キー名    
                "1",   // 値が取得できなかった場合に返される初期値
                oct4From_ini,             // 格納先
                Convert.ToUInt32(oct4From_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "OCT4To",          // キー名    
                "254",   // 値が取得できなかった場合に返される初期値
                oct4To_ini,             // 格納先
                Convert.ToUInt32(oct4To_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "IP1",          // キー名    
                "",   // 値が取得できなかった場合に返される初期値
                ip1_ini,             // 格納先
                Convert.ToUInt32(ip1_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "IP2",          // キー名    
                "",   // 値が取得できなかった場合に返される初期値
                ip2_ini,             // 格納先
                Convert.ToUInt32(ip2_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "IP3",          // キー名    
                "",   // 値が取得できなかった場合に返される初期値
                ip3_ini,             // 格納先
                Convert.ToUInt32(ip3_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "IP4",          // キー名    
                "",   // 値が取得できなかった場合に返される初期値
                ip4_ini,             // 格納先
                Convert.ToUInt32(ip4_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            GetPrivateProfileString(
                "SECTION",      // セクション名
                "IP5",          // キー名    
                "",   // 値が取得できなかった場合に返される初期値
                ip5_ini,             // 格納先
                Convert.ToUInt32(ip5_ini.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            port = int.Parse(port_ini.ToString());

            oct1 = int.Parse(oct1_ini.ToString());
            oct2 = int.Parse(oct2_ini.ToString());
            oct3 = int.Parse(oct3_ini.ToString());
            oct4From = int.Parse(oct4From_ini.ToString());
            oct4To = int.Parse(oct4To_ini.ToString());

            ip1 = ip1_ini.ToString();
            ip2 = ip2_ini.ToString();
            ip3 = ip3_ini.ToString();
            ip4 = ip4_ini.ToString();
            ip5 = ip5_ini.ToString();



            string[] ports = GetDeviceNames();
            if (ports != null)
            {
                foreach (string port in ports)
                {
                    COMport = "COM" + port;
                    Console.WriteLine(COMport);
                }
            }

            /**
             * 
             *  COMが設定されていればサーバーモードとしても起動する
             * 
             **/
            if (COMport != "" && ports != null)
            {

                Callback cb = CallbackMethod;
                Aloha_Callback = sendCTI;
            
                foreach (string port in ports)
                {
                    COMport = "COM" + port;
                    //Task.Run(() =>
                    //{
                    cb(COMport, Aloha_Callback, IntPtr.Zero);
                    //});
                }
            }
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
        public static void send(string telNo, int port, string ip)
        {


            

            var tcp = new System.Net.Sockets.TcpClient();
            var result = tcp.BeginConnect(ip, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.1));//0.01);//);// 01));
            Console.WriteLine(result.IsCompleted);
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
                        Console.WriteLine(ip);
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
                        byte[] sendBytes = enc.GetBytes(telNo + '\n');
                        //データを送信する
                        ns.Write(sendBytes, 0, sendBytes.Length);
                        Console.WriteLine(telNo + "送信");

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
        }




        public static void Main(string telNo, int port, int oct1, int oct2, int oct3, int oct4From, int oct4To,
                                string ip1, string ip2, string ip3, string ip4, string ip5)
        {

            Console.WriteLine(ip1);
            Console.WriteLine(ip2);
            Console.WriteLine(ip3);
            Console.WriteLine(ip4);
            Console.WriteLine(ip5);

            //System.Net.NetworkInformation.Ping mainPing = null;
            string sendMsg = telNo;
            //何も入力されなかった時は終了
            if (sendMsg == null || sendMsg.Length == 0)
            {
                return;
            }

            string address = string.Empty; // 送信するIPアドレス
                                           // 第4オクテットを1から255までループします



            if (ip1 == "") {
              for (int oct4 = oct4From; oct4 <= oct4To; oct4++)
                {
                   
                    address = string.Format("{0}.{1}.{2}.{3}", oct1, oct2, oct3, oct4);
                    //クライアントのIPアドレス（または、ホスト名）とポート番号
                    string ipOrHost = address;
                    send(sendMsg, port, ipOrHost);
                }

            }
            else
            {
                if (ip1 != "") { send(sendMsg, port, ip1); }
                if (ip2 != "") { send(sendMsg, port, ip2); }
                if (ip3 != "") { send(sendMsg, port, ip3); }
                if (ip4 != "") { send(sendMsg, port, ip4); }
                if (ip5 != "") { send(sendMsg, port, ip5); }
            }
        }
    }
}