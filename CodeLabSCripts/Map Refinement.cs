// Name:Map Refinement
// Submenu:Chris
// Author:
// Title:Map Refinement
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
ListBoxControl Amount1 = 0; // R From:|Source R|Source G| Source B|Source A|Clipboard R|Clipbaord G|Clipboard B|Clipboard A|0|255|Inverted Source R|Inverted Source G|Inverted Source B|Inverted Source A|Inverted Clipboard R|Inverted Clipbaord G|Inverted Clipboard B|Inverted Clipboard A
ListBoxControl Amount2 = 1; // G From:|Source R|Source G| Source B|Source A|Clipboard R|Clipbaord G|Clipboard B|Clipboard A|0|255|Inverted Source R|Inverted Source G|Inverted Source B|Inverted Source A|Inverted Clipboard R|Inverted Clipbaord G|Inverted Clipboard B|Inverted Clipboard A
ListBoxControl Amount3 = 2; // B From:|Source R|Source G| Source B|Source A|Clipboard R|Clipbaord G|Clipboard B|Clipboard A|0|255|Inverted Source R|Inverted Source G|Inverted Source B|Inverted Source A|Inverted Clipboard R|Inverted Clipbaord G|Inverted Clipboard B|Inverted Clipboard A
ListBoxControl Amount4 = 3; // A From:|Source R|Source G| Source B|Source A|Clipboard R|Clipbaord G|Clipboard B|Clipboard A|0|255|Inverted Source R|Inverted Source G|Inverted Source B|Inverted Source A|Inverted Clipboard R|Inverted Clipbaord G|Inverted Clipboard B|Inverted Clipboard A
ListBoxControl Amount5 = 0; // Full Transparency To:|Leave Alone|Transparent|Transparent Black|Black|White|Red|Green|Blue
CheckboxControl Amount6 = false; // [0,1] Invert RGB
CheckboxControl Amount7 = false; // [0,1] Invert A
IntSliderControl Amount8 = 0; // [0,255] Low R
IntSliderControl Amount9 = 255; // [0,255] High R
IntSliderControl Amount10 = 0; // [0,255] Low B
IntSliderControl Amount11 = 255; // [0,255] High B
IntSliderControl Amount12 = 0; // [0,255] Low G
IntSliderControl Amount13 = 255; // [0,255] High G
IntSliderControl Amount14 = 0; // [0,255] Low A
IntSliderControl Amount15 = 255; // [0,255] High A
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{   
    ColorBgra CurrentPixel;
    var working = new Surface(src.Size);
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        if ((Amount1 >= 4 && Amount1 < 8 ) ||
            (Amount2 >= 4 && Amount2 < 8 ) ||
            (Amount3 >= 4 && Amount3 < 8 ) ||
            (Amount4 >= 4 && Amount4 < 8 ) ||
            (Amount1 >= 14 && Amount1 < 18 ) ||
            (Amount2 >= 14 && Amount2 < 18 ) ||
            (Amount3 >= 14 && Amount3 < 18 ) ||
            (Amount4 >= 14 && Amount4 < 18 )   )
        {
            var noClipboardImage = false;
        
            if (img == null)
            {
                noClipboardImage = true;
            }
            
            for (int x = rect.Left; x < rect.Right; x++)
            {
                if (IsCancelRequested) return;
                
                CurrentPixel = src[x,y];
        
                CurrentPixel.R = GetColor(x,y,(int)Amount1, src, noClipboardImage ? src : img);
                CurrentPixel.G = GetColor(x,y,(int)Amount2, src, noClipboardImage ? src : img);
                CurrentPixel.B = GetColor(x,y,(int)Amount3, src, noClipboardImage ? src : img);
                CurrentPixel.A = GetColor(x,y,(int)Amount4, src, noClipboardImage ? src : img);
                
                working[x,y] = CurrentPixel;
            }
        }
        else 
        {
            for (int x = rect.Left; x < rect.Right; x++)
            {
                if (IsCancelRequested) return;
                
                CurrentPixel = src[x,y];
        
                CurrentPixel.R = GetColor(x,y,(int)Amount1, src, src);
                CurrentPixel.G = GetColor(x,y,(int)Amount2, src, src);
                CurrentPixel.B = GetColor(x,y,(int)Amount3, src, src);
                CurrentPixel.A = GetColor(x,y,(int)Amount4, src, src);
                
                working[x,y] = CurrentPixel;
            }
        }
    }
        
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
    
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
            
            var final = working[x,y];
            
            if (final.A == 0)
            {
                switch(Amount5)
                {
                    case 0:
                    break;
                    case 1:
                        final = ColorBgra.Transparent;
                    break;
                    case 2:
                        final = ColorBgra.TransparentBlack;
                    break;
                    case 3:
                        final = ColorBgra.FromBgr(0,0,0);
                    break;
                    case 4:
                        final = ColorBgra.FromBgr(255,255,255);
                    break;
                    case 5:
                        final = ColorBgra.FromBgr(0,0,255);
                    break;
                    case 6:
                        final = ColorBgra.FromBgr(0,255,0);
                    break;
                    case 7:
                        final = ColorBgra.FromBgr(255,0,0);
                    break;
                }
            }
            
             working[x,y] = final;
            if (Amount6)
            {
                working[x,y] = ColorBgra.FromBgra((byte)(255-final.B), (byte)(255-final.G), (byte)(255-final.R), (byte)(final.A));
            }
            
            if (Amount7)     
            {
                working[x,y] = ColorBgra.FromBgra((byte)(final.B), (byte)(final.G), (byte)(final.R), (byte)(255-final.A));
            }
        
            var updated = working[x,y];
            if (updated.R < Amount8) updated.R = (byte)(int)Amount8;
            if (updated.R > Amount9) updated.R = (byte)(int)Amount9;
            if (updated.G < Amount10) updated.G = (byte)(int)Amount10;
            if (updated.G > Amount11) updated.G = (byte)(int)Amount11;
            if (updated.B < Amount12) updated.B = (byte)(int)Amount12;
            if (updated.B > Amount13) updated.B = (byte)(int)Amount13;
            if (updated.A < Amount14) updated.A = (byte)(int)Amount14;
            if (updated.A > Amount15) updated.A = (byte)(int)Amount15;
                
           dst[x,y] = updated;
        }
    }
}

protected Surface img
{
    get { if (_img != null)
            return _img; 
          else
          {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(GetImageFromClipboard));
            t.SetApartmentState(System.Threading.ApartmentState.STA); 
            t.Start();
            t.Join();
            return _img;
          }
        }
}
private Surface _img = null;
private void GetImageFromClipboard()
{
    Image aimg = null;
    IDataObject clippy;
    try
    {
            // Try to paste PNG data.
        if (Clipboard.ContainsData("PNG"))
        {
            Object png_object = Clipboard.GetData("PNG");
            if (png_object is MemoryStream)
            {
                MemoryStream png_stream = png_object as MemoryStream;
                aimg = Image.FromStream(png_stream);
            }
        }
        else if (Clipboard.ContainsImage())
        {
            aimg = Clipboard.GetImage();
        }
    }
    catch (Exception )
    {
    }
    if (aimg != null)
    {
        _img = Surface.CopyFromGdipImage(aimg);
    }
    else
    {
        _img = null;
    }
}

private byte GetColor(int x, int y, int pixelType, Surface source, Surface clipboard)
{
    switch(pixelType)
    {
        case 0:
            return source[x,y].R;
        break;
        case 1:
            return source[x,y].G;
        break;
        case 2:
           return source[x,y].B;
        break;
        case 3:
            return source[x,y].A;
        break;
        case 4:
            if (clipboard == null) return byte.MinValue;
            return clipboard[x,y].R;
        break;
        case 5:
            if (clipboard == null) return byte.MinValue;
            return clipboard[x,y].G;
        break;
        case 6:
            if (clipboard == null) return byte.MinValue;
            return clipboard[x,y].B;
        break;
        case 7:
           if (clipboard == null) return byte.MinValue;
           return clipboard[x,y].A;
        break;
        case 8:
           return 0;
        break;
        case 9:
           return 255;
        break;
        case 10:
            return (byte)(255-source[x,y].R);
        break;
        case 11:
            return(byte)(255-source[x,y].G);
        break;
        case 12:
           return(byte)(255-source[x,y].B);
        break;
        case 13:
            return(byte)(255-source[x,y].A);
        break;
        case 14:
            if (clipboard == null) return byte.MinValue;
            return(byte)(255-clipboard[x,y].R);
        break;
        case 15:
            if (clipboard == null) return byte.MinValue;
            return(byte)(255-clipboard[x,y].G);
        break;
        case 16:
            if (clipboard == null) return byte.MinValue;
            return(byte)(255-clipboard[x,y].B);
        break;
        case 17:
           if (clipboard == null) return byte.MinValue;
            return(byte)(255-clipboard[x,y].A);
        break;
    }
    
    return byte.MinValue;
}