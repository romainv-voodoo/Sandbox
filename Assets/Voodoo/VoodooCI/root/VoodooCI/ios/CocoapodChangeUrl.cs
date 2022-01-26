using System;
using System.IO;
using System.Text;

public class CocoapodChangeUrl {
    public static void Main() 
    {
        var newLink = "source 'https://cdn.cocoapods.org/'";

        var textFileAddress = $"{Directory.GetCurrentDirectory()}/Builds/ios/Podfile";

        bool firstLine = true;
  
        var newFileText = new StringBuilder();
        
        foreach (string line in System.IO.File.ReadLines(textFileAddress))
        {  
            if(firstLine)
            {
                newFileText.Append($"{newLink}\n");
                firstLine = false;
            }else{
                newFileText.Append($"{line}\n");
            }            
        }  
        
        System.Console.WriteLine(newFileText.ToString()); 
        File.WriteAllText(textFileAddress, newFileText.ToString());
    }
}