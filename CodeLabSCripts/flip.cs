// Name:Simple Flipper
// Submenu:Chris
// Author:
// Title:Simple Flipper
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 0; // [0,4096] Flip Region Start X
IntSliderControl Amount2 = 400; // [0,4096] Flip Region End X
IntSliderControl Amount3 = 400; // [0,4096] Flip Region Start Y
CheckboxControl Amount4 = false; // Show Lines
#endregion

// Working surface
Surface wrk = null;

void PreRenderInternal(Surface dst, Surface src)
{
    if (wrk == null)
    {
        wrk = new Surface(src.Size);
    }

    wrk.CopySurface(src);
}

int flipRegionStartX;
int flipRegionEndX;
int flipRegionStartY;
bool showLines;

void RenderInternal(Surface dst, Surface src, Rectangle rect)
{
    flipRegionStartX = Amount1%wrk.Width;
    flipRegionEndX = Amount2%wrk.Width;
    flipRegionStartY = Amount3%wrk.Height;
    showLines = Amount4;
    
    if (flipRegionStartX > wrk.Width) return;
    if (flipRegionEndX > wrk.Width) return;
    if (flipRegionStartX >= flipRegionEndX) return;
    if (flipRegionStartY > wrk.Height) return;
    
    var topFlipRegionSize = flipRegionStartY;
    var bottomFlipRegionSize = src.Height - flipRegionStartY;
    
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = wrk[x,y];
            
            if (x < flipRegionStartX || x >= flipRegionEndX)
            {
                dst[x,y] = CurrentPixel;
            }
            else 
            {
                dst[x,y] = wrk[x,y];
                
                if (y < flipRegionStartY)
                {
                    dst[x,y] = wrk[x,dst.Height-flipRegionStartY+y];
                }
                else 
                {
                    dst[x,y] = wrk[x,y-flipRegionStartY];
                }
            }
        }
    }
}

void PostRenderInternal(Surface dst, Surface src, Rectangle rect)
{
    if (showLines)
    {
        var thickness = 5;
        var halfThickness = thickness/2;
        
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            if (IsCancelRequested) return;
            for (int x = rect.Left; x < rect.Right; x++)
            {
                if (x == flipRegionStartX || x == flipRegionEndX-1)
                {
                    for(var i = -halfThickness; i < halfThickness; i++)
                    {
                        if (x+i > rect.Left && x+i < rect.Right)
                        {
                            dst[x+i,y] = ColorBgra.Cyan;
                        }
                        
                    }
                }
                else if (x >= flipRegionStartX && x < flipRegionEndX && y == flipRegionStartY)
                {
                    for(var i = -halfThickness; i < halfThickness; i++)
                    {
                        if (y+i > rect.Top && y+i < rect.Bottom)
                        {
                            dst[x,y+i] = ColorBgra.Magenta;
                        }
                        
                    }
                }
            }
        }
    }
}

void PreRender(Surface dst, Surface src)
{
    try
    {
        PreRenderInternal(dst,src);
    } 
    catch(Exception x)
    {
        Debug.WriteLine(x);
        return;
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    try
    {
        RenderInternal(dst,src,rect);
        PostRenderInternal(dst,src,rect);
    } 
    catch(Exception x)
    {
        Debug.WriteLine(x);
        return;
    }
}



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
