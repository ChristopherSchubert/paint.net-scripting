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
IntSliderControl Amount1 = 24; // [0,256] Blend Start
IntSliderControl Amount2 = 24; // [0,256] Blend Distance
BinaryPixelOp Amount3 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Blend Type
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    ColorBgra CurrentPixel;
    var working = new Surface(dst.Size);
    
    var width = rect.Right - rect.Left;
    var height = rect.Bottom - rect.Top;
    
    //if (width%2 != 0 || height%2 != 0)
    //{
    //    return;
    //}
    
    var splitPointX = width/2;
    var splitPointY = height/2;
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
        
            var targetX = 0;
            var targetY = 0;
            
            if (x >= splitPointX)
            {
                targetX = x - splitPointX;
            }
            else 
            {
                targetX = x + splitPointX;
            }
            
            if (y >= splitPointY)
            {
                targetY = y - splitPointY;
            }
            else 
            {
                targetY = y + splitPointY;
            }
            
            working[targetX, targetY] = src[x,y];
            //dst[x, y] = ColorBgra.Black;
               
        }
    }
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
      
        for (int x = rect.Left; x < rect.Right; x++)
        {            
            if (IsCancelRequested) return;
            
            dst[x,y] = working[x,y];
        }
    }
}
