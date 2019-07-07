// Name:Extract Channel Quarters
// Submenu:Chris
// Author:
// Title:Extract Channel Quarters
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode

ListBoxControl Amount1 = 2; // Red As:|Black|White|Red|Green|Blue|Magenta|Cyan|Yellow|TransparentBlack|TransparentWhite
ListBoxControl Amount2 = 3; // Green As:|Black|White|Red|Green|Blue|Magenta|Cyan|Yellow|TransparentBlack|TransparentWhite
ListBoxControl Amount3 = 4; // Blue As:|Black|White|Red|Green|Blue|Magenta|Cyan|Yellow|TransparentBlack|TransparentWhite
ListBoxControl Amount4 = 1; // Alpha As:|Black|White|Red|Green|Blue|Magenta|Cyan|Yellow|TransparentBlack|TransparentWhite
CheckboxControl Amount5 = false; // Invert RGB
CheckboxControl Amount6 = false; // Invert A

#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{

    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            var percentageX = ((float)x) / (float)dst.Width;
            var percentageY = ((float)y) / (float)dst.Height;
            
            var minX = 0;
            var maxX = src.Width-1;
            var minY = 0;
            var maxY = src.Height-1;
            var colorStyle = 0;
            Func<ColorBgra, byte> comparisonPixelFunc;
           
            if (percentageX < .5f && percentageY < .5f)
            {
                // top left
                maxX = (int)(((float)src.Width)/2f) -1;
                maxY = (int)(((float)src.Height)/2f) -1;
                colorStyle = Amount1;
                comparisonPixelFunc = (color) => color.R;
            } 
            else if (percentageX >= .5f && percentageY < .5f)
            {
                // top right
                minX = (int)(((float)src.Width)/2f);
                maxY = (int)(((float)src.Height)/2f) -1;
                colorStyle = Amount2;
                comparisonPixelFunc = (color) => color.G;
            }
            else if (percentageX < .5f && percentageY >= .5f)
            {
                // bottom left
                maxX = (int)(((float)src.Width)/2f) -1;
                minY = (int)(((float)src.Height)/2f);
                colorStyle = Amount3;
                comparisonPixelFunc = (color) => color.B;
            }
            else
            {
                 // bottom right
                minX = (int)(((float)src.Width)/2f);
                minY = (int)(((float)src.Height)/2f);
                colorStyle = Amount4;
                comparisonPixelFunc = (color) => color.A;
            }
            
            var normalizedX = ((float)x - (float)minX) / ((float)maxX - (float)minX);  
            var normalizedY = ((float)y - (float)minY) / ((float)maxY - (float)minY); 
            
            var sourceX = (int)(normalizedX*(src.Width-1));
            var sourceY = (int)(normalizedY*(src.Height-1));
                
            var sourcePixel = src[sourceX, sourceY];
            var pixelValue = comparisonPixelFunc(sourcePixel);
            CurrentPixel = ColorFromInt(colorStyle, pixelValue);
            
            if (Amount5)
            {
                CurrentPixel.R =  (byte)(byte.MaxValue - CurrentPixel.R);
                CurrentPixel.G =  (byte)(byte.MaxValue - CurrentPixel.G);
                CurrentPixel.B =  (byte)(byte.MaxValue - CurrentPixel.B);
            }
            
            if (Amount6)
            {
                CurrentPixel.A =  (byte)(byte.MaxValue - CurrentPixel.A);
            }
            
            dst[x,y] = CurrentPixel;
        }
    }
}

public ColorBgra ColorFromInt(int colorType, byte sourcePixel)
{
    var result = ColorBgra.Black;
    
    switch(colorType)
    {
        // |Black|White|Red|Green|Blue|Magenta|Cyan|Yellow|TransparentBlack|TransparentWhite
        
        case 0:
        result = ColorBgra.FromBgra(
            (byte)(byte.MaxValue-sourcePixel),
            (byte)(byte.MaxValue-sourcePixel),
            (byte)(byte.MaxValue-sourcePixel),
            byte.MaxValue);
        break;
        case 1:
        result = ColorBgra.FromBgra(
            (byte)(sourcePixel),
            (byte)(sourcePixel),
            (byte)(sourcePixel),
            byte.MaxValue);
        break;
        case 2:
        result = ColorBgra.FromBgra(
            (byte)(0),
            (byte)(0),
            (byte)(sourcePixel),
            byte.MaxValue);
        break;
        case 3:
        result = ColorBgra.FromBgra(
            (byte)(0),
            (byte)(sourcePixel),
            (byte)(0),
            byte.MaxValue);
        break;
        case 4:
        result = ColorBgra.FromBgra(
            (byte)(sourcePixel),
            (byte)(0),
            (byte)(0),
            byte.MaxValue);
        break;
        case 5:
        result = ColorBgra.FromBgra(
            (byte)(sourcePixel),
            (byte)(0),
            (byte)(sourcePixel),
            byte.MaxValue);
        break;
        case 6:
        result = ColorBgra.FromBgra(
            (byte)(sourcePixel),
            (byte)(sourcePixel),
            (byte)(0),
            byte.MaxValue);
        break;
        case 7:
        result = ColorBgra.FromBgra(
            (byte)(0),
            (byte)(sourcePixel),
            (byte)(sourcePixel),
            byte.MaxValue);
        break;
        case 8:
        result = ColorBgra.FromBgra(
            (byte)(byte.MaxValue-sourcePixel),
            (byte)(byte.MaxValue-sourcePixel),
            (byte)(byte.MaxValue-sourcePixel),
            (byte)(byte.MaxValue-sourcePixel));
        break;
        
        case 9:
        default:
        result = ColorBgra.FromBgra(
            (byte)(sourcePixel),
            (byte)(sourcePixel),
            (byte)(sourcePixel),
            (byte)(sourcePixel));
        break;
    }
    
    return result;
}
