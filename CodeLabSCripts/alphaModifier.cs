// Name:Alpha Modifier
// Submenu:Chris
// Author:
// Title:Alpha Modifier
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
ListBoxControl Amount1 = 1; // Option 1|None|Debug|Alpha Black|Alpha White|Threshold (Midpoint)|Threshold (Extreme Opaque)|Threshold (Extreme Transparent)|Threshold (Midpoint+SaveRGB)|Threshold (Extreme Opaque+SaveRGB)|Threshold (Extreme Transparent+SaveRGB)|Alpha To Grey|Alpha To Red|Alpha To Green|Alpha To Blue
ListBoxControl Amount2 = 0; // Option 2|None|Debug|Alpha Black|Alpha White|Threshold (Midpoint)|Threshold (Extreme Opaque)|Threshold (Extreme Transparent)|Threshold (Midpoint+SaveRGB)|Threshold (Extreme Opaque+SaveRGB)|Threshold (Extreme Transparent+SaveRGB)|Alpha To Grey|Alpha To Red|Alpha To Green|Alpha To Blue
ListBoxControl Amount3 = 0; // Option 3|None|Debug|Alpha Black|Alpha White|Threshold (Midpoint)|Threshold (Extreme Opaque)|Threshold (Extreme Transparent)|Threshold (Midpoint+SaveRGB)|Threshold (Extreme Opaque+SaveRGB)|Threshold (Extreme Transparent+SaveRGB)|Alpha To Grey|Alpha To Red|Alpha To Green|Alpha To Blue
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    // Delete any of these lines you don't need
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            CurrentPixel = ProcessAlphaPixel(CurrentPixel, (int)Amount1);
            CurrentPixel = ProcessAlphaPixel(CurrentPixel, (int)Amount2);
            CurrentPixel = ProcessAlphaPixel(CurrentPixel, (int)Amount3);
            dst[x,y] = CurrentPixel;
        }
    }
}

ColorBgra ProcessAlphaPixel(ColorBgra currentPixel, int style)
{ 
    switch(style)
    {
        case 0:
            return currentPixel;
        case 1:
            if (currentPixel == ColorBgra.Transparent) return ColorBgra.White;
            if (currentPixel == ColorBgra.TransparentBlack) return ColorBgra.Black;
            HsvColor color =new HsvColor(255-currentPixel.A, 100,100);
            return ColorBgra.FromColor(color.ToColor());
        break;
        case 2:
            if (currentPixel == ColorBgra.Transparent) { return ColorBgra.TransparentBlack; } else {return currentPixel;}
        break;
        case 3:
            if (currentPixel == ColorBgra.TransparentBlack) { return ColorBgra.Transparent; } else {return currentPixel;}
        break;
        case 4:
            if (currentPixel.A > 127) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue); }
        break;
        case 5:
            if (currentPixel.A > 0) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(byte.MinValue, byte.MinValue, byte.MinValue,  byte.MinValue); }
        break;
        case 6:
            if (currentPixel.A > 254) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue); }
        break;
        case 7:
            if (currentPixel.A > 127) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MinValue); }
        break;
        case 8:
            if (currentPixel.A > 0) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MinValue); }
        break;
        case 9:
            if (currentPixel.A > 254) { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MaxValue); } else { return ColorBgra.FromBgra(currentPixel.B, currentPixel.G, currentPixel.R, byte.MinValue); }
        break;
        case 10:
            return ColorBgra.FromBgra(currentPixel.A,currentPixel.A,currentPixel.A,byte.MaxValue);
        case 11:
            return ColorBgra.FromBgra(byte.MinValue,byte.MinValue,currentPixel.A,byte.MaxValue);
        case 12:
            return ColorBgra.FromBgra(byte.MinValue,currentPixel.A,byte.MinValue,byte.MaxValue);
        case 13:
            return ColorBgra.FromBgra(currentPixel.A,byte.MinValue,byte.MinValue,byte.MaxValue);
        default: 
            throw new NotSupportedException("Can not support alpha modifier option " + style.ToString());
    }
}
