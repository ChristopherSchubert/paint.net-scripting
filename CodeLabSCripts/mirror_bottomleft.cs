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
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
  
    for(var x = 0; x < rect.Right; x++)
    {
        for(var y = 0; y < rect.Bottom; y++)
        {
            if (IsCancelRequested)
            {
                return;
            }
            
            var targetY = rect.Bottom-1-x;
            var targetX = rect.Left + y;
            
            dst[targetX, targetY] = src[x,y];
        }
    }
    
    
}
