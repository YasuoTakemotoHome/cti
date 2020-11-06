using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace AX300
{
    public partial class ax300 : UserControl
    {


        //  プロパティ
        public string COMport { get; set; }             //  ctiのポート番号(COM3など)
        public int BaudRate { get; set; }
        public string barcodedata { get; set; }               //  電話番号が設定される場所


        public delegate void EventHandler(EventArgs e);
        //  イベントデリゲートの宣言
        public event EventHandler CustomEvent;
        //  イベントデリゲートの宣言
        public event EventHandler HopperEmpty;

        public string ReadData = "";
        public string EOT = ((char)4).ToString();
        public string ENQ = ((char)5).ToString();

        public ax300()
        {
            InitializeComponent();
        }

        private void Ax300_Load(object sender, EventArgs e)
        {

        }

        public void start()
        {
            //! オープンするシリアルポートをコンボボックスから取り出す.
            serialPort1.PortName = COMport;// cmbPortName.SelectedItem.ToString();

            //! ボーレートをコンボボックスから取り出す.
            //BuadRateItem baud = (BuadRateItem)cmbBaudRate.SelectedItem;
            serialPort1.BaudRate = BaudRate;//baud.BAUDRATE;

            //! データビットをセットする. (データビット = 8ビット)
            serialPort1.DataBits = 8;

            //! パリティビットをセットする. (パリティビット = なし)
            serialPort1.Parity = Parity.None;

            //! ストップビットをセットする. (ストップビット = 1ビット)
            serialPort1.StopBits = StopBits.One;

            //! フロー制御をコンボボックスから取り出す.
            //HandShakeItem ctrl = (HandShakeItem)cmbHandShake.SelectedItem;
            serialPort1.Handshake = Handshake.None;//ctrl.HANDSHAKE;

            //! 文字コードをセットする.
            //serialPort1.Encoding = Encoding.Unicode;

            serialPort1.RtsEnable = true;
            // serialPort1.DsrHolding = true;
            //serialPort1.

            try
            {
                //! シリアルポートをオープンする.
                serialPort1.Open();

                //! ボタンの表示を[接続]から[切断]に変える.

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        public void readstart()
        {
            Ordersend(ENQ);
        }


        public void close()
        {
            Ordersend(EOT);
            //! シリアルポートをクローズする.
            serialPort1.Close();
            serialPort1.Dispose();

            //! ボタンの表示を[切断]から[接続]に変える.
        }





        /****************************************************************************/
        /*!
         *  @brief  [送信]ボタンを押して、データ送信を行う.
         *
         *  @param  [in]    sender  イベントの送信元のオブジェクト.
         *  @param  [in]    e       イベント情報.
         *
         *  @retval なし.
         */
        private void Ordersend(string Order)
        {





            //! シリアルポートをオープンしていない場合、処理を行わない.
            if (serialPort1.IsOpen == false)
            {
                return;
            }
            //! テキストボックスから、送信するテキストを取り出す.
            String data = Order;// sndTextBox.Text;

            //! 送信するテキストがない場合、データ送信は行わない.
            if (string.IsNullOrEmpty(data) == true)
            {
                return;
            }

            try
            {
                //! シリアルポートからテキストを送信する.
                serialPort1.Write(data);

                //! 送信データを入力するテキストボックスをクリアする.
                //sndTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /****************************************************************************/
        /*!
         *  @brief  データ受信が発生したときのイベント処理.
         *
         *  @param  [in]    sender  イベントの送信元のオブジェクト.
         *  @param  [in]    e       イベント情報.
         *
         *  @retval なし.
         */
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //! シリアルポートをオープンしていない場合、処理を行わない.
            if (serialPort1.IsOpen == false)
            {
                return;
            }

            try
            {
                //! 受信データを読み込む.
                string data = serialPort1.ReadExisting();

                ReadData += data;
               

                if (data.Contains("\r")) {

                    barcodedata = ReadData;

                    if(barcodedata == "e\r")
                    {
                        Console.WriteLine("bbbbbbb");
                        if (HopperEmpty != null)
                        {
                            Console.WriteLine("aaaaa");
                            HopperEmpty(new EventArgs());
                        }
                    }


                    if (CustomEvent != null)
                    {
                        CustomEvent(new EventArgs());
                    }

                    Console.WriteLine(ReadData);
                    ReadData = "";
                };

                
                //! 受信したデータをテキストボックスに書き込む.
                //Invoke(new Delegate_RcvDataToTextBox(RcvDataToTextBox), new Object[] { data });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
        }
    }
}
