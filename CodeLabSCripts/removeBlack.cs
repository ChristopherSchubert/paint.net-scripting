// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
#endregion

void PreRender(Surface dst, Surface src)
{
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
   
    ColorBgra CurrentPixel;
    
    var borderAmount = 4;

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];

            if (x < borderAmount || y < borderAmount || x > rect.Right-borderAmount-1 || y > rect.Bottom-borderAmount-1)
            {
                dst[x,y] = ColorBgra.TransparentBlack;
            }
            else if (
                CurrentPixel.R > 000 &&
            CurrentPixel.G > 000 &&
            CurrentPixel.B > 000)
            {
                dst[x,y] = ColorBgra.FromBgr(255,255,255);
            }
            else if (CurrentPixel.R > 000)
            {
                dst[x,y] = ColorBgra.FromBgr(000,000,255);
            }
            else if (CurrentPixel.G > 000)
            {
                dst[x,y] = ColorBgra.FromBgr(000,255,000);
            }
            else if (CurrentPixel.B > 000)
            {
                dst[x,y] =ColorBgra.FromBgr(255,000,000);
            }
            else
            {
                dst[x,y] = ColorBgra.TransparentBlack;
            }
        }
    }
}

