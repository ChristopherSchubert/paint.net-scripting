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
BinaryPixelOp Amount1 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Blending Mode
#endregion

// Working surface
Surface wrk = null;


protected override void OnDispose(bool disposing)
{
    if (disposing)
    {
        // Release any surfaces or effects you've created.
        if (wrk != null) wrk.Dispose();
        wrk = null;
    }

    base.OnDispose(disposing);
}

void PreRender(Surface dst, Surface src)
{
    if (wrk == null)
    {
        wrk = new Surface(src.Size);
    }

    wrk.CopySurface(src);
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
    // Call the copy function
    wrk.CopySurface(src,rect.Location,rect);
    
    var rangeX = rect.Right - rect.Left-1;
    var rangeY = rect.Bottom - rect.Top-1;
    
    var splitX = rangeX/2;
    var splitY = rangeY/2;

    // Now in the main render loop, the wrk canvas has a copy of the src canvas
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            ColorBgra CurrentPixel = wrk[x,y];

            // TODO: Add additional pixel processing code here

            //CurrentPixel = Amount1.Apply(src[x,y], CurrentPixel);
            
            var targetX = 0;
            var targetY = 0;
            
            if (x >= splitX)
            {
                targetX = x - splitX;
            }
            else
            {
                targetX = x + splitX;
            }
            
            if (y >= splitY)
            {
                targetY = y - splitY;
            }
            else
            {
                targetY = y + splitY;
            }

            dst[x,y] = src[targetX, targetY];
        }
    }
}

