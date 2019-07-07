// Name:Draw Lines At Tile Points
// Submenu:Chris
// Author:
// Title:Draw Lines At Tile Points
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 26; // [1,200] Line Width
ColorWheelControl Amount2 = ColorBgra.FromBgr(255,255,0); // [Aqua] Line Color
DoubleSliderControl Amount3 = 0.33; // [0,1] X Division 1
DoubleSliderControl Amount4 = 0.66; // [0,1] X Division 2
DoubleSliderControl Amount5 = 1; // [0,1] X Division 3
DoubleSliderControl Amount6 = 1; // [0,1] X Division 4
DoubleSliderControl Amount7 = 1; // [0,1] X Division 5
DoubleSliderControl Amount8 = 1; // [0,1] X Division 6
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var plusMinus = (float)Amount1 / 2 / (float)src.Width;
    
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            var normalized = (float)x / (float)src.Width;
            var normalizedPrevious = ((float)x-1) / (float)src.Width;
            var normalizedNext = ((float)x+1) / (float)src.Width;
            
            var comparisonAmount = 0d;
            
            if (Amount3 != 1 && normalized > (Amount3 - plusMinus) && normalized < (Amount3 + plusMinus))
            {
                comparisonAmount = Amount3;
            }
            else if (Amount4 != 1 && normalized > (Amount4 - plusMinus) && normalized < (Amount4 + plusMinus))
            {
                comparisonAmount = Amount4;
            }
            else if (Amount5 != 1 && normalized > (Amount5 - plusMinus) && normalized < (Amount5 + plusMinus))
            {
                comparisonAmount = Amount5;
            }
            else if (Amount6 != 1 && normalized > (Amount6- plusMinus) && normalized < (Amount6 + plusMinus))
            {
                comparisonAmount = Amount6;
            }
            else if (Amount7 != 1 && normalized > (Amount7 - plusMinus) && normalized < (Amount7 + plusMinus))
            {
                comparisonAmount = Amount7;
            }
            else if (Amount8 != 1 && normalized > (Amount8 - plusMinus) && normalized < (Amount8 + plusMinus))
            {
                comparisonAmount = Amount8;
            }
            
            if (comparisonAmount != 0)
            {
                if (normalized == comparisonAmount || (normalized > comparisonAmount && normalizedPrevious < comparisonAmount) || (normalized < comparisonAmount && normalizedNext > comparisonAmount))
                {
                    if (src[x,y] != ColorBgra.TransparentBlack && src[x,y] != ColorBgra.Transparent)
                    {
                        dst[x,y] = ColorBgra.White;
                    }
                    else 
                    {
                        dst[x,y] = ColorBgra.Black;
                    }
                }
                else 
                {
                    if (src[x,y] != ColorBgra.TransparentBlack && src[x,y] != ColorBgra.Transparent)
                    {
                        dst[x,y] = ColorBgra.FromBgr((byte)(255 - (int)Amount2.B), (byte)(255 - (int)Amount2.G), (byte)(255 - (int)Amount2.R));
                    }
                    else 
                    {
                        dst[x,y] = Amount2;
                    }
                }
                
            }
            else 
            {
                dst[x,y] = src[x,y];
            }
        }
    }
}
