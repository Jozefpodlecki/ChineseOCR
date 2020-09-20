using System;
using System.Collections.Generic;
using System.Text;

namespace Gui.Models
{
    public class OCRSpaceServiceResponse
    {
        public TextOverlay[] ParsedResults { get; set; }

        public class TextOverlay
        {
            public Line[] Lines { get; set; }
            public string ParsedText { get; set; }
            public string Message { get; set; }

            public class Line
            {
                public Word[] Words { get; set; }

                public class Word
                {
                    public string WordText { get; set; }
                }
            }
        }
    }
}
