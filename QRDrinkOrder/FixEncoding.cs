using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding win1252 = Encoding.GetEncoding(1252);
        Encoding utf8 = Encoding.UTF8;

        foreach (string file in args)
        {
            try
            {
                // Read the corrupted UTF-8 file
                string corruptedText = File.ReadAllText(file, utf8);
                
                // Convert the corrupted string back to the bytes that Windows-1252 thought it was
                byte[] originalUtf8Bytes = win1252.GetBytes(corruptedText);
                
                // Decode those bytes as the original UTF-8
                string fixedText = utf8.GetString(originalUtf8Bytes);
                
                // Save it back correctly as UTF-8
                File.WriteAllText(file, fixedText, new UTF8Encoding(false));
                Console.WriteLine("Fixed: " + file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fixing " + file + ": " + ex.Message);
            }
        }
    }
}
