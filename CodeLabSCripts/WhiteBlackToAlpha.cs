// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 0; // [0,100] Slider 1 Description
IntSliderControl Amount2 = 0; // [0,100] Slider 2 Description
IntSliderControl Amount3 = 0; // [0,100] Slider 3 Description
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
            
            if (CurrentPixel == ColorBgra.Black)
            {
                CurrentPixel = ColorBgra.TransparentBlack;
            }
            else 
            {
                CurrentPixel = ColorBgra.Black;
            }
            dst[x,y] = CurrentPixel;
        }
    }
}
