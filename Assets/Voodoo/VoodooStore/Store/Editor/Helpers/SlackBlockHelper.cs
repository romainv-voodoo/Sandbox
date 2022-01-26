using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Voodoo.Store
{
    public static class SlackBlockHelper
    {
        public static string BuildContent(IReadOnlyList<string> parts)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append("{");
            sb.Append("\"blocks\": [");

            for (int i = 0; i < parts.Count; i++)
            {
                sb.Append(parts[i]);
                if (i + 1 < parts.Count)
                {
                    sb.Append(",");
                }
            }
            
            sb.Append("]");
            sb.Append("}");

            return sb.ToString();
        }
        public static string Header(string content, string type ="plain_text")
        {
            Header header = new Header()
            {
                type = "header",
                text = new TextEmoji()
                {
                    type = type,
                    text = content,
                    emoji = true
                }
            };
            
            return JsonUtility.ToJson(header);
        }
        public static string Body(string content, string type ="mrkdwn")
        {
            Section markdownSection = new Section()
            {
                type = "section",
                text = new Text()
                {
                    type = type,
                    text = content,
                },
            };
            
            return JsonUtility.ToJson(markdownSection);
        }
        public static string Divider()
        {
            Block markdownSection = new Block()
            {
                type = "divider",
            };
            
            return JsonUtility.ToJson(markdownSection);
        }
        public static string ButtonURL(string url, string text, string buttonText = "Click Me")
        {
            AccessoryBlock accessory = new AccessoryBlock()
            {
                type = "section",
                text = new Text()
                {
                    type = "mrkdwn",
                    text = text,
                },
                accessory = new Accessory() 
                {
                    type = "button",
                    text = new TextEmoji()
                    {
                        type = "plain_text",
                        text = buttonText,
                        emoji = true
                    },
                    value = "click_me_123",
                    url = url,
                    action_id = "button-action"
                }
            };
            
            return JsonUtility.ToJson(accessory);
        }
    }
    
    [Serializable]
    public class Text
    {
        public string type;
        public string text;
    }

    [Serializable]
    public class TextEmoji : Text
    {
        public bool emoji;
    }

    [Serializable]
    public class Accessory
    {
        public string type;
        public TextEmoji text;
        public string value;
        public string url;
        public string action_id;
    }

    [Serializable]
    public class AccessoryBlock : Block
    {
        public Text text;
        public Accessory accessory;
    }

    [Serializable]
    public class Section : Block
    {
        public Text text;
    }

    [Serializable]
    public class Header : Block
    {
        public TextEmoji text;
    }

    [Serializable]
    public class Block
    {
        public string type;
    }
}