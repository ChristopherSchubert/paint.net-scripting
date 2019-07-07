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
IntSliderControl Amount1 = 255; // [0,255] R
IntSliderControl Amount2 = 0; // [0,255] G
IntSliderControl Amount3 = 0; // [0,255] B
#endregion

void PreRender(Surface dst, Surface src)
{
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
    Surface working = new Surface(dst.Size);
    ColorBgra masterColor = ColorBgra.FromBgra((byte)Amount3, (byte)Amount2, (byte)Amount1, 255);
    ColorBgra CurrentPixel;

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
        
            CurrentPixel = src[x,y];
            
            if (CurrentPixel == ColorBgra.Black)
            {
                if (
                    (src[x+1,y] == ColorBgra.Black || src[x+1,y] == ColorBgra.TransparentBlack) &&
                    (src[x+1,y+1] == ColorBgra.Black || src[x+1,y+1] == ColorBgra.TransparentBlack) &&
                    (src[x,y+1] == ColorBgra.Black || src[x,y+1] == ColorBgra.TransparentBlack) &&
                    (src[x-1,y+1] == ColorBgra.Black || src[x-1,y+1] == ColorBgra.TransparentBlack) &&
                    (src[x-1,y] == ColorBgra.Black || src[x-1,y] == ColorBgra.TransparentBlack) &&
                    (src[x-1,y-1] == ColorBgra.Black || src[x-1,y-1] == ColorBgra.TransparentBlack) &&
                    (src[x,y-1] == ColorBgra.Black || src[x,y-1] == ColorBgra.TransparentBlack) &&
                    (src[x+1,y-1] == ColorBgra.Black || src[x+1,y-1] == ColorBgra.TransparentBlack)
                )
                {
                    working[x,y] = ColorBgra.TransparentBlack;
                }
                else 
                {
                    working[x,y] = CurrentPixel;
                }
            }
            else 
            {
                working[x,y] = CurrentPixel;
            }
        }
    }
    
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        if (working[rect.Right-2,y] == ColorBgra.Black)
        {
            working[rect.Right-1,y] = ColorBgra.Black;
            
            if (working[rect.Right-3,y] != ColorBgra.Black)
            {
                working[rect.Right-2,y] = ColorBgra.TransparentBlack;
            }
        }
        
        if (working[1,y] == ColorBgra.Black)
        {
            working[0,y] = ColorBgra.Black;

            if (working[2,y] != ColorBgra.Black)
            {
                working[1,y] = ColorBgra.TransparentBlack;
            }
        }
    }
    
    for (int x = rect.Left; x < rect.Right; x++)
    {
        if (IsCancelRequested) return;
        
        if (working[x,rect.Bottom-2] == ColorBgra.Black)
        {
            working[x,rect.Bottom-1] = ColorBgra.Black;
            
            if (working[x,rect.Bottom-3] != ColorBgra.Black)
            {
                working[x,rect.Bottom-2] = ColorBgra.TransparentBlack;
            }
        }
        
        if (working[x,1] == ColorBgra.Black)
        {
            working[x,0] = ColorBgra.Black;

            if (working[x,2] != ColorBgra.Black)
            {
                working[x,1] = ColorBgra.TransparentBlack;
            }
        }
    }
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
        
            if (working[x,y] == ColorBgra.Black)
            {
                working[x,y] = masterColor;
            }
            else if (working[x,y] == ColorBgra.White || working[x,y] == ColorBgra.TransparentBlack)
            {
                working[x,y] = ColorBgra.Black;
            }
        }
    }
    
    dst.CopySurface(working);
    
}


void FloodFill(int x, int y, Surface dst, Rectangle rect, Func<ColorBgra, bool> comparison, Func<ColorBgra, ColorBgra> fillOperation)
{
    var stack = new Stack<Tuple<int,int>>();
    
    stack.Push(new Tuple<int,int>(x,y));
    
    Tuple<int,int> next;
    
    //while(false)
    while(stack.Count > 0)
    {
        if (IsCancelRequested) return;
        
        next = stack.Pop();
        
        x = next.Item1;
        y = next.Item2;
    
        if ((x < 0) || (x >= rect.Right)) continue;
        if ((y < 0) || (y >= rect.Bottom)) continue;    
        
        if (comparison(dst[x,y]))
        {
            if (IsCancelRequested) return;
        
            dst[x,y] = fillOperation(dst[x,y]);
            
            stack.Push(new Tuple<int,int>(x+1,y));
            stack.Push(new Tuple<int,int>(x,y+1));
            stack.Push(new Tuple<int,int>(x-1,y));
            stack.Push(new Tuple<int,int>(x,y-1));
            
        }
    }
}

Tuple<int,int> FindNearestTransparentPixel(int x, int y, Surface dst, Rectangle rect)
{
    var minDirectionIndex = -1;
    var minDistance = Int32.MaxValue;
    
    int currentDistance, currentX, currentY;

    for(var currentDirectionIndex = 0; currentDirectionIndex < 8; currentDirectionIndex++)
    {
        currentX = x;
        currentY = y;
        currentDistance = 0;
        
        var movement = GetXYForMovementIndex(currentDirectionIndex);
        var found = false;
        
        while(currentX >= 0 && currentX <= rect.Right-1 && currentY >= 0 && currentY <= rect.Bottom-1 && currentDistance < minDistance)
        {
            
            if (IsCancelRequested) return new Tuple<int,int>(-1,-1);
    
            currentX += movement.Item1;
            currentY += movement.Item2;
            
            currentDistance += 1;
        
            if (dst[currentX,currentY] == ColorBgra.TransparentBlack)
            {
                found = true;
                break;
            }
        }    
        
        if (found && currentDistance < minDistance)
        {
            minDistance = currentDistance;
            minDirectionIndex = currentDirectionIndex;
        }
    }
    
    if (minDirectionIndex == -1)
    {
        return new Tuple<int,int>(-1,-1);
    }
    
    var finalMovementDirection = GetXYForMovementIndex(minDirectionIndex);
    return new Tuple<int,int>(x + (finalMovementDirection.Item1*minDistance), y + (finalMovementDirection.Item2*minDistance));
}

Tuple<int, int> GetXYForMovementIndex(int movementIndex)
{
    int x = 0, y = 0;
    
    if (movementIndex == 0 || movementIndex == 1 || movementIndex == 7)
    {
        x = 1;
    }
    else if (movementIndex == 3 || movementIndex == 4 || movementIndex == 5)
    {
        x = -1;
    }
    
    if (movementIndex == 1 || movementIndex == 2 || movementIndex == 3)
    {
        y = 1;
    }
    else if (movementIndex == 5 || movementIndex == 6 || movementIndex == 7)
    {
        y = -1;
    }
    
    return new Tuple<int,int>(x, y);
}

List<Tuple<int,int>> GetStraightPathBetweenTwoPoints(int startingX, int startingY, int endingX, int endingY, int thickness)
{
    Debug.WriteLine("Getting path from (" + startingX.ToString() + "," +startingY.ToString() + ") -to- (" + endingX.ToString() + "," + endingY.ToString() + ")");
    
    var results = new List<Tuple<int,int>>();
    if (IsCancelRequested) return results;
    
    int[] xValues;
    
    if (startingX == endingX)
    {
        xValues = new int[0];
    }
    else
    {
        xValues = new int[Math.Abs(startingX-endingX)+1];
        
        if (startingX < endingX)
        {
            for(var i = 0; i < xValues.Length; i++)
            {
                xValues[i] = startingX+i;
            }
        }
        else
        {
            for(var i = 0; i < xValues.Length; i++)
            {
                xValues[i] = startingX-i;
            }
        }
    }
    
    int[] yValues;
    
    if (startingY == endingY)
    {
        yValues = new int[0];
    }
    else
    {
        yValues = new int[Math.Abs(startingY-endingY)+1];
        
        if (startingY < endingY)
        {
            for(var i = 0; i < yValues.Length; i++)
            {
                yValues[i] = startingY+i;
            }
        }
        else
        {
            for(var i = 0; i < yValues.Length; i++)
            {
                yValues[i] = startingY-i;
            }
        }
    }
    
    var maxSteps = Math.Max(yValues.Length, xValues.Length);
    
    for(var i = 0; i < maxSteps; i++)
    {
        if (xValues.Length == 0)
        {
            results.Add(new Tuple<int,int>(endingX, yValues[i]));
        }
        else if (yValues.Length == 0)
        {
            results.Add(new Tuple<int,int>(xValues[i], endingY));
        }
        else
        {
            results.Add(new Tuple<int,int>(xValues[i], yValues[i]));
        }
    }
    
    for(var i = results.Count-1; i >= 0; i--)    
    {  
        for(var thicknessIterator = 1; thicknessIterator <= thickness; thicknessIterator++)
        {
            if (yValues.Length == 0)
            {
                results.Add(new Tuple<int,int>(xValues[i], endingY+thicknessIterator));
            }
            else if (xValues.Length == 0)
            {
                results.Add(new Tuple<int,int>(endingX+thicknessIterator, yValues[i]));
            }
            else if (startingX < endingX)
            {
                results.Add(new Tuple<int,int>(xValues[i]-thicknessIterator, yValues[i]));
            }
            else
            {
                results.Add(new Tuple<int,int>(xValues[i]+thicknessIterator, yValues[i]));
            }
        }
    }
    
    return results;
}


void RemoveIslandPixels (Surface dst, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;

            if (dst[x,y] == ColorBgra.TransparentBlack)
            {
                continue;
            }
            
            if (
                dst[x+1,y+0] == ColorBgra.TransparentBlack &&
                dst[x+1,y+1] == ColorBgra.TransparentBlack &&
                dst[x+0,y+1] == ColorBgra.TransparentBlack &&
                dst[x-1,y+1] == ColorBgra.TransparentBlack &&
                dst[x-1,y+0] == ColorBgra.TransparentBlack &&
                dst[x-1,y-1] == ColorBgra.TransparentBlack &&
                dst[x+0,y-1] == ColorBgra.TransparentBlack &&
                dst[x+1,y-1] == ColorBgra.TransparentBlack)
            {
                Debug.WriteLine("Removing island at (" + x.ToString() + "," + y.ToString() + ").");
                dst[x,y] = ColorBgra.TransparentBlack;
            }
            
        }
    }
}