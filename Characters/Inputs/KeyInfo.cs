using System;

namespace Characters.Inputs
{
    public struct KeyInfo
    {
        public char Key;
        public bool IsDown; 
        public int Frame;

        public override string ToString()
        {
            return $"Input:{Key}, IsDown:{IsDown}, Frame:{Frame}";
        }

        public string ToSaveFileString()
        {
            return $"{Key},{IsDown},{Frame}";
        }

        public string ToStringWithEmoji()
        {
            var emojiKey = Key switch
            {
                '1' => "↙️",
                '2' => "⬇",
                '3' => "↘️",
                '4' => "⬅️",
                '5' => "⏺",
                '6' => "➡️",
                '7' => "↖️",
                '8' => "⬆️",
                '9' => "↗️",
                'A' => "👊",
                'J' => "🏃",
                'M' => "💪🏼",
                'L' => "😡",
                'R' => "👍",
                _ => "  "
            };

            var isDownText = IsDown switch
            {
                true => "ON ",
                false => "OFF"
            };

            return $"{emojiKey} {isDownText} F:{Frame}";
        }
        
        public static KeyInfo ConvertTextToKeyInfo(string text)
        {
            var keyParamText = text.Split(',');

            var key = Convert.ToChar(keyParamText[0]);
            var isDown = keyParamText[1] == "True";
            var frame = int.Parse(keyParamText[2]);

            return new KeyInfo {Key = key, IsDown = isDown, Frame = frame};
        }
    
    }
}
