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
IntSliderControl Amount1 = 4; // [1,100] TrimAmount
#endregion

void PreRender(Surface dst, Surface src)
{
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
   
    ColorBgra CurrentPixel;
    
    var borderAmount = Amount1;

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
        }
    }
}

