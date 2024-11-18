using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TextUtil
{
    public static string alphabeth = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-0123456789:;<=>?@[\\]^_`/{|}~.";
    
    public static string ToSingleLine(string text)
    {
        string outText = "";
        
        foreach (char letter in text)
        {
            
            if (alphabeth.Contains(letter))
                outText += letter;
            else
                outText += ' ';
        }
        
        outText = Regex.Replace(outText, @"[\s\n\\]+", " ").Trim(' ');

        // check if there are double spaces
        bool prevSpace = false;
        foreach (char c in outText)
        {
            if (c == ' ' && prevSpace)
            {
                throw new Exception("Double Space found");
            }
            if (c == ' ')
            {
                prevSpace = true;
            }
            else
            {
                prevSpace = false;
            }
        }
        return outText;
    }

    public static int CharToInt(char c)
    {
        return alphabeth.IndexOf(c);
    }

    public static char IntToChar(int i)
    {
        return alphabeth[i];
    }

    public static string ReadTextFile(string path)
    {
        return ToSingleLine(File.ReadAllText(path));
    }
}
